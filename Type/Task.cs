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

        public bool Done
        {
            get { return done; }
            set { done = value; }
        }
        public bool Archive 
        {
            get { return archive; }
            set { archive = value; }
        }

        // only getters
        public DateTime Start
        {
            get { return start; }
        }
        public DateTime End
        {
            get { return end; }
        }
        public IList<string> Tags
        {
            get { return tags.AsReadOnly(); }
        }
        public IList<Tuple<string, int>> Tokens
        {
            get { return tokens.AsReadOnly(); }
        }
        public string RawText
        {
            get { return rawText; }
        }
        public int Id
        {
            get { return id; }
        }

        public override string ToString()
        {
            //@civics, return the name of the task
            return base.ToString();
        }
    }
}
