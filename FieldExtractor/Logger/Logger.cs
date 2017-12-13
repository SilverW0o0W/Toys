using System;
using System.IO;

namespace LoggerManager
{
    public class Logger
    {
        public enum LogLevel
        {
            DEBUG,
            INFO,
            WARNING,
            ERROR
        }

        public string LogPath { get; private set; }
        public string LogFileName { get; private set; }
        public string LogFullName { get; private set; }

        public Logger()
        {
            LogFileName = "log.log";
            LogPath = GetDirectoryPath(System.Reflection.Assembly.GetExecutingAssembly().Location);
            GenerateFullName();
        }

        public Logger(string logFileName)
        {
            LogFileName = logFileName;
            LogPath = GetDirectoryPath(System.Reflection.Assembly.GetExecutingAssembly().Location);
            GenerateFullName();
        }

        public Logger(string logFileName, string logPath)
        {
            LogFileName = logFileName;
            if (CheckPath(logPath))
            {
                LogPath = logPath;
            }
            else
            {
                LogPath = GetDirectoryPath(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            GenerateFullName();
        }

        public bool SetLogPath(string path)
        {
            bool result = false;
            if (!CheckPath(path))
            {
                return false;
            }
            if (string.IsNullOrEmpty(LogPath))
            {
                LogPath = path;
                GenerateFullName();
                result = true;
            }
            else
            {
                lock (LogPath)
                {
                    ResetLogPath(path);
                }
                GenerateFullName();
                result = true;
            }
            return result;
        }

        private bool CheckPath(string path)
        {
            bool result = false;
            try
            {
                result = string.IsNullOrEmpty(path);
                if (!result)
                {
                    DirectoryInfo info = new DirectoryInfo(path);
                    if (info.Exists)
                    {
                        result = true;
                    }
                    else
                    {
                        info.Create();
                        result = true;
                    }
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        private void ResetLogPath(string path)
        {
            this.Log(LogLevel.WARNING, "The log path is changed.New path: {0}.", path);
            LogPath = path;
        }

        private string GetDirectoryPath(string path)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            return info.Parent.FullName;
        }

        private void GenerateFullName()
        {
            this.LogFullName = LogPath + Path.DirectorySeparatorChar + LogFileName;
        }

        public void Log(LogLevel level, string format, params object[] args)
        {
            string message = string.Format("{0} {1} :{2}", level.ToString(), DateTime.Now.ToString("MM-dd HH:mm:ss,fff"), string.Format(format, args));
            using (StreamWriter writer = new StreamWriter(LogFullName, true))
            {
                writer.WriteLine(message);
            }
        }

        public void Debug(string format, params object[] args)
        {
            this.Log(LogLevel.DEBUG, format, args);
        }

        public void Info(string format, params object[] args)
        {
            this.Log(LogLevel.INFO, format, args);
        }

        public void Warning(string format, params object[] args)
        {
            this.Log(LogLevel.WARNING, format, args);
        }

        public void Error(string format, params object[] args)
        {
            this.Log(LogLevel.ERROR, format, args);
        }
    }
}
