using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using RedditSharp.Extensions;

namespace RedditSharp.Things
{
    public class Comment : VotableThing
    {
        private const string CommentUrl = "/api/comment";
        private const string EditUserTextUrl = "/api/editusertext";
        private const string SetAsReadUrl = "/api/read_message";

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="json"></param>
        /// <param name="webAgent"></param>
        /// <returns></returns>
        public async Task<Comment> InitAsync(Reddit reddit, JToken json, IWebAgent webAgent, Thing sender)
        {
            var data = await CommonInitAsync(reddit, json, webAgent, sender);
            await ParseCommentsAsync(reddit, json, webAgent, sender);
            JsonConvert.PopulateObject(data.ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="json"></param>
        /// <param name="webAgent"></param>
        /// <returns></returns>
        public Comment Init(Reddit reddit, JToken json, IWebAgent webAgent, Thing sender)
        {
            var data = CommonInit(reddit, json, webAgent, sender);
            ParseComments(reddit, json, webAgent, sender);
            JsonConvert.PopulateObject(data.ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

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
                [this.FullName] = Tuple.Create(this, new List<Comment>())
            };
            while (things.MoveNext() && (first is Comment || first is More))
            {
                first = things.Current;
                if (first is Comment)
                {
                    Comment comment = (Comment)first;
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
                else if (first is More)
                {
                    More more = (More)first;
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

        private JToken CommonInit(Reddit reddit, JToken json, IWebAgent webAgent, Thing sender)
        {
            Init(reddit, webAgent, json);
            var data = json["data"];
            Reddit = reddit;
            WebAgent = webAgent;
            Parent = sender;

            // Handle Reddit's API being horrible
            if (data["context"] != null)
            {
                var context = data["context"].Value<string>();
                LinkId = context.Split('/')[4];
            }

            return data;
        }
        private async Task<JToken> CommonInitAsync(Reddit reddit, JToken json, IWebAgent webAgent, Thing sender)
        {
            await InitAsync(reddit, webAgent, json);
            var data = json["data"];
            Reddit = reddit;
            WebAgent = webAgent;
            Parent = sender;

            // Handle Reddit's API being horrible
            if (data["context"] != null)
            {
                var context = data["context"].Value<string>();
                LinkId = context.Split('/')[4];
            }

            return data;
        }

        private void ParseComments(Reddit reddit, JToken data, IWebAgent webAgent, Thing sender)
        {
            // Parse sub comments
            var replies = data["data"]["replies"];
            var subComments = new List<Comment>();
            if (replies != null && replies.Count() > 0)
            {
                foreach (var comment in replies["data"]["children"])
                    subComments.Add(new Comment().Init(reddit, comment, webAgent, sender));
            }
            Comments = subComments.ToArray();
        }

        private async Task ParseCommentsAsync(Reddit reddit, JToken data, IWebAgent webAgent, Thing sender)
        {
            // Parse sub comments
            var replies = data["data"]["replies"];
            var subComments = new List<Comment>();
            if (replies != null && replies.Count() > 0)
            {
                foreach (var comment in replies["data"]["children"])
                    subComments.Add(await new Comment().InitAsync(reddit, comment, webAgent, sender));
            }
            Comments = subComments.ToArray();
        }

        /// <summary>
        /// Comment author user name.
        /// </summary>
        [JsonIgnore]
        [Obsolete("Use AuthorName instead.", false)]
        public string Author => base.AuthorName;

        /// <summary>
        /// Comment body markdown.
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; set; }

        /// <summary>
        /// Comment body html.
        /// </summary>
        [JsonProperty("body_html")]
        public string BodyHtml { get; set; }

        /// <summary>
        /// Id of the parent <see cref="VotableThing"/>.
        /// </summary>
        [JsonProperty("parent_id")]
        public string ParentId { get; set; }

        /// <summary>
        /// Parent subreddit name.
        /// </summary>
        [JsonProperty("subreddit")]
        public string Subreddit { get; set; }

        /// <summary>
        /// Link id.
        /// </summary>
        [JsonProperty("link_id")]
        public string LinkId { get; set; }

        /// <summary>
        /// Parent link title.
        /// </summary>
        [JsonProperty("link_title")]
        public string LinkTitle { get; set; }

        /// <summary>
        /// More comments.
        /// </summary>
        [JsonIgnore]
        public More More { get; set; }

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
            get { return Permalink; }
        }

        /// <summary>
        /// Reply to this comment.
        /// </summary>
        /// <param name="message">markdown text of the reply.</param>
        /// <returns></returns>
        public Comment Reply(string message)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = WebAgent.CreatePost(CommentUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                text = message,
                thing_id = FullName,
                uh = Reddit.User.Modhash,
                api_type = "json"
                //r = Subreddit
            });
            stream.Close();
            try
            {
                var response = request.GetResponse();
                var data = WebAgent.GetResponseString(response.GetResponseStream());
                var json = JObject.Parse(data);
                if (json["json"]["ratelimit"] != null)
                    throw new RateLimitException(TimeSpan.FromSeconds(json["json"]["ratelimit"].ValueOrDefault<double>()));
                return new Comment().Init(Reddit, json["json"]["data"]["things"][0], WebAgent, this);
            }
            catch (WebException ex)
            {
                var error = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                return null;
            }
        }

        /// <summary>
        /// Replaces the text in this comment with the input text.
        /// </summary>
        /// <param name="newText">The text to replace the comment's contents</param>        
        public void EditText(string newText)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");

            var request = WebAgent.CreatePost(EditUserTextUrl);
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                text = newText,
                thing_id = FullName,
                uh = Reddit.User.Modhash
            });
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
            JToken json = JToken.Parse(result);
            if (json["json"].ToString().Contains("\"errors\": []"))
                Body = newText;
            else
                throw new Exception("Error editing text. Error: " + json["json"]["errors"][0][0].ToString());
        }

        /// <summary>
        /// Mark this comment as read.
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
    }
}
