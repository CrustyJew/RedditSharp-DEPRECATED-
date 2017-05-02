using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp.Extensions;
using System.Net;
using System.Reactive.Linq;
using System.Threading;

namespace RedditSharp.Things
{
    /// <summary>
    /// A post.
    /// </summary>
    public class Post : VotableThing
    {
        private const string CommentUrl = "/api/comment";
        private const string GetCommentsUrl = "/comments/{0}.json";
        private const string EditUserTextUrl = "/api/editusertext";
        private const string HideUrl = "/api/hide";
        private const string UnhideUrl = "/api/unhide";
        private string SetFlairUrl => $"/r/{SubredditName}/api/flair";
        private const string MarkNSFWUrl = "/api/marknsfw";
        private const string UnmarkNSFWUrl = "/api/unmarknsfw";
        private const string ContestModeUrl = "/api/set_contest_mode";
        private const string StickyModeUrl = "/api/set_subreddit_sticky";

        #pragma warning disable 1591
        public Post(IWebAgent agent, JToken json) : base(agent, json)
        {
        }
        #pragma warning restore 1591

        /// <summary>
        /// Author of this post.
        /// </summary>
        [JsonProperty("author")]
        public new string AuthorName { get; private set; }

        /// <summary>
        /// Domain of this post.
        /// </summary>
        [JsonProperty("domain")]
        public string Domain { get; private set; }

        /// <summary>
        /// Returns true if this is a self post.
        /// </summary>
        [JsonProperty("is_self")]
        public bool IsSelfPost { get; private set; }

        /// <summary>
        /// Css class of the link flair.
        /// </summary>
        [JsonProperty("link_flair_css_class")]
        public string LinkFlairCssClass { get; private set; }

        /// <summary>
        /// Text of the link flair.
        /// </summary>
        [JsonProperty("link_flair_text")]
        public string LinkFlairText { get; private set; }

        /// <summary>
        /// Number of comments on this post.
        /// </summary>
        [JsonProperty("num_comments")]
        public int CommentCount { get; private set; }

        /// <summary>
        /// Returns true if this post is marked not safe for work.
        /// </summary>
        [JsonProperty("over_18")]
        public bool NSFW { get; private set; }

        /// <summary>
        /// Post permalink.
        /// </summary>
        [JsonProperty("permalink")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Permalink { get; private set; }

        /// <summary>
        /// Post self text markdown.
        /// </summary>
        [JsonProperty("selftext")]
        public string SelfText { get; private set; }

        /// <summary>
        /// Post self text html.
        /// </summary>
        [JsonProperty("selftext_html")]
        public string SelfTextHtml { get; private set; }

        /// <summary>
        /// Uri to the thumbnail image of this post.
        /// </summary>
        [JsonProperty("thumbnail")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Thumbnail { get; private set; }

        /// <summary>
        /// Post title.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; private set; }

        /// <summary>
        /// Parent subreddit name.
        /// </summary>
        [JsonProperty("subreddit")]
        public string SubredditName { get; private set; }
        

        /// <summary>
        /// Prefix for fullname. Includes trailing underscore
        /// </summary>
        public static string KindPrefix { get { return "t3_"; } }

        /// <summary>
        /// Post uri.
        /// </summary>
        [JsonProperty("url")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Url { get; private set; }

        /// <summary>
        /// Returns the parent <see cref="Subreddit"/> for this post
        /// </summary>
        /// <returns></returns>
        public Task<Subreddit> GetSubredditAsync()
        {
            return Subreddit.GetByNameAsync(WebAgent, SubredditName);
        }

        /// <summary>
        /// Comment on this post.
        /// </summary>
        /// <param name="message">Markdown text.</param>
        /// <returns></returns>
        public async Task<Comment> CommentAsync(string message)
        {
            var json = await WebAgent.Post(CommentUrl, new
            {
                text = message,
                thing_id = FullName,
                api_type = "json"
            }).ConfigureAwait(false);
            if (json["json"]["ratelimit"] != null)
                throw new RateLimitException(TimeSpan.FromSeconds(json["json"]["ratelimit"].ValueOrDefault<double>()));
            return new Comment(WebAgent, json["json"]["data"]["things"][0], this);
        }

        private async Task<JToken> SimpleActionToggleAsync(string endpoint, bool value, bool requiresModAction = false)
        {
             return await WebAgent.Post(endpoint, new
            {
                id = FullName,
                state = value
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Hide this post.
        /// </summary>
        public Task HideAsync() => SimpleActionAsync(HideUrl);

        /// <summary>
        /// Unhide this post.
        /// </summary>
        public Task UnhideAsync() => SimpleActionAsync(UnhideUrl);

        /// <summary>
        /// Mark this post not safe for work.
        /// </summary>
        public Task MarkNSFWAsync() => SimpleActionAsync(MarkNSFWUrl);

        /// <summary>
        /// Mark this post as safe for work.
        /// </summary>
        public Task UnmarkNSFWAsync() => SimpleActionAsync(UnmarkNSFWUrl);

        /// <summary>
        /// Set contest mode state.  Logged in user must be a moderator of parent subreddit.
        /// </summary>
        /// <param name="state"></param>
        public Task ContestModeAsync(bool state) => SimpleActionAsync(ContestModeUrl);

        /// <summary>
        /// Set sticky state.  Logged in user must be a moderator of parent subreddit.
        /// </summary>
        /// <param name="state"></param>
        public Task StickyModeAsync(bool state) => SimpleActionToggleAsync(StickyModeUrl, state, true);

        /// <summary>
        /// Replaces the text in this post with the input text.
        /// </summary>
        /// <param name="newText">The text to replace the post's contents</param>
        public async Task EditTextAsync(string newText)
        {
            if (!IsSelfPost)
                throw new Exception("Submission to edit is not a self-post.");

            var json = await WebAgent.Post(EditUserTextUrl, new
            {
                api_type = "json",
                text = newText,
                thing_id = FullName
            }).ConfigureAwait(false);
            if (json["json"].ToString().Contains("\"errors\": []"))
                SelfText = newText;
            else
                throw new Exception("Error editing text.");
        }

        /// <summary>
        /// Updates data retrieved for this post.
        /// </summary>
        public async Task UpdateAsync() => Helpers.PopulateObject(GetJsonData(await Helpers.GetTokenAsync(WebAgent,Url)), this);

        /// <summary>
        /// Sets your flair
        /// </summary>
        /// <param name="flairText">Text to set your flair</param>
        /// <param name="flairClass">class of the flair</param>
        public async Task SetFlairAsync(string flairText, string flairClass)
        {
            //TODO Unit test
            await WebAgent.Post(SetFlairUrl, new
            {
                api_type = "json",
                css_class = flairClass,
                link = FullName,
                text = flairText
            }).ConfigureAwait(false);
            LinkFlairText = flairText;
        }

        /// <summary>
        /// Get a <see cref="List{T}"/> of comments.
        /// </summary>
        /// <param name="limit">Maximum number of comments to return</param>
        /// <returns></returns>
        public async Task<List<Comment>> GetCommentsAsync(int limit = 0)
        {
            var url = string.Format(GetCommentsUrl, Id);
            if (limit > 0)
            {
                var query = "limit=" + limit;
                url = string.Format("{0}?{1}", url, query);
            }
            var json = await WebAgent.Get(url).ConfigureAwait(false);
            var postJson = json.Last()["data"]["children"];

            var comments = new List<Comment>();
            foreach (var comment in postJson)
            {
                Comment newComment = new Comment(WebAgent, comment, this);
                if (newComment.Kind != "more")
                    comments.Add(newComment);
            }

            return comments;
        }

        /// <summary>
        /// Returns a <see cref="List{T}"/> of <see cref="Thing"/> that contains <see cref="Comment"/> and <see cref="More"/>
        /// </summary>
        /// <param name="limit">Maximum number of comments to return. Returned list may be larger than this number though due to <see cref="More"/></param>
        /// <returns></returns>
        public async Task<List<Thing>> GetCommentsWithMoresAsync(int limit = 0)
        {
            var url = string.Format(GetCommentsUrl, Id);
            if (limit > 0)
            {
                var query = "limit=" + limit;
                url = string.Format("{0}?{1}", url, query);
            }
            var json = await WebAgent.Get(url).ConfigureAwait(false);
            var postJson = json.Last()["data"]["children"];

            var things = new List<Thing>();
            foreach (var comment in postJson)
            {
                Comment newComment = new Comment(WebAgent, comment, this);
                if (newComment.Kind != "more")
                {
                    things.Add(newComment);
                }
                else 
                {
                    things.Add(new More(WebAgent, comment));
                }
            }

            return things;

        }
        /// <summary>
        /// Returns an <see cref="IAsyncEnumerable{T}"/> of <see cref="Comment"/> containing all comments in a post.
        /// This will cause multiple web requests on larger comment sections.
        /// </summary>
        /// <param name="limitPerRequest">Maximum number of comments to retrieve at a time. 0 for Reddit maximum</param>
        /// <returns></returns>
        public IAsyncEnumerable<Comment> EnumerateCommentTreeAsync(int limitPerRequest = 0)
        {
            return new CommentsEnumarable(WebAgent, this, limitPerRequest);
        }
        
    }

}

