using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Type;

namespace DataStoreTest
{
    //@author A0088574M
    [TestClass]
    public class GetWithIndexTest
    {     
        [TestMethod]
        [ExpectedException(typeof(MissingFieldException))]
        //will throw an exception if the file is changed until the index is missing
        //the missing index is checked before the target is found
        public void GetWithIndexExceptionTest()
        {
            //create the file with data
            DataStore myData = new DataStore("Exception_GetWithIndex");
            List<string> myList = new List<string>();

            myList.Add("meeting on 9/11");
            myList.Add(true.ToString());
            myList.Add(false.ToString());
            myData.InsertRow(myList);

            myList.Clear();
            myList.Add("project deadline 12/nov #cs +100");
            myList.Add("False");
            myList.Add("True");
            myData.InsertRow(myList);

            myList.Clear();
            myList.Add("007 skyfall");
            myList.Add("False");
            myList.Add(false.ToString());
            myData.InsertRow(myList);

            //change the file data illegally
            FileStream fs = new FileStream("Exception_GetWithIndex", FileMode.Truncate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

            sw.WriteLine(",meeting on 9/11,True,True");
            sw.WriteLine("2,project deadline 12/nov #cs +100,False,True");
            sw.WriteLine("3,007 skyfall,False,False");

            sw.Close();
            fs.Close();

            //testing, should throw exception here
            myData.Get(2);
        }

        [TestMethod]
        //if the target index is out of bound, will return null
        public void OutOfBoundTest()
        {
            //create file with data
            DataStore myData = new DataStore("OutOfBound_Get");
            List<string> myList = new List<string>();

            //normal list
            myList.Add("meeting on 9/11");
            myList.Add(true.ToString());
            myList.Add(false.ToString());
            myData.InsertRow(myList);

            //list with empty string
            myList.Clear();
            myList.Add("");
            myData.InsertRow(myList);

            //empty list
            myList.Clear();
            myData.InsertRow(myList);

            List<string> actual;
            actual = myData.Get(4);//out of bound
            Assert.AreEqual(null, actual);

            actual = myData.Get(0);//out of bound
            Assert.AreEqual(null, actual);

            actual = myData.Get(1);//normal case
            Assert.AreNotEqual(null, actual);

            actual = myData.Get(2);//list with empty string
            Assert.AreNotEqual(null, actual);

            actual = myData.Get(3);//empty list
            Assert.AreNotEqual(null, actual);
        }

        [TestMethod]
        public void Normal_GetWithIndex()
        {
            //create file with data
            DataStore myData = new DataStore("Normal_GetWithIndex");
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

            //testing
            List<string> actualList;
            actualList = myData.Get(1);
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(myList1.ElementAt(i), actualList.ElementAt(i));
            }

            actualList = myData.Get(2);
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(myList2.ElementAt(i), actualList.ElementAt(i));
            }
            
            actualList = myData.Get(3);
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(myList3.ElementAt(i), actualList.ElementAt(i));
            }
        }
    }
}
