using System;

namespace BitMinistry.Exceptions
{
    public class ThreadSafeResponseRedirectException : Exception
    {
        public string Url { get; private set; }
        public ThreadSafeResponseRedirectException(string url)
        {
            Url = url;
        }
    }
}
