using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    /// <summary>
    /// A private message (or modmail).
    /// </summary>
    public class PrivateMessage : Thing
    {

        #region Properties
        private const string SetAsReadUrl = "/api/read_message";
        private const string CommentUrl = "/api/comment";

        private Listing<PrivateMessage> thread;

        /// <summary>
        /// Message body markdown.
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; private set; }

        /// <summary>
        /// Message body html.
        /// </summary>
        [JsonProperty("body_html")]
        public string BodyHtml { get; private set; }

        /// <summary>
        /// Returns true if is comment.
        /// </summary>
        [JsonProperty("was_comment")]
        public bool IsComment { get; private set; }

        /// <summary>
        /// DateTime message was sent.
        /// </summary>
        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime Sent { get; private set; }

        /// <summary>
        /// DateTime message was sent in UTC.
        /// </summary>
        [JsonProperty("created_utc")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime SentUTC { get; private set; }

        /// <summary>
        /// Destination user or subreddit name.
        /// </summary>
        [JsonProperty("dest")]
        public string Destination { get; private set; }

        /// <summary>
        /// Message author.
        /// </summary>
        [JsonProperty("author")]
        public string Author { get; private set; }

        /// <summary>
        /// Subreddit (for comments).
        /// </summary>
        [JsonProperty("subreddit")]
        public string Subreddit { get; private set; }

        /// <summary>
        /// Returns true if the message is unread.
        /// </summary>
        [JsonProperty("new")]
        public bool Unread { get; private set; }

        /// <summary>
        /// Message subject.
        /// </summary>
        [JsonProperty("subject")]
        public string Subject { get; private set; }

        /// <summary>
        /// Parent id.
        /// </summary>
        [JsonProperty("parent_id")]
        public string ParentID { get; private set; }

        /// <summary>
        /// Prefix for fullname. Includes trailing underscore
        /// </summary>
        public static string KindPrefix { get { return "t4_"; } }

        /// <summary>
        /// full name of the first message in this message chain.
        /// </summary>
        [JsonProperty("first_message_name")]
        public string FirstMessageName { get; private set; }

        /// <summary>
        /// Replies to this message.
        /// </summary>
        [JsonIgnore]
        public PrivateMessage[] Replies { get; private set; }

        #endregion Properties

#pragma warning disable 1591
        public PrivateMessage(IWebAgent agent, JToken json) : base(agent, json)
        {
            var data = json["data"];
            if (data["replies"] != null && data["replies"].Any())
            {
                if (data["replies"]["data"] != null)
                {
                    if (data["replies"]["data"]["children"] != null)
                    {
                        var replies = new List<PrivateMessage>();
                        foreach (var reply in data["replies"]["data"]["children"])
                            replies.Add(new PrivateMessage(WebAgent, reply));
                        Replies = replies.ToArray();
                    }
                }
            }
        }
#pragma warning restore 1591

        /// <summary>
        /// Get the Original message.
        /// </summary>
        public PrivateMessage GetParent()
        {
            if (this.thread == null)
            {
                this.thread = GetThread();
                if (this.thread == null)
                    return null;
            }
            //TODO: Convert this into an async function
            var firstPage = thread.First();
            firstPage.Wait();
            var firstMessage = firstPage.Result;
            if (firstMessage?.FullName == ParentID)
                return firstMessage;
            else
                return firstMessage.Replies.First(x => x.FullName == ParentID);
        }

        /// <summary>
        /// The thread of messages
        /// </summary>
        public Listing<PrivateMessage> GetThread()
        {
            if (string.IsNullOrEmpty(ParentID))
                return null;
            var id = ParentID.Remove(0, 3);
            this.thread = new Listing<PrivateMessage>(WebAgent, $"/message/messages/{id}.json");
            return this.thread;
        }
        // Awaitables don't have to be called asynchronously

        /// <inheritdoc />
        internal override JToken GetJsonData(JToken json) => json["data"];

        /// <summary>
        /// Mark the message read
        /// </summary>
        public async Task SetAsReadAsync()
        {
            await WebAgent.Post(SetAsReadUrl, new
            {
                id = FullName,
                api_type = "json"
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Reply to the message
        /// </summary>
        /// <param name="message">Markdown text.</param>
        public async Task ReplyAsync(string message)
        {
            await WebAgent.Post(CommentUrl, new
            {
                text = message,
                thing_id = FullName
            }).ConfigureAwait(false);
        }
    }
}
