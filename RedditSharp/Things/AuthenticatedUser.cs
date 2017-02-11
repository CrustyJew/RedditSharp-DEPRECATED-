using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    public class AuthenticatedUser : RedditUser
    {
        public AuthenticatedUser(Reddit reddit, JToken json) : base (reddit, json) {
        }

        private const string ModeratorUrl = "/reddits/mine/moderator.json";
        private const string UnreadMessagesUrl = "/message/unread.json?mark=true&limit=25";
        private const string ModQueueUrl = "/r/mod/about/modqueue.json";
        private const string UnmoderatedUrl = "/r/mod/about/unmoderated.json";
        private const string ModMailUrl = "/message/moderator.json";
        private const string MessagesUrl = "/message/messages.json";
        private const string InboxUrl = "/message/inbox.json";
        private const string SentUrl = "/message/sent.json";

        protected override JToken GetJsonData(JToken json) {
          return json["name"] == null ? json["data"] : json;
        }

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of subreddits moderated by the logged in user.
        /// </summary>
        public Listing<Subreddit> ModeratorSubreddits => new Listing<Subreddit>(Reddit, ModeratorUrl);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of unread messages.
        /// </summary>
        public Listing<Thing> UnreadMessages => new Listing<Thing>(Reddit, UnreadMessagesUrl);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of items in the Moderation Queue.
        /// </summary>
        public Listing<VotableThing> ModerationQueue => new Listing<VotableThing>(Reddit, ModQueueUrl);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of unmoderated Posts.
        /// </summary>
        public Listing<Post> UnmoderatedLinks => new Listing<Post>(Reddit, UnmoderatedUrl);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of (old style) modmail.
        /// </summary>
        public Listing<PrivateMessage> ModMail => new Listing<PrivateMessage>(Reddit, ModMailUrl);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of private messages.
        /// </summary>
        public Listing<PrivateMessage> PrivateMessages => new Listing<PrivateMessage>(Reddit, MessagesUrl);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of messages in the inbox.
        /// </summary>
        public Listing<PrivateMessage> Inbox => new Listing<PrivateMessage>(Reddit, InboxUrl);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of sent messages.
        /// </summary>
        public Listing<PrivateMessage> Sent => new Listing<PrivateMessage>(Reddit, SentUrl);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of unmoderated links.
        /// </summary>
        public Listing<Post> GetUnmoderatedLinks() => new Listing<Post>(Reddit, UnmoderatedUrl);

        #region Obsolete Getter Methods

        [Obsolete("Use ModeratorSubreddits property instead")]
        public Listing<Subreddit> GetModeratorReddits() => ModeratorSubreddits;

        [Obsolete("Use UnreadMessages property instead")]
        public Listing<Thing> GetUnreadMessages() => UnreadMessages;

        [Obsolete("Use ModerationQueue property instead")]
        public Listing<VotableThing> GetModerationQueue() => new Listing<VotableThing>(Reddit, ModQueueUrl);

        [Obsolete("Use ModMail property instead")]
        public Listing<PrivateMessage> GetModMail() => new Listing<PrivateMessage>(Reddit, ModMailUrl);

        [Obsolete("Use PrivateMessages property instead")]
        public Listing<PrivateMessage> GetPrivateMessages() => new Listing<PrivateMessage>(Reddit, MessagesUrl);

        [Obsolete("Use Inbox property instead")]
        public Listing<PrivateMessage> GetInbox() => new Listing<PrivateMessage>(Reddit, InboxUrl);

        #endregion Obsolete Getter Methods

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
