using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartManEncryptor
{
    internal class SmartManEncryptorLog
    {
        private readonly string logDirectory;
        public SmartManEncryptorLog(string logDirectory)
        {
            string logFileName = $"encryptor_log_{DateTime.Now:yyyyMMdd}.txt";
            // 空路徑，直接產出在同資料夾
            if (string.IsNullOrEmpty(logDirectory))
            {
                this.logDirectory = logFileName;
            }
            else
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string directoryPath = Path.Combine(baseDirectory, logDirectory);
                if (!Path.IsPathFullyQualified(logDirectory) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                this.logDirectory = Path.Combine(directoryPath, logFileName);
            }
        }
        public void LogText(string message)
        {
            string formattedMessage = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} --- {message}";
            Console.WriteLine(formattedMessage);
            using var logFile = new StreamWriter(logDirectory, true);
            logFile.WriteLine(formattedMessage);
        }
        public void LogError(string message, Exception? ex = null)
        {
            string formattedMessage = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} --- [Error] {message}";
            if (ex != null)
            {
                formattedMessage += $" {ex.Message}, [StackTrace] {ex.StackTrace}";
            }
            Console.WriteLine(formattedMessage);
            using var logFile = new StreamWriter(logDirectory, true);
            logFile.WriteLine(formattedMessage);
        }
    }
}
