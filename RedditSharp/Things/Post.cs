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
        private const string RemoveUrl = "/api/remove";
        private const string DelUrl = "/api/del";
        private const string GetCommentsUrl = "/comments/{0}.json";
        private const string ApproveUrl = "/api/approve";
        private const string EditUserTextUrl = "/api/editusertext";
        private const string HideUrl = "/api/hide";
        private const string UnhideUrl = "/api/unhide";
        private const string SetFlairUrl = "/r/{0}/api/flair";
        private const string MarkNSFWUrl = "/api/marknsfw";
        private const string UnmarkNSFWUrl = "/api/unmarknsfw";
        private const string ContestModeUrl = "/api/set_contest_mode";
        private const string StickyModeUrl = "/api/set_subreddit_sticky";
        private const string IgnoreReportsUrl = "/api/ignore_reports";
        private const string UnIgnoreReportsUrl = "/api/unignore_reports";

        [JsonIgnore]
        private Reddit Reddit { get; set; }

        [JsonIgnore]
        private IWebAgent WebAgent { get; set; }
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
            await Task.Factory.StartNew(() => JsonConvert.PopulateObject(post["data"].ToString(), this, reddit.JsonSerializerSettings));
            return this;
        }
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

        [JsonProperty("author")]
        public string AuthorName { get; set; }

        
        //TODO Discuss
        public IObservable<Comment> Comments
        {
            get
            {
                return GetComments();
            }
        }

        [JsonProperty("approved_by")]
        public string ApprovedBy { get; set; }

        [JsonProperty("author_flair_css_class")]
        public string AuthorFlairCssClass { get; set; }

        [JsonProperty("author_flair_text")]
        public string AuthorFlairText { get; set; }

        [JsonProperty("banned_by")]
        public string BannedBy { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("edited")]
        public bool Edited { get; set; }

        [JsonProperty("is_self")]
        public bool IsSelfPost { get; set; }

        [JsonProperty("link_flair_css_class")]
        public string LinkFlairCssClass { get; set; }

        [JsonProperty("link_flair_text")]
        public string LinkFlairText { get; set; }

        [JsonProperty("num_comments")]
        public int CommentCount { get; set; }

        [JsonProperty("over_18")]
        public bool NSFW { get; set; }

        [JsonProperty("permalink")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Permalink { get; set; }

        [JsonProperty("selftext")]
        public string SelfText { get; set; }

        [JsonProperty("selftext_html")]
        public string SelfTextHtml { get; set; }

        [JsonProperty("thumbnail")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Thumbnail { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("subreddit")]
        public string SubredditName { get; set; }

        [JsonProperty("archived")]
        public bool IsArchived { get; set; }

        [JsonProperty("stickied")]
        public bool IsStickied { get; set; }

        [JsonIgnore]
        public Subreddit Subreddit
        {
            get
            {
                return Task.Run(async () => { return await Reddit.GetSubredditAsync("/r/" + SubredditName); }).Result;
            }
        }

        [JsonProperty("url")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Url { get; set; }

        [JsonProperty("num_reports")]
        public int? Reports { get; set; }

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
            return new Comment().Init(Reddit, json["json"]["data"]["things"][0], WebAgent, this);
        }

        private async Task<string> SimpleActionAsync(string endpoint)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = WebAgent.CreatePost(endpoint);
           
            WebAgent.WritePostBody(request, new
            {
                id = FullName,
                uh = Reddit.User.Modhash
            });
            var response = await WebAgent.GetResponseAsync(request);
            var data = await response.Content.ReadAsStringAsync();
            return data;
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

        public Task ApproveAsync()
        {
            return SimpleActionAsync(ApproveUrl);
        }

        public Task RemoveAsync()
        {
            return RemoveImplAsync(false);
        }

        public Task RemoveSpamAsync()
        {
            return RemoveImplAsync(true);
        }

        private async Task RemoveImplAsync(bool spam)
        {
            var request = WebAgent.CreatePost(RemoveUrl);
            WebAgent.WritePostBody(request, new
            {
                id = FullName,
                spam = spam,
                uh = Reddit.User.Modhash
            });
            var response = await WebAgent.GetResponseAsync(request);
            var data = await response.Content.ReadAsStringAsync();
        }

        public Task DelAsync()
        {
            return SimpleActionAsync(DelUrl);
        }

        public Task HideAsync()
        {
            return SimpleActionAsync(HideUrl);
        }

        public Task UnhideAsync()
        {
            return SimpleActionAsync(UnhideUrl);
        }

        public Task IgnoreReportsAsync()
        {
            return SimpleActionAsync(IgnoreReportsUrl);
        }

        public Task UnIgnoreReportsAsync()
        {
            return SimpleActionAsync(UnIgnoreReportsUrl);
        }

        public Task MarkNSFWAsync()
        {
            return SimpleActionAsync(MarkNSFWUrl);
        }

        public Task UnmarkNSFWAsync()
        {
            return SimpleActionAsync(UnmarkNSFWUrl);
        }

        public Task ContestModeAsync(bool state)
        {
            return SimpleActionToggleAsync(ContestModeUrl, state);
        }

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
        public async Task UpdateAsync()
        {
            JToken post = await Reddit.GetTokenAsync(this.Url);
            JsonConvert.PopulateObject(post["data"].ToString(), this, Reddit.JsonSerializerSettings);
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

        //TODO discuss this
        public IEnumerable<Comment> EnumerateCommentsAsync() {
            return GetComments().ToEnumerable();
        }

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
                    Comment newComment = new Comment().Init(Reddit, comment, WebAgent, this);
                    if (newComment.Kind == "more")
                    {
                        moreComments = new More().Init(Reddit, comment, WebAgent);
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
