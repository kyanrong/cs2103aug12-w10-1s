using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Type;

namespace AutoCompleteTests
{
    [TestClass]
    public class Object
    {
        [TestMethod]
        public void CanInstantiate()
        {
            //Act
            AutoComplete ac = new AutoComplete();

            //Assert
            Assert.IsNotNull(ac);
        }

        [TestMethod]
        public void CanInstantiateWithInitialDictionary()
        {
            //Arrange
            string[] dict = { "adolph", "bucket sort", "charlie" };

            //Act
            AutoComplete ac = new AutoComplete(dict);

            //Assert
            Assert.IsNotNull(ac);
        }

        [TestMethod]
        public void DictionaryEmptyOnEmptyConstructor()
        {
            //Act
            AutoComplete ac = new AutoComplete();

            //Assert
            Assert.AreEqual(0, ac.DictionarySize);
        }

        [TestMethod]
        public void DictionaryBuiltSuccessfullyWithInitialDictionary()
        {
            //Arrange
            string[] dict = { "adolph", "bucket sort", "charlie" };

            //Act
            AutoComplete ac = new AutoComplete(dict);

            //Assert
            Assert.IsTrue(ac.ContainsSuggestion("adolph"));
            Assert.IsTrue(ac.ContainsSuggestion("bucket sort"));
            Assert.IsTrue(ac.ContainsSuggestion("charlie"));
            Assert.AreEqual(dict.Length, ac.DictionarySize);
        }

        [TestMethod]
        public void DictionaryCanAddWords()
        {
            //Arrange
            string[] dict = { "adolph", "bucket sort", "charlie" };
            AutoComplete ac = new AutoComplete(dict);
            int originalSize = ac.DictionarySize;

            //Act
            ac.AddSuggestion("dominos");
            int newSize = ac.DictionarySize;

            //Assert
            Assert.IsTrue(ac.ContainsSuggestion("dominos"));
            Assert.AreEqual(newSize, (originalSize + 1));
        }

        [TestMethod]
        public void DictionaryCanRemoveWords()
        {
            //Arrange
            string[] dict = { "adolph", "bucket sort", "charlie" };
            AutoComplete ac = new AutoComplete(dict);
            int originalSize = ac.DictionarySize;

            //Act
            ac.RemoveSuggestion("adolph");
            int newSize = ac.DictionarySize;

            //Assert
            Assert.IsFalse(ac.ContainsSuggestion("adolph"));
            Assert.IsTrue(ac.ContainsSuggestion("bucket sort"));
            Assert.IsTrue(ac.ContainsSuggestion("charlie"));
            Assert.AreEqual(newSize, (originalSize - 1));
        }
    }
}
