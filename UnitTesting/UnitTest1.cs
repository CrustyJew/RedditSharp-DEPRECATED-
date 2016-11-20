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
        public void GetSubreddit()
        { //Discovered a bug where reddit.getSubreddit() will produce a 403 forbidden if you're not logged in
            if (System.Text.RegularExpressions.Regex.Replace("/r/text", "(r/|/)", "") != "text")
                throw new Exception("The regexes don't work!");
        }
    }
}
