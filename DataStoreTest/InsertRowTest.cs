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
    public class InsertRowTest
    {
        //@author A0088574M
        [TestMethod]
        public void InsertEmptyListTest()
        {
            DataStore myData = new DataStore("InsertEmptyList");
            List<string> myList = new List<string>();
            
            //testing adding an empty list
            int actualUniqueID = myData.InsertRow(myList);

            //check if the unique ID is correct
            Assert.AreEqual(1, actualUniqueID);

            //read data from the file to check
            FileStream fs = new FileStream("InsertEmptyList", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            Assert.AreEqual("1", sr.ReadLine());
            Assert.AreEqual(null, sr.ReadLine());//end of file

            sr.Close();
            fs.Close();
        }

        [TestMethod]
        public void InsertListWithEmptyStringTest()
        {
            DataStore myData = new DataStore("InsertListWithEmptyString");
            List<string> myList = new List<string>();

            myList.Add("");

            //check if the unique ID is correct
            int actualUniqueID = myData.InsertRow(myList);
            Assert.AreEqual(1, actualUniqueID); 

            //read data from the file to check
            FileStream fs = new FileStream("InsertListWithEmptyString", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            Assert.AreEqual("1,", sr.ReadLine());
            Assert.AreEqual(null, sr.ReadLine());//end of file

            sr.Close();
            fs.Close();
        }

        [TestMethod]
        //testing normal circumstances with symbol ",", integer, boolean,
        //date, priority, hash tag, taskcollection.csv and undostack.csv format
        public void NormalTest()
        {
            DataStore myData = new DataStore("Normal_InsertRow");
            List<string> myList = new List<string>();

            myList.Add("project meeting");
            myList.Add(",");//test special symbol ","
            myList.Add(3.ToString());//test integer
            myList.Add(true.ToString());//test boolean

            //check if the unique ID is correct
            int actualUniqueID = myData.InsertRow(myList);
            Assert.AreEqual(1, actualUniqueID);

            myList.Clear();
            myList.Add("homework");
            myList.Add("3/11");//test date
            myList.Add("-1");//test priority
            myList.Add("#cs");//test hash tag

            //check if the unique ID is correct
            actualUniqueID = myData.InsertRow(myList);
            Assert.AreEqual(2, actualUniqueID);

            //actual taskcollection.csv format
            myList.Clear();
            myList.Add("bill on 7/nov #personal +3");
            myList.Add(false.ToString());
            myList.Add(true.ToString());

            //check if the unique ID is correct
            actualUniqueID = myData.InsertRow(myList);
            Assert.AreEqual(3, actualUniqueID);

            //actual undostack.csv format for edit
            myList.Clear();
            myList.Add("edit");
            myList.Add(23.ToString());
            myList.Add("task description");

            //check if the unique ID is correct
            actualUniqueID = myData.InsertRow(myList);
            Assert.AreEqual(4, actualUniqueID);

            //read data from the file to check
            FileStream fs = new FileStream("Normal_InsertRow", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            Assert.AreEqual("1,project meeting,,,3,True", sr.ReadLine());
            Assert.AreEqual("2,homework,3/11,-1,#cs", sr.ReadLine());
            Assert.AreEqual("3,bill on 7/nov #personal +3,False,True", sr.ReadLine());//taskcollection.csv format
            Assert.AreEqual("4,edit,23,task description", sr.ReadLine());//undostack.csv format
            Assert.AreEqual(null, sr.ReadLine());//end of file

            sr.Close();
            fs.Close();
        }
    }
}
