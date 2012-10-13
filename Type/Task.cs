using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Type
{
    public class Task
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
        
        // Parsed Types
        public enum ParsedType {
            STRING,
            HASHTAG,
            DATETIME
        }

        private List<Tuple<string, ParsedType>> tokens;

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
           this.parse();
        }

        private void parse()
        {
            List<Tuple<string, ParsedType>> result = new List<Tuple<string, ParsedType>>();

            // TMP.
            // TODO.
            Tuple<string, ParsedType> t = Tuple.Create(this.rawText, ParsedType.STRING);
            result.Add(t);
            
            // Set tokens
            this.tokens = result;
        }

        // Task Done
        public bool Done { get; private set; }
        public bool ToggleDone()
        {
            done = !done;
            return done;
        }

        // Task Archive
        public bool Archive { get; private set; }
        public bool ToggleArchive()
        {
            archive = !archive;
            return archive;
        }

        // Other Properties
        public int Id { get; set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public string RawText {
            get { return rawText; }
            set
            {
                // reset new raw text for task
                this.rawText = value;
                
                // re-parse task obj
                this.parse();
            }
        }

        public IList<string> Tags
        {
            get { return tags.AsReadOnly(); }
        }
        public IList<Tuple<string, ParsedType>> Tokens
        {
            get { return tokens.AsReadOnly(); }
        }

        public override string ToString()
        {
            return rawText;
        }
    }
}
