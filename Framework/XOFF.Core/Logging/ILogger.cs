using System;
using System.Diagnostics;
using System.IO;

namespace XOFF.Core.Logging
{
    public static class XOFFLoggerSingleton
    {
        private static IXOFFLogger _instance;

        public static IXOFFLogger Instance
        {
            get {
                return _instance ?? 
                    new XOFFDebugLogger(); }
            set { _instance = value; }
        }
    }


    public enum XOFFErrorSeverity : int
	{
		Warning = 0,
		Failure = 1
	}

    public interface IXOFFLogger
    {
        void Identify(string username);

        void LogMessage(string section, string message);

        void LogException(string section, AggregateException aggEx, XOFFErrorSeverity severity);
        void LogException(Exception ex, XOFFErrorSeverity severity);
        void LogException(string message, Exception ex, XOFFErrorSeverity severity);
        FileStream GetLogFileStream();
        string GetLog();
    }

    public class XOFFDebugLogger : IXOFFLogger
    {
        public void Identify(string username)
        {
            
        }

        public void LogMessage(string section, string message)
        {
            
        }

        public void LogException(string section, AggregateException aggEx, XOFFErrorSeverity severity)
        {
            
        }

        public void LogException(Exception ex, XOFFErrorSeverity severity)
        {
            
        }

        public void LogException(string message, Exception ex, XOFFErrorSeverity severity)
        {
            
        }

        public FileStream GetLogFileStream()
        {
            return null;
        }

        public string GetLog()
        {
            return "";
        }
    }

    public class XOFFFileLogger : IXOFFLogger
    {

        private static object locker = new object();

        private string LogFilename
        {
            get { return string.Format("Log_{0:MM}-{0:dd}-{0:yyyy}.txt", DateTime.Now); }
        }

        private string LogDirectory
        {
            get
            {
                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return Path.Combine(documents, "logs");
            }
        }

        private string CurrentLogFilePath
        {
            get { return Path.Combine(LogDirectory, LogFilename); }
        }

        public XOFFFileLogger()
        {
            Init();
            Clean();

            // Indicate we are starting a new session for logging
            Write(string.Format("---------- New Session Beginning / {0} -----------", DateTime.Now));
        }

        #region ILogger implementation

        public void Identify(string username)
        {
            try
            {
                Write(string.Format("User: {0}", username));
            }
            catch (Exception ex)
            {
                //logger should not throw any exceptions that could crash the app
            }
        }

        public void LogMessage(string section, string message)
        {
            try
            {
                Write(string.Format("{2} - {0}: {1}", DateTime.Now.ToString(), message, section));
            }
            catch (Exception ex)
            {
                //logger should not throw any exceptions that could crash the app
            }

        }

        public void LogException(string section, AggregateException aggEx, XOFFErrorSeverity severity)
        {
            try
            {
                Write(string.Format("({1}) Aggregate: {0}", aggEx.Message, section));
                foreach (var ex in aggEx.InnerExceptions)
                {
                    LogException(ex, severity);
                }
                Write("Aggregate Complete");
            }
            catch (Exception ex)
            {
                //logger should not throw any exceptions that could crash the app
            }


        }

        public void LogException(Exception ex, XOFFErrorSeverity severity)
        {
            try
            {
                Write("Printing out Exception");
                int level = 0;

                do
                {
                    Write(string.Format("Level {0}: {1} ({2})", level, ex.Message, severity.ToString()));
                    Write(ex.StackTrace);

                    ex = ex.InnerException;
                    level += 1;
                } while (ex != null);

                Write("Done Printing out Exception");
            }
            catch (Exception e)
            {
                //logger should not throw any exceptions that could crash the app
            }
        }

        public void LogException(string message, Exception ex, XOFFErrorSeverity severity)
        {
            try
            {
                Write(message);
                LogException(ex, severity);
            }
            catch (Exception e)
            {
                //logger should not throw any exceptions that could crash the app
            }

        }


        public string GetLog()
        {
            try
            {
                return File.ReadAllText(CurrentLogFilePath);
            }
            catch (Exception e)
            {
                //logger should not throw any exceptions that could crash the app
            }
            return "";

        }

        public FileStream GetLogFileStream()
        {
            return File.OpenRead(CurrentLogFilePath);
        }

        #endregion

        void Init()
        {
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);
        }

        // ensure that only the current filename is kept in the directory (reduce bloat problems)
        void Clean()
        {
            var logs = Directory.GetFiles(LogDirectory);
            foreach (var log in logs)
            {
                var fi = new FileInfo(log);
                if (!LogFilename.EndsWith(fi.Name.ToLower()))
                    File.Delete(log);
            }
        }

        void Write(string contents)
        {
            lock (locker)
            {
                // file is created here via the Append mode
                using (var file = File.Open(CurrentLogFilePath, FileMode.Append, FileAccess.Write))
                {
                    using (var writer = new StreamWriter(file))
                    {
                        Debug.WriteLine(contents);
                        Debug.Write("");
                        writer.WriteLine(contents);
                        writer.WriteLine();
                    }
                }
            }
        }
    }
}