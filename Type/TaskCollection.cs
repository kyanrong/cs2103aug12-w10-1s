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
            // to prevent going over the range
            number = number > tasks.Count ? number : tasks.Count; 
            return tasks.GetRange(skip, number);
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
            // TODO
            return this.GetTask(id);
        }

        // Update archive
        public Task UpdateArchive(int id, bool archive)
        {
            // TODO
            return this.GetTask(id);
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