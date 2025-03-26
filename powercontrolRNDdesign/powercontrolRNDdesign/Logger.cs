using System;
using System.IO;
using System.Windows.Forms;

namespace powercontrolRNDdesign
{
    /// <summary>
    /// A simple static logger class that writes daily log files.
    /// By default, we create/use a "logs" folder in the same directory
    /// as the application's .exe file. Each day gets its own log file.
    /// </summary>
    public static class Logger
    {
        // A lock object to ensure thread-safe writing to our log files
        private static readonly object lockObj = new object();

        /// <summary>
        /// Logs an action or event, along with a severity level.
        /// This is our main logging entry point. 
        /// Example severities: "Info", "Warning", "Error".
        /// 
        /// Another user reading this should note:
        /// 1) We build the path to a "logs" folder under the current .exe path.
        /// 2) If the folder does not exist, we create it.
        /// 3) We store log lines in a daily file: "applicationYYYYMMDD.log".
        /// </summary>
        /// <param name="actionDescription">Descriptive text for the action/event.</param>
        /// <param name="severity">A short severity label, e.g. "Info".</param>
        public static void LogAction(string actionDescription, string severity = "Info")
        {
            // Example log line format:
            // "2025-03-25 10:20:45 [Info] [User: JohnDoe] Some action description"
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string userName = Environment.UserName; // The logged-in Windows user
            string logLine = $"{timeStamp} [{severity}] [User: {userName}] {actionDescription}";

            // 1) Build path to a "logs" subfolder in the exe directory
            string logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

            // 2) Ensure the logs folder exists. If not, create it.
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }

            // 3) Build the daily log file path. 
            //    For instance: "logs/application20250325.log"
            string logFileName = $"application{DateTime.Now:yyyyMMdd}.log";
            string logFilePath = Path.Combine(logsDirectory, logFileName);

            // Use a lock to ensure multiple threads don't corrupt the file
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
                    // Show a warning if writing fails.
                    MessageBox.Show($"Failed to write to log file: {ex.Message}",
                                    "Logging Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                }
            }
        }

        /// <summary>
        /// A convenience method for code that just wants to log a message 
        /// at default severity "Info".
        /// Another user can call Logger.Log("Some message") 
        /// instead of Logger.LogAction(...).
        /// </summary>
        /// <param name="message">Description of the event/action.</param>
        public static void Log(string message)
        {
            LogAction(message, "Info");
        }

        /// <summary>
        /// A convenience method for logging an error message 
        /// at severity "Error".
        /// </summary>
        /// <param name="errorMessage">Short description of the error.</param>
        public static void LogError(string errorMessage)
        {
            LogAction(errorMessage, "Error");
        }
    }
}
