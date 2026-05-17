using System;
using System.IO;

namespace OurIPH.Services
{
    public static class AppLogger
    {
        public static void Info(string message)
        {
            Write("OurIPH-runtime.log", "INFO", message);
        }

        public static void Error(string message, Exception exception)
        {
            Write("OurIPH-runtime.log", "ERROR", message + Environment.NewLine + exception);
            Write("OurIPH-crash.log", "ERROR", message + Environment.NewLine + exception);
        }

        public static void Startup(string message)
        {
            Write("OurIPH-startup.log", "STARTUP", message);
            Info(message);
        }

        private static void Write(string fileName, string level, string message)
        {
            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                var line = string.Format("[{0:O}] [{1}] {2}{3}", DateTime.Now, level, message, Environment.NewLine);
                File.AppendAllText(path, line);
            }
            catch
            {
                // Logging must never make startup or shutdown fail.
            }
        }
    }
}
