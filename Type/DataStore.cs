using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Type
{
    public class DataStore
    {
        private string path;
        private int nextIndex;

        // Constructor
        public DataStore(string fileName)
        {
            // initialize Data Store with fileName
            this.path = fileName;

            // if file exists. read file and load contents
            nextIndex = 1;
            if (File.Exists(path))
            {
                Dictionary<int, List<string>> allRows = this.Get();
                foreach (int index in allRows.Keys)
                {
                    // update index
                    nextIndex = index + 1;
                }
            }
            else
            {
                // Create file since it does not exist.
                FileStream fs = File.Create(path);
                fs.Close();
            }

        }

        // Insert New Row to file
        public int InsertRow(List<string> row)
        {
            // keep value of current index
            var currentIndex = nextIndex;

            // append to row file
            string str = ProcessListToString(currentIndex, row);
            AppendStringToFile(str);

            // update the next index
            nextIndex++;

            // return current index
            return currentIndex;
        }

        // Change the value of a row
        public void ChangeRow(int index, List<string> row)
        {
            // read file contents
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            List<string> list = new List<string>();

            while (!sr.EndOfStream)
            {
                string rawString = sr.ReadLine();
                Tuple<int, List<string>> processedRow = ProcessRow(rawString);

                if (processedRow.Item1 != index)
                {
                    list.Add(rawString);
                }
                else
                {
                    list.Add(ProcessListToString(index, row));
                }
            }

            // close file/writer
            sr.Close();
            fs.Close();

            // replace the contents of the file with new contents.
            WriteToFile(list);
        }

        // returns row given index
        public List<string> Get(int index)
        {
            // read file contents
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            // returns null if index cannot be found.
            Tuple<int, List<string>> row = null;
            while (!sr.EndOfStream)
            {
                row = ProcessRow(sr.ReadLine());
                if (row.Item1 == index)
                {
                    break;
                }
            }

            // close file/reader
            sr.Close();
            fs.Close();

            // return row contents
            return row.Item2;
        }

        // Get all rows in the data store
        public Dictionary<int, List<string>> Get()
        {
            // read file contents
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            // hashtable to store rows.
            Dictionary<int, List<string>> hashTable = new Dictionary<int, List<string>>();

            while (!sr.EndOfStream)
            {
                Tuple<int, List<string>> processedString = ProcessRow(sr.ReadLine());
                hashTable.Add(processedString.Item1, processedString.Item2);
            }

            // close file/reader
            sr.Close();
            fs.Close();

            // return all rows
            return hashTable;
        }

        // Converts List<string> into a string of comma separated values
        // appends index in front.
        private string ProcessListToString(int index, List<string> list)
        {
            string result = index.ToString();
            foreach (string str in list)
            {
                result += "," + str;
            }
            return result;
        }

        // Append String as a new line at the end of the file
        private void AppendStringToFile(string str)
        {
            // open file
            FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

            // append to file
            sw.WriteLine(str);

            // close file/writer
            sw.Close();
            fs.Close();
        }

        // Clears File and replace contents with rows
        private void WriteToFile(List<string> rows)
        {
            // open and truncate file.
            FileStream fs = new FileStream(path, FileMode.Truncate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

            // write every one into the file.
            foreach (string str in rows)
            {
                sw.WriteLine(str);
            }

            // close file/write
            sw.Close();
            fs.Close();
        }

        // returns a Tuple of the row's index and it's contents
        private Tuple<int, List<string>> ProcessRow(string rawString)
        {
            // split comma separated line into tokens
            string[] tokens = rawString.Split(',');

            // coerce first value to index.
            int index = int.Parse(tokens[0]);

            // build row contents
            List<string> contents = new List<string>();
            for (int i = 1; i < tokens.Length; i++)
            {
                contents.Add(tokens[i]);
            }

            return Tuple.Create(index, contents);
        }
    }
}
