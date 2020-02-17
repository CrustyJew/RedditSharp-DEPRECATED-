using System;
using RedditSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Linq.Expressions;
using Moq;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace RedditSharp.UnitTesting
{
    
    public class SubredditImageTests
    {
        [Fact]
        public void SubredditImageTest()
        {
            Mock<IWebAgent> mockWebAgent = new Mock<IWebAgent>(MockBehavior.Strict);
            JObject json = JObject.FromObject(new { data= new { name = "TestSub" }, kind = "t5" });

            Things.Subreddit sub = new Things.Subreddit(mockWebAgent.Object,json);
            
            var subStyle = new SubredditStyle(sub );

            
            SubredditImage img = new SubredditImage( subStyle, "link", "imagename", "12345");
            Assert.Equal( "http://thumbs.reddit.com/TestSub_12345.png", img.Url.ToString());

            img = new SubredditImage( subStyle, "link", "imagename", "https://testuri.uri/");
            Assert.Equal("https://testuri.uri/", img.Url.ToString());
        }
    }
}