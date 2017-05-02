using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    /// <summary>
    /// An authenticated user.
    /// </summary>
    public class AuthenticatedUser : RedditUser
    {
#pragma warning disable 1591
        public AuthenticatedUser(IWebAgent agent, JToken json) : base (agent, json) {
        }
#pragma warning restore 1591

        private const string ModeratorUrl = "/reddits/mine/moderator.json";
        private const string UnreadMessagesUrl = "/message/unread.json?mark=true&limit=25";
        private const string ModQueueUrl = "/r/mod/about/modqueue.json";
        private const string UnmoderatedUrl = "/r/mod/about/unmoderated.json";
        private const string ModMailUrl = "/message/moderator.json";
        private const string MessagesUrl = "/message/messages.json";
        private const string InboxUrl = "/message/inbox.json";
        private const string SentUrl = "/message/sent.json";

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
        /// User modhash.
        /// <para>A modhash is a token that the reddit API requires to help prevent CSRF. Modhashes can be
        /// obtained via the /api/me.json call or in response data of listing endpoints.  The preferred way
        /// to send a modhash is to include an X-Modhash custom HTTP header with your requests.</para>
        ///<para>Modhashes are not required when authenticated with OAuth.</para>
        /// </summary>
        [JsonProperty("modhash")]
        public string Modhash { get; private set; }

        /// <summary>
        /// Returns true of the user has mail.
        /// </summary>
        [JsonProperty("has_mail")]
        public bool HasMail { get; private set; }

        /// <summary>
        /// Returns true of the user has modmail.
        /// </summary>
        [JsonProperty("has_mod_mail")]
        public bool HasModMail { get; private set; }
    }
}
