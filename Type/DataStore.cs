using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Type
{
    public class DataStore
    {
        private const char SEPERATOR = ',';
        private const char ESCAPE = '/';

        private string path;
        private int nextIndex;

        //@author A0088574M
        // only Constructor
        public DataStore(string fileName)
        {
            // initialize Data Store with fileName
            this.path = fileName;

            // initialize nextIndex
            nextIndex = 1;

            // if file exists update nextIndex, else create file
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
                FileStream fs = File.Create(path);
                fs.Close();
            }
        }

        // append a new row to the file and return a unique ID that identify that row value
        public int InsertRow(List<string> row)
        {
            // keep value of current index to be return
            int currentIndex = nextIndex;

            string str = ProcessListToString(currentIndex, row);

            AppendStringToFile(str);

            nextIndex++;

            return currentIndex;
        }

        // The value of the row with the index will be changed to the new value
        // will not change the file data if the index out of bound
        // throw MissingFieldException if any index checked in the file before the target index is found missing
        public void ChangeRow(int index, List<string> row)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            List<string> list = new List<string>();

            while (!sr.EndOfStream)
            {
                string rawString = sr.ReadLine();

                Tuple<int, List<string>> processedRow;
                try
                {
                    processedRow = ProcessRow(rawString);
                }
                catch (MissingFieldException exception)
                {
                    sr.Close();
                    fs.Close();

                    throw exception;
                };

                if (processedRow.Item1 != index)
                {
                    list.Add(rawString);
                }
                else
                {
                    list.Add(ProcessListToString(index, row));
                }
            }

            sr.Close();
            fs.Close();

            // replace the contents of the file with new contents.
            WriteToFile(list);
        }

        // returns row given index
        // if index out of bound or not found will return null
        // throw MissingFieldException if any index checked in the file before the target index is found missing
        public List<string> Get(int index)
        {
            // read file contents
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            // returns null if index cannot be found.
            Tuple<int, List<string>> row;
            Tuple<int, List<string>> target = new Tuple<int, List<string>>(0, null);

            while (!sr.EndOfStream)
            {
                try
                {
                    row = ProcessRow(sr.ReadLine());
                }
                catch (MissingFieldException exception)
                {
                    sr.Close();
                    fs.Close();

                    throw exception;
                }

                if (row.Item1 == index)
                {
                    target = row;
                    break;
                }
            }

            sr.Close();
            fs.Close();

            return target.Item2;//Item2 is the value
        }

        // Get all rows in the data store
        // throw MissingFieldException if any index in the file is missing
        public Dictionary<int, List<string>> Get()
        {
            // read file contents
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            // hashtable to store rows.
            Dictionary<int, List<string>> hashTable = new Dictionary<int, List<string>>();

            while (!sr.EndOfStream)
            {
                Tuple<int, List<string>> processedString;
                try
                {
                    processedString = ProcessRow(sr.ReadLine());
                }
                catch (MissingFieldException exception)
                {
                    sr.Close();
                    fs.Close();

                    throw exception;
                }

                hashTable.Add(processedString.Item1, processedString.Item2);
            }

            sr.Close();
            fs.Close();

            return hashTable;
        }

        // The value of the row with the index will be deleted
        // throw MissingFieldException if any index checked in the file before the target index is found missing
        public void DeleteRow(int index)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            List<string> list = new List<string>();

            while (!sr.EndOfStream)
            {
                string rawString = sr.ReadLine();

                Tuple<int, List<string>> processedRow;
                try
                {
                    processedRow = ProcessRow(rawString);
                }
                catch (MissingFieldException exception)
                {
                    sr.Close();
                    fs.Close();

                    throw exception;
                };

                if (processedRow.Item1 != index)
                {
                    list.Add(rawString);
                }
            }

            sr.Close();
            fs.Close();

            // replace the contents of the file with new contents.
            WriteToFile(list);
        }

        public void ClearFile(String fileName)
        {
            File.Delete(fileName);
        }

        //@author A0088574M
        // Converts List<string> into a string of comma separated values
        // appends index in front.
        private string ProcessListToString(int index, List<string> list)
        {
            string result = index.ToString();

            foreach (string str in list)
            {
                string tempString = "";

                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] == ESCAPE || str[i] == SEPERATOR)
                    {
                        tempString += ESCAPE;
                    }
                    tempString += str[i];
                }
                result += SEPERATOR + tempString;
            }

            return result;
        }

        // Append String as a new line at the end of the file
        private void AppendStringToFile(string str)
        {
            FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

            sw.WriteLine(str);

            sw.Close();
            fs.Close();
        }

        // Clear the file and replace the contents with a list of string
        private void WriteToFile(List<string> rows)
        {
            // open and truncate file.
            FileStream fs = new FileStream(path, FileMode.Truncate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

            foreach (string str in rows)
            {
                sw.WriteLine(str);
            }

            sw.Close();
            fs.Close();
        }

        // returns a Tuple of the row's index and it's contents
        // throw MissingFieldException if the index of that row is missing
        private Tuple<int, List<string>> ProcessRow(string rawString)
        {
            // split comma separated line into tokens
            List<string> contents = new List<string>();
            contents.Add("");
            int count = 0;

            for (int i = 0; i < rawString.Length; i++)
            {
                if (rawString[i] == SEPERATOR)
                {
                    contents.Add("");
                    count++;
                }
                else if (rawString[i] == ESCAPE)
                {
                    i++;
                    contents[count] += rawString[i];
                }
                else
                {
                    contents[count] += rawString[i];
                }
            }

            // coerce first value to index.
            int index;
            if (!int.TryParse(contents[0], out index))
            {
                throw new System.MissingFieldException("missing index");
            }

            // build row contents
            contents.RemoveAt(0);

            return Tuple.Create(index, contents);
        }
    }
}
