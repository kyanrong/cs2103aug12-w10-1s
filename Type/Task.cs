using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Type
{
    public class Task
    {        
        // Parsed Types
        public enum ParsedType {
            STRING,
            HASHTAG,
            DATETIME
        }
        private List<Tuple<string, ParsedType>> tokens;
        private List<string> tags;

        // Constructor
        // from row.
        public Task(List<string> row)
        {
            // saves the rawText
            // parsing should be idempotent
            // re-parsing on the same rawText
            // should return the same values
            this.RawText = row[0];
            this.Done = Boolean.Parse(row[1]);
            this.Archive = Boolean.Parse(row[2]);
            this.Setup();
        }
        // from rawText
        public Task(string rawText)
        {
            this.RawText = rawText;
            this.Done = false;
            this.Archive = false;

            this.Setup();
        }

        private void Setup()
        {
            // default token.
            var result = new List<Tuple<string, ParsedType>>();
            Tuple<string, ParsedType> t = Tuple.Create(this.RawText, ParsedType.STRING);
            result.Add(t);
            this.tokens = result;

            // parse the input
            this.Parse();
        }

        private void Parse()
        {
            // parse hashtags
            List<string> hashtags = RegExp.HashTags(this.RawText);

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

        // returns row of strings for storing
        public List<string> ToRow()
        {
            var row = new List<string>();
            row.Add(this.RawText);
            row.Add(this.Done.ToString());
            row.Add(this.Archive.ToString());
            return row;
        }

        // Task Done
        public bool Done { get; set; }

        // Task Archive
        public bool Archive { get; set; }

        // Other Properties
        public int Id { get; set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public string RawText { get; set; }
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
            return this.RawText;
        }
    }
}
