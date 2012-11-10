using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Type;

namespace Language
{
    [TestClass]
    //@author A0082877M
    public class PriorityTest
    {
        [TestMethod]
        public void Nothing()
        {
            Assert.AreEqual(
                RegExp.Priority("blah blah blah").Item1,
                string.Empty
            );

            Assert.AreEqual(
                RegExp.Priority("blah blah blah").Item2,
                0
            );
        }

        [TestMethod]
        public void Positive()
        {
            Assert.AreEqual(
                RegExp.Priority("blah blah blah +1").Item1,
                "+1"
            );

            Assert.AreEqual(
                RegExp.Priority("blah blah blah +1").Item2,
                1
            );

            Assert.AreEqual(
                RegExp.Priority("blah blah blah +10").Item2,
                10
            );

            Assert.AreEqual(
                RegExp.Priority("blah blah blah +10").Item1,
                "+10"
            );

            Assert.AreEqual(
                RegExp.Priority("blah blah blah +110").Item2,
                110
            );
        }

        [TestMethod]
        public void Negative()
        {
            Assert.AreEqual(
                RegExp.Priority("blah blah blah -1").Item2,
                -1
            );

            Assert.AreEqual(
                RegExp.Priority("blah blah blah -10").Item2,
                -10
            );
            
            Assert.AreEqual(
                RegExp.Priority("blah blah blah -100").Item2,
                -100
            );
        }
    }
}
