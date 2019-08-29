using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp.Things.User;
using System;
using System.Threading.Tasks;

namespace RedditSharp.Things
{
    /// <summary>
    /// A reddit user.
    /// </summary>
    public class RedditUser : PartialUser
    {
        /// <inheritdoc />
        public RedditUser(IWebAgent agent, JToken json) : base(agent, json)
        {
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
        /// This method returns itself as the full <see cref="RedditUser"/> is
        /// already initalized so no more further initialization needs to
        /// be done.
        /// </summary>
        /// <returns>This same <see cref="RedditUser"/>.</returns>
        /// <seealso cref="PartialUser.GetFullUserAsync"/>
        public override Task<RedditUser> GetFullUserAsync() => Task.FromResult(this);

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

        #endregion

        static string QueryString(Sort sort, int limit, FromTime time) =>
          $"?sort={sort.ToString("g")}&limit={limit}&t={time.ToString("g")}";

        static void CheckRange(int limit, int max_limit)
        {
            if ((limit < 1) || (limit > max_limit))
                throw new ArgumentOutOfRangeException(nameof(limit), $"Valid range: [1, {max_limit}]");
        }

        /// <inheritdoc/>
        public override string ToString() => Name;

    }
    /// <summary>
    /// A sorting system for how their posts are sorted (E.G. new, hot, etc.)
    /// </summary>
    public enum Sort
    {
        New,
        Hot,
        Top,
        Controversial
    }
    /// <summary>
    /// How to sort a post's comments (Qa style, best, etc.)
    /// </summary>
    public enum CommentSort
    {
        Best,
        Top,
        New,
        Controversial,
        Old,
        Qa
    }
    /// <summary>
    /// A time to end what you're looking at (year, etc.)
    /// </summary>
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
