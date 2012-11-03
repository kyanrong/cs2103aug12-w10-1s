using System;
using System.Linq;
using System.Text;

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Type
{
    public class RegExp
    {
        // date re
        // 1. DDMM[YY[YY]]
        public static string DATE1 = "\\d{1,2}\\/\\d{1,2}(?:\\/\\d{2,4})?";
        // 2 DD string_rep_of_month [YY[YY]]
        public static string DATE2 = "\\d{1,2}\\s(?:january|febuary|march|april|may|june|july|august|september|october|november|december|jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)(?:\\s\\d{2,4})?";
        
        // time re
        // 1. 2pm, 2am, 12am/pm
        public static string TIME1 = "\\b\\d{1,2}(?:am|pm)\\b";
        // 2. 12:00, 13:00am/pm, 9:00
        public static string TIME2 = "\b\\d{1,2}:\\d{2}(?:am|pm)?\\b";

        // Combine cases.
        public static string DateRE = DATE1 + "|" + DATE2;
        public static string TimeRE = TIME1 + "|" + TIME2;

        // datetime re
        // 1. date [time]
        public static string DateTime1 = DateRE + "(?:\\s" + TimeRE + ")?";
        // 2. time [date]
        public static string DateTime2 = TimeRE + "(?:\\s" + DateRE + ")?";

        // Combine cases
        public static string DateTimeRE = DateTime1 + "|" + DateTime2;


        public static Tuple<string, int> Priority(string input)
        {
            Match m;
            Regex plus = new Regex("\\B\\+(\\d+)$");
            m = plus.Match(input);
            if (m.Success)
            {
                return Tuple.Create(m.Value, int.Parse(m.Groups[1].Value));
            }
            
            Regex minus = new Regex("\\B\\-(\\d+)$");
            m = minus.Match(input);
            if (m.Success)
            {
                return Tuple.Create(m.Value, -1 * int.Parse(m.Groups[1].Value));
            }

            return Tuple.Create(string.Empty, 0);
        }

        public static List<string> HashTags(string input)
        {
            Regex r = new Regex("#(.+?)\\b");
            List<string> result = new List<string>();
            Match m = r.Match(input);
            while (m.Success)
            {
                result.Add(m.Groups[0].Value);
                m = m.NextMatch();
            }
            return result;
        }

        public static Tuple<string, DateTime?, DateTime?> Date(string input)
        {
            DateTime? datetime = null;
            Match m;

            // PERIOD TASKS
            Regex fromto = new Regex("\\bfrom\\s("+ DateTimeRE + ")\\sto\\s(" + DateTimeRE + ")\\b");

            m = fromto.Match(input);
            if (m.Success)
            {
                var matches = m.Groups;
                DateTime? start = ParseDate(matches[1].Value);
                DateTime? end = ParseDate(matches[2].Value);
                return Tuple.Create(m.Value, start, end);
            }

            // DEADLINE TASKS
            Regex deadline = new Regex("\\b(?:(?:by|due|on)\\s)?("+ DateTimeRE +")", RegexOptions.IgnoreCase);
            m = deadline.Match(input);
            if (m.Success)
            {
                var matches = m.Groups;

                var x = new List<string>();
                for (int i = 0; i < matches.Count; i++)
                {
                    x.Add(matches[i].Value);
                }

                DateTime? end = ParseDate(matches[1].Value);
                return Tuple.Create(m.Value, datetime, end);
            }

            // if none of the regexp matches
            // return the empty string.
            return Tuple.Create(String.Empty, datetime, datetime);
        }

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

        private static int GetYearFromToken(string yearToken, int year)
        {
            if (yearToken.Length == 4)
            {
                year = int.Parse(yearToken);
            }
            else
            {
                int twoDigitYear = year % 100;
                int yearTokenValue = int.Parse(yearToken);
                if (yearTokenValue > twoDigitYear)
                {
                    year = 1900 + yearTokenValue;
                }
                else
                {
                    year = 2000 + yearTokenValue;
                }
            }
            return year;
        }

        private static DateTime? ParseDate(string input)
        {
            Match m;
            DateTime? datetime = null;
            Regex ddmm = new Regex(DATE1, RegexOptions.IgnoreCase);
            m = ddmm.Match(input);
            if (m.Success)
            {
                string[] tokens = input.Split('/');
                int date = int.Parse(tokens[0]);
                int month = int.Parse(tokens[1]);
                int year = DateTime.Today.Year;
                if (tokens.Length == 3)
                {
                    year = GetYearFromToken(tokens[2], year);
                }

                try
                {
                    return new DateTime(year, month, date);
                }
                catch (Exception e)
                {
                    // invalid date. dont parse further.
                    // do nothing.
                }
            }

            Regex ddMonthyyyy = new Regex(DATE2, RegexOptions.IgnoreCase);
            m = ddMonthyyyy.Match(input);
            if (m.Success)
            {
                string[] tokens = input.Split(' ');
                int date = int.Parse(tokens[0]);
                int month = MonthFromString(tokens[1]);
                int year = DateTime.Today.Year;
                if (tokens.Length == 3)
                {
                    year = GetYearFromToken(tokens[2], year);
                }

                try
                {
                    return new DateTime(year, month, date);
                }
                catch (Exception e)
                {
                    // invalid date. dont parse further.
                    // do nothing.
                }
            }

            // if none of the regexp matches
            // return the empty string.
            return datetime;
        }

        public static DateTime? ParseDateTime(string input)
        {
            DateTime? result = null;



            return result;
        }

    }
}
