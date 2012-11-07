using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Type;

namespace DataStoreTest
{
    [TestClass]
    public class GetWithoutIndexTest
    {
        //@author A0088574M
        [TestMethod]
        //if the file is empty then will get empty table
        public void WhenFileIsEmptyTest()
        {
            //create empty file
            DataStore myData = new DataStore("WhenFileIsEmpty");

            Dictionary<int, List<string>> expectedTable = new Dictionary<int, List<string>>();

            //testing
            Dictionary<int, List<string>> actualTable = myData.Get();
            
            //count should be equal to 0 since the dictionary is empty
            Assert.AreEqual(expectedTable.Count, actualTable.Count());
        }

        [TestMethod]
        public void NormalTest()
        {
            //create file with data
            DataStore myData = new DataStore("Normal_GetWithoutIndex");
            List<string> myList1 = new List<string>(), myList2 = new List<string>(), myList3 = new List<string>();

            myList1.Add("first");
            myList1.Add("try");
            myData.InsertRow(myList1);

            myList2.Add("second");
            myList2.Add("try");
            myData.InsertRow(myList2);

            myList3.Add("third");
            myList3.Add("try");
            myData.InsertRow(myList3);

            Dictionary<int, List<string>> expectedTable = new Dictionary<int, List<string>>();
            expectedTable.Add(1, myList1);
            expectedTable.Add(2, myList2);
            expectedTable.Add(3, myList3);

            //testing
            Dictionary<int, List<string>> actualTable = myData.Get();

            //testing by comparing each string inside each list inside the table
            //possible to have different number of element in the list but a bit complicated to test
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Assert.AreEqual(expectedTable.ElementAt(i).Value.ElementAt(j), actualTable.ElementAt(i).Value.ElementAt(j));
                }
            }
        }
    }
}
