using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Type
{
    class Task
    {
        // Store's the user's raw input
        private string rawText;
        
        // Standard Properties of all tasks
        private int id;
        private bool done;
        private bool archive;

        // Parsed Properties
        private DateTime start;
        private DateTime end;
        private List<string> tags;
        private List<Tuple<string, int>> tokens;

        // Constructor
        public Task(string rawText)
        {
            // saves the rawText
            // parsing should be idempotent
            // re-parsing on the same rawText
            // should return the same values
            this.rawText = rawText;

           // default values
           this.done = false;
           this.archive = false;

           // parse the input

        }

        public int Id
        {
            get
            {
                return id;
            }
        }

        public string RawText
        {
            get { return rawText; }
        }

        public bool Done { get; set; }
        public bool Archive { get; set; }

        // only getters
        public DateTime Start { get; }
        public DateTime End { get; }
        public IList<string> Tags { get; }
        public IList<Tuple<string, int>> Tokens { get; }
    }
}
