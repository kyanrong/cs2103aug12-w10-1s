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

        private int nextIndex;

        // Constructor
        public TaskCollection()
        {
            // instantiate task models
            tasks = new List<Task>();
            // create data store
            dataStore = new DataStore("taskcollection.csv");

            // HACK
            nextIndex = 0;

            // load flat file into memory.
            this.Fetch();
        }

        // Fetch Tasks from Flatfile
        public void Fetch()
        {
            Dictionary<int, List<string>> allRows = dataStore.Get();
            foreach (KeyValuePair<int, List<string>> entry in allRows)
            {
                int index = entry.Key;
                List<string> row = entry.Value;

                // TODO.
            }
        }

        // Create Task
        public Task Create(string input)
        {
            Task t = new Task(input);
            
            // HACK
            t.Id = nextIndex++;
            
            tasks.Add(t);
            return t;
        }

        // Get Task
        public Task GetTask(int id)
        {
            return tasks.Find(task => task.Id == id);
        }

        // Get All Tasks
        public IList<Task> Get()
        {
            return tasks;
        }

        // Get number of Tasks starting from skip
        public IList<Task> Get(int number, int skip = 0)
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
            // TODO
            return this.GetTask(id);
        }

        // Update done
        public Task UpdateDone(int id, bool done)
        {
            Task t = this.GetTask(id);
            t.Done = done;
            return t;
        }

        // Update archive
        public Task UpdateArchive(int id, bool archive)
        {
            Task t = this.GetTask(id);
            t.Archive = archive;
            return t;
        }

        // Helper Methods
        // Filter All
        public IList<Task> FilterAll(string input)
        {
            return tasks.FindAll(
                task => task.RawText.StartsWith(input)
            ).AsReadOnly();
        }
    }
}