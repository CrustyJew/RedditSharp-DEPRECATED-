﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedditSharp.Things
{
    /// <summary>
    /// A private message (or modmail).
    /// </summary>
    public class PrivateMessage : ModeratableThing
    {

        #region Properties
        private const string SetAsUnReadUrl = "/api/unread_message";
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
        /// <inheritdoc />
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
            var firstPage = thread.FirstAsync();
            firstPage.GetAwaiter();
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

        /// <summary>
        /// Mark this comment as read.
        /// </summary>
        public async Task SetAsReadAsync()
        {
            await SetReadStatusAsync(SetAsReadUrl);
        }

        /// <summary>
        /// Mark this comment as unread.
        /// </summary>
        public async Task SetAsUnReadAsync()
        {
            await SetReadStatusAsync(SetAsUnReadUrl);
        }

        private async Task SetReadStatusAsync(string statusUrl)
        {
            await WebAgent.Post(statusUrl, new
            {
                id = FullName,
                api_type = "json"
            }).ConfigureAwait(false);
        }
    }
}
