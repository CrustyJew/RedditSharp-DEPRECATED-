using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RedditSharp.Things;
using RedditSharp;
using System.Net;
using UnitTesting.TestData;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;
namespace UnitTesting
{
    /// <summary>
    /// Summary description for MoreUnitTest
    /// </summary>
    [TestClass]
    public class MoreUnitTest
    {

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void EnumerateComments()
        {
            Mock<WebAgent> mockWebAgent = new Mock<WebAgent>(MockBehavior.Strict);
            Mock<HttpWebRequest> mockRequest = new Mock<HttpWebRequest>(MockBehavior.Strict);
            Mock<HttpWebRequest> mockRequestMore1 = new Mock<HttpWebRequest>(MockBehavior.Strict);

            Mock<WebResponse> mockResponse = new Mock<WebResponse>(MockBehavior.Strict);
            Mock<WebResponse> mockResponseMore1 = new Mock<WebResponse>(MockBehavior.Strict);

            mockRequest.Setup(mr =>
                mr.GetResponse()
            ).Returns(mockResponse.Object);

            mockRequestMore1.Setup(mr =>
                mr.GetResponse()
            ).Returns(mockResponseMore1.Object);


            MemoryStream ms1 = new MemoryStream();
            MemoryStream ms2 = new MemoryStream();

            mockResponse.Setup(mr => mr.GetResponseStream()).Returns(ms1);
            mockResponseMore1.Setup(mr => mr.GetResponseStream()).Returns(ms2);

            mockWebAgent.Setup(wa => wa.CreateGet("/comments/post.json")).Returns(mockRequest.Object);
            mockWebAgent.Setup(wa => wa.CreateGet("/api/morechildren.json?link_id=post&children=3,3-1,3-1-more,4,5,5-1,5-1-1,5-1-more&api_type=json")).Returns(mockRequestMore1.Object);


            mockWebAgent.Setup(wa => wa.GetResponseString(It.Is<Stream>(x => x == ms1))).Returns(JsonGetComments.JsonComments());
            mockWebAgent.Setup(wa => wa.GetResponseString(It.Is<Stream>(x => x == ms2))).Returns(More_3_4_5.GetMore_3_4_5());

            Mock<Reddit> mockReddit = new Mock<Reddit>(MockBehavior.Strict);
            JToken jObject = JsonGetComments.GetComments()[0]["data"]["children"].First;

            Reddit r = mockReddit.Object;
            WebAgent waz = mockWebAgent.Object;


            Post p = new Post().Init(mockReddit.Object, jObject, mockWebAgent.Object);

            string[] fullNames = { "t1_1", "t1_2", "t1_3", "t1_4", "t1_5" };
            Comment[] comments = p.EnumerateComments().ToArray();
            Assert.AreEqual(fullNames.Count(), comments.Count());

            foreach (var element in
                comments.Zip(fullNames,
                (c, fn) => new { comment = c, fullName = fn }))
            {
                Assert.AreEqual(element.comment.FullName, element.fullName);
            }

            Assert.AreEqual(comments[0].Comments.Count, 1);
        }
    }
}
