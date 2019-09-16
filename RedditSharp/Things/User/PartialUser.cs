using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp.Extensions;
using System;
using System.Threading.Tasks;

namespace RedditSharp.Things.User
{
    /// <summary>
    /// A partial user
    /// </summary>
    public class PartialUser : CreatedThing
    {
        /// <inheritdoc />
        public PartialUser(IWebAgent agent, JToken json) : base(agent, json)
        {
            var data = json["name"] == null ? json["data"] : json;
            Name = data["name"].ValueOrDefault<string>();
            var id = data["id"].ValueOrDefault<string>();
            if (id.Contains("_"))
            {
                base.Id = id.Split('_')[1];
                base.FullName = id;
            }
        }
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
        /// Prefix for fullname. Includes trailing underscore
        /// </summary>
        public static string KindPrefix { get { return "t2_"; } }

        #endregion

        /// <summary>
        /// Gets a full <see cref="RedditUser"/> object which includes all user information.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<RedditUser> GetFullUserAsync() => await GetUserAsync(WebAgent, Name);

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

        static void CheckRange(int limit, int max_limit)
        {
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
}