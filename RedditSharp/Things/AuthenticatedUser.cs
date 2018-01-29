using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace RedditSharp.Things
{
    /// <summary>
    /// An authenticated user.
    /// </summary>
    public class AuthenticatedUser : RedditUser
    {
        /// <inheritdoc />
        public AuthenticatedUser(IWebAgent agent, JToken json) : base (agent, json) {
        }

        private const string ModeratorUrl = "/reddits/mine/moderator.json";
        private const string UnreadMessagesUrl = "/message/unread.json?mark=true&limit=25";
        private const string ModQueueUrl = "/r/mod/about/modqueue.json";
        private const string UnmoderatedUrl = "/r/mod/about/unmoderated.json";
        private const string ModMailUrl = "/message/moderator.json";
        private const string MessagesUrl = "/message/messages.json";
        private const string InboxUrl = "/message/inbox.json";
        private const string SentUrl = "/message/sent.json";
        private const string MentionsUrl = "/message/mentions.json";

        /// <inheritdoc />
        internal override JToken GetJsonData(JToken json) {
            return json["name"] == null ? json["data"] : json;
        }

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of subreddits moderated by the logged in user.
        /// </summary>
        public Listing<Subreddit> GetModeratorSubreddits(int max = -1) => Listing<Subreddit>.Create(WebAgent, ModeratorUrl, max, 100);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of unread messages.
        /// </summary>
        public Listing<Thing> GetUnreadMessages(int max = -1) => Listing<Thing>.Create(WebAgent, UnreadMessagesUrl, max, 100);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of items in the Moderation Queue.
        /// </summary>
        public Listing<VotableThing> GetModerationQueue(int max = -1) => Listing<VotableThing>.Create(WebAgent, ModQueueUrl, max, 100);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of unmoderated Posts.
        /// </summary>
        public Listing<Post> GetUnmoderatedLinks(int max = -1) => Listing<Post>.Create(WebAgent, UnmoderatedUrl, max, 100);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of (old style) modmail.
        /// </summary>
        public Listing<PrivateMessage> GetModMail(int max = -1) => Listing<PrivateMessage>.Create(WebAgent, ModMailUrl, max, 100);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of private messages.
        /// </summary>
        public Listing<PrivateMessage> GetPrivateMessages(int max = -1) => Listing<PrivateMessage>.Create(WebAgent, MessagesUrl, max, 100);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of messages in the inbox.
        /// </summary>
        public Listing<PrivateMessage> GetInbox(int max = -1) => Listing<PrivateMessage>.Create(WebAgent, InboxUrl, max, 100);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of sent messages.
        /// </summary>
        public Listing<PrivateMessage> GetSent(int max = -1) => Listing<PrivateMessage>.Create(WebAgent, SentUrl, max, 100);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of username mentions.
        /// </summary>
        public Listing<Comment> GetUsernameMentions(int max = -1) => Listing<Comment>.Create(WebAgent, MentionsUrl, max, 100);

        /// <summary>
        /// User modhash.
        /// <para>A modhash is a token that the reddit API requires to help prevent CSRF. Modhashes can be
        /// obtained via the /api/me.json call or in response data of listing endpoints.  The preferred way
        /// to send a modhash is to include an X-Modhash custom HTTP header with your requests.</para>
        ///<para>Modhashes are not required when authenticated with OAuth.</para>
        /// </summary>
        [JsonProperty("modhash")]
        public string Modhash { get; private set; }

        /// <summary>
        /// How many creddits this user has.
        /// </summary>
        /// <seealso cref="RedditUser.HasGold"/>
        /// <seealso cref="GoldExpiration"/>
        [JsonProperty("gold_creddits")]
        public int Creddits { get; private set; }

        /// <summary>
        /// The time a user's gold status expires, or null if the user does not
        /// have gold.
        /// </summary>
        /// <seealso cref="RedditUser.HasGold"/>
        /// <seealso cref="Creddits"/>
        [JsonProperty("gold_expiration")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? GoldExpiration { get; set; }

        /// <summary>
        /// Returns true of the user has mail.
        /// </summary>
        [JsonProperty("has_mail")]
        public bool HasMail { get; private set; }

        /// <summary>
        /// How many unread messages this user has.
        /// </summary>
        [JsonProperty("inbox_count")]
        public int InboxCount { get; private set; }

        /// <summary>
        /// Returns true of the user has modmail.
        /// </summary>
        [JsonProperty("has_mod_mail")]
        public bool HasModMail { get; private set; }

        /// <summary>
        /// Returns true if the user is a moderator of a subreddit that is
        /// enrolled in new modmail.
        /// </summary>
        [JsonProperty("new_modmail_exists")]
        public bool? NewModmailExists { get; private set; }

        /// <summary>
        /// Returns trus if this user is in the reddit beta program.
        /// </summary>
        [JsonProperty("in_beta")]
        public bool InBeta { get; private set; }

        /// <summary>
        /// Returns true if this user is a sponsor. Sponsorship status grants
        /// advertisers on reddit some extra permissions, such as the ability to
        /// run longer campaigns, use geotargeting, among other features.
        /// </summary>
        [JsonProperty("is_sponsor")]
        public bool IsSponsor { get; private set; }

        /// <summary>
        /// Returns true if this user is suspended. Suspensions prevent a user from
        /// performing most actions on the website that have a visible effect.
        /// Suspended users can still change their own preferences, but cannot
        /// view or reply to mail, vote, post or comment, or perform moderator
        /// actions, among other things.
        /// </summary>
        /// <seealso cref="SuspensionExpirationUtc"/>
        [JsonProperty("is_suspended")]
        public bool IsSuspended { get; private set; }

        /// <summary>
        /// The time a user's suspension expires, or null if not suspended or
        /// permanently suspended.
        /// </summary>
        /// <seealso cref="IsSuspended"/>
        [JsonProperty("suspension_expiration_utc")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? SuspensionExpirationUtc { get; private set; }
    }
}
