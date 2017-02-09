using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    public class PrivateMessage : Thing
    {
        public PrivateMessage(Reddit reddit, JToken json) : base(reddit, json) {
            var data = json["data"];
            if (data["replies"] != null && data["replies"].Any())
            {
                if (data["replies"]["data"] != null)
                {
                    if (data["replies"]["data"]["children"] != null)
                    {
                        var replies = new List<PrivateMessage>();
                        foreach (var reply in data["replies"]["data"]["children"])
                            replies.Add(new PrivateMessage(Reddit, reply));
                        Replies = replies.ToArray();
                    }
                }
            }
        }

        private const string SetAsReadUrl = "/api/read_message";
        private const string CommentUrl = "/api/comment";

        /// <summary>
        /// Message body markdown.
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; }

        /// <summary>
        /// Message body html.
        /// </summary>
        [JsonProperty("body_html")]
        public string BodyHtml { get; }

        /// <summary>
        /// Returns true if is comment.
        /// </summary>
        [JsonProperty("was_comment")]
        public bool IsComment { get; }

        /// <summary>
        /// DateTime message was sent.
        /// </summary>
        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime Sent { get; }

        /// <summary>
        /// DateTime message was sent in UTC.
        /// </summary>
        [JsonProperty("created_utc")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime SentUTC { get; }

        /// <summary>
        /// Destination user or subreddit name.
        /// </summary>
        [JsonProperty("dest")]
        public string Destination { get; }

        /// <summary>
        /// Message author.
        /// </summary>
        [JsonProperty("author")]
        public string Author { get; }

        /// <summary>
        /// Subreddit (for comments).
        /// </summary>
        [JsonProperty("subreddit")]
        public string Subreddit { get; }

        /// <summary>
        /// Returns true if the message is unread.
        /// </summary>
        [JsonProperty("new")]
        public bool Unread { get; }

        /// <summary>
        /// Message subject.
        /// </summary>
        [JsonProperty("subject")]
        public string Subject { get; }

        /// <summary>
        /// Parent id.
        /// </summary>
        [JsonProperty("parent_id")]
        public string ParentID { get; }

        /// <summary>
        /// full name of the first message in this message chain.
        /// </summary>
        [JsonProperty("first_message_name")]
        public string FirstMessageName { get; }

        /// <summary>
        /// Replies to this message.
        /// </summary>
        [JsonIgnore]
        public PrivateMessage[] Replies { get; private set; }

        /// <summary>
        /// Original message
        /// </summary>
        [JsonIgnore]
        public PrivateMessage Parent
        {
            get
            {
                var listing = Thread;
                if (listing == null)
                  return null;
                var firstMessage = listing.First();
                if (firstMessage.FullName == ParentID)
                    return listing.First();
                else
                    return firstMessage.Replies.First(x => x.FullName == ParentID);
            }
        }

        /// <summary>
        /// The thread of messages
        /// </summary>
        public Listing<PrivateMessage> Thread
        {
            get
            {
                if (string.IsNullOrEmpty(ParentID))
                    return null;
                var id = ParentID.Remove(0, 3);
                return new Listing<PrivateMessage>(Reddit, $"/message/messages/{id}.json");
            }
        }
        // Awaitables don't have to be called asynchronously

        protected override JToken GetJsonData(JToken json) => json["data"];

        /// <summary>
        /// Mark the message read
        /// </summary>
        public async Task SetAsReadAsync()
        {
            await WebAgent.Post(SetAsReadUrl, new
            {
                id = FullName,
                uh = Reddit.User.Modhash,
                api_type = "json"
            });
        }

        /// <summary>
        /// Reply to the message
        /// </summary>
        /// <param name="message">Markdown text.</param>
        public async Task ReplyAsync(string message)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            await WebAgent.Post(CommentUrl, new
            {
                text = message,
                thing_id = FullName,
                uh = Reddit.User.Modhash
            });
        }
    }
}
