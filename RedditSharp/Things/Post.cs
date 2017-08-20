using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp.Extensions;

namespace RedditSharp.Things
{
    public class Post : VotableThing
    {
        private const string CommentUrl = "/api/comment";
        private const string GetCommentsUrl = "/comments/{0}.json";
        private const string EditUserTextUrl = "/api/editusertext";
        private const string HideUrl = "/api/hide";
        private const string UnhideUrl = "/api/unhide";
        private const string SetFlairUrl = "/r/{0}/api/flair";
        private const string MarkNSFWUrl = "/api/marknsfw";
        private const string UnmarkNSFWUrl = "/api/unmarknsfw";
        private const string ContestModeUrl = "/api/set_contest_mode";
        private const string StickyModeUrl = "/api/set_subreddit_sticky";
        private const string SpoilerUrl = "/api/spoiler";
        private const string UnSpoilerUrl = "/api/unspoiler";
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="post"></param>
        /// <param name="webAgent"></param>
        /// <returns></returns>
        public async Task<Post> InitAsync(Reddit reddit, JToken post, IWebAgent webAgent)
        {
            await CommonInitAsync(reddit, post, webAgent);
            JsonConvert.PopulateObject(post["data"].ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="post"></param>
        /// <param name="webAgent"></param>
        /// <returns></returns>
        public Post Init(Reddit reddit, JToken post, IWebAgent webAgent)
        {
            CommonInit(reddit, post, webAgent);
            JsonConvert.PopulateObject(post["data"].ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

        private void CommonInit(Reddit reddit, JToken post, IWebAgent webAgent)
        {
            base.Init(reddit, webAgent, post);
            Reddit = reddit;
            WebAgent = webAgent;
        }

        private async Task CommonInitAsync(Reddit reddit, JToken post, IWebAgent webAgent)
        {
            await base.InitAsync(reddit, webAgent, post);
            Reddit = reddit;
            WebAgent = webAgent;
        }

        /// <summary>
        /// Author of this post.
        /// </summary>
        [JsonIgnore]
        public RedditUser Author
        {
            get
            {
                return Reddit.GetUser(AuthorName);
            }
        }

        /// <summary>
        /// An array of comments on this post.
        /// </summary>
        public Comment[] Comments
        {
            get
            {
                return ListComments().ToArray();
            }
        }
        /// <summary>
        /// Returns true if post is marekd as spoiler
        /// </summary>
        [JsonProperty("spoiler")]
        public bool IsSpoiler { get; set; }
        /// <summary>
        /// Domain of this post.
        /// </summary>
        [JsonProperty("domain")]
        public string Domain { get; set; }

        /// <summary>
        /// Returns true if this is a self post.
        /// </summary>
        [JsonProperty("is_self")]
        public bool IsSelfPost { get; set; }

        /// <summary>
        /// Css class of the link flair.
        /// </summary>
        [JsonProperty("link_flair_css_class")]
        public string LinkFlairCssClass { get; set; }

        /// <summary>
        /// Text of the link flair.
        /// </summary>
        [JsonProperty("link_flair_text")]
        public string LinkFlairText { get; set; }

        /// <summary>
        /// Number of comments on this post.
        /// </summary>
        [JsonProperty("num_comments")]
        public int CommentCount { get; set; }

        /// <summary>
        /// Returns true if this post is marked not safe for work.
        /// </summary>
        [JsonProperty("over_18")]
        public bool NSFW { get; set; }

        /// <summary>
        /// Post permalink.
        /// </summary>
        [JsonProperty("permalink")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Permalink { get; set; }

        /// <summary>
        /// Post self text markdown.
        /// </summary>
        [JsonProperty("selftext")]
        public string SelfText { get; set; }

        /// <summary>
        /// Post self text html.
        /// </summary>
        [JsonProperty("selftext_html")]
        public string SelfTextHtml { get; set; }

        /// <summary>
        /// Uri to the thumbnail image of this post.
        /// </summary>
        [JsonProperty("thumbnail")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Thumbnail { get; set; }

        /// <summary>
        /// Post title.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Parent subreddit name.
        /// </summary>
        [JsonProperty("subreddit")]
        public string SubredditName { get; set; }

        /// <summary>
        /// Parent subreddit.
        /// </summary>
        [JsonIgnore]
        public Subreddit Subreddit
        {
            get
            {
                return Reddit.GetSubreddit("/r/" + SubredditName);
            }
        }

        /// <summary>
        /// Post uri.
        /// </summary>
        [JsonProperty("url")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Url { get; set; }

        /// <summary>
        /// Comment on this post.
        /// </summary>
        /// <param name="message">Markdown text.</param>
        /// <returns></returns>
        public Comment Comment(string message)
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
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(data);
            if (json["json"]["ratelimit"] != null)
                throw new RateLimitException(TimeSpan.FromSeconds(json["json"]["ratelimit"].ValueOrDefault<double>()));
            return new Comment().Init(Reddit, json["json"]["data"]["things"][0], WebAgent, this);
        }
        /// <summary>
        /// Marks post as spoiler
        /// </summary>
        public void Spoiler()
        {
            var data = SimpleAction(SpoilerUrl);
        }
        /// <summary>
        /// Unmarks a post as being a spoiler
        /// </summary>
        public void UnSpoiler()
        {
            var data = SimpleAction(UnSpoilerUrl);
        }

        private string SimpleActionToggle(string endpoint, bool value, bool requiresModAction = false)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");

            var modNameList = this.Subreddit.Moderators.Select(b => b.Name).ToList();

            if (requiresModAction && !modNameList.Contains(Reddit.User.Name))
                throw new AuthenticationException(
                    string.Format(
                        @"User {0} is not a moderator of subreddit {1}.",
                        Reddit.User.Name,
                        this.Subreddit.Name));

            var request = WebAgent.CreatePost(endpoint);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                id = FullName,
                state = value,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            return data;
        }

        /// <summary>
        /// Hide this post.
        /// </summary>
        public void Hide()
        {
            var data = SimpleAction(HideUrl);
        }

        /// <summary>
        /// Unhide this post.
        /// </summary>
        public void Unhide()
        {
            var data = SimpleAction(UnhideUrl);
        }

        /// <summary>
        /// Mark this post not safe for work.
        /// </summary>
        public void MarkNSFW()
        {
            var data = SimpleAction(MarkNSFWUrl);
        }

        /// <summary>
        /// Unmark this post not safe for work.
        /// </summary>
        public void UnmarkNSFW()
        {
            var data = SimpleAction(UnmarkNSFWUrl);
        }

        /// <summary>
        /// Set contest mode state.  Logged in user must be a moderator of parent subreddit.
        /// </summary>
        /// <param name="state"></param>
        public void ContestMode(bool state)
        {
            var data = SimpleActionToggle(ContestModeUrl, state);
        }

        /// <summary>
        /// Set sticky state.  Logged in user must be a moderator of parent subreddit.
        /// </summary>
        /// <param name="state"></param>
        public void StickyMode(bool state)
        {
            var data = SimpleActionToggle(StickyModeUrl, state, true);
        }

        #region Obsolete Getter Methods

        [Obsolete("Use Comments property instead")]
        public Comment[] GetComments()
        {
            return Comments;
        }

        #endregion Obsolete Getter Methods

        /// <summary>
        /// Replaces the text in this post with the input text.
        /// </summary>
        /// <param name="newText">The text to replace the post's contents</param>
        public void EditText(string newText)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");
            if (!IsSelfPost)
                throw new Exception("Submission to edit is not a self-post.");

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
                SelfText = newText;
            else
                throw new Exception("Error editing text. Error: " + json["json"]["errors"][0][0].ToString());
        }

        /// <summary>
        /// Update this post.
        /// </summary>
        public void Update()
        {
            JToken post = Reddit.GetToken(this.Url);
            JsonConvert.PopulateObject(post["data"].ToString(), this, Reddit.JsonSerializerSettings);
        }
        /// <summary>
        /// Sets your claim
        /// </summary>
        /// <param name="flairText">Text to set your flair</param>
        /// <param name="flairClass">class of the flair</param>
        public void SetFlair(string flairText, string flairClass)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");

            var request = WebAgent.CreatePost(string.Format(SetFlairUrl, SubredditName));
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                css_class = flairClass,
                link = FullName,
                name = Reddit.User.Name,
                text = flairText,
                uh = Reddit.User.Modhash
            });
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(result);
            LinkFlairText = flairText;
        }

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of comments.
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public List<Comment> ListComments(int? limit = null)
        {
            var url = string.Format(GetCommentsUrl, Id);

            if (limit.HasValue)
            {
                var query = HttpUtility.ParseQueryString(string.Empty);
                query.Add("limit", limit.Value.ToString());
                url = string.Format("{0}?{1}", url, query);
            }

            var request = WebAgent.CreateGet(url);
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JArray.Parse(data);
            var postJson = json.Last()["data"]["children"];

            var comments = new List<Comment>();
            foreach (var comment in postJson)
            {
                Comment newComment = new Comment().Init(Reddit, comment, WebAgent, this);
                if (newComment.Kind == "more")
                {
                }
                else
                {
                    comments.Add(newComment);
                }
            }

            return comments;
        }

        /// <summary>
        /// Enumerate more comments.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Comment> EnumerateComments()
        {
            var url = string.Format(GetCommentsUrl, Id);
            var request = WebAgent.CreateGet(url);
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JArray.Parse(data);
            var postJson = json.Last()["data"]["children"];
            More moreComments = null;
            foreach (var comment in postJson)
            {
                Comment newComment = new Comment().Init(Reddit, comment, WebAgent, this);
                if (newComment.Kind == "more")
                {
                    moreComments = new More().Init(Reddit, comment, WebAgent);
                }
                else
                {
                    yield return newComment;
                }
            }


            if (moreComments != null)
            {
                IEnumerator<Thing> things = moreComments.Things().GetEnumerator();
                things.MoveNext();
                Thing currentThing = null;
                while (currentThing != things.Current)
                {
                    currentThing = things.Current;
                    if (things.Current is Comment)
                    {
                        Comment next = ((Comment)things.Current).PopulateComments(things);
                        yield return next;
                    }
                    if (things.Current is More)
                    {
                        More more = (More)things.Current;
                        if (more.ParentId != FullName) break;
                        things = more.Things().GetEnumerator();
                        things.MoveNext();
                    }
                }
            }
        }
    }
}
