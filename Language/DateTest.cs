using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Type;

namespace Language
{
    [TestClass]
    public class DateTest
    {
        [TestMethod]
        public void NegativeTests()
        {
            List<string> l = new List<string>();
            l.Add("This should not match");
            l.Add("This 12 13 13 should not match");
            l.Add("This 12112 should not match");
            l.Add("This January");

            foreach (string x in l)
            {
                Assert.AreEqual(RegExp.DateTimeT(x).Item1, string.Empty);
            }
        }

        [TestMethod]
        public void dd_m()
        {
            string x = "This should 12/1 match";
            Assert.AreEqual(RegExp.DateTimeT(x).Item1, "12/1");
            int year = DateTime.Today.Year;
            Assert.AreEqual(RegExp.DateTimeT(x).Item3, new DateTime(year, 1, 12));
        }

        [TestMethod]
        public void dd_mm()
        {
            string x = "This should 12/11 match";
            Assert.AreEqual(RegExp.DateTimeT(x).Item1, "12/11");
            int year = DateTime.Today.Year;
            Assert.AreEqual(RegExp.DateTimeT(x).Item3, new DateTime(year, 11, 12));
        }

        [TestMethod]
        public void dd_mm_yy()
        {
            string x = "This should 12/11/11 match";
            Assert.AreEqual(RegExp.DateTimeT(x).Item1, "12/11/11");
            Assert.AreEqual(RegExp.DateTimeT(x).Item3, new DateTime(2011, 11, 12));
        }

        [TestMethod]
        public void dd_mm_yyyy()
        {
            string x = "This should 12/11/2011 match";
            Assert.AreEqual(RegExp.DateTimeT(x).Item1, "12/11/2011");
            Assert.AreEqual(RegExp.DateTimeT(x).Item3, new DateTime(2011, 11, 12));
        }

        [TestMethod]
        public void dd_month()
        {
            string[] months = {
               "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
            };
            for (int i = 0; i < months.Length; i++)
            {
                string month = months[i];
                string x = "This should 11 " + month + " match";
                Assert.AreEqual(RegExp.DateTimeT(x).Item1, "11 " + month);
                int year = DateTime.Today.Year;
                Assert.AreEqual(RegExp.DateTimeT(x).Item3, new DateTime(year, i + 1, 11));
            }
            string[] months2 = {
                "January", "Febuary", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"
            };
            for (int i = 0; i < months2.Length; i++)
            {
                string month = months2[i];
                string x = "This should 11 " + month + " match";
                Assert.AreEqual(RegExp.DateTimeT(x).Item1, "11 " + month);
                int year = DateTime.Today.Year;
                Assert.AreEqual(RegExp.DateTimeT(x).Item3, new DateTime(year, i + 1, 11));
            }
        }

        [TestMethod]
        public void dd_month_yyyy()
        {
            string x = "This should 12 march 2012 match";
            Assert.AreEqual(RegExp.DateTimeT(x).Item1, "12 march 2012");
            Assert.AreEqual(RegExp.DateTimeT(x).Item3, new DateTime(2012, 3, 12));
        }

        [TestMethod]
        public void dd_month_yy()
        {
            string x = "This should 12 march 12 match";
            Assert.AreEqual(RegExp.DateTimeT(x).Item1, "12 march 12");
            Assert.AreEqual(RegExp.DateTimeT(x).Item3, new DateTime(2012, 3, 12));
        }

        [TestMethod]
        public void deadline_dd_mm_yy()
        {
            string x = "This should by 12/11/11 match";
            Assert.AreEqual(RegExp.DateTimeT(x).Item1, "by 12/11/11");
            Assert.AreEqual(RegExp.DateTimeT(x).Item3, new DateTime(2011, 11, 12));
        }

        [TestMethod]
        public void deadline_dd_mm_yyyy()
        {
            string x = "This should On 12/11/2011 match";
            Assert.AreEqual(RegExp.DateTimeT(x).Item1, "On 12/11/2011");
            Assert.AreEqual(RegExp.DateTimeT(x).Item3, new DateTime(2011, 11, 12));
        }

        [TestMethod]
        public void deadline_dd_month()
        {
            string[] months = {
               "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
            };
            for (int i = 0; i < months.Length; i++)
            {
                string month = months[i];
                string x = "This should by 11 " + month + " match";
                Assert.AreEqual(RegExp.DateTimeT(x).Item1, "by 11 " + month);
                int year = DateTime.Today.Year;
                Assert.AreEqual(RegExp.DateTimeT(x).Item3, new DateTime(year, i + 1, 11));
            }
            string[] months2 = {
                "January", "Febuary", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"
            };
            for (int i = 0; i < months2.Length; i++)
            {
                string month = months2[i];
                string x = "This should due 11 " + month + " match";
                Assert.AreEqual(RegExp.DateTimeT(x).Item1, "due 11 " + month);
                int year = DateTime.Today.Year;
                Assert.AreEqual(RegExp.DateTimeT(x).Item3, new DateTime(year, i + 1, 11));
            }
        }

        [TestMethod]
        public void deadline_dd_month_yyyy()
        {
            string x = "This should on 12 march 2012 match";
            Assert.AreEqual(RegExp.DateTimeT(x).Item1, "on 12 march 2012");
            Assert.AreEqual(RegExp.DateTimeT(x).Item3, new DateTime(2012, 3, 12));
        }

        [TestMethod]
        public void NegativeTests2()
        {
            List<string> l = new List<string>();
            l.Add("Thisby 12 January");
            l.Add("This overdue 12 January");

            foreach (string x in l)
            {
                Assert.AreEqual(RegExp.DateTimeT(x).Item1, "12 January");
            }
        }

        [TestMethod]
        public void NegativeTests3()
        {
            List<string> l = new List<string>();
            l.Add("Thisby12/12/12");
            foreach (string x in l)
            {
                // Assert.AreEqual(RegExp.Date(x).Item1, string.Empty);
            }
        }

        [TestMethod]
        public void from_to()
        {
            string x = "This should from 12/11/2011 to 12/11 match";
            var tuple = RegExp.DateTimeT(x);
            Assert.AreEqual(tuple.Item1, "from 12/11/2011 to 12/11");
            Assert.AreEqual(tuple.Item2, new DateTime(2011, 11, 12));
            Assert.AreEqual(tuple.Item3, new DateTime(2012, 11, 12));
        }

        [TestMethod]
        public void nice()
        {
            string x = "This should today match";
            Assert.AreEqual(RegExp.DateTimeT(x).Item1, "today");
            Assert.AreEqual(RegExp.DateTimeT(x).Item3, DateTime.Today);

            x = "This should tdy match";
            Assert.AreEqual(RegExp.DateTimeT(x).Item1, "tdy");
            Assert.AreEqual(RegExp.DateTimeT(x).Item3, DateTime.Today);

            x = "This should tomorrow match";
            Assert.AreEqual(RegExp.DateTimeT(x).Item1, "tomorrow");
            Assert.AreEqual(RegExp.DateTimeT(x).Item3, DateTime.Today.AddDays(1));

            x = "This should tmr match";
            Assert.AreEqual(RegExp.DateTimeT(x).Item1, "tmr");
            Assert.AreEqual(RegExp.DateTimeT(x).Item3, DateTime.Today.AddDays(1));
        }
    }
}
