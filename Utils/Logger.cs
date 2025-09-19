using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TerrariaTweaker.Utils
{
    /// <summary>
    /// Log levels for categorizing log messages
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }

    /// <summary>
    /// Simple but effective logging system for debugging and error tracking
    /// Provides both console and file logging capabilities
    /// </summary>
    public static class Logger
    {
        private static readonly object lockObject = new object();
        private static LogLevel minimumLogLevel = LogLevel.Info;
        private static bool enableConsoleLogging = true;
        private static bool enableFileLogging = true;
        private static string logFilePath = string.Empty;
        private static readonly Queue<string> logQueue = new Queue<string>();
        private static readonly int maxLogQueueSize = 1000;

        /// <summary>
        /// Gets or sets the minimum log level that will be processed
        /// Messages below this level will be ignored
        /// </summary>
        public static LogLevel MinimumLogLevel
        {
            get => minimumLogLevel;
            set => minimumLogLevel = value;
        }

        /// <summary>
        /// Gets or sets whether to output log messages to the console
        /// </summary>
        public static bool EnableConsoleLogging
        {
            get => enableConsoleLogging;
            set => enableConsoleLogging = value;
        }

        /// <summary>
        /// Gets or sets whether to write log messages to a file
        /// </summary>
        public static bool EnableFileLogging
        {
            get => enableFileLogging;
            set => enableFileLogging = value;
        }

        /// <summary>
        /// Gets the current log file path
        /// </summary>
        public static string LogFilePath => logFilePath;

        static Logger()
        {
            // Initialize default log file path
            try
            {
                string logDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "TerrariaTweaker",
                    "Logs");

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd");
                logFilePath = Path.Combine(logDirectory, $"TerrariaTweaker_{timestamp}.log");
            }
            catch
            {
                // If we can't create the log directory, disable file logging
                enableFileLogging = false;
                Console.WriteLine("Warning: Could not create log directory. File logging disabled.");
            }
        }

        /// <summary>
        /// Sets a custom path for the log file
        /// </summary>
        /// <param name="path">Full path to the log file</param>
        public static void SetLogFilePath(string path)
        {
            lock (lockObject)
            {
                try
                {
                    // Ensure the directory exists
                    string? directory = Path.GetDirectoryName(path);
                    if (directory != null && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    logFilePath = path;
                    enableFileLogging = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to set log file path: {ex.Message}");
                    enableFileLogging = false;
                }
            }
        }

        /// <summary>
        /// Logs a debug message (lowest priority)
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="category">Optional category for organizing logs</param>
        public static void Debug(string message, string category = "")
        {
            Log(LogLevel.Debug, message, category);
        }

        /// <summary>
        /// Logs an informational message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="category">Optional category for organizing logs</param>
        public static void Info(string message, string category = "")
        {
            Log(LogLevel.Info, message, category);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="category">Optional category for organizing logs</param>
        public static void Warning(string message, string category = "")
        {
            Log(LogLevel.Warning, message, category);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="category">Optional category for organizing logs</param>
        public static void Error(string message, string category = "")
        {
            Log(LogLevel.Error, message, category);
        }

        /// <summary>
        /// Logs a critical error message (highest priority)
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="category">Optional category for organizing logs</param>
        public static void Critical(string message, string category = "")
        {
            Log(LogLevel.Critical, message, category);
        }

        /// <summary>
        /// Logs an exception with its stack trace
        /// </summary>
        /// <param name="exception">Exception to log</param>
        /// <param name="message">Additional message to include</param>
        /// <param name="category">Optional category for organizing logs</param>
        public static void Exception(Exception exception, string message = "", string category = "")
        {
            string fullMessage = string.IsNullOrEmpty(message) 
                ? exception.ToString() 
                : $"{message}: {exception}";
            
            Log(LogLevel.Error, fullMessage, category);
        }

        /// <summary>
        /// Main logging method that handles the actual output
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="message">Message to log</param>
        /// <param name="category">Optional category</param>
        private static void Log(LogLevel level, string message, string category)
        {
            // Check if this log level should be processed
            if (level < minimumLogLevel)
                return;

            lock (lockObject)
            {
                try
                {
                    // Format the log message
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string levelStr = level.ToString().ToUpper().PadRight(8);
                    string categoryStr = string.IsNullOrEmpty(category) ? "" : $"[{category}] ";
                    string threadId = Thread.CurrentThread.ManagedThreadId.ToString().PadLeft(3);
                    
                    string formattedMessage = $"{timestamp} {levelStr} T{threadId} {categoryStr}{message}";

                    // Output to console if enabled
                    if (enableConsoleLogging)
                    {
                        ConsoleColor originalColor = Console.ForegroundColor;
                        
                        // Set color based on log level
                        Console.ForegroundColor = level switch
                        {
                            LogLevel.Debug => ConsoleColor.Gray,
                            LogLevel.Info => ConsoleColor.White,
                            LogLevel.Warning => ConsoleColor.Yellow,
                            LogLevel.Error => ConsoleColor.Red,
                            LogLevel.Critical => ConsoleColor.Magenta,
                            _ => ConsoleColor.White
                        };

                        Console.WriteLine(formattedMessage);
                        Console.ForegroundColor = originalColor;
                    }

                    // Add to log queue for file writing
                    if (enableFileLogging && !string.IsNullOrEmpty(logFilePath))
                    {
                        logQueue.Enqueue(formattedMessage);
                        
                        // Limit queue size to prevent memory issues
                        while (logQueue.Count > maxLogQueueSize)
                        {
                            logQueue.Dequeue();
                        }
                        
                        // Flush to file periodically or on errors/critical messages
                        if (level >= LogLevel.Error || logQueue.Count >= 10)
                        {
                            FlushToFile();
                        }
                    }
                }
                catch
                {
                    // If logging itself fails, we can't do much without creating infinite loops
                    // Just ignore the error and continue
                }
            }
        }

        /// <summary>
        /// Flushes all queued log messages to the file
        /// </summary>
        public static void FlushToFile()
        {
            if (!enableFileLogging || string.IsNullOrEmpty(logFilePath) || logQueue.Count == 0)
                return;

            lock (lockObject)
            {
                try
                {
                    using var writer = new StreamWriter(logFilePath, append: true);
                    while (logQueue.Count > 0)
                    {
                        writer.WriteLine(logQueue.Dequeue());
                    }
                }
                catch
                {
                    // If file writing fails, disable file logging to prevent repeated failures
                    enableFileLogging = false;
                }
            }
        }

        /// <summary>
        /// Gets recent log messages from memory (last N messages)
        /// Useful for displaying in UI or crash reports
        /// </summary>
        /// <param name="count">Number of recent messages to return</param>
        /// <returns>List of recent log messages</returns>
        public static List<string> GetRecentLogs(int count = 50)
        {
            lock (lockObject)
            {
                var messages = new List<string>();
                var queueArray = logQueue.ToArray();
                
                int startIndex = Math.Max(0, queueArray.Length - count);
                for (int i = startIndex; i < queueArray.Length; i++)
                {
                    messages.Add(queueArray[i]);
                }
                
                return messages;
            }
        }

        /// <summary>
        /// Clears all queued log messages
        /// </summary>
        public static void ClearLogs()
        {
            lock (lockObject)
            {
                logQueue.Clear();
            }
        }

        /// <summary>
        /// Creates a new log file for the current session
        /// Useful for separating logs by application runs
        /// </summary>
        public static void StartNewLogSession()
        {
            lock (lockObject)
            {
                FlushToFile();
                
                try
                {
                    string logDirectory = Path.GetDirectoryName(logFilePath) ?? "";
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    logFilePath = Path.Combine(logDirectory, $"TerrariaTweaker_{timestamp}.log");
                    
                    Info("=== New log session started ===", "Logger");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to start new log session: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Performs cleanup when the application is shutting down
        /// </summary>
        public static void Shutdown()
        {
            lock (lockObject)
            {
                Info("=== Application shutting down ===", "Logger");
                FlushToFile();
            }
        }

        /// <summary>
        /// Helper method for logging memory operations
        /// </summary>
        /// <param name="operation">Description of the memory operation</param>
        /// <param name="address">Memory address involved</param>
        /// <param name="success">Whether the operation succeeded</param>
        /// <param name="additionalInfo">Any additional information</param>
        public static void LogMemoryOperation(string operation, IntPtr address, bool success, string additionalInfo = "")
        {
            string status = success ? "SUCCESS" : "FAILED";
            string info = string.IsNullOrEmpty(additionalInfo) ? "" : $" - {additionalInfo}";
            
            LogLevel level = success ? LogLevel.Debug : LogLevel.Warning;
            Log(level, $"{operation} at 0x{address:X}: {status}{info}", "Memory");
        }

        /// <summary>
        /// Helper method for logging UI operations
        /// </summary>
        /// <param name="operation">Description of the UI operation</param>
        /// <param name="details">Additional details</param>
        public static void LogUIOperation(string operation, string details = "")
        {
            string fullMessage = string.IsNullOrEmpty(details) ? operation : $"{operation}: {details}";
            Debug(fullMessage, "UI");
        }

        /// <summary>
        /// Helper method for logging process operations
        /// </summary>
        /// <param name="operation">Description of the process operation</param>
        /// <param name="processName">Name of the process involved</param>
        /// <param name="success">Whether the operation succeeded</param>
        public static void LogProcessOperation(string operation, string processName, bool success)
        {
            string status = success ? "SUCCESS" : "FAILED";
            LogLevel level = success ? LogLevel.Info : LogLevel.Error;
            Log(level, $"{operation} for process '{processName}': {status}", "Process");
        }
    }
}