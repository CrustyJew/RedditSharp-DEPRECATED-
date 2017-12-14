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
        private const string SetAsReadUrl = "/api/read_message";
        private const string CommentUrl = "/api/comment";


        /// <summary>
        /// Message body markdown.
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; set; }

        /// <summary>
        /// Message body html.
        /// </summary>
        [JsonProperty("body_html")]
        public string BodyHtml { get; set; }

        /// <summary>
        /// Returns true if is comment.
        /// </summary>
        [JsonProperty("was_comment")]
        public bool IsComment { get; set; }

        /// <summary>
        /// Date and time message was sent.
        /// </summary>
        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTimeOffset Sent { get; set; }

        /// <summary>
        /// Date and time message was sent in UTC.
        /// </summary>
        [JsonProperty("created_utc")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTimeOffset SentUTC { get; set; }

        /// <summary>
        /// Destination user or subreddit name.
        /// </summary>
        [JsonProperty("dest")]
        public string Destination { get; set; }

        /// <summary>
        /// Message author.
        /// </summary>
        [JsonProperty("author")]
        public string Author { get; set; }

        /// <summary>
        /// Subreddit (for comments).
        /// </summary>
        [JsonProperty("subreddit")]
        public string Subreddit { get; set; }

        /// <summary>
        /// Returns true if the message is unread.
        /// </summary>
        [JsonProperty("new")]
        public bool Unread { get; set; }

        /// <summary>
        /// Message subject.
        /// </summary>
        [JsonProperty("subject")]
        public string Subject { get; set; }

        /// <summary>
        /// Parent id.
        /// </summary>
        [JsonProperty("parent_id")]
        public string ParentID { get; set; }

        /// <summary>
        /// full name of the first message in this message chain.
        /// </summary>
        [JsonProperty("first_message_name")]
        public string FirstMessageName { get; set; }

        /// <summary>
        /// Replies to this message.
        /// </summary>
        [JsonIgnore]
        public PrivateMessage[] Replies { get; set; }

        /// <summary>
        /// Original message
        /// </summary>
        [JsonIgnore]
        public PrivateMessage Parent
        {
            get
            {
                if (string.IsNullOrEmpty(ParentID))
                    return null;
                var id = ParentID.Remove(0, 3);
                var listing = new Listing<PrivateMessage>(Reddit, "/message/messages/" + id + ".json", WebAgent);
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
                return new Listing<PrivateMessage>(Reddit, "/message/messages/" + id + ".json", WebAgent);
            }
        }
        // Awaitables don't have to be called asynchronously

        /// <summary>
        /// initializes trying to get the messages
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="json"></param>
        /// <param name="webAgent"></param>
        /// <returns>A private message</returns>
        public async Task<PrivateMessage> InitAsync(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(reddit, json, webAgent);
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

        /// <summary>
        /// initializes trying to get the messages
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="json"></param>
        /// <param name="webAgent"></param>
        /// <returns>A private message</returns>
        public PrivateMessage Init(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(reddit, json, webAgent);
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

        private void CommonInit(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            base.Init(json);
            Reddit = reddit;
            WebAgent = webAgent;
            var data = json["data"];
            if (data["replies"] != null && data["replies"].Any())
            {
                if (data["replies"]["data"] != null)
                {
                    if (data["replies"]["data"]["children"] != null)
                    {
                        var replies = new List<PrivateMessage>();
                        foreach (var reply in data["replies"]["data"]["children"])
                            replies.Add(new PrivateMessage().Init(reddit, reply, webAgent));
                        Replies = replies.ToArray();
                    }
                }
            }
        }

        #region Obsolete Getter Methods

        [Obsolete("Use Thread property instead")]
        public Listing<PrivateMessage> GetThread()
        {
            return Thread;
        }

        #endregion Obsolete Gettter Methods

        /// <summary>
        /// Mark the message read
        /// </summary>
        public void SetAsRead()
        {
            var request = WebAgent.CreatePost(SetAsReadUrl);
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                id = FullName,
                uh = Reddit.User.Modhash,
                api_type = "json"
            });
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
        }

        /// <summary>
        /// Mark the message as read.
        /// </summary>
        /// <returns></returns>
        public async Task SetAsReadAsync()
        {
            var request = WebAgent.CreatePost(SetAsReadUrl);
            WebAgent.WritePostBody(await request.GetRequestStreamAsync(), new
            {
                id = FullName,
                uh = Reddit.User.Modhash,
                api_type = "json"
            });
            var response = await request.GetResponseAsync();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
        }

        /// <summary>
        /// Reply to the message
        /// </summary>
        /// <param name="message">Markdown text.</param>
        public void Reply(string message)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = WebAgent.CreatePost(CommentUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                text = message,
                thing_id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(data);
        }

        /// <summary>
        /// Reply to the message async.
        /// </summary>
        /// <param name="message">Markdown text</param>
        /// <returns></returns>
        public async Task ReplyAsync(string message)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = WebAgent.CreatePost(CommentUrl);
            var stream = await request.GetRequestStreamAsync();
            WebAgent.WritePostBody(stream, new
            {
                text = message,
                thing_id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = await request.GetResponseAsync();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(data);
        }
    }
}
