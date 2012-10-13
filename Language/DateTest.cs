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
                Assert.AreEqual(RegExp.Date(x), string.Empty);
            }
        }

        [TestMethod]
        public void dd_m()
        {
            string x = "This should 12/1 match";
            Assert.AreEqual(RegExp.Date(x), "12/1");
        }

        [TestMethod]
        public void dd_mm()
        {
            string x = "This should 12/11 match";
            Assert.AreEqual(RegExp.Date(x), "12/11");
        }

        [TestMethod]
        public void dd_mm_yy()
        {
            string x = "This should 12/11/11 match";
            Assert.AreEqual(RegExp.Date(x), "12/11/11");
        }

        [TestMethod]
        public void dd_mm_yyyy()
        {
            string x = "This should 12/11/2011 match";
            Assert.AreEqual(RegExp.Date(x), "12/11/2011");
        }

        [TestMethod]
        public void dd_month()
        {
            string[] months = {
               "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
               "January", "Febuary", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"
            };
            foreach (string month in months)
            {
                string x = "This should 11 " + month + " match";
                Assert.AreEqual(RegExp.Date(x), "11 " + month);
            }
        }

        [TestMethod]
        public void dd_month_yyyy()
        {
            string x = "This should 12 march 2012 match";
            Assert.AreEqual(RegExp.Date(x), "12 march 2012");
        }

        [TestMethod]
        public void dd_month_yy()
        {
            string x = "This should 12 march 12 match";
            Assert.AreEqual(RegExp.Date(x), "12 march 12");
        }
    }
}
