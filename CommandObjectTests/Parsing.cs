using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Type;

//@author A0092104U
namespace CommandObjectTests
{
    [TestClass]
    public class Parsing
    {
        [TestMethod]
        public void InputWithoutTokenIsAdd()
        {
            var input = "hello world";

            var result = Command.Parse(input);

            Assert.AreEqual(Command.Add, result.CommandText);
            Assert.AreEqual(input, result.Text);
        }

        [TestMethod]
        public void CanRecognizeSearch()
        {
            var input = Command.SearchToken;

            var result = Command.Parse(input);

            Assert.AreEqual(Command.Search, result.CommandText);
        }

        [TestMethod]
        public void SearchCommandPreservesInput()
        {
            var inputText = "hello world";
            var full = Command.SearchToken + " " + inputText;

            var result = Command.Parse(full);

            Assert.AreEqual(Command.Search, result.CommandText);
            Assert.AreEqual(inputText, result.Text);
        }

        [TestMethod]
        public void CanRecognizeHelp()
        {
            var input = Command.HelpToken;

            var result = Command.Parse(input);

            Assert.AreEqual(Command.Help, result.CommandText);
        }

        [TestMethod]
        public void HelpCommandIgnoresInput()
        {
            var input = Command.HelpToken + " hello world";

            var result = Command.Parse(input);

            Assert.AreEqual(Command.Help, result.CommandText);
            Assert.AreEqual(string.Empty, result.Text);
        }

        [TestMethod]
        public void SingleCommandTokenIsInvalid()
        {
            var input = Command.Token;

            var result = Command.Parse(input);

            Assert.AreEqual(Command.Invalid, result.CommandText);
        }

        [TestMethod]
        public void CommandAliasingWorks()
        {
            var text = " hello world";
            var input = Command.Done.Substring(0, 2);
            var full = Command.Token + input + text;

            var result = Command.Parse(full);

            Assert.AreEqual(Command.Done, result.CommandText);
        }

        [TestMethod]
        public void FailedCommandAliasIsInvalid()
        {
            var text = " hello world";
            var input = Command.Done.Substring(0, 2);
            input += "zzzzz";
            var full = Command.Token + input + text;

            var result = Command.Parse(full);

            Assert.AreEqual(Command.Invalid, result.CommandText);
        }

        [TestMethod]
        public void CommandsCanMatch()
        {
            var text = " hello world";
            var input = Command.Archive;
            var full = Command.Token + input + text;

            var result = Command.Parse(full);

            Assert.AreEqual(Command.Archive, result.CommandText);
        }
    }
}
