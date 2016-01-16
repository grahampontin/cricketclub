using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDAL;

namespace CricketClubMiddle.Logging
{
    public class Logger
    {

        private static Severity _Level = Severity.Error;
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
                myDao.LogMessage(message, e.Message+Environment.NewLine+e.StackTrace, severity.ToString(), DateTime.Now, e.InnerException?.ToString());
            }
        }
    }

    public enum Severity
    {
        Info = 3,
        Warn = 2,
        Error = 1,
        Debug = 4

    }
}
