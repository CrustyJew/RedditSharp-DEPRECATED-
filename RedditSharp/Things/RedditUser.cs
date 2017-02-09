using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    public class RedditUser : Thing
    {
        public RedditUser(Reddit reddit, JToken json) : base(reddit, json) {
        }

        private string OverviewUrl => $"/user/{Name}.json";
        private string CommentsUrl => $"/user/{Name}/comments.json";
        private string LinksUrl => $"/user/{Name}/submitted.json";
        private const string SubscribedSubredditsUrl = "/subreddits/mine.json";
        private string LikedUrl => $"/user/{Name}/liked.json";
        private string DislikedUrl => $"/user/{Name}/disliked.json";
        private string SavedUrl => $"/user/{Name}/saved.json";

        private const int MAX_LIMIT = 100;

        protected override JToken GetJsonData(JToken json) => json["name"] == null ? json["data"] : json;

        /// <summary>
        /// Reddit username.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; }

        /// <summary>
        /// Returns true if the user has reddit gold.
        /// </summary>
        [JsonProperty("is_gold")]
        public bool HasGold { get; }

        /// <summary>
        /// Returns true if the user is a moderator of any subreddit.
        /// </summary>
        [JsonProperty("is_mod")]
        public bool IsModerator { get; }

        /// <summary>
        /// Total link karma of the user.
        /// </summary>
        [JsonProperty("link_karma")]
        public int LinkKarma { get; }

        /// <summary>
        /// Total comment karma of the user.
        /// </summary>
        [JsonProperty("comment_karma")]
        public int CommentKarma { get; }

        /// <summary>
        /// Date the user was created.
        /// </summary>
        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime Created { get; }

        /// <summary>
        /// Return the users overview.
        /// </summary>
        public Listing<VotableThing> Overview => new Listing<VotableThing>(Reddit, OverviewUrl);

        /// <summary>
        /// Return a <see cref="Listing{T}"/> of posts liked by the logged in user.
        /// </summary>
        public Listing<Post> LikedPosts => new Listing<Post>(Reddit,LikedUrl);

        /// <summary>
        /// Return a <see cref="Listing{T}"/> of posts disliked by the logged in user.
        /// </summary>
        public Listing<Post> DislikedPosts => new Listing<Post>(Reddit, DislikedUrl);

        /// <summary>
        /// Return a <see cref="Listing{T}"/> of comments made by the user.
        /// </summary>
        public Listing<Comment> Comments => new Listing<Comment>(Reddit, CommentsUrl);

        /// <summary>
        /// Return a <see cref="Listing{T}"/> of posts made by the user.
        /// </summary>
        public Listing<Post> Posts => new Listing<Post>(Reddit, LinksUrl);

        /// <summary>
        /// Return a list of subscribed subreddits for the logged in user.
        /// </summary>
        public Listing<Subreddit> SubscribedSubreddits => new Listing<Subreddit>(Reddit, SubscribedSubredditsUrl);

        static string QueryString(Sort sort, int limit, FromTime time) =>
          $"?sort={sort.ToString("g")}&limit={limit}&t={time.ToString("g")}";

        static void CheckRange(int limit, int max_limit) {
            if ((limit < 1) || (limit > max_limit))
                throw new ArgumentOutOfRangeException(nameof(limit), $"Valid range: [1, {max_limit}]");
        }

        /// <summary>
        /// Get a listing of comments and posts from the user sorted by <paramref name="sorting"/>, from time <paramref name="fromTime"/>
        /// and limited to <paramref name="limit"/>.
        /// </summary>
        /// <param name="sorting">How to sort the comments (hot, new, top, controversial).</param>
        /// <param name="limit">How many comments to fetch per request. Max is 100.</param>
        /// <param name="fromTime">What time frame of comments to show (hour, day, week, month, year, all).</param>
        /// <returns>The listing of comments requested.</returns>
        public Listing<VotableThing> GetOverview(Sort sorting = Sort.New, int limit = 25, FromTime fromTime = FromTime.All)
        {
            CheckRange(limit, MAX_LIMIT);
            string overviewUrl = OverviewUrl + QueryString(sorting, limit, fromTime);
            return new Listing<VotableThing>(Reddit, overviewUrl);
        }

        /// <summary>
        /// Get a listing of comments from the user sorted by <paramref name="sorting"/>, from time <paramref name="fromTime"/>
        /// and limited to <paramref name="limit"/>.
        /// </summary>
        /// <param name="sorting">How to sort the comments (hot, new, top, controversial).</param>
        /// <param name="limit">How many comments to fetch per request. Max is 100.</param>
        /// <param name="fromTime">What time frame of comments to show (hour, day, week, month, year, all).</param>
        /// <returns>The listing of comments requested.</returns>
        public Listing<Comment> GetComments(Sort sorting = Sort.New, int limit = 25, FromTime fromTime = FromTime.All)
        {
            CheckRange(limit, MAX_LIMIT);
            string commentsUrl = CommentsUrl + QueryString(sorting, limit, fromTime);
            return new Listing<Comment>(Reddit, commentsUrl);
        }

        /// <summary>
        /// Get a listing of posts from the user sorted by <paramref name="sorting"/>, from time <paramref name="fromTime"/>
        /// and limited to <paramref name="limit"/>.
        /// </summary>
        /// <param name="sorting">How to sort the posts (hot, new, top, controversial).</param>
        /// <param name="limit">How many posts to fetch per request. Max is 100.</param>
        /// <param name="fromTime">What time frame of posts to show (hour, day, week, month, year, all).</param>
        /// <returns>The listing of posts requested.</returns>
        public Listing<Post> GetPosts(Sort sorting = Sort.New, int limit = 25, FromTime fromTime = FromTime.All)
        {
            CheckRange(limit, 100);
            string linksUrl = LinksUrl + QueryString(sorting, limit, fromTime);
            return new Listing<Post>(Reddit, linksUrl);
        }

        /// <summary>
        /// Get a listing of comments and posts saved by the user sorted by <paramref name="sorting"/>, from time <paramref name="fromTime"/>
        /// and limited to <paramref name="limit"/>.
        /// </summary>
        /// <param name="sorting">How to sort the comments (hot, new, top, controversial).</param>
        /// <param name="limit">How many comments to fetch per request. Max is 100.</param>
        /// <param name="fromTime">What time frame of comments to show (hour, day, week, month, year, all).</param>
        /// <returns>The listing of posts and/or comments requested that the user saved.</returns>
        public Listing<VotableThing> GetSaved(Sort sorting = Sort.New, int limit = 25, FromTime fromTime = FromTime.All)
        {
            CheckRange(limit, 100);
            string savedUrl = SavedUrl + QueryString(sorting, limit, fromTime);
            return new Listing<VotableThing>(Reddit, savedUrl);
        }

        /// <inheritdoc/>
        public override string ToString() => Name;

    }

    public enum Sort
    {
        New,
        Hot,
        Top,
        Controversial
    }

    public enum FromTime
    {
        All,
        Year,
        Month,
        Week,
        Day,
        Hour
    }
}
