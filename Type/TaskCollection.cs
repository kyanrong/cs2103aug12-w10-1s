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

            // instantiate task models
            tasks = new List<Task>();
        }

        // Create Task
        public Task Create(string input)
        {
            Task t = new Task(input);
            tasks.Add(t);
            return t;
        }



        // Helper Methods
        // Filter
        public List<Task> filter(string input)
        {
            return tasks.FindAll(
                task => task.RawText.StartsWith(input)
            );
        }
    }
}