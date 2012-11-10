using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Type
{
    //@author A0092104U
    sealed class Logger
    {
        #region Constants
        private string TEXT_EXCEPTION = "Exception: ";
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
        }

        ~Logger()
        {
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        #endregion

        #region Logging Methods
        public void Log(string text)
        {
            sw.WriteLine(TimeStamp() + text);
            sw.Flush();
        }

        public void LogException(string text)
        {
            sw.WriteLine(TimeStamp() + TEXT_EXCEPTION + text);
            sw.Flush();
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
                File.Create(path);
            }
        }
        #endregion
    }
}
