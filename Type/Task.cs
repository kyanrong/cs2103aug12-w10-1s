 using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Type
{
    public class Task
    {
        // Parsed Types
        public enum ParsedType
        {
            String,
            HashTag,
            DateTime,
            PriorityHigh,
            PriorityLow
        }

        private string rawText;
        private List<Tuple<string, ParsedType>> tokens;
        private List<string> tags;

        #region Sort Order
        // @author A0092104
        //Sort descending. Smallest value at the bottom.
        public static int DefaultComparison(Task a, Task b)
        {
            long aHash = a.DefaultOrderHash();
            long bHash = b.DefaultOrderHash();
            return aHash > bHash ? -1 : aHash == bHash ? 0 : 1;
        }

        // @author A0092104
        // We create a natural ordering on a Task based on its properties.
        // 2's Int64
        // M                                                                              L
        // ---- ---- ---- ---- ---- ---- ---- ----  ---- ---- ---- ---- ---- ---- ---- ----
        // DOMM MMMM MMMM MMMM MMMM MMMM MMMM MTPP  PPPP PPPP IIII IIII IIII IIII IIII IIII
        // D = Done      - Bit Flag (0-false)
        // O = Overdue   - Bit Flag (0-false)
        // M = Magnitude - Unsigned Little Endian Integer representing number of days
        // T = Due Today - Bit Flag (0-false)
        // P = Priority  - Unsigned Little Endian Integer (Excess-256)
        // I = Identity  - Unique Task ID
        public long DefaultOrderHash()
        {
            long isDone = 0;
            long isDueToday = 0;
            long isOverdue = 0;
            long magnitude = 0;

            if (this.Done)
            {
                isDone = ((long)1 << 63);
            }

            if (this.DueToday())
            {
                isDueToday = ((long)1 << 34);
            }

            if (this.OverdueToday())
            {
                isOverdue = ((long)1 << 62);
            }

            long minutes = (long)(DateTime.Now.Date - this.End.Date).TotalMinutes;
            magnitude = (minutes << 35) & (long)0x3FFFFFF800000000;

            Debug.Assert(this.priority >= -256 && this.priority <= 511);
            long priority256 = ((long)(this.priority + 256) << 24) & (long)0x00000003FF000000;

            long taskId = ((long)this.Id & (long)0x0000000000FFFFFF);
            Debug.Assert(this.Id == taskId);

            return (isDone | isDueToday | isOverdue | priority256 | taskId | magnitude);
        }

        private bool OverdueToday()
        {
            if (hasEnd)
            {
                return this.End.Date < DateTime.Now.Date;
            }
            return false;
        }

        private bool DueToday()
        {
            if (hasEnd)
            {
                return this.End.Date == DateTime.Now.Date;
            }
            return false;
        }
        #endregion

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
            this.hasEnd = false;
            this.hasStart = false;

            this.Setup();
        }

        private void Setup()
        {
            // other misc setup
            // TODO.

            // parse the input
            this.Parse();
        }

        private void Parse()
        {
            // default token.
            var result = new List<Tuple<string, ParsedType>>();
            Tuple<string, ParsedType> t = Tuple.Create(this.RawText, ParsedType.String);
            result.Add(t);
            this.tokens = result;

            // parse hashtags
            this.tags = RegExp.HashTags(this.RawText);

            foreach (string hashtag in this.tags)
            {
                // find token contain hashtag.
                var res = new List<Tuple<string, ParsedType>>();
                foreach (Tuple<string, ParsedType> token in this.tokens)
                {
                    // if not a string. token has been parsed.
                    if (token.Item2 != ParsedType.String)
                    {
                        // add to result.
                        // no further processing required.
                        res.Add(token);
                    }
                    else
                    {
                        if (token.Item1.Contains(hashtag))
                        {
                            string[] split = token.Item1.Split(new string[] { hashtag }, StringSplitOptions.None);
                            res.Add(Tuple.Create(split[0], ParsedType.String));
                            
                            res.Add(Tuple.Create(hashtag, ParsedType.HashTag));

                            res.Add(Tuple.Create(split[1], ParsedType.String));
                        }
                        else
                        {
                            // hashtag not in token.
                            // add to result.
                            res.Add(token);
                        }
                    }
                }
                // replace this.tokens.
                this.tokens = res;
            }

            // parse dates
            Tuple<string, DateTime?, DateTime?> dateTimeMatch = RegExp.DateTime(this.rawText);
            if (dateTimeMatch.Item1 != string.Empty)
            {
                // we have a match
                var datetime = dateTimeMatch.Item1;

                if (dateTimeMatch.Item2 != null)
                {
                    this.Start = (DateTime) dateTimeMatch.Item2;
                    this.hasStart = true;
                }

                if (dateTimeMatch.Item3 != null)
                {
                    this.End = (DateTime) dateTimeMatch.Item3;
                    this.hasEnd = true;
                }

                // find token contain datetime.
                var res = new List<Tuple<string, ParsedType>>();
                foreach (Tuple<string, ParsedType> token in this.tokens)
                {
                    // if not a string. token has been parsed.
                    if (token.Item2 != ParsedType.String)
                    {
                        // add to result.
                        // no further processing required.
                        res.Add(token);
                    }
                    else
                    {
                        if (token.Item1.Contains(datetime))
                        {
                            string[] split = token.Item1.Split(new string[] { datetime }, StringSplitOptions.None);
                            res.Add(Tuple.Create(split[0], ParsedType.String));
                            res.Add(Tuple.Create(datetime, ParsedType.DateTime));
                            res.Add(Tuple.Create(split[1], ParsedType.String));
                        }
                        else
                        {
                            // date not in token.
                            // add to result.
                            res.Add(token);
                        }
                    }
                }
                // replace this.tokens.
                this.tokens = res;
            }

            // parse priority
            Tuple<string, int> priority = RegExp.Priority(this.rawText);
            if (priority.Item1 != string.Empty)
            {
                this.priority = priority.Item2;

                // find token containing priority
                var res = new List<Tuple<string, ParsedType>>();
                foreach (Tuple<string, ParsedType> token in this.tokens)
                {
                    // if not a string. token has been parsed.
                    if (token.Item2 != ParsedType.String)
                    {
                        // add to result.
                        // no further processing required.
                        res.Add(token);
                    }
                    else
                    {
                        if (token.Item1.Contains(priority.Item1))
                        {
                            string[] split = token.Item1.Split(new string[] { priority.Item1 }, StringSplitOptions.None);
                            res.Add(Tuple.Create(split[0], ParsedType.String));

                            if (priority.Item2 > 0)
                            {
                                res.Add(Tuple.Create(priority.Item1, ParsedType.PriorityHigh));
                            }
                            else
                            {
                                res.Add(Tuple.Create(priority.Item1, ParsedType.PriorityLow));
                            }
                            res.Add(Tuple.Create(split[1], ParsedType.String));
                        }
                        else
                        {
                            // date not in token.
                            // add to result.
                            res.Add(token);
                        }
                    }
                }
                // replace this.tokens.
                this.tokens = res;
            }
        }

        public Task Clone()
        {
            return new Task(this.ToRow());
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
        public int priority { get; private set; }
        private bool hasStart;
        public DateTime Start { get; private set; }
        private bool hasEnd;
        public DateTime End { get; private set; }
        
        public string RawText
        {
            get
            {
                return rawText;
            }
            set
            {
                rawText = value;
                this.Parse();
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
            return this.RawText;
        }
    }
}
