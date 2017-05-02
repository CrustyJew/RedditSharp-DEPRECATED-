using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using RedditSharp.Extensions;

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

        #pragma warning disable 1591
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
        #pragma warning restore 1591

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
        public override string Shortlink
        {
            get
            {
                // Not really a "short" link, but you can't actually use short links for comments
                string linkId = "";
                int index = this.LinkId.IndexOf('_');
                if (index > -1)
                {
                    linkId = this.LinkId.Substring(index + 1);
                }

                return string.Format("{0}://{1}/r/{2}/comments/{3}/_/{4}",
                                     RedditSharp.WebAgent.Protocol, RedditSharp.WebAgent.RootDomain,
                                     this.Subreddit, this.Parent != null ? this.Parent.Id : linkId, this.Id);
            }
        }

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
                if (json["json"]["ratelimit"] != null)
                    throw new RateLimitException(TimeSpan.FromSeconds(json["json"]["ratelimit"].ValueOrDefault<double>()));
                return new Comment(WebAgent, json["json"]["data"]["things"][0], this);
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
            if (json["json"].ToString().Contains("\"errors\": []"))
                Body = newText;
            else
                throw new Exception("Error editing text.");
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
            await WebAgent.Post(SetAsReadUrl, new
            {
                id = FullName,
                api_type = "json"
            }).ConfigureAwait(false);
        }
    }
}
