using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Type;

namespace CommandObjectTests
{
    [TestClass]
    public class Completion
    {
        [TestMethod]
        public void EmptyCompletionIsEmpty()
        {
            var input = string.Empty;

            var result = Command.TryComplete(input);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void FailedCompletionIsEmpty()
        {
            var input = Command.Done.Substring(0, 2) + "zzz";

            var result = Command.TryComplete(input);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void CompletionOfExactMatchIsEmpty()
        {
            var input = Command.Done;

            var result = Command.TryComplete(input);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void ParitalCompletionIsCorrect()
        {
            var input = Command.Archive.Substring(0, 2);
            var expected = Command.Archive.Substring(2);

            var result = Command.TryComplete(input);

            Assert.AreEqual(expected, result);
        }
    }
}
