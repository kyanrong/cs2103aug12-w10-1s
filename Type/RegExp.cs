﻿using System;
using System.Linq;
using System.Text;

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Type
{
    public class RegExp
    {
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

        public static Tuple<string, DateTime> Date(string input)
        {
            DateTime datetime = new DateTime();
            Match m;

            // DEADLINE TASKS
            string deadline = "((by|due|on)\\s)";
            string deadlineOptional = deadline + "?";

            // match DD/MM/[YY[YY]]
            Regex ddmm = new Regex(deadlineOptional + "\\d{1,2}\\/\\d{1,2}(\\/\\d{2,4})?", RegexOptions.IgnoreCase);
            m = ddmm.Match(input);
            if (m.Success)
            {
                string[] tokens = SanitizeToken(m.Value, deadline, '/');
                int date = int.Parse(tokens[0]);
                int month = int.Parse(tokens[1]);
                int year = DateTime.Today.Year;
                if (tokens.Length == 3)
                {
                    year = GetYearFromToken(tokens[2], year);
                }

                try
                {
                    return Tuple.Create(m.Value, new DateTime(year, month, date));
                }
                catch (Exception e)
                {
                    // invalid date. dont parse further.
                    // do nothing.
                }
            }

            // match DD <Month> [YY[YY]]
            Regex ddMonthyyyy = new Regex(deadlineOptional +
                "\\d{1,2}\\s(january|febuary|march|april|may|june|july|august|september|october|november|december|jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)(\\s\\d{2,4})?",
                RegexOptions.IgnoreCase
            );
            m = ddMonthyyyy.Match(input);
            if (m.Success)
            {
                string[] tokens = SanitizeToken(m.Value, deadline, ' ');
                int date = int.Parse(tokens[0]);
                int month = MonthFromString(tokens[1]);
                int year = DateTime.Today.Year;
                if (tokens.Length == 3)
                {
                    year = GetYearFromToken(tokens[2], year);
                }

                try
                {
                    return Tuple.Create(m.Value, new DateTime(year, month, date));
                }
                catch (Exception e)
                {
                    // invalid date. dont parse further.
                    // do nothing.
                }
            }

            // if none of the regexp matches
            // return the empty string.
            return Tuple.Create(String.Empty, datetime);
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
    }
}
