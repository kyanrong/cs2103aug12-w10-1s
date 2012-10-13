using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Type;

namespace AutoCompleteTests
{
    [TestClass]
    public class Suggestions
    {
        [TestMethod]
        public void ReturnsEmptyArrayWhenNoSuggestions()
        {
            //Arrange
            string[] dict = { "alligator", "carpenter", "carpool", "pig farm", "loanshark" };
            AutoComplete ac = new AutoComplete(dict);

            //Act
            string[] result = ac.GetSuggestions("meow");

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Length, 0);
        }

        [TestMethod]
        public void ReturnsSuggestionListOfQuery()
        {
            //Arrange
            string[] dict = { "sheep", "she", "sap", "salmon", "shingles" };
            AutoComplete ac = new AutoComplete(dict);

            //Act
            string[] result = ac.GetSuggestions("sh");

            //Assert
            Assert.IsFalse(result.Contains("salmon"));
            Assert.IsFalse(result.Contains("sap"));
            Assert.AreEqual(result.Length, 3);
            Assert.IsTrue(result.Contains("sheep"));
            Assert.IsTrue(result.Contains("she"));
            Assert.IsTrue(result.Contains("shingles"));
        }
    }
}
