using RedditSharp.Things;
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
        public async Task GetCommentsLimit()
        {
            RedditSharp.WebAgent agent = new RedditSharp.WebAgent(authFixture.AccessToken);
            RedditSharp.Reddit reddit = new RedditSharp.Reddit(agent);
            var post = (Post) await reddit.GetThingByFullnameAsync("t3_5u37lj");

            var comments = await post.GetCommentsAsync(limit: 9);

            Assert.NotEmpty(comments);
            Assert.Equal(9, comments.Count);

        }

        [Fact]
        public async Task GetCommentsMore()
        {
            RedditSharp.WebAgent agent = new RedditSharp.WebAgent(authFixture.AccessToken);
            RedditSharp.Reddit reddit = new RedditSharp.Reddit(agent);
            var post = (Post)await reddit.GetThingByFullnameAsync("t3_5u37lj");

            var comments = await post.GetCommentsWithMoresAsync(limit: 9);

            Assert.NotEmpty(comments);
            Assert.Equal(10, comments.Count);

        }

        [Fact]
        public async Task EnumerateAllComments()
        {
            RedditSharp.WebAgent agent = new RedditSharp.WebAgent(authFixture.AccessToken);
            RedditSharp.Reddit reddit = new RedditSharp.Reddit(agent);
            var post = (Post)await reddit.GetThingByFullnameAsync("t3_5u37lj");

            var comments = post.EnumerateCommentTreeAsync(5);
            List<Comment> commentsList = new List<Comment>();

            await comments.ForEachAsync(c => commentsList.Add(c));

            Assert.NotEmpty(commentsList);
            Assert.Equal(25, commentsList.Count);

        }
    }
}
