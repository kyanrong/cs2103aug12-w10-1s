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
        private int nextTaskID;

        public TypeFile(string path)
        {
            if (path != null)
            {
                this.path = OpenFile(path);
            }
            else
            {
                this.path = OpenFile("type");
            }
            FileStream fs = new FileStream(this.path, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            if (!sr.EndOfStream)
            {
                nextTaskID = int.Parse(sr.ReadLine());
            }
            else
            {
                nextTaskID = 0;
            }
        }
        private string OpenFile(string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
                fs.Close();
                return path;
            }
            catch { return null; }
        }
        protected int Add(string task)
        {
            int taskID = 0;
            return taskID;
        }
        protected Tuple<string, bool, bool> Get(int taskID)
        {
            return new Tuple<string, bool, bool>(null, false, false);
        }
        protected List<string> GetTodoAndDoneTask()
        {
            return new List<string>();
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
        protected bool Clear()
        {
            return false;
        }
    }
}
