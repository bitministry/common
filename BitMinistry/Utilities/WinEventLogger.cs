using System.Diagnostics;

namespace BitMinistry.Utilities
{
    public class WinEvent
    {
        public static string LogName = "Application";
        public static string SourceName;

        public static void Info(string mesage, int eventId = 1000) => Log(EventLogEntryType.Information, mesage, eventId);
        public static void Warm (string mesage, int eventId = 1000) => Log(EventLogEntryType.Warning, mesage, eventId);
        public static void Error (string mesage, int eventId = 1000) => Log(EventLogEntryType.Error, mesage, eventId);

        public static void Log(EventLogEntryType severity, string mesage, int eventId = 1000  ) {

            using (EventLog eventLog = new EventLog(LogName))
            {
                eventLog.Source = SourceName;

                eventLog.WriteEntry(mesage,
                                    severity,
                                    eventID: eventId); 

            }        
        
        }
    }
}
