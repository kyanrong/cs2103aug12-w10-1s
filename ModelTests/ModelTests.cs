using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Type;

namespace ModelTests
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void TaskCollection()
        {
            // Create Task Collection
            TaskCollection tc = new TaskCollection();
            tc.Create("This is a task");
            tc.Create("This is another task");
            tc.Create("Not another task");

            List<Type.Task> result = tc.filter("This");

            // Expected
            Assert.AreEqual(result.Count, 2);
            Assert.AreEqual(result[0].RawText, "This is a task");
            Assert.AreEqual(result[1].RawText, "This is another task");
        }
    }
}
