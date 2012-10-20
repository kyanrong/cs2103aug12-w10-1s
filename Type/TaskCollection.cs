using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Type
{
    public class TaskCollection
    {
        private List<Task> tasks;
        private DataStore dataStore;

        // Constructor
        public TaskCollection()
        {
            // instantiate task models
            tasks = new List<Task>();
            // create data store
            dataStore = new DataStore("taskcollection.csv");

            // load flat file into memory.
            this.Fetch();
        }

        // Fetch Tasks from Flatfile
        private void Fetch()
        {
            Dictionary<int, List<string>> allRows = dataStore.Get();

            foreach (KeyValuePair<int, List<string>> entry in allRows)
            {
                int index = entry.Key;
                List<string> row = entry.Value;

                var t = new Task(row);
                
                // set Id of task
                t.Id = index;

                tasks.Add(t);
            }
        }

        // Create Task
        public Task Create(string input)
        {
            Task t = new Task(input);
            tasks.Add(t);
            
            // returns list of strings for storage 
            var row = t.ToRow();

            // save to datastore
            int taskId = dataStore.InsertRow(row);

            // assign id to task
            t.Id = taskId;

            // return task
            return t;
        }

        // Get Task
        private Task GetTask(int id)
        {
            return tasks.Find(task => task.Id == id);
        }

        // Get All Tasks
        private IList<Task> Get()
        {
            return tasks;
        }

        // Get number of Tasks starting from skip
        public List<Task> Get(int number, int skip = 0)
        {
            // only return pending tasks
            List<Task> pending = tasks.FindAll(
                task => task.Archive == false
            );

            // to prevent going over the range
            number = number < pending.Count ? number : pending.Count;
            return pending.GetRange(skip, skip + number);
        }

        // Update Functions
        // Update rawText
        public Task UpdateRawText(int id, string str)
        {
            Task t = this.GetTask(id);
            t.RawText = str;

            // change row in datastore
            List<string> row = t.ToRow();
            dataStore.ChangeRow(t.Id, row);

            return t;
        }

        // Update done
        public Task UpdateDone(int id, bool done)
        {
            Task t = this.GetTask(id);
            t.Done = true;
            System.Diagnostics.Debug.Write(t.Done);

            // change row in datastore
            List<string> row = t.ToRow();
            dataStore.ChangeRow(t.Id, row);

            return t;
        }

        // Update archive
        public Task UpdateArchive(int id, bool archive)
        {
            Task t = this.GetTask(id);
            t.Archive = archive;

            // change row in datastore
            List<string> row = t.ToRow();
            dataStore.ChangeRow(t.Id, row);

            return t;
        }

        // Marks all done tasks as archived
        public void ArchiveAll()
        {
            foreach (var t in tasks)
            {
                if (t.Done)
                {
                    t.Archive = true;
                }
            }
        }

        // Helper Methods
        // Filter All
        public List<Task> FilterAll(string input)
        {
            return tasks.FindAll(
                task =>
                    task.Archive == false &&
                    task.RawText.StartsWith(input)
            );
        }

        public List<Task> ByHashTags(IList<string> hashTags)
        {
            var resultSet = new HashSet<Task>();
            foreach (var tag in hashTags)
            {
                var queryResults = tasks.FindAll(task => task.Tags.Contains(tag));
                resultSet.UnionWith(queryResults);
            }
            return resultSet.ToList();
        }
    }
}