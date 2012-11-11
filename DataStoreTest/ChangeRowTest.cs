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
    public class ChangeRowTest
    {
        [TestMethod]
        public void ChangeFirstRowTest()
        {
            //create file with data
            DataStore myData = new DataStore("ChangeFirstRow");
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

            //test change row
            myList.Clear();
            myList.Add("meeting on 11/9");
            myList.Add(true.ToString());
            myList.Add(true.ToString());

            myData.ChangeRow(1, myList);

            //read data from the file to check
            FileStream fs = new FileStream("ChangeFirstRow", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            Assert.AreEqual("1,meeting on 11/9,True,True", sr.ReadLine());
            Assert.AreEqual("2,project deadline 12/nov #cs +100,False,True", sr.ReadLine());
            Assert.AreEqual("3,007 skyfall,False,False", sr.ReadLine());
            Assert.AreEqual(null, sr.ReadLine());

            sr.Close();
            fs.Close();
        }

        [TestMethod]
        public void ChangeLastRow()
        {
            //create file with data
            DataStore myData = new DataStore("ChangeLastRow");
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

            //test change row
            myList.Clear();
            myList.Add("meeting on 11/9");
            myList.Add(true.ToString());
            myList.Add(true.ToString());

            myData.ChangeRow(3, myList);

            //read data from the file to check
            FileStream fs = new FileStream("ChangeLastRow", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            Assert.AreEqual("1,meeting on 9/11,True,False", sr.ReadLine());
            Assert.AreEqual("2,project deadline 12/nov #cs +100,False,True", sr.ReadLine());
            Assert.AreEqual("3,meeting on 11/9,True,True", sr.ReadLine());
            Assert.AreEqual(null, sr.ReadLine());

            sr.Close();
            fs.Close();
        }

        [TestMethod]
        //when the index is out of bound, nothing in the file will be changed
        public void OutOfBoundTest()
        {
            //create file with data
            DataStore myData = new DataStore("OutOfBound_ChangeRow");
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

            //test change row
            myList.Clear();
            myList.Add("meeting on 11/9");
            myList.Add(true.ToString());
            myList.Add(true.ToString());

            myData.ChangeRow(6, myList);

            //read data from the file to check
            FileStream fs = new FileStream("OutOfBound_ChangeRow", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            Assert.AreEqual("1,meeting on 9/11,True,False", sr.ReadLine());
            Assert.AreEqual("2,project deadline 12/nov #cs +100,False,True", sr.ReadLine());
            Assert.AreEqual("3,007 skyfall,False,False", sr.ReadLine());
            Assert.AreEqual(null, sr.ReadLine());

            sr.Close();
            fs.Close();
        }

        [TestMethod]
        //will throw an exception if the file is changed until the index is missing
        //the missing index is checked before the target is found
        public void ExceptionTest()
        {
            //create file with data
            DataStore myData = new DataStore("Exception_ChangeRow");
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
            FileStream fs = new FileStream("Exception_ChangeRow", FileMode.Truncate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

            sw.WriteLine("meeting on 9/11,True,True");
            sw.WriteLine("2,project deadline 12/nov #cs +100,False,True");
            sw.WriteLine("3,007 skyfall,False,False");

            sw.Close();
            fs.Close();

            //testing
            try
            {
                myData.ChangeRow(2, myList);
                Assert.Fail("no exception throw");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is MissingFieldException);
            }
        }
    }
}
