using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    public class AuthenticatedUser : RedditUser
    {
        private const string ModeratorUrl = "/reddits/mine/moderator.json";
        private const string UnreadMessagesUrl = "/message/unread.json?mark=true&limit=25";
        private const string ModQueueUrl = "/r/mod/about/modqueue.json";
        private const string UnmoderatedUrl = "/r/mod/about/unmoderated.json";
        private const string ModMailUrl = "/message/moderator.json";
        private const string MessagesUrl = "/message/messages.json";
        private const string InboxUrl = "/message/inbox.json";
        private const string SentUrl = "/message/sent.json";

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="json"></param>
        /// <param name="webAgent"></param>
        /// <returns></returns>
        public new async Task<AuthenticatedUser> InitAsync(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            await CommonInitAsync(reddit, json, webAgent);
            JsonConvert.PopulateObject(json["name"] == null ? json["data"].ToString() : json.ToString(), this,
                reddit.JsonSerializerSettings);
            return this;
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="json"></param>
        /// <param name="webAgent"></param>
        /// <returns></returns>
        public new AuthenticatedUser Init(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(reddit, json, webAgent);
            JsonConvert.PopulateObject(json["name"] == null ? json["data"].ToString() : json.ToString(), this,
                reddit.JsonSerializerSettings);
            return this;
        }

        private void CommonInit(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            base.Init(reddit, json, webAgent);
        }

        private async Task CommonInitAsync(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            await base.InitAsync(reddit, json, webAgent).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of subreddits moderated by the logged in user.
        /// </summary>
        public Listing<Subreddit> ModeratorSubreddits
        {
            get
            {
                return new Listing<Subreddit>(Reddit, ModeratorUrl, WebAgent);
            }
        }

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of unread messages.
        /// </summary>
        public Listing<Thing> UnreadMessages
        {
            get
            {
                return new Listing<Thing>(Reddit, UnreadMessagesUrl, WebAgent);
            }
        }

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of items in the Moderation Queue.
        /// </summary>
        public Listing<VotableThing> ModerationQueue
        {
            get
            {
                return new Listing<VotableThing>(Reddit, ModQueueUrl, WebAgent);
            }
        }

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of unmoderated Posts.
        /// </summary>
        public Listing<Post> UnmoderatedLinks
        {
            get
            {
                return new Listing<Post>(Reddit, UnmoderatedUrl, WebAgent);
            }
        }

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of (old style) modmail.
        /// </summary>
        public Listing<PrivateMessage> ModMail
        {
            get
            {
                return new Listing<PrivateMessage>(Reddit, ModMailUrl, WebAgent);
            }
        }

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of private messages.
        /// </summary>
        public Listing<PrivateMessage> PrivateMessages
        {
            get
            {
                return new Listing<PrivateMessage>(Reddit, MessagesUrl, WebAgent);
            }
        }

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of messages in the inbox.
        /// </summary>
        public Listing<Thing> Inbox
        {
            get
            {
                return new Listing<Thing>(Reddit, InboxUrl, WebAgent);
            }
        }

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of sent messages.
        /// </summary>
        public Listing<PrivateMessage> Sent
        {
            get
            {
                return new Listing<PrivateMessage>(Reddit, SentUrl, WebAgent);
            }
        }

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of unmoderated links.
        /// </summary>
        public Listing<Post> GetUnmoderatedLinks()
        {
            return new Listing<Post>(Reddit, UnmoderatedUrl, WebAgent);
        }

        #region Obsolete Getter Methods

        [Obsolete("Use ModeratorSubreddits property instead")]
        public Listing<Subreddit> GetModeratorReddits()
        {
            return ModeratorSubreddits;
        }

        [Obsolete("Use UnreadMessages property instead")]
        public Listing<Thing> GetUnreadMessages()
        {
            return UnreadMessages;
        }

        [Obsolete("Use ModerationQueue property instead")]
        public Listing<VotableThing> GetModerationQueue()
        {
            return new Listing<VotableThing>(Reddit, ModQueueUrl, WebAgent);
        }

        [Obsolete("Use ModMail property instead")]
        public Listing<PrivateMessage> GetModMail()
        {
            return new Listing<PrivateMessage>(Reddit, ModMailUrl, WebAgent);
        }

        [Obsolete("Use PrivateMessages property instead")]
        public Listing<PrivateMessage> GetPrivateMessages()
        {
            return new Listing<PrivateMessage>(Reddit, MessagesUrl, WebAgent);
        }

        [Obsolete("Use Inbox property instead")]
        public Listing<PrivateMessage> GetInbox()
        {
            return new Listing<PrivateMessage>(Reddit, InboxUrl, WebAgent);
        }

        #endregion Obsolete Getter Methods

        /// <summary>
        /// User modhash.
        /// <para>A modhash is a token that the reddit API requires to help prevent CSRF. Modhashes can be 
        /// obtained via the /api/me.json call or in response data of listing endpoints.  The preferred way
        /// to send a modhash is to include an X-Modhash custom HTTP header with your requests.</para>
        ///<para>Modhashes are not required when authenticated with OAuth.</para>
        /// </summary>
        [JsonProperty("modhash")]
        public string Modhash { get; set; }

        /// <summary>
        /// Returns true of the user has mail.
        /// </summary>
        [JsonProperty("has_mail")]
        public bool HasMail { get; set; }

        /// <summary>
        /// Returns true of the user has modmail.
        /// </summary>
        [JsonProperty("has_mod_mail")]
        public bool HasModMail { get; set; }
    }
}
