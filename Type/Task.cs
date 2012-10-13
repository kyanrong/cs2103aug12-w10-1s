﻿using System;
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
        
        // Parsed Types
        private static enum ParsedType {
            STRING,
            HASHTAG,
            DATETIME
        }

        private List<Tuple<string, types>> tokens;

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
            List<Tuple<string, types>> result = new List<Tuple<string, types>>();

            // TMP.
            // TODO.
            Tuple<string, types> t = Tuple.Create(this.rawText, types.String);
            result.Add(t);
            
            // Set tokens
            this.tokens = result;
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
        public IList<Tuple<string, types>> Tokens
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
            return rawText;
        }
    }
}
