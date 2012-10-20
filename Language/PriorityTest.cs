using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Type;

namespace Language
{
    [TestClass]
    public class PriorityTest
    {
        [TestMethod]
        public void Nothing()
        {
            Assert.AreEqual(
                RegExp.Priority("blah blah blah"),
                0
            );
        }

        [TestMethod]
        public void Positive()
        {
            Assert.AreEqual(
                RegExp.Priority("blah blah blah +1"),
                1
            );

            Assert.AreEqual(
                RegExp.Priority("blah blah blah +10"),
                10
            );

            Assert.AreEqual(
                RegExp.Priority("blah blah blah +110"),
                110
            );
        }

        [TestMethod]
        public void Negative()
        {
            Assert.AreEqual(
                RegExp.Priority("blah blah blah -1"),
                -1
            );

            Assert.AreEqual(
                RegExp.Priority("blah blah blah -10"),
                -10
            );
            
            Assert.AreEqual(
                RegExp.Priority("blah blah blah -100"),
                -100
            );
        }
    }
}
