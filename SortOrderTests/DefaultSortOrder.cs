using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Type;

namespace SortOrderTests
{
    [TestClass]
    public class DefaultSortOrder
    {
        /// <summary>
        /// Undated Tasks should be arranged in order of their priority.
        /// </summary>
        [TestMethod]
        public void UndatedTaskPriorityOrder()
        {
            // Arrange
            var t1 = new Task("i +1");
            t1.Id = 0;
            var t2 = new Task("j");
            t2.Id = 1;
            var t3 = new Task("k -1");
            t3.Id = 2;

            var h1 = t1.DefaultOrderHash();
            var h2 = t2.DefaultOrderHash();
            var h3 = t3.DefaultOrderHash();

            // Assert
            Assert.IsTrue(h1 > h2);
            Assert.IsTrue(h2 > h3);
            Assert.IsTrue(h1 > h3);
        }

        /// <summary>
        /// Tasks should be ranked in order of insertion.
        /// </summary>
        [TestMethod]
        public void IdDifferent()
        {
            // Arrange
            var t1 = new Task("something");
            t1.Id = 0;
            var t2 = new Task("something");
            t2.Id = 1;

            var h1 = t1.DefaultOrderHash();
            var h2 = t2.DefaultOrderHash();

            // Assert
            Assert.IsTrue(h2 > h1);
        }

        /// <summary>
        /// Overdue tasks should always rank higher than future tasks.
        /// </summary>
        [TestMethod]
        public void OverdueFutureOrder()
        {
            // Arrange
            var t1 = new Task("something 12 may");
            t1.Id = 0;
            var t2 = new Task("something 1 jan 2014");
            t2.Id = 2;
           
            var h1 = t1.DefaultOrderHash();
            var h2 = t2.DefaultOrderHash();

            // Assert
            Assert.IsTrue(h1 > h2);
        }

        /// <summary>
        /// Overdue tasks should ranked in order of their lateness, defined as number of minutes overdue.
        /// </summary>
        [TestMethod]
        public void OverdueOrder()
        {
            // Arrange
            var t1 = new Task("something 12 may");
            t1.Id = 0;
            var t2 = new Task("something 10 jan");
            t2.Id = 2;

            var h1 = t1.DefaultOrderHash();
            var h2 = t2.DefaultOrderHash();

            // Assert
            Assert.IsTrue(h2 > h1);
        }

        /// <summary>
        /// Future tasks should rank in order of their due dates, defined as number of minutes from now.
        /// </summary>
        [TestMethod]
        public void FutureOrder()
        {
            // Arrange
            var t1 = new Task("something 12 may 2015");
            t1.Id = 0;
            var t2 = new Task("something 10 jan 2013");
            t2.Id = 2;

            var h1 = t1.DefaultOrderHash();
            var h2 = t2.DefaultOrderHash();

            // Assert
            Assert.IsTrue(h2 > h1);
        }

        /// <summary>
        /// Done tasks should rank smaller than all other tasks.
        /// </summary>
        [TestMethod]
        public void DoneOrder()
        {
            // Arrange
            var t1 = new Task("something 12 may 2015");
            t1.Id = 0;
            var t2 = new Task("something 10 jan 2013");
            t2.Id = 2;
            var t3 = new Task("something else 19 jan 2011 +100");
            t3.Id = 3;
            t3.Done = true;

            var h1 = t1.DefaultOrderHash();
            var h2 = t2.DefaultOrderHash();
            var h3 = t3.DefaultOrderHash();

            // Assert
            Assert.IsTrue(h3 < h1);
            Assert.IsTrue(h3 < h2);
        }
    }
}
