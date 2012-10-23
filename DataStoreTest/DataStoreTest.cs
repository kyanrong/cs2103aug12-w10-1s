using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Type;

namespace DataStoreTest
{
    [TestClass]
    public class DataStoreTest
    {
        [TestMethod]
        public void TestInsertRow()
        {
            DataStore myData = new DataStore("testInsert");
            List<string> myList = new List<string>();

            myList.Add("how");
            myList.Add("try");
            int actual = myData.InsertRow(myList);

            //check if the unique ID is correct
            Assert.AreEqual(1, actual);

            //read data from the file to check
            FileStream fs = new FileStream("testInsert", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            Assert.AreEqual("1,how,try", sr.ReadLine());
            
            sr.Close();
            fs.Close();
        }

        [TestMethod]
        public void TestChangeRow()
        {
            DataStore myData = new DataStore("testChange");
            List<string> myList = new List<string>();

            myList.Add("first");
            myList.Add("try");
            myData.InsertRow(myList);

            myList.Clear();
            myList.Add("second");
            myList.Add("try");
            myData.InsertRow(myList);

            myList.Clear();
            myList.Add("third");
            myList.Add("try");
            myData.InsertRow(myList);

            myList.Clear();
            myList.Add("change try");
            myList.Add("1-3");

            myData.ChangeRow(2, myList);

            //read data from the file to check

            FileStream fs = new FileStream("testChange", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            Assert.AreEqual("1,first,try", sr.ReadLine());
            Assert.AreEqual("2,change try,1-3", sr.ReadLine());
            Assert.AreEqual("3,third,try", sr.ReadLine());

            sr.Close();
            fs.Close();

            //test exception
            DataStore myDataEx = new DataStore("testException");

            List<string> myListEx = new List<string>();

            myListEx.Add("first");
            myListEx.Add("try");
            myDataEx.InsertRow(myListEx);
            myDataEx.InsertRow(myListEx);

            //change the file data
            FileStream fsEx = new FileStream("testException", FileMode.Truncate, FileAccess.Write);
            StreamWriter swEx = new StreamWriter(fsEx);

            swEx.WriteLine("first,try");
            swEx.WriteLine("2,try,try");

            swEx.Close();
            fsEx.Close();

            try
            {
                myDataEx.ChangeRow(2,myListEx);
                Assert.Fail("no exception throw");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is MissingFieldException);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MissingFieldException))]
        public void TestGetWithIndexException()
        {
            DataStore myData = new DataStore("testGetWithIndexException");

            List<string> myList = new List<string>();

            myList.Add("first");
            myList.Add("try");
            myData.InsertRow(myList);
            myData.InsertRow(myList);

            //change the file data
            FileStream fs = new FileStream("testGetWithIndexException", FileMode.Truncate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

            sw.WriteLine("first,try");
            sw.WriteLine("2,try,try");

            sw.Close();
            fs.Close();

            myData.Get(2);
        }

        [TestMethod]
        public void TestGetWithIndex()
        {
            DataStore myData = new DataStore("testGetWithIndex");
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
            
            List<string> actualList = myData.Get(3);
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(myList3.ElementAt(i), actualList.ElementAt(i));
            }

            actualList = myData.Get(2);
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(myList2.ElementAt(i), actualList.ElementAt(i));
            }
        }

        [TestMethod]
        public void TestGetWithoutIndex()
        {
            DataStore myData = new DataStore("testGetWithoutIndex");
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

            Dictionary<int, List<string>> expectedTable = new Dictionary<int,List<string>>();
            expectedTable.Add(1, myList1);
            expectedTable.Add(2, myList2);
            expectedTable.Add(3, myList3);

            Dictionary<int, List<string>> actualTable = myData.Get();

            //testing by comparing each string inside each list inside the table
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
