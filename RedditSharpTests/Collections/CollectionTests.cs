using System;
using System.Linq;
using System.Threading.Tasks;
using RedditSharp;
using RedditSharp.Things;
using Retry;
using Xunit;

namespace RedditSharpTests.Collections
{
    [Collection("AuthenticatedTests")]
    public class CollectionTests
    {
        private readonly Reddit _reddit;
        private readonly string _subredditName;

        public CollectionTests(AuthenticatedTestsFixture authenticatedFixture)
        {
            var authFixture = authenticatedFixture;
            var agent = new WebAgent(authFixture.AccessToken);
            _reddit = new Reddit(agent, true);
            _subredditName = authFixture.Config["TestSubreddit"];
        }

        [SkippableFact]
        public async Task CreatingACollection()
        {
            var guid = GenerateGuid();
            var currentDate = DateTime.UtcNow.AddMinutes(-2);

            var sub = await _reddit.GetSubredditAsync(_subredditName);
            SkipIfNotModerator(sub);

            var title = $"A collection with no posts {guid}";
            var description = $"Collection description {GenerateGuid()}";

            var result = await sub.CreateCollectionAsync(title, description);

            Assert.Equal(description, result.Description);
            Assert.Equal(title, result.Title);
            Assert.Equal(sub.FullName, result.SubredditId);
            Assert.True(result.CreatedAtUtc >= currentDate);
            Assert.True(result.LastUpdateUtc >= currentDate);

            var collections = await sub.GetCollectionsAsync();
            Assert.True(collections.Count >= 1, "there should be at least one collection");
            var collection = collections.FirstOrDefault(x => x.CollectionId == result.CollectionId);
            Assert.NotNull(collection);

            await _reddit.DeleteCollectionAsync(collection.CollectionId);
        }

        [SkippableFact]
        public async Task CreatingACollectionAndAddingPosts()
        {
            var post1Guid = GenerateGuid();
            var post2Guid = GenerateGuid();
            var title = $"Collection of {post1Guid} and {post2Guid}";
            var description = $"Awesome new collection {GenerateGuid()}";

            var sub = await _reddit.GetSubredditAsync(_subredditName);
            SkipIfNotModerator(sub);

            var post1Task = sub.SubmitPostAsync($"Post {post1Guid}", "https://github.com/CrustyJew/RedditSharp", resubmit: true);
            var post2Task = sub.SubmitTextPostAsync($"Post {post2Guid}", $"Post {post2Guid}");

            var createCollectionTask = sub.CreateCollectionAsync(title, description);

            var post1 = await post1Task;
            var post2 = await post2Task;
            var collectionResult = await createCollectionTask;

            Assert.NotNull(post1);
            Assert.NotNull(post2);

            var addPost1Task = collectionResult.AddPostAsync(post1.FullName);
            var addPost2Task = collectionResult.AddPostAsync(post2.FullName);

            await addPost1Task;
            await addPost2Task;

            var collection = await RetryHelper.Instance
                .Try(() => _reddit.GetCollectionAsync(collectionResult.CollectionId))
                .WithTryInterval(TimeSpan.FromSeconds(0.5))
                .WithMaxTryCount(10)
                .Until(c => c.LinkIds.Length > 1);

            Assert.Equal(2, collection.LinkIds.Length);
            Assert.Contains(post1.FullName, collection.LinkIds);
            Assert.Contains(post2.FullName, collection.LinkIds);

            Assert.Equal(2, collection.Posts.Length);

            var collectionWithLinkContent = await _reddit.GetCollectionAsync(collectionResult.CollectionId, includePostsContent: false);

            Assert.Empty(collectionWithLinkContent.Posts);

            await _reddit.DeleteCollectionAsync(collection.CollectionId);
        }

        [Fact]
        public async Task DeletingANonExistentCollection()
        {
            var exception =  await Assert.ThrowsAsync<RedditException>(() => _reddit.DeleteCollectionAsync("00000000-0000-0000-1111-111111111111"));
            Assert.Contains(exception.Errors, error => error[0].ToString().Equals("INVALID_COLLECTION_ID"));
        }

        private void SkipIfNotModerator(Subreddit sub)
        {
            Skip.If(sub.UserIsModerator != true, $"User isn't a moderator of ${_subredditName} so a collection cannot be made.");
        }

        private static string GenerateGuid()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 5);
        }
    }
}