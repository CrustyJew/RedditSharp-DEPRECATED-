using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedditSharp.Things
{
    /// <summary>
    /// A comment.
    /// </summary>
    public class Comment : VotableThing
    {
        private const string CommentUrl = "/api/comment";
        private const string EditUserTextUrl = "/api/editusertext";
        private const string SetAsReadUrl = "/api/read_message";
        private const string SetAsUnReadUrl = "/api/unread_message";

        /// <inheritdoc />
        public Comment(IWebAgent agent, JToken json, Thing sender) : base(agent, json) {
            var data = json["data"];
            Parent = sender;

            // Handle Reddit's API being horrible
            if (data["context"] != null)
            {
                var context = data["context"].Value<string>();
                LinkId = context.Split('/')[4];
            }
            ParseComments(json, sender);
        }

        /// <inheritdoc />
        internal override JToken GetJsonData(JToken json) => json["data"];
        
        /// <summary>
        /// Prefix for fullname. Includes trailing underscore
        /// </summary>
        public static string KindPrefix { get { return "t1_"; } }

        /// <summary>
        /// Fill the object with comments.
        /// </summary>
        /// <param name="things"></param>
        /// <returns></returns>
        public Comment PopulateComments(IEnumerator<Thing> things)
        {
            Thing first = things.Current;
            Dictionary<string, Tuple<Comment, List<Comment>>> comments = new Dictionary<string, Tuple<Comment, List<Comment>>>
            {
                [this.FullName] = Tuple.Create<Comment, List<Comment>>(this, new List<Comment>())
            };
            while (things.MoveNext() && (first is Comment || first is More))
            {
                first = things.Current;
                if (first is Comment comment)
                {
                    comments[comment.FullName] = Tuple.Create<Comment, List<Comment>>(comment, new List<Comment>());
                    if (comments.ContainsKey(comment.ParentId))
                    {
                        comments[comment.ParentId].Item2.Add(comment);
                    }
                    else if (comment.ParentId == this.ParentId)
                    {
                        //only want sub comments.
                        break;
                    }
                }
                else if (first is More more)
                {
                    if (comments.ContainsKey(more.ParentId))
                    {
                        comments[more.ParentId].Item1.More = more;
                    }
                    else if (more.ParentId == this.ParentId)
                    {
                        // This is more for parent.
                        // Need to process the comments dictionary.
                        break;
                    }
                }
                //things.MoveNext();

            }

            foreach (KeyValuePair<string, Tuple<Comment, List<Comment>>> kvp in comments)
            {
                kvp.Value.Item1.Comments = kvp.Value.Item2.ToArray();
            }

            return this;
        }

        private void ParseComments(JToken data, Thing sender)
        {
            // Parse sub comments
            var replies = data["data"]["replies"];
            var subComments = new List<Comment>();
            if (replies != null && replies.Count() > 0)
            {
                foreach (var comment in replies["data"]["children"])
                    subComments.Add(new Comment(WebAgent, comment, sender));
            }
            Comments = subComments.ToArray();
        }

        /// <summary>
        /// Comment body markdown.
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; private set; }

        /// <summary>
        /// Comment body html.
        /// </summary>
        [JsonProperty("body_html")]
        public string BodyHtml { get; private set; }

        /// <summary>
        /// Id of the parent <see cref="VotableThing"/>.
        /// </summary>
        [JsonProperty("parent_id")]
        public string ParentId { get; private set; }

        /// <summary>
        /// Parent subreddit name.
        /// </summary>
        [JsonProperty("subreddit")]
        public string Subreddit { get; private set; }

        /// <summary>
        /// Link id.
        /// </summary>
        [JsonProperty("link_id")]
        public string LinkId { get; private set; }

        /// <summary>
        /// Parent link title.
        /// </summary>
        [JsonProperty("link_title")]
        public string LinkTitle { get; private set; }

        [JsonProperty("new")]
        public bool Unread { get; private set; }

        /// <summary>
        /// More comments.
        /// </summary>
        [JsonIgnore]
        public More More { get; private set; }

        /// <summary>
        /// Replies to this comment.
        /// </summary>
        [JsonIgnore]
        public IList<Comment> Comments { get; private set; }

        /// <summary>
        /// Parent <see cref="VotableThing"/>
        /// </summary>
        [JsonIgnore]
        public Thing Parent { get; internal set; }

        /// <inheritdoc/>
        public override string Shortlink => Permalink.ToString();

        /// <summary>
        /// Reply to this comment.
        /// </summary>
        /// <param name="message">markdown text of the reply.</param>
        /// <returns></returns>
        public async Task<Comment> ReplyAsync(string message)
        {
            // TODO actual error handling. This just hides the error and returns null
            //try
            //{
            var json = await WebAgent.Post(CommentUrl, new
            {
                text = message,
                thing_id = FullName,
                api_type = "json"
                //r = Subreddit
            }).ConfigureAwait(false);
            if (json["errors"].Any())
            {
                if (json["errors"][0].Any(x => x.ToString() == "RATELIMIT" || x.ToString() == "ratelimit"))
                {
                    var timeToReset = TimeSpan.FromMinutes(Convert.ToDouble(Regex.Match(json["errors"][0].ElementAt(1).ToString(), @"\d+").Value));
                    throw new RateLimitException(timeToReset);
                }
                else
                {
                    throw new Exception(json["errors"][0][0].ToString());
                }
            }
            return new Comment(WebAgent, json["data"]["things"][0], this);
            //}
            //catch (HttpRequestException ex)
            //{
            //    var error = new StreamReader(ex..GetResponseStream()).ReadToEnd();
            //    return null;
            //}
        }

        /// <summary>
        /// Replaces the text in this comment with the input text.
        /// </summary>
        /// <param name="newText">The text to replace the comment's contents</param>
        public async Task EditTextAsync(string newText)
        {
            var json = await WebAgent.Post(EditUserTextUrl, new
            {
                api_type = "json",
                text = newText,
                thing_id = FullName
            }).ConfigureAwait(false);
            if (!json["errors"].Any())
                Body = newText;
            else
                throw new Exception($"Errors editing text {json["errors"][0][0].ToString()}");
        }

        /// <inheritdoc />
        protected override async Task<JToken> SimpleActionAsync(string endpoint)
        {
            return await WebAgent.Post(endpoint, new
            {
                id = FullName
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

        #region Static Methods
        /// <summary>
        /// Post a reply to a specific comment
        /// </summary>
        /// <param name="webAgent"></param>
        /// <param name="commentFullName">e.g. "t1_12345"</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task Reply(IWebAgent webAgent, string commentFullName, string message) {
            // TODO actual error handling. This just hides the error and returns null
            //try
            //{
            var json = await webAgent.Post(CommentUrl, new {
                text = message,
                thing_id = commentFullName,
                api_type = "json"
                //r = Subreddit
            }).ConfigureAwait(false);
            if (json["json"]["ratelimit"] != null) {
                throw new RateLimitException(TimeSpan.FromSeconds(json["json"]["ratelimit"].ValueOrDefault<double>()));
            }
            

            //}
            //catch (HttpRequestException ex)
            //{
            //    var error = new StreamReader(ex..GetResponseStream()).ReadToEnd();
            //    return null;
            //}
        }
        #endregion

    }
}
