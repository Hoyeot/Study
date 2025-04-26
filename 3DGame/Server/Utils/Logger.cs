using Serilog.Core;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace GameServerCS.Utils
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logDir = "LOG";
        private static string _logFileName = "log.txt";
        private static string _logFilePath => Path.Combine(_logDir, _logFileName);
        private static bool _initialized = false;
        private const int MAX_BACKUP_FILES = 10;
        private const long MAX_LOG_SIZE = 100 * 1024 * 1024;

        public static void Initialize(string logDir = null, string logFilePath = null)
        {
            if (!_initialized)
            {
                if (!string.IsNullOrEmpty(logDir))
                {
                    _logDir = logDir;
                }

                if (!string.IsNullOrEmpty(logFilePath))
                {
                    _logFileName = logFilePath;
                }

                if (!Directory.Exists(_logDir))
                {
                    Directory.CreateDirectory(_logDir);
                    LogInternal($"LOG 폴더 생성: {Path.GetFullPath(_logDir)}", "INIT");
                }

                if (File.Exists(_logFilePath))
                {
                    RotateLogFiles();
                }

                File.WriteAllText(_logFilePath, $"[{DateTime.Now}]");
                _initialized = true;
            }
        }

        private static void RotateLogFiles()
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupPath = Path.Combine(_logDir, $"log_{timestamp}.log");

                var backupFiles = Directory.GetFiles(_logDir, "log_*.log")
                                         .OrderByDescending(f => f)
                                         .ToList();

                while (backupFiles.Count >= MAX_BACKUP_FILES)
                {
                    File.Delete(backupFiles.Last());
                    backupFiles.RemoveAt(backupFiles.Count - 1);
                }

                File.Copy(_logFilePath, backupPath);
                LogInternal($"로그 파일 백업 완료: {backupPath}", "INIT");

                File.WriteAllText(_logFilePath, string.Empty);
            }
            catch (Exception ex)
            {
                LogInternal($"로그 파일 회전 실패: {ex.Message}", "ERROR");
                throw;
            }
        }


        public static void Log(string message, string category = "INFO")
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(_logFilePath) && new FileInfo(_logFilePath).Length > MAX_LOG_SIZE)
                    {
                        RotateLogFiles();
                    }

                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{category}] [{Thread.CurrentThread.ManagedThreadId}] {message}\n";
                    File.AppendAllText(_logFilePath, logEntry);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"로그 기록 실페 : {ex.Message}");
                }
            }
        }

        private static void LogInternal(string message, string category)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{category}] {message}\n";
        }

        public static void LogError(string message, Exception ex = null)
        {
            string errMsg = $"{message} {(ex != null ? $"- 예외 : {ex.Message}\n스택 트레이스 : {ex.StackTrace}" : "")}";
            Log(errMsg, "Error");
            if (ex != null)
            {
                Log($"스택 트레이스: {ex.StackTrace}", "ERROR");
            }
        }

        public static void LogMessage(IPEndPoint endpoint, string direction, string message)
        {
            Log($"[{endpoint?.Address}:{endpoint?.Port}] {direction}: {message.Replace("\n", "\\n")}", "MSG");
        }
    }
}