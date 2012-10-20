using System;
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
            // match DD/MM/[YY[YY]]
            Regex ddmm = new Regex("\\d{1,2}\\/\\d{1,2}(\\/\\d{2,4})?");
            m = ddmm.Match(input);
            if (m.Success)
            {
                string[] tokens = m.Value.Split('/');
                int date = int.Parse(tokens[0]);
                int month = int.Parse(tokens[1]);
                int year = DateTime.Today.Year;
                if (tokens.Length == 3)
                {
                    year = GetYearFromToken(tokens[2], year);
                }

                // TODO.
                // catch invalid date errors.
                return Tuple.Create(m.Value, new DateTime(year, month, date));
            }

            // match DD <Month> [YY[YY]]
            Regex ddMonthyyyy = new Regex(
                "\\d{1,2}\\s(january|febuary|march|april|may|june|july|august|september|october|november|december|jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)(\\s\\d{2,4})?",
                RegexOptions.IgnoreCase
            );
            m = ddMonthyyyy.Match(input);
            if (m.Success)
            {
                string[] tokens = m.Value.Split(' ');
                int date = int.Parse(tokens[0]);
                
                // month
                string monthToken = tokens[1];
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
                int year = DateTime.Today.Year;
                if (tokens.Length == 3)
                {
                    year = GetYearFromToken(tokens[2], year);
                }
                return Tuple.Create(m.Value, new DateTime(year, month, date));
            }

            // if none of the regexp matches
            // return the empty string.
            return Tuple.Create(String.Empty, datetime);
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
