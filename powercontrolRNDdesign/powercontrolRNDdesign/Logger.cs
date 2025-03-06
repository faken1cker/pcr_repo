using System;
using System.IO;
using System.Windows.Forms;

namespace powercontrolRNDdesign
{
    public static class Logger
    {
        // A lock object to ensure thread-safe access when writing to the file
        private static readonly object lockObj = new object();

        /// <summary>
        /// Logs a message with a specified severity. Default severity is "Info".
        /// The log file is created (or appended) in the application's directory.
        /// The filename follows the daily pattern: applicationYYYYMMDD.log
        /// 
        /// Example log line:
        /// 2025-03-01 10:20:45 [Info] [User: Emma] Some action description
        /// </summary>
        /// <param name="actionDescription">A short description of the action/event</param>
        /// <param name="severity">Severity label, for instance "Info", "Warning", "Error"</param>
        public static void LogAction(string actionDescription, string severity = "Info")
        {
            // Construct the log line
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string userName = Environment.UserName;
            string logLine = $"{timeStamp} [{severity}] [User: {userName}] {actionDescription}";

            // Compose the path for the daily log file
            string logFilePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                $"application{DateTime.Now:yyyyMMdd}.log"
            );

            // Use a lock to avoid file corruption if multiple threads call this method simultaneously
            lock (lockObj)
            {
                try
                {
                    using (var writer = new StreamWriter(logFilePath, true))
                    {
                        writer.WriteLine(logLine);
                    }
                }
                catch (Exception ex)
                {
                    // Show a warning if writing fails. This is optional; you might prefer silent handling.
                    MessageBox.Show($"Failed to write to log file: {ex.Message}",
                                    "Logging Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                }
            }
        }

        /// <summary>
        /// Convenience method for backward compatibility, if other code calls Logger.Log(...).
        /// Calls LogAction internally with default severity "Info".
        /// </summary>
        /// <param name="message">A short description of the action/event</param>
        public static void Log(string message)
        {
            LogAction(message, "Info");
        }

        /// <summary>
        /// Convenience method for logging errors or critical messages.
        /// Internally calls LogAction with severity "Error".
        /// </summary>
        /// <param name="errorMessage">A short description of the error</param>
        public static void LogError(string errorMessage)
        {
            LogAction(errorMessage, "Error");
        }
    }
}
