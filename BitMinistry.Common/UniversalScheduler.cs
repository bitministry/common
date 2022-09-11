using System;
using System.Collections.Generic;
using System.Timers;
using Timer = System.Threading.Timer;

namespace BitMinistry.Common
{
    public class UniversalScheduler
    {
        private static UniversalScheduler _instance;
        public readonly Dictionary<string,Timer> Timers = new Dictionary<string, Timer>();

        public static UniversalScheduler I => _instance ?? (_instance = new UniversalScheduler());

        public void ScheduleDaily(string key, int hour, int min, TimeSpan interval, Action task)
        {
            DateTime now = DateTime.UtcNow;
            DateTime firstRun = new DateTime(now.Year, now.Month, now.Day, hour, min, 0);
            if (now > firstRun)
                firstRun = firstRun.AddDays(1);

            Schedule(key, interval, task, firstRun, now);
        }


        public void ScheduleMonthly(string key, int day, int hour, int min, TimeSpan interval, Action task)
        {
            DateTime now = DateTime.UtcNow;
            DateTime firstRun = new DateTime(now.Year, now.Month, day, hour, min, 0);
            if (now > firstRun)
                firstRun = firstRun.AddMonths(1);

            Schedule(key, interval, task, firstRun, now);
        }

        public void Schedule(string key, TimeSpan interval, Action task, DateTime? firstRun = null, DateTime? now = null)
        {
            if (Timers.ContainsKey(key))
                Timers[key].Dispose();

            now = now ?? DateTime.UtcNow;
            firstRun = firstRun ?? DateTime.UtcNow;

            TimeSpan timeToGo = firstRun.Value - now.Value;
            if (timeToGo <= TimeSpan.Zero)
                timeToGo = TimeSpan.Zero;

            var timer = new Timer(x => { task.Invoke(); }, null, timeToGo, interval);
            
            Timers[key] = timer;



        }
    }

    public class BTimer : IDisposable
    {
        public System.Timers.Timer T;

        public BTimer(System.Timers.Timer t = null )
        {
            T = t ?? new System.Timers.Timer();
        }

        private readonly List<ElapsedEventHandler> _handlers = new List<ElapsedEventHandler>();
        public void AddElapsedCallBack(ElapsedEventHandler act)
        {
            T.Elapsed += act;
            _handlers.Add(act);
        }

        public void ClearHandlers()
        {
            foreach (var handler in _handlers)
                T.Elapsed -= handler;
        }

        public void Dispose()
        {
            T.Dispose();
            T = null;
        }
    }

}
