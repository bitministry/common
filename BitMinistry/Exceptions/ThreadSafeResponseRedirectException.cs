using System;

namespace BitMinistry.Exceptions
{
    public class ThreadSafeResponseRedirectException : Exception
    {
        public string Url { get; private set; }
        public bool RefreshLogin { get; private set; }
        public ThreadSafeResponseRedirectException(string url, bool refreshLogin = false)
        {
            Url = url;
            RefreshLogin = refreshLogin;
        }
    }
}
