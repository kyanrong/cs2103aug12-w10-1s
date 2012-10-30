using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Type;
namespace Language
{
    [TestClass]
    public class HashTagsTest
    {
        [TestMethod]
        public void HashTags1()
        {
            string x = "This should not match";
            List<string> result = RegExp.HashTags(x);
            Assert.AreEqual(result.Count, 0);
        }

        [TestMethod]

        public void HashTags2()
        {
            string x = "This should #match";
            List<string> result = RegExp.HashTags(x);
            Assert.AreEqual(result[0], "#match");
        }

        [TestMethod]
        public void HashTags3()
        {
            string x = "This #should #match #";
            List<string> result = RegExp.HashTags(x);
            Assert.AreEqual(result[0], "#should");
            Assert.AreEqual(result[1], "#match");
            Assert.AreEqual(result.Count, 2);
        }
    }
}
