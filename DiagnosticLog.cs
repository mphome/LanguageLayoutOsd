using System;
using System.IO;
using System.Text;

namespace LanguageLayoutOsd
{
    internal static class DiagnosticLog
    {
        private static readonly object Sync = new object();
        private static string _logPath;

        public static string Initialize()
        {
            var appName = "LanguageLayoutOsd";
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var folder = Path.Combine(localAppData, appName);

            try
            {
                Directory.CreateDirectory(folder);
                _logPath = Path.Combine(folder, "diagnostic.log");
            }
            catch
            {
                _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "diagnostic.log");
            }

            Write("=== Application starting ===");
            Write("Process: " + AppDomain.CurrentDomain.FriendlyName);
            Write("BaseDir: " + AppDomain.CurrentDomain.BaseDirectory);
            Write("LogPath: " + _logPath);
            return _logPath;
        }

        public static void Write(string message)
        {
            if (string.IsNullOrEmpty(_logPath))
            {
                return;
            }

            lock (Sync)
            {
                try
                {
                    var line = string.Format(
                        "{0:yyyy-MM-dd HH:mm:ss.fff} | {1}{2}",
                        DateTime.Now,
                        message ?? string.Empty,
                        Environment.NewLine);
                    File.AppendAllText(_logPath, line, Encoding.UTF8);
                }
                catch
                {
                }
            }
        }

        public static void WriteException(string context, Exception ex)
        {
            if (ex == null)
            {
                Write(context + ": <null>");
                return;
            }

            Write(context + ": " + ex);
        }

        public static string GetLogDirectory()
        {
            if (!string.IsNullOrEmpty(_logPath))
            {
                var dir = Path.GetDirectoryName(_logPath);
                if (!string.IsNullOrEmpty(dir))
                {
                    return dir;
                }
            }

            var appName = "LanguageLayoutOsd";
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, appName);
        }
    }
}
