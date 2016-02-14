//using System;
//using System.Drawing;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace UnitTestProject
//{
//    [TestClass]
//    public class UnitTest1
//    {
//        private readonly string imageTest = @"C:\TestProjects\ImageBaseTest\UnitTestProject\template.png";
//        [TestMethod]
//        public void TestFind()
//        {
//            Assert.IsTrue(ImageBaseTest.ImageTestUtil.Exists(imageTest));
//        }

//        [TestMethod]
//        public void TestClick()
//        {
//            ImageBaseTest.ImageTestUtil.Click(imageTest);
//        }

//        [TestMethod]
//        public void TestClickOffset()
//        {
//            ImageBaseTest.ImageTestUtil.Click(imageTest,new Size(0,0));
//        }

//        [TestMethod]
//        public void TestRightClick()
//        {
//            ImageBaseTest.ImageTestUtil.Click(imageTest,isRightClick:true);
//        }
//    }
//}
