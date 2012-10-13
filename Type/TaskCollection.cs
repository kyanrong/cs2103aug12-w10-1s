using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Type
{
    public class TaskCollection
    {
        private List<Task> tasks;

        // Constructor
        public TaskCollection()
        {
            // load flat file into memory.
            this.Fetch();

            // instantiate task models
            tasks = new List<Task>();
        }

        // Fetch Tasks from Flatfile
        public void Fetch()
        {
            
        }

        // Create Task
        public Task Create(string input)
        {
            Task t = new Task(input);
            tasks.Add(t);
            return t;
        }

        // Get Task
        public Task Get(int id)
        {
            return tasks.Find(task => task.Id == id);
        }

        // Helper Methods
        // Filter All
        public List<Task> filterAll(string input)
        {
            return tasks.FindAll(
                task => task.RawText.StartsWith(input)
            );
        }
    }
}