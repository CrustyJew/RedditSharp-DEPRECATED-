using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace RedditSharp.Things
{
    /// <summary>
    /// A reddit user.
    /// </summary>
    public class RedditUser : CreatedThing
    {
        #pragma warning disable 1591
        public RedditUser(IWebAgent agent, JToken json) : base(agent, json) {
        }
#pragma warning restore 1591
        #region Properties
        private string OverviewUrl => $"/user/{Name}.json";
        private string CommentsUrl => $"/user/{Name}/comments.json";
        private string LinksUrl => $"/user/{Name}/submitted.json";
        private const string SubscribedSubredditsUrl = "/subreddits/mine.json";
        private string LikedUrl => $"/user/{Name}/liked.json";
        private string DislikedUrl => $"/user/{Name}/disliked.json";
        private string SavedUrl => $"/user/{Name}/saved.json";
        private const string UserInfoUrl = "/user/{0}/about.json";

        private const int MAX_LIMIT = 100;

        /// <inheritdoc/>
        internal override JToken GetJsonData(JToken json) => json["name"] == null ? json["data"] : json;

        /// <summary>
        /// Reddit username.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Returns true if the user has reddit gold.
        /// </summary>
        [JsonProperty("is_gold")]
        public bool HasGold { get; private set; }

        /// <summary>
        /// Returns true if the user has subscribed to a subreddit.
        /// </summary>
        [JsonProperty("has_subscribed")]
        public bool HasSubscribed { get; private set; }

        /// <summary>
        /// Returns true if this user's email has been verified.
        /// </summary>
        [JsonProperty("has_verified_email")]
        public bool? HasVerifiedEmail { get; private set; }

        /// <summary>
        /// Returns true if the user is an employee. This does not denote someone
        /// who can use the full administrative features of reddit, but users
        /// who are employees can distinguish as an admin (marked by a red [A]).
        /// </summary>
        [JsonProperty("is_employee")]
        public bool IsEmployee { get; private set; }

        /// <summary>
        /// Returns true if the user is a friend of this user. Users who the
        /// current logged in user has marked as a friend will have their
        /// posts and comments distinguished with an orange [F].
        /// </summary>
        [JsonProperty("is_friend")]
        public bool IsFriend { get; private set; }

        /// <summary>
        /// Returns true if the user is a moderator of any subreddit.
        /// </summary>
        [JsonProperty("is_mod")]
        public bool IsModerator { get; private set; }

        /// <summary>
        /// Returns true if the user is verified. This appears to not be working
        /// correctly and instead returns if the user has a new profile.
        /// </summary>
        [JsonProperty("verified")]
        public bool IsVerified { get; private set; }

        /// <summary>
        /// Total link karma of the user.
        /// </summary>
        [JsonProperty("link_karma")]
        public int LinkKarma { get; private set; }

        /// <summary>
        /// Total comment karma of the user.
        /// </summary>
        [JsonProperty("comment_karma")]
        public int CommentKarma { get; private set; }

        /// <summary>
        /// Whether this user profile should be hidden from crawler robots.
        /// </summary>
        [JsonProperty("hide_from_robots")]
        public bool HideFromRobots { get; private set; }

        /// <summary>
        /// Whether to show this user's snoovatar
        /// </summary>
        [JsonProperty("pref_show_snoovatar")]
        public bool ShowSnoovatar { get; private set; }

        /// <summary>
        /// Prefix for fullname. Includes trailing underscore
        /// </summary>
        public static string KindPrefix { get { return "t2_"; } }

#endregion

        /// <summary>
        /// Return the users overview.
        /// </summary>
        public Listing<VotableThing> GetOverview(int max = -1) => Listing<VotableThing>.Create(WebAgent, OverviewUrl, max, 100);

        /// <summary>
        /// Return a <see cref="Listing{T}"/> of posts liked by the logged in user.
        /// </summary>
        public Listing<Post> GetLikedPosts(int max = -1) => Listing<Post>.Create(WebAgent, LikedUrl, max, 100);

        /// <summary>
        /// Return a <see cref="Listing{T}"/> of posts disliked by the logged in user.
        /// </summary>
        public Listing<Post> GetDislikedPosts(int max = -1) => Listing<Post>.Create(WebAgent, DislikedUrl, max, 100);

        /// <summary>
        /// Return a <see cref="Listing{T}"/> of comments made by the user.
        /// </summary>
        public Listing<Comment> GetComments(int max = -1) => Listing<Comment>.Create(WebAgent, CommentsUrl, max, 100);

        /// <summary>
        /// Return a <see cref="Listing{T}"/> of posts made by the user.
        /// </summary>
        public Listing<Post> GetPosts(int max = -1) => Listing<Post>.Create(WebAgent, LinksUrl, max, 100);

        /// <summary>
        /// Return a list of subscribed subreddits for the logged in user.
        /// </summary>
        public Listing<Subreddit> GetSubscribedSubreddits(int max = -1) => Listing<Subreddit>.Create(WebAgent, SubscribedSubredditsUrl, max, 100);

        static string QueryString(Sort sort, int limit, FromTime time) =>
          $"?sort={sort.ToString("g")}&limit={limit}&t={time.ToString("g")}";

        static void CheckRange(int limit, int max_limit) {
            if ((limit < 1) || (limit > max_limit))
                throw new ArgumentOutOfRangeException(nameof(limit), $"Valid range: [1, {max_limit}]");
        }
        /// <summary>
        /// Returns a <see cref="RedditUser"/> by username
        /// </summary>
        /// <param name="agent">WebAgent to perform search</param>
        /// <param name="username">Username of user to return</param>
        /// <returns></returns>
        public static async Task<RedditUser> GetUserAsync(IWebAgent agent, string username)
        {
            var json = await agent.Get(string.Format(UserInfoUrl, username)).ConfigureAwait(false);
            return new RedditUser(agent, json);
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
            return new Listing<VotableThing>(WebAgent, overviewUrl);
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
            return new Listing<Comment>(WebAgent, commentsUrl);
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
            return new Listing<Post>(WebAgent, linksUrl);
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
            return new Listing<VotableThing>(WebAgent, savedUrl);
        }

        /// <inheritdoc/>
        public override string ToString() => Name;

    }
#pragma warning disable 1591
    public enum Sort
    {
        New,
        Hot,
        Top,
        Controversial
    }
    public enum CommentSort
    {
        Best,
        Top,
        New,
        Controversial,
        Old,
        Qa
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
#pragma warning restore 1591
}
