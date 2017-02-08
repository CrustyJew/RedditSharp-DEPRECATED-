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

        public Post(Reddit reddit, JToken json) : base(reddit, json) {
        }

        /// <summary>
        /// Author of this post.
        /// </summary>
        [JsonProperty("author")]
        public new string AuthorName { get; set; }

        //TODO Discuss
        public IObservable<Comment> Comments
        {
            get
            {
                return GetComments();
            }
        }

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
        /// Parent subkkeddit name.
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
                return Task.Run(async () => { return await Reddit.GetSubredditAsync("/r/" + SubredditName); }).Result;
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
        public async Task<Comment> CommentAsync(string message)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = WebAgent.CreatePost(CommentUrl);
            WebAgent.WritePostBody(request, new
            {
                text = message,
                thing_id = FullName,
                uh = Reddit.User.Modhash,
                api_type = "json"
            });
            var response = await WebAgent.GetResponseAsync(request);
            var data = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(data);
            if (json["json"]["ratelimit"] != null)
                throw new RateLimitException(TimeSpan.FromSeconds(json["json"]["ratelimit"].ValueOrDefault<double>()));
            return new Comment(Reddit, json["json"]["data"]["things"][0], this);
        }

        private async Task<string> SimpleActionToggleAsync(string endpoint, bool value, bool requiresModAction = false)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");

            var modNameList = (await this.Subreddit.GetModeratorsAsync()).Select(b => b.Name).ToList();

            if (requiresModAction && !modNameList.Contains(Reddit.User.Name))
                throw new AuthenticationException(
                    string.Format(
                        @"User {0} is not a moderator of subreddit {1}.",
                        Reddit.User.Name,
                        this.Subreddit.Name));

            var request = WebAgent.CreatePost(endpoint);
            WebAgent.WritePostBody(request, new
            {
                id = FullName,
                state = value,
                uh = Reddit.User.Modhash
            });
            var response = await WebAgent.GetResponseAsync(request);
            var data = await response.Content.ReadAsStringAsync();
            return data;
        }

        /// <summary>
        /// Hide this post.
        /// </summary>
        public Task HideAsync()
        {
            return SimpleActionAsync(HideUrl);
        }

        /// <summary>
        /// Unhide this post.
        /// </summary>
        public Task UnhideAsync()
        {
            return SimpleActionAsync(UnhideUrl);
        }

        /// <summary>
        /// Mark this post not safe for work.
        /// </summary>
        public Task MarkNSFWAsync()
        {
            return SimpleActionAsync(MarkNSFWUrl);
        }

        /// <summary>
        /// Mark this post as safe for work.
        /// </summary>
        public Task UnmarkNSFWAsync()
        {
            return SimpleActionAsync(UnmarkNSFWUrl);
        }

        /// <summary>
        /// Set contest mode state.  Logged in user must be a moderator of parent subreddit.
        /// </summary>
        /// <param name="state"></param>
        public Task ContestModeAsync(bool state)
        {
            return SimpleActionAsync(ContestModeUrl);
        }

        /// <summary>
        /// Set sticky state.  Logged in user must be a moderator of parent subreddit.
        /// </summary>
        /// <param name="state"></param>
        public Task StickyModeAsync(bool state)
        {
            return SimpleActionToggleAsync(StickyModeUrl, state, true);
        }

        /// <summary>
        /// Replaces the text in this post with the input text.
        /// </summary>
        /// <param name="newText">The text to replace the post's contents</param>
        public async Task EditTextAsync(string newText)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");
            if (!IsSelfPost)
                throw new Exception("Submission to edit is not a self-post.");

            var request = WebAgent.CreatePost(EditUserTextUrl);
            WebAgent.WritePostBody(request, new
            {
                api_type = "json",
                text = newText,
                thing_id = FullName,
                uh = Reddit.User.Modhash
            });
            var response = await WebAgent.GetResponseAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            JToken json = JToken.Parse(result);
            if (json["json"].ToString().Contains("\"errors\": []"))
                SelfText = newText;
            else
                throw new Exception("Error editing text.");
        }

        /// <summary>
        /// Update this post.
        /// </summary>
        public async Task UpdateAsync()
        {
            Reddit.PopulateObject(GetJsonData(await Reddit.GetTokenAsync(Url)), this);
        }

        /// <summary>
        /// Sets your claim
        /// </summary>
        /// <param name="flairText">Text to set your flair</param>
        /// <param name="flairClass">class of the flair</param>
        public async Task SetFlairAsync(string flairText, string flairClass)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");

            var request = WebAgent.CreatePost(string.Format(SetFlairUrl, SubredditName));
            WebAgent.WritePostBody(request, new
            {
                api_type = "json",
                css_class = flairClass,
                link = FullName,
                name = Reddit.User.Name,
                text = flairText,
                uh = Reddit.User.Modhash
            });
            var response = await WebAgent.GetResponseAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            var json = JToken.Parse(result);
            LinkFlairText = flairText;
        }

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of comments.
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<List<Comment>> ListCommentsAsync(int? limit = null)
        {
            var url = string.Format(GetCommentsUrl, Id);
            var request = WebAgent.CreateGet(url);

            if (limit.HasValue)
            {
                var query = WebUtility.UrlEncode("limit="+limit.Value.ToString());
                url = string.Format("{0}?{1}", url, query);
            }


            var response = await WebAgent.GetResponseAsync(request);
            var data = await response.Content.ReadAsStringAsync();
            var json = JArray.Parse(data);
            var postJson = json.Last()["data"]["children"];

            var comments = new List<Comment>();
            foreach (var comment in postJson)
            {
                Comment newComment = new Comment(Reddit, comment, this);
                if (newComment.Kind != "more")
                    comments.Add(newComment);
            }

            return comments;
        }

        //TODO discuss this
        public IEnumerable<Comment> EnumerateCommentsAsync() {
            return GetComments().ToEnumerable();
        }

        /// <summary>
        /// Enumerate more comments.
        /// </summary>
        /// <returns></returns>
        public IObservable<Comment> GetComments()
        {
            return Observable.Create<Comment>(async obs =>
            {


                var url = string.Format(GetCommentsUrl, Id);
                var request = WebAgent.CreateGet(url);
                var response = await WebAgent.GetResponseAsync(request);
                var data = await response.Content.ReadAsStringAsync();
                var json = JArray.Parse(data);
                var postJson = json.Last()["data"]["children"];
                More moreComments = null;
                foreach (var comment in postJson)
                {
                    Comment newComment = new Comment(Reddit, comment, this);
                    if (newComment.Kind == "more")
                    {
                        moreComments = new More(Reddit, comment);
                    }
                    else
                    {
                        obs.OnNext(newComment);
                    }
                }


                if (moreComments != null)
                {
                    IEnumerator<Thing> things = (await moreComments.GetThingsAsync()).GetEnumerator();
                    things.MoveNext();
                    Thing currentThing = null;
                    while (currentThing != things.Current)
                    {
                        currentThing = things.Current;
                        if (things.Current is Comment)
                        {
                            Comment next = ((Comment)things.Current).PopulateComments(things);
                            obs.OnNext(next);
                        }
                        if (things.Current is More)
                        {
                            More more = (More)things.Current;
                            if (more.ParentId != FullName) break;
                            things = (await more.GetThingsAsync()).GetEnumerator();
                            things.MoveNext();
                        }
                    }
                }

                obs.OnCompleted();
            });
        }
    }
}
