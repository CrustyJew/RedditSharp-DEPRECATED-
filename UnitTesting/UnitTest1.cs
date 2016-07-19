using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedditSharp;
using RedditSharp.Things;

namespace UnitTesting
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void getSubreddit()
        {
            Reddit reddit = new Reddit();
            Subreddit test = reddit.GetSubreddit("/r/text");
            if(test.Id != "text")
            {
                throw new Exception("The regexes don't work!");
            }
        }
    }
}
