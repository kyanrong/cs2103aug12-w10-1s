﻿using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            l.Add("This 121212 should not match");
            l.Add("This January");

            foreach (string x in l)
            {
                Assert.AreEqual(RegExp.Date(x), string.Empty);
            }
        }

        [TestMethod]
        public void Dates2()
        {
            string x = "This should 12/1 match";
            Assert.AreEqual(RegExp.Date(x), "12/1");
        }

        [TestMethod]
        public void Dates3()
        {
            string x = "This should 12/11 match";
            Assert.AreEqual(RegExp.Date(x), "12/11");
        }

        [TestMethod]
        public void Dates3_1()
        {
            string x = "This should 12/11/11 match";
            Assert.AreEqual(RegExp.Date(x), "12/11/11");
        }

        [TestMethod]
        public void Dates3_2()
        {
            string x = "This should 12/11/2011 match";
            Assert.AreEqual(RegExp.Date(x), "12/11/2011");
        }

        [TestMethod]
        public void Dates4()
        {
            string x = "This should 11 Jan match";
            Assert.AreEqual(RegExp.Date(x), "11 Jan");
        }

        [TestMethod]
        public void Dates5()
        {
            string x = "This should 12 march 2012 match";
            Assert.AreEqual(RegExp.Date(x), "12 march 2012");
        }

        [TestMethod]
        public void Dates6()
        {
            string x = "This should 12 march 12 match";
            Assert.AreEqual(RegExp.Date(x), "12 march 12");
        }
    }
}