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

           // default token.
           var result = new List<Tuple<string, ParsedType>>();
           Tuple<string, ParsedType> t = Tuple.Create(this.rawText, ParsedType.STRING);
           result.Add(t);
           this.tokens = result;

           // parse the input
           this.parse();
        }

        private void parse()
        {
            // parse hashtags
            List<string> hashtags = RegExp.HashTags(rawText);

            foreach (string hashtag in hashtags)
            {
                // find token contain hashtag.
                var result = new List<Tuple<string, ParsedType>>();
                foreach (Tuple<string, ParsedType> t in this.tokens)
                {
                    // if not a string. token has been parsed.
                    if (t.Item2 != ParsedType.STRING)
                    {
                        // add to result.
                        // no further processing required.
                        result.Add(t);
                    }
                    else
                    {
                        if (t.Item1.Contains(hashtag))
                        {
                            string[] split = t.Item1.Split(new string[] { hashtag }, StringSplitOptions.None);
                            result.Add(Tuple.Create(split[0], ParsedType.STRING));
                            
                            result.Add(Tuple.Create(hashtag, ParsedType.HASHTAG));

                            result.Add(Tuple.Create(split[1], ParsedType.STRING));
                        }
                        else
                        {
                            // hashtag not in token.
                            // add to result.
                            result.Add(t);
                        }
                    }
                }
                // replace this.tokens.
                this.tokens = result;
            }

            
        }

        // Task Done
        public bool Done { get; set; }

        // Task Archive
        public bool Archive { get; set; }

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
