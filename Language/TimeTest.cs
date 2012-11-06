﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

using Type;

namespace Language
{
    [TestClass]
    public class TimeTest
    {
        [TestMethod]
        public void TimeFromTimeString1()
        {
            Assert.AreEqual(
                RegExp.TimeFromTimeString("12pm"),
                Tuple.Create(12, 0)
            );
        }

        [TestMethod]
        public void TimeFromTimeString2()
        {
            Assert.AreEqual(
                RegExp.TimeFromTimeString("12:30"),
                Tuple.Create(12, 30)
            );
        }

        [TestMethod]
        public void TimeFromTimeString3()
        {
            Assert.AreEqual(
                RegExp.TimeFromTimeString("12:30am"),
                Tuple.Create(0, 30)
            );

        }

        [TestMethod]
        public void datetime_regex1()
        {
            string x = "This should 12/1 2pm match";

            Match m;
            Regex deadline = new Regex("\\b(?:(?:by|due|on)\\s)?(" + RegExp.DateTimeRE + ")", RegexOptions.IgnoreCase);
            m = deadline.Match(x);

            Assert.AreEqual(m.Groups[1].Value, "12/1 2pm");
        }

        [TestMethod]
        public void dd_m()
        {
            string x = "This should 12/1 2pm match";
            int year = DateTime.Today.Year;
            Assert.AreEqual(RegExp.DateTimeT(x).Item3, new DateTime(year, 1, 12, 14, 0, 0));
        }

    }
}
