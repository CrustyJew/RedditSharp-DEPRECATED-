using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RedditSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditSharp.Tests
{
    [TestClass()]
    public class SubredditImageTests
    {
        [TestMethod()]
        public void SubredditImageTest()
        {
            //var sub = new Mock<Things.Subreddit>();
            Things.Subreddit sub = new Things.Subreddit();
            sub.FullName = "TestSub";
            
            var subStyle = new SubredditStyle(null, sub ,null);

            SubredditImage img = new SubredditImage(null, subStyle, "link", "imagename", "12345", null);
            Assert.IsTrue(img.Url.ToString() == "http://thumbs.reddit.com/TestSub_12345.png");

            img = new SubredditImage(null, subStyle, "link", "imagename", "https://testuri.uri/", null);
            Assert.IsTrue(img.Url.ToString() == "https://testuri.uri/");
        }
    }
}