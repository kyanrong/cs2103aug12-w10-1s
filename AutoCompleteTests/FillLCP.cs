using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Type;

namespace AutoCompleteTests
{
    [TestClass]
    public class FillLCP
    {
        [TestMethod]
        public void FillsNothingIfNoMatch()
        {
            //Arrange
            string[] dict = { "catnip", "weed", "ice", "wraith" };
            AutoComplete ac = new AutoComplete(dict);

            //Act
            string result = ac.CompleteToCommonPrefix("a");

            //Assert
            Assert.AreEqual(result, "");
        }

        [TestMethod]
        public void FillsWholeStringIfOneMatch()
        {
            //Arrange
            string[] dict = { "catnip", "weed", "ice", "wraith" };
            AutoComplete ac = new AutoComplete(dict);

            //Act
            string result = ac.CompleteToCommonPrefix("c");

            //Assert
            Assert.AreEqual(result, "atnip");
        }

        [TestMethod]
        public void FillsToLongestCommonPrefix()
        {
            //Arrange
            string[] dict = { "catnip", "weed", "ice", "wraith", "warzone", "wrong", "cat" };
            AutoComplete ac = new AutoComplete(dict);

            //Act
            string result = ac.CompleteToCommonPrefix("c");

            //Assert
            Assert.AreEqual(result, "at");
        }
    }
}