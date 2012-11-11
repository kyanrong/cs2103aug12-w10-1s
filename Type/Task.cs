using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Type
{
    public class Task
    {
        #region Enumerations
        public enum ParsedType
        {
            String,
            HashTag,
            DateTime,
            PriorityHigh,
            PriorityLow
        }
        #endregion

        #region Fields
        private string rawText;
        private List<Tuple<string, ParsedType>> tokens;
        public List<string> Tags;

        // Task Done
        public bool Done { get; set; }

        // Task Archive
        public bool Archive { get; set; }

        // Other Properties
        public int Id { get; set; }
        public DateTime LastMod { get; set; }
        public int Priority { get; private set; }
        private bool HasStart;
        public DateTime Start { get; private set; }
        private bool HasEnd;
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

        public IList<Tuple<string, ParsedType>> Tokens
        {
            get { return tokens.AsReadOnly(); }
        }
        #endregion

        #region Sort Orders
        //@author A0092104U
        //Sort descending. Smallest value at the bottom.
        public static int DefaultComparison(Task a, Task b)
        {
            long aHash = a.DefaultOrderHash();
            long bHash = b.DefaultOrderHash();
            return aHash > bHash ? -1 : aHash == bHash ? 0 : 1;
        }

        //@author A0092104U
        // We create a natural ordering on a Task based on its properties.
        // 2's Int64
        // M                                                                              L
        // ---- ---- ---- ---- ---- ---- ---- ----  ---- ---- ---- ---- ---- ---- ---- ----
        // DSOM MMMM MMMM MMMM MMMM MMMM MMMM MTPP  PPPP PPPP IIII IIII IIII IIII IIII IIII
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
            long isStarted = 0;

            if (this.Done)
            {
                isDone = ((long)1 << 63);
            }

            if (this.HasStarted())
            {
                isStarted = ((long)1 << 62);
            }

            var ticksDiff = DateTime.Now.Ticks - this.End.Ticks;
            long minutes = 0;
            if (this.DueToday())
            {
                isDueToday = ((long)1 << 34);
            }
            else if (this.OverdueToday())
            {
                isOverdue = ((long)1 << 61);
                minutes = (long)(new TimeSpan(ticksDiff)).TotalMinutes;
            }
            else if (this.Future())
            {
                minutes = (long)(new TimeSpan(ticksDiff)).TotalMinutes;
            }
            magnitude = (minutes << 35) & (long)0x1FFFFFF800000000;

            Debug.Assert(this.Priority >= -256 && this.Priority <= 511);
            long priority256 = ((long)(this.Priority + 256) << 24) & (long)0x00000003FF000000;

            long taskId = ((long)this.Id & (long)0x0000000000FFFFFF);
            Debug.Assert(this.Id == taskId);

            return (isDone | isDueToday | isOverdue | priority256 | taskId | magnitude | isStarted);
        }
        #endregion

        #region Sort Order Helper Methods
        //@author A0092104U
        private bool HasStarted()
        {
            if (this.HasStart & this.Start >= DateTime.Now)
            {
                return false;
            }
            return true;
        }

        //@author A0092104U
        private bool Future()
        {
            if (HasEnd)
            {
                return this.End > DateTime.Now;
            }
            return false;
        }

        //@author A0092104U
        private bool OverdueToday()
        {
            if (HasEnd)
            {
                return this.End < DateTime.Now;
            }
            return false;
        }

        //@author A0092104U
        private bool DueToday()
        {
            if (HasEnd)
            {
                return this.End == DateTime.Now;
            }
            return false;
        }
        #endregion

        #region Constructors
        //@author A0082877M
        // from row.
        public Task(List<string> row)
        {
            // saves the rawText
            // parsing should be idempotent
            // re-parsing on the same rawText
            // should return the same values
            this.rawText = row[0];
            this.Done = Boolean.Parse(row[1]);
            this.Archive = Boolean.Parse(row[2]);
            this.LastMod = row.Count < 4 ? DateTime.Today : DateTime.Parse(row[3]);
            this.Setup();
        }
        //@author A0082877M
        // from rawText
        public Task(string rawText)
        {
            this.rawText = rawText;
            this.Done = false;
            this.Archive = false;
            this.HasEnd = false;
            this.HasStart = false;
            this.LastMod = DateTime.Today;
            this.Setup();
        }
        //@author A0082877M
        private void Setup()
        {
            // other misc setup steps
            // none atm.

            // parse the input
            this.Parse();
        }
        #endregion

        #region Parsing
        //@author A0082877M
        public void Parse()
        {
            // default token.
            var result = new List<Tuple<string, ParsedType>>();
            Tuple<string, ParsedType> t = Tuple.Create(this.RawText, ParsedType.String);
            result.Add(t);
            this.tokens = result;

            // parse hashtags
            this.Tags = RegExp.HashTags(this.RawText);
            // sort tags by lengths.
            var lengths = from element in this.Tags
                          orderby -element.Length
                          select element;


            foreach (string hashtag in lengths)
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
            Tuple<string, DateTime?, DateTime?> dateTimeMatch = RegExp.GetDateTime(this.rawText, this.LastMod);

            string datestring = dateTimeMatch.Item1;
            if (datestring != string.Empty)
            {
                // check lastMod and string.
                // reparse if necessary
                Match m;
                Regex re3 = new Regex(RegExp.DATE3, RegexOptions.IgnoreCase);
                m = re3.Match(datestring);
                if (m.Success)
                {
                    // check if lastMod is today
                    if (this.LastMod != DateTime.Today)
                    {
                        // if not today.
                        // change to date.
                        string replaceby = this.LastMod.Day + "/" + this.LastMod.Month;
                        string newRawText = re3.Replace(this.rawText, replaceby);

                        // replace rawtext
                        this.rawText = newRawText;

                        // re-parse;
                        this.Parse();
                        return;
                    }
                }

                Regex re4 = new Regex(RegExp.DATE4, RegexOptions.IgnoreCase);
                m = re4.Match(datestring);
                if (m.Success)
                {
                    // check if lastMod is today
                    if (this.LastMod != DateTime.Today)
                    {
                        // if not today.
                        // change to date.
                        string replaceby = this.LastMod.AddDays(1).Day + "/" + this.LastMod.AddDays(1).Month;
                        string newRawText = re4.Replace(this.rawText, replaceby);

                        // replace rawtext
                        this.rawText = newRawText;

                        // re-parse;
                        this.Parse();
                        return;
                    }
                }


                // we have a match
                var datetime = dateTimeMatch.Item1;

                if (dateTimeMatch.Item2 != null)
                {
                    this.Start = (DateTime) dateTimeMatch.Item2;
                    this.HasStart = true;
                }

                if (dateTimeMatch.Item3 != null)
                {
                    this.End = (DateTime) dateTimeMatch.Item3;
                    this.HasEnd = true;
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
                this.Priority = priority.Item2;

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
        #endregion

        #region Helper Methods
        //@author A0082877M
        public Task Clone()
        {
            return new Task(this.ToRow());
        }

        // returns row of strings for storing
        //@author A0082877M
        public List<string> ToRow()
        {
            var row = new List<string>();
            row.Add(this.RawText);
            row.Add(this.Done.ToString());
            row.Add(this.Archive.ToString());
            row.Add(this.LastMod.ToString());
            return row;
        }

        public override string ToString()
        {
            return this.RawText;
        }
        #endregion
    }
}
