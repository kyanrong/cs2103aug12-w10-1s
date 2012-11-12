using System;

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Type
{
    //@author A0082877M
    public class RegExp
    {
        #region regular expressions
        // Date Regular Expressions
        // 1. DDMM[YY[YY]]
        public static string DATE1 = "\\b\\d{1,2}\\/\\d{1,2}(?:\\/\\d{2,4})?\\b";
        // 2 DD string_rep_of_month [YY[YY]]
        public static string DATE2 = "\\b\\d{1,2}\\s(?:january|febuary|march|april|may|june|july|august|september|october|november|december|jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)(?:\\s\\d{2,4})?\\b";
        
        // 3 Nice Dates
        public static string DATE3 = "\\btoday|tdy\\b";
        public static string DATE4 = "\\btomorrow|tmr\\b";
        
        // Time Regular Expressions
        // 1. NN(am|pm)
        public static string TIME1 = "\\b\\d{1,2}(?:am|pm)\\b";
        // 2. NN:NN[am|pm]
        public static string TIME2 = "\\b\\d{1,2}:\\d{2}(?:am|pm)?\\b";

        // Combine cases.
        public static string DateRE = "(?:" + DATE1 + "|" + DATE2 + "|" + DATE3 + "|" + DATE4 + ")";
        public static string TimeRE = "(?:" + TIME1 + "|" + TIME2 + ")";

        // Date + Time Regular Expressions
        // 1. date [time]
        public static string DateTime1 = DateRE + "(?:\\s" + TimeRE + ")?";
        // 2. time [date]
        public static string DateTime2 = TimeRE + "(?:\\s" + DateRE + ")?";

        // Combined Cases
        public static string DateTimeRE = DateTime1 + "|" + DateTime2;
        
        // Priority Regular Expressions
        public static string PLUS = "\\B\\+(\\d+)$";
        public static string MINUS = "\\B\\-(\\d+)$";

        // Hash Tag Regular Expressions
        public static string HASHTAG = "#(.+?)\\b";
        #endregion

        #region priority
        // Returns a tuple of the matching string and corresponding priority given an input string
        public static Tuple<string, int> Priority(string input)
        {
            Match m;
            // check for positive priority
            Regex plus = new Regex(PLUS);
            m = plus.Match(input);
            if (m.Success)
            {
                return Tuple.Create(m.Value, int.Parse(m.Groups[1].Value));
            }

            // check for negative priority
            Regex minus = new Regex(MINUS);
            m = minus.Match(input);
            if (m.Success)
            {
                return Tuple.Create(m.Value, -1 * int.Parse(m.Groups[1].Value));
            }
            
            // return default value of 0.
            return Tuple.Create(string.Empty, 0);
        }
        #endregion

        #region hashtag
        // Returns a list of hashtags present in the input string.
        public static List<string> HashTags(string input)
        {
            Regex r = new Regex(HASHTAG);
            List<string> result = new List<string>();
            Match m = r.Match(input);
            while (m.Success)
            {
                result.Add(m.Groups[0].Value);
                m = m.NextMatch();
            }
            return result;
        }
        #endregion

        #region datetime
        public static Tuple<string, DateTime?, DateTime?> GetDateTime(string input, DateTime today)
        {
            DateTime? datetime = null;
            Match m;

            // PERIOD TASKS
            Regex fromto = new Regex("\\bfrom\\s("+ DateTimeRE + ")\\sto\\s(" + DateTimeRE + ")\\b");

            m = fromto.Match(input);
            if (m.Success)
            {
                var matches = m.Groups;
                DateTime? start = ParseDateTime(matches[1].Value, today);
                DateTime? end = ParseDateTime(matches[2].Value, today);
                if (start != null && end != null)
                {
                    return Tuple.Create(m.Value, start, end);
                }
            }

            // DEADLINE TASKS
            Regex deadline = new Regex("\\b(?:(?:by|due|on)\\s)?("+ DateTimeRE +")", RegexOptions.IgnoreCase);
            m = deadline.Match(input);
            if (m.Success)
            {
                var matches = m.Groups;
                DateTime? end = ParseDateTime(matches[1].Value, today);
                if (end != null)
                {
                    return Tuple.Create(m.Value, datetime, end);
                }
                
            }

            // if none of the regexp matches
            // return the empty string.
            return Tuple.Create(String.Empty, datetime, datetime);
        }

        public static Tuple<string, DateTime?, DateTime?> GetDateTime(string input)
        {
            return GetDateTime(input, DateTime.Today);
        }
        #endregion

        #region datetime helpers
        private static string[] SanitizeToken(string value, string match, char split)
        {
            // check if keyword is matched
            Regex keyword = new Regex(match, RegexOptions.IgnoreCase);
            if (keyword.IsMatch(value))
            {
                // strip out keyword for date processing
                var tmp = new List<string>(value.Split(' '));
                tmp.RemoveAt(0);
                value = String.Join(" ", tmp.ToArray());
            }
            return value.Split(split);
        }

        private static int MonthFromString(string monthToken)
        {
            Dictionary<string, int> monthDict = new Dictionary<string, int>();
            monthDict.Add("january", 1);
            monthDict.Add("febuary", 2);
            monthDict.Add("march", 3);
            monthDict.Add("april", 4);
            monthDict.Add("may", 5);
            monthDict.Add("june", 6);
            monthDict.Add("july", 7);
            monthDict.Add("august", 8);
            monthDict.Add("september", 9);
            monthDict.Add("october", 10);
            monthDict.Add("november", 11);
            monthDict.Add("december", 12);
            monthDict.Add("jan", 1);
            monthDict.Add("feb", 2);
            monthDict.Add("mar", 3);
            monthDict.Add("apr", 4);
            monthDict.Add("jun", 6);
            monthDict.Add("jul", 7);
            monthDict.Add("aug", 8);
            monthDict.Add("sep", 9);
            monthDict.Add("oct", 10);
            monthDict.Add("nov", 11);
            monthDict.Add("dec", 12);

            int month;
            monthDict.TryGetValue(monthToken.ToLower(), out month);
            return month;
        }

        private static int GetYearFromToken(string yearToken)
        {
            int year = DateTime.Today.Year;
            if (yearToken.Length == 4)
            {
                year = int.Parse(yearToken);
            }
            else
            {
                int yearTokenValue = int.Parse(yearToken);
                year = 2000 + yearTokenValue;
            }
            return year;
        }
        #endregion

        #region date
        // extracts information from a date string and returns it in a tuple
        public static Tuple<int, int, int> DateFromDateString(string input, DateTime today)
        {
            // case 1.
            // DD/MM[/YY[YY]]
            Match m;
            Regex re1 = new Regex(DATE1, RegexOptions.IgnoreCase);
            m = re1.Match(input);
            if (m.Success)
            {
                string[] tokens = input.Split('/');
                int date = int.Parse(tokens[0]);
                int month = int.Parse(tokens[1]);

                int year = DateTime.Today.Year;
                if (tokens.Length == 3)
                {
                    year = GetYearFromToken(tokens[2]);
                }
                else
                {
                    // check if year is this year/next
                    DateTime res = new DateTime(year, month, date);
                    if (res < DateTime.Today)
                    {
                        year = year + 1;
                    }
                }
                return Tuple.Create(year, month, date);
            }

            // case 2.
            // DD string_rep_of_month [YY[YY]]
            Regex re2 = new Regex(DATE2, RegexOptions.IgnoreCase);
            m = re2.Match(input);
            if (m.Success)
            {
                string[] tokens = input.Split(' ');
                int date = int.Parse(tokens[0]);
                int month = MonthFromString(tokens[1]);
                int year = DateTime.Today.Year;
                if (tokens.Length == 3)
                {
                    year = GetYearFromToken(tokens[2]);
                }
                else
                {
                    // check if year is this year/next
                    DateTime res = new DateTime(year, month, date);
                    if (res < DateTime.Today)
                    {
                        year = year + 1;
                    }
                }
                return Tuple.Create(year, month, date);
            }

            // case 3.
            // nice, today
            Regex re3 = new Regex(DATE3, RegexOptions.IgnoreCase);
            m = re3.Match(input);
            if (m.Success)
            {
                int date = today.Day;
                int month = today.Month;
                int year = today.Year;
                return Tuple.Create(year, month, date);
            }

            // case 4.
            // nice, today
            Regex re4 = new Regex(DATE4, RegexOptions.IgnoreCase);
            m = re4.Match(input);
            if (m.Success)
            {
                DateTime tmr = today.AddDays(1);
                int date = tmr.Day;
                int month = tmr.Month;
                int year = tmr.Year;
                return Tuple.Create(year, month, date);
            }

            // invalid match.
            // return invalid date tuple
            return Tuple.Create(-1, -1, -1);
        }

        public static Tuple<int, int, int> DateFromDateString(string input)
        {
            return DateFromDateString(input, DateTime.Today);
        }
        #endregion

        #region time
        // extracts information from time string and returns it in a tuple
        public static Tuple<int, int> TimeFromTimeString(string input)
        {
            // case 1.
            // HH[:MM][(am|pm)]
            Match m;
            Regex re1 = new Regex("([0-9:]+)(am|pm)?", RegexOptions.IgnoreCase);
            m = re1.Match(input);
            if (m.Success)
            {
                var matches = m.Groups;
                
                // hours and minutes
                string[] tokens = matches[1].Value.Split(':');
                int hour = int.Parse(tokens[0]);
                int minutes = tokens.Length == 1 ? 0 : int.Parse(tokens[1]); 

                // AM or PM
                if (matches[2].Value == "am")
                {
                    if (hour == 12)
                    {
                        hour = 0;
                    }
                }
                else if (matches[2].Value == "pm")
                {
                    if (hour < 12)
                    {
                        hour = hour + 12;
                    }
                }

                return Tuple.Create(hour, minutes);
            }


            
            // invalid match.
            // return invalid time tuple
            return Tuple.Create(-1, -1);
        }
        #endregion

        #region date + time
        // returns datetime object if input string matches one of the acceptable formats
        private static DateTime? ParseDateTime(string input, DateTime today)
        {
            // booleans to keep track if either date/time is matched.
            // either one is necessary for input to qualify as a date/time
            bool date = false;
            bool time = false;
            DateTime result = today;

            Match m;
            // get date information
            Regex getDate = new Regex(DateRE, RegexOptions.IgnoreCase);
            m = getDate.Match(input);
            if (m.Success)
            {
                try
                {
                    date = true;
                    string dateString = m.Groups[0].Value;
                    var dateTuple = DateFromDateString(dateString, today);

                    int year = dateTuple.Item1;
                    int month = dateTuple.Item2;
                    int day = dateTuple.Item3;
                
                    result = new DateTime(year, month, day);
                }
                catch (Exception)
                {
                    // invalid date.
                    // set date match to false.
                    date = false;
                }
            }

            // get time information
            Regex getTime = new Regex(TimeRE, RegexOptions.IgnoreCase);
            m = getTime.Match(input);
            if (m.Success)
            {
                time = true;
                string timeString = m.Groups[0].Value;
                var timeTuple = TimeFromTimeString(timeString);

                int hour = timeTuple.Item1;
                int minutes = timeTuple.Item2;

                try
                {
                    int day = result.Day;
                    int month = result.Month;
                    int year = result.Year;
                    result = new DateTime(year, month, day, hour, minutes, 0);
                }
                catch (Exception)
                {
                    // invalid time
                    // set time match to false
                    time = false;
                }
            }

            // none of the regexp matches
            if (!date && !time)
            {
                return null;
            }
            else
            {
                return result;
            }
        }

        private static DateTime? ParseDateTime(string input)
        {
            return ParseDateTime(input, DateTime.Today);
        }
        #endregion
    }
}
