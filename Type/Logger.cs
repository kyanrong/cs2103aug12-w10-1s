using System;
using System.IO;

namespace Type
{
    //@author A0092104U
    sealed class Logger
    {
        #region Constants
        private const string TEXT_EXCEPTION = "Exception: ";
        #endregion

        #region Fields
        private string path;

        private FileStream fs;
        private StreamWriter sw;
        #endregion

        #region Constructors
        public Logger(string path)
        {
            this.path = path;

            Touch(this.path);
            fs = new FileStream(this.path, FileMode.Append);
            sw = new StreamWriter(fs);
            sw.AutoFlush = true;
        }

        ~Logger()
        {
            fs.Close();
        }
        #endregion

        #region Logging Methods
        public void Log(string text)
        {
            sw.WriteLine(TimeStamp() + text);
        }

        public void LogException(string text)
        {
            sw.WriteLine(TimeStamp() + TEXT_EXCEPTION + text);
        }
        #endregion

        #region Static Helper Methods
        private string TimeStamp()
        {
            return ("[" + DateTime.Now.ToString() + "] ");
        }

        private static void Touch(string path)
        {
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
        }
        #endregion
    }
}
