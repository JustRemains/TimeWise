using System;
using System.IO;

namespace TimeWise.Utilities
{
    /// <summary>
    /// ??????????
    /// </summary>
    public static class FileLogger
    {
        private static readonly string LogFilePath;
        private static readonly object LockObject = new object();

        static FileLogger()
        {
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TimeWise", "Logs");
            Directory.CreateDirectory(logDirectory);
            
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            LogFilePath = Path.Combine(logDirectory, $"timewise_startup_{timestamp}.log");
        }

        /// <summary>
        /// ????
        /// </summary>
        public static void Log(string message)
        {
            try
            {
                lock (LockObject)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logEntry = $"[{timestamp}] {message}";
                    
                    File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
                    
                    // ??????????????
                    Console.WriteLine(logEntry);
                }
            }
            catch
            {
                // ????????????????
            }
        }

        /// <summary>
        /// ????
        /// </summary>
        public static void LogException(string context, Exception exception)
        {
            try
            {
                Log($"ERROR in {context}: {exception.Message}");
                Log($"Stack trace: {exception.StackTrace}");
                if (exception.InnerException != null)
                {
                    Log($"Inner exception: {exception.InnerException.Message}");
                    Log($"Inner stack trace: {exception.InnerException.StackTrace}");
                }
            }
            catch
            {
                // ????????
            }
        }

        /// <summary>
        /// ????????
        /// </summary>
        public static string GetLogFilePath()
        {
            return LogFilePath;
        }
    }
}