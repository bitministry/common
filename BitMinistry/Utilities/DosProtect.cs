using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BitMinistry
{
    public static class DosProtect
    {
        public struct InitParameters
        {
            public int ShortLimit;
            public int ShortWindowSeconds;
            public int ShortBanMinutes;
            public int LongLimit;
            public int LongWindowSeconds;
            public int LongBanMinutes;
        }

        private static readonly ConcurrentDictionary<string, TrackedQueue> _requests =
            new ConcurrentDictionary<string, TrackedQueue>();

        private static readonly ConcurrentDictionary<string, DateTime> _banned =
            new ConcurrentDictionary<string, DateTime>();

        private static int ShortLimit;
        private static int ShortWindow;
        private static int ShortBanMin;
        private static int LongLimit;
        private static int LongWindow;
        private static int LongBanMin;

        private static System.Timers.Timer _cleanupTimer;
        private static int _cleaning = 0;
        private static bool _disposed;

        // Wrap Queue with last activity for efficient inactivity check
        private class TrackedQueue : Queue<DateTime>
        {
            public DateTime LastActivity { get; private set; } = DateTime.UtcNow;

            public new void Enqueue(DateTime item)
            {
                base.Enqueue(item);
                LastActivity = item;
            }
        }

        public static void Init(InitParameters pars)
        {
            ShortLimit = pars.ShortLimit;
            ShortWindow = pars.ShortWindowSeconds;
            ShortBanMin = pars.ShortBanMinutes;
            LongLimit = pars.LongLimit;
            LongWindow = pars.LongWindowSeconds;
            LongBanMin = pars.LongBanMinutes;

            _cleanupTimer = new System.Timers.Timer(60000);
            _cleanupTimer.AutoReset = true;
            _cleanupTimer.Elapsed += (s, e) => Cleanup();
            _cleanupTimer.Start();
        }

        /// <summary>
        /// Checks whether the given IP may proceed.
        /// Returns null if allowed, or a descriptive string if banned or over limit.
        /// </summary>
        public static string[] Check(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip) || ip == "unknown")
                return null;  // Skip unknowns

            var now = DateTime.UtcNow;

            // Check active bans
            if (_banned.TryGetValue(ip, out var until) && until > now)
                return new[] { $"IP is banned until {until:u}" };
            _banned.TryRemove(ip, out _);

            var trackedQ = _requests.GetOrAdd(ip, _ => new TrackedQueue());

            lock (trackedQ)
            {
                // Remove timestamps outside long window
                while (trackedQ.Count > 0 && (now - trackedQ.Peek()).TotalSeconds > LongWindow)
                    trackedQ.Dequeue();

                trackedQ.Enqueue(now);

                int shortCount = shortCount = trackedQ.Count(t => (now - t).TotalSeconds <= ShortWindow);

                int longCount = trackedQ.Count;

                if (shortCount > ShortLimit)
                {
                    _banned[ip] = now.AddMinutes(ShortBanMin);
                    _requests.TryRemove(ip, out _);
                    return new[] { 
                        $"Too many requests (> {ShortLimit} in {ShortWindow}s). Banned for {ShortBanMin} min.",
                        "short" 
                    };
                }

                if (longCount > LongLimit)
                {
                    _banned[ip] = now.AddMinutes(LongBanMin);
                    _requests.TryRemove(ip, out _);
                    return new[] {
                        $"Too many requests (> {LongLimit} in {LongWindow}s). Banned for {LongBanMin} min.",
                        "long"
                    };
                }
            }

            return null; // allowed
        }

        /// <summary>
        /// Periodically removes stale IP entries and expired bans.
        /// </summary>
        private static void Cleanup()
        {
            if (_disposed || Interlocked.Exchange(ref _cleaning, 1) == 1)
                return;

            try
            {
                var now = DateTime.UtcNow;

                // Purge expired bans
                var expiredBans = _banned.Where(kvp => kvp.Value <= now).Select(kvp => kvp.Key).ToArray();
                foreach (var key in expiredBans)
                    _banned.TryRemove(key, out _);

                // Purge inactive request queues
                var inactiveIps = new List<string>();
                foreach (var kvp in _requests)
                {
                    var q = kvp.Value;
                    lock (q)
                    {
                        // Prune old first
                        while (q.Count > 0 && (now - q.Peek()).TotalSeconds > LongWindow)
                            q.Dequeue();

                        if (q.Count == 0 || (now - q.LastActivity).TotalSeconds > LongWindow * 3)
                            inactiveIps.Add(kvp.Key);
                    }
                }
                foreach (var key in inactiveIps)
                    _requests.TryRemove(key, out _);
            }
            catch
            {
                // Ignore cleanup errors
            }
            finally
            {
                Interlocked.Exchange(ref _cleaning, 0);
            }
        }

        /// <summary>
        /// Manually clears all tracked requests and bans.
        /// </summary>
        public static void PurgeAll()
        {
            _requests.Clear();
            _banned.Clear();
        }

        /// <summary>
        /// Stops the cleanup timer (call at app shutdown).
        /// </summary>
        public static void Dispose()
        {
            if (_disposed) return;
            _cleanupTimer?.Stop();
            _cleanupTimer?.Dispose();
            _disposed = true;
        }

    }
}