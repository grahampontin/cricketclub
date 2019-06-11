using System;

namespace CricketClubDAL
{
    public class DbLogger
    {

        private static Severity _Level = Severity.Debug;
        public static Severity LoggingLevel
        {
            get { return _Level; }
            set { _Level = value; }
        }

        public static void Log(string message, Exception e, Severity severity)
        {
            if (severity <= LoggingLevel)
            {
                Dao myDao = new Dao();
                myDao.LogMessage(message, e?.Message+Environment.NewLine+e?.StackTrace, severity.ToString(), DateTime.Now, e?.InnerException?.ToString());
            }
        }
    }
}