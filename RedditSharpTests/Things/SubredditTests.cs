using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RedditSharpTests.Things
{
    [Collection("AuthenticatedTests")]
    public class PostTests
    {
        private AuthenticatedTestsFixture authFixture;
        public PostTests(AuthenticatedTestsFixture authenticatedFixture)
        {
            authFixture = authenticatedFixture;
        }

        [Fact]
        public async Task GetCommentsMultiPage()
        {
            RedditSharp.WebAgent agent = new RedditSharp.WebAgent(authFixture.AccessToken);
            RedditSharp.Reddit reddit = new RedditSharp.Reddit(agent);
            var post = (Post) await reddit.GetThingByFullnameAsync("t3_5u37lj");
            var comments = post.Comments;
            comments.MaximumLimit = 10;
            comments.LimitPerRequest = 5;
            List<Comment> commentList = new List<Comment>();
            await comments.Take(9).ForEachAsync(comment =>
            {
                commentList.Add(comment);
            });


            Assert.NotEmpty(commentList);
            Assert.Equal(9, commentList.Count);

        }
    }
}
