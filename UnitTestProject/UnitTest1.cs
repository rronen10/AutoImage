using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestFind()
        {
            var x = ImageBaseTest.ImageTestUtil.GetImagePosition(@"C:\TestProjects\ImageBaseTest\UnitTestProject\template.png");
        }

        [TestMethod]
        public void TestClick()
        {
            ImageBaseTest.ImageTestUtil.Click(@"C:\TestProjects\ImageBaseTest\UnitTestProject\template.png");
        }
    }
}
