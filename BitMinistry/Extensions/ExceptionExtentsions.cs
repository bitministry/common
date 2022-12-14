using System;

namespace BitMinistry
{
    public static class ExceptionExtentsions
    {

        public static void ThrowIfNull(this object obj, string message = null )
        {
            if (obj == null)
                throw new NullReferenceException(message ?? "No such object");
        }
        public static void ThrowIfArgumentNull(this object obj, string argumentName )
        {
            if (obj == null)
                throw new ArgumentNullException( argumentName );
        }
        
        public static string AllMessages (this Exception mainEx )
        {
            var obj = new object();
            lock (obj)
            {
                var msg = mainEx?.Message;
                var ex = mainEx?.InnerException;
                while (ex?.Message != null)
                {
                    msg += Environment.NewLine + ex.Message;
                    ex = ex.InnerException;
                }
                return msg;
            }
        }


        public static string InnerMostMessage(this Exception mainEx)
        {
            var obj = new object();
            lock (obj)
            {
                var msg = mainEx?.Message;
                var ex = mainEx?.InnerException;
                while (ex?.Message != null)
                {
                    msg = ex.Message;
                    ex = ex.InnerException;
                }
                return msg;
            }
        }


        public static void Log(this Exception ex, string logName = "Exception", string preliminaryInfo = null)
        {
            var errorMsg = $@"

{DateTime.UtcNow}
{preliminaryInfo}

{ex.AllMessages()}


{ex.StackTrace}

{new string('=', 222)}


";
            SimpleLogger.AppendToFile( errorMsg, logName + ".error.txt");
        }




    }
}