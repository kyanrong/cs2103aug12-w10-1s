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

        public static string Date(string input)
        {
            Match m;
            // match DD/MM/[YY[YY]]
            Regex ddmm = new Regex("\\d{1,2}\\/\\d{1,2}(\\/\\d{2,4})?");
            m = ddmm.Match(input);
            if (m.Success)
            {
                return m.Value;
            }

            // match DD <Month> [YY[YY]]
            Regex ddMonthyyyy = new Regex(
                "\\d{1,2}\\s(january|febuary|march|april|may|june|july|august|september|october|november|december|jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)(\\s\\d{2,4})?",
                RegexOptions.IgnoreCase
            );
            m = ddMonthyyyy.Match(input);
            if (m.Success)
            {
                return m.Value;
            }

            // if none of the regexp matches
            // return the empty string.
            return String.Empty;
        }
    }
}
