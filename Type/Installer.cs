using System;
using System.IO;
using Microsoft.Win32;

namespace Type
{
    //@author A0092104U
#if !DEBUG
    class Installer
    {
        #region Constants
        private const int NAME_LEN = 8;
        private const char START_CHAR = 'a';
        private const char QUOT = '\"';
        private const string KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string KEY_VALUE = "type-bin";
        private const Environment.SpecialFolder EMBED_LOC = Environment.SpecialFolder.LocalApplicationData;
        private const string EMBED_SUBFOLDER = @"\Type-App";
        private const string EXE_NAME = @"\type.exe";
        #endregion

        #region Static Methods
        /// <summary>
        /// Checks if the program has already been installed.
        /// </summary>
        /// <returns>True if installed and autorun set, False otherwise.</returns>
        public static bool IsInstalled()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(KEY, true);
            string path;
            if ((path = (string)key.GetValue(KEY_VALUE)) == null)
            {
                return false;
            }
            else
            {
                path = RemoveQuotes(path);
                if (File.Exists(path))
                {
                    return true;
                }
                else
                {
                    key.DeleteValue(KEY_VALUE, false);
                    return false;
                }
            }
        }

        /// <summary>
        /// Installs the program if it has not already been installed.
        /// </summary>
        public static void EmbedOnFirstRun()
        {
            if (!IsInstalled())
            {
                Embed();
            }
        }
        #endregion

        #region Helper Methods
        private static string RemoveQuotes(string path)
        {
            return (path.Substring(1, path.Length - 2));
        }

        private static void Embed()
        {
            string me = System.Reflection.Assembly.GetExecutingAssembly().Location;
            FileInfo applicationInfo = new FileInfo(me);

            SetRegistryAutoStart(me);
        }

        private static void SetRegistryAutoStart(string target)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(KEY, true);
            key.SetValue(KEY_VALUE, Quote(target));
        }

        private static string Quote(string text)
        {
            return (QUOT + text + QUOT);
        }
        #endregion
    }
#endif
}
