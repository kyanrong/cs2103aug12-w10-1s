using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Type;

namespace ModelTests
{
    [TestClass]
    //@author A0082877M
    public class TaskCollection
    {
        [TestMethod]
        public void FilterAll()
        {
            // Create Task Collection
            Type.TaskCollection tc = new Type.TaskCollection();
            tc.Create("This is a task");
            tc.Create("This is another task");
            tc.Create("Not another task");

            IList<Type.Task> result = tc.FilterAll("This");

            // Expected
            Assert.AreEqual(result.Count, 2);
            Assert.AreEqual(result[0].RawText, "This is a task");
            Assert.AreEqual(result[1].RawText, "This is another task");
        }
    }
}
