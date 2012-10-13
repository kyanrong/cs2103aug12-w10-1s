using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Type
{
    class TypeFile
    {
        private string path;

        public TypeFile(string fileName)
        {
            path = tryOpenFile(fileName);
        
            FileStream fs = new FileStream(this.path, FileMode.Open, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            
            sw.WriteLine("0");
            
            fs.Close();
            sw.Close();
        }
        private string tryOpenFile(string fileName)
        {
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
                fs.Close();
                return fileName;
            }
            catch { return null; }
        }
        protected int Add(List<string> task)
        {
            int nextTaskID = getCurrentTaskID() + 1;
            string str = processListToString(nextTaskID, task);
            AppendStringToFile(str);
            return nextTaskID;
        }
        private int getCurrentTaskID()
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            int currentTaskID = int.Parse(sr.ReadLine());

            fs.Close();
            sr.Close();

            return currentTaskID;
        }
        private void AppendStringToFile(string str)
        {
            FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(str);
            fs.Close();
            sw.Close();
        }
        private void WriteToFile(int latestTaskID, List<string> list)
        {
            FileStream fs = new FileStream(path, FileMode.Truncate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(latestTaskID);
            foreach (string str in list)
            {
                sw.WriteLine(str);
            }
            fs.Close();
            sw.Close();
        }
        protected string Get(int taskID)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            Tuple<int, string> processedString = new Tuple<int,string>(0, null);

            sr.ReadLine();//to skip the taskID line
            while(!sr.EndOfStream)
            {
                processedString = processRawString(sr.ReadLine());
                if (processedString.Item1 == taskID)
                {
                    break;
                }
            }
            fs.Close();
            sr.Close();
            return processedString.Item2;
        }
        protected Dictionary<int,string> Get()
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            Dictionary<int, string> hashTable = new Dictionary<int, string>();
           
            sr.ReadLine();
            while (!sr.EndOfStream)
            {
                Tuple<int,string> processedString =  processRawString(sr.ReadLine());
                hashTable.Add(processedString.Item1, processedString.Item2);
            }
            fs.Close();
            sr.Close();
            return hashTable;
        }
        private Tuple<int, string> processRawString(string rawString)
        {
            const int LOCATIONOFID = 0, LOCATIONOFFISTTASKDESCRIPTION = 1;
            string[] temp = rawString.Split(' ');
            string taskDescription = temp[LOCATIONOFFISTTASKDESCRIPTION];
            for (int i = 2; i < temp.Length; i++)
            {
                taskDescription += ' ' + temp[i];
            }
            return new Tuple<int, string>(int.Parse(temp[LOCATIONOFID]), taskDescription);
        }
        protected void replaceTargetIDTask(int taskID, List<string> description)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            List<string> list = new List<string>();
            Tuple<int, string> processedString;

            int currentTaskID = int.Parse(sr.ReadLine());
            while (!sr.EndOfStream)
            {
                string rawString = sr.ReadLine();
                processedString = processRawString(rawString);
                if (processedString.Item1 != taskID)
                {
                    list.Add(rawString);
                }
                else
                {
                    list.Add(processListToString(taskID, description));
                }
            }
            WriteToFile(currentTaskID, list);
            fs.Close();
            sr.Close();
        }
        private string processListToString(int taskID, List<string> list){
            string result = taskID.ToString();

            foreach (string str in list)
            {
                result += ", " + str;
            }
            return result;
        }

        protected List<string> GetArchiveTask()
        {
            return new List<string>();
        }
        protected bool SetArchive(int taskID, bool flag)
        {
            return false;
        }
        protected bool SetDone(int taskID, bool flag)
        {
            return false;
        }
        protected bool Remove(int taskID)
        {
            return false;
        }
        protected void Clear()
        {
            FileStream fs = new FileStream(path, FileMode.Truncate);
            fs.Close();
        }
    }
}
