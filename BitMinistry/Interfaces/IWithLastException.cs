using System;

namespace BitMinistry 
{

    public interface IWithLastException
    {
        TimedException LastException { get; }
    }

    public class TimedException
    {
        private TimedException() { }

        public TimedException(Exception ex)
        {
            Ex = ex;
            When = DateTime.UtcNow;
        }

        public Exception Ex { get; }
        public DateTime When { get; }
    }
}
