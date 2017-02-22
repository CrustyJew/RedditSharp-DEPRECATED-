using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
namespace RedditSharpTests.Wiki
{

    [Collection("AuthenticatedTests")]
    public class WikiTests
    {
        private AuthenticatedTestsFixture authFixture;
        private RedditSharp.Reddit reddit;
        private RedditSharp.Things.Subreddit sub;
        private const string WIKI_PAGE_NAME = "redditsharptest";

        public WikiTests(AuthenticatedTestsFixture authenticatedFixture)
        {
            authFixture = authenticatedFixture;
            RedditSharp.WebAgent agent = new RedditSharp.WebAgent(authFixture.AccessToken);
            reddit = new RedditSharp.Reddit(agent);
            sub = reddit.GetSubredditAsync(authFixture.Config["TestSubreddit"]).Result;

            var names = sub.Wiki.GetPageNamesAsync().Result;
            if (!names.Select(n => n.ToLower()).Contains(WIKI_PAGE_NAME))
            {
                sub.Wiki.EditPageAsync(WIKI_PAGE_NAME, "**test** content ***up*** in *hur*").Wait();
            }
        }

        [Fact]
        public async Task ChangeWikiPageSettings()
        {
            await sub.Wiki.SetPageSettingsAsync(WIKI_PAGE_NAME, true, RedditSharp.WikiPageSettings.WikiPagePermissionLevel.Contributors);

            var settings = await sub.Wiki.GetPageSettingsAsync(WIKI_PAGE_NAME);

            Assert.NotNull(settings);
            Assert.Equal(true, settings.Listed);
            Assert.Equal(RedditSharp.WikiPageSettings.WikiPagePermissionLevel.Contributors, settings.PermLevel);
        }
    }
}
