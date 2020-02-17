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
        public async Task GetCommentsWithMoresAsync()
        {
            RedditSharp.WebAgent agent = new RedditSharp.WebAgent(authFixture.AccessToken);
            RedditSharp.Reddit reddit = new RedditSharp.Reddit(agent);
            var post = (Post)await reddit.GetThingByFullnameAsync("t3_f1bo6u");

            var things = await post.GetCommentsWithMoresAsync(limit: 9, depth: 2);
            Assert.NotEmpty(things);
            Assert.Equal(typeof(More), things.Last().GetType());
            Assert.NotNull(((Comment)things[0]).More);
            Assert.NotNull(((Comment)things[0]).Comments[0].More);

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
        [Fact]
        public async Task CrosspostParentList()
        {
            RedditSharp.WebAgent agent = new RedditSharp.WebAgent(authFixture.AccessToken);
            RedditSharp.Reddit reddit = new RedditSharp.Reddit(agent);
            var post = (Post)await reddit.GetThingByFullnameAsync("t3_f1e0jg");


            Assert.NotNull(post.CrossPostParents);
            Assert.True(post.CrossPostParents.Count > 0);
            Assert.True(post.CrossPostParents[0].CrossPostParents.Count == 0);

        }

    }
}
