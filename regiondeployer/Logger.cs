using System;
using System.IO;

namespace regiondeployer
{
    public class LoggingLock
    {
        public static readonly object Lock = new object();
    }

    public class Logger
    {
        private readonly string _logFile;

        public Logger()
        {
            _logFile = Directory.GetCurrentDirectory() + "/log.txt";
            using (var init = File.Create(_logFile)) { }
        }

        public void LogToConsoleAndFile(string message)
        {
            lock (LoggingLock.Lock)
            {
                Console.WriteLine(message);
                using (var logFileStream = File.AppendText(_logFile))
                {
                    logFileStream.WriteLine(message);
                }
            }
        }
    }
}