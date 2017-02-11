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
        private const string GetCommentsUrl = "/comments/{Id}.json";
        private const string EditUserTextUrl = "/api/editusertext";
        private const string HideUrl = "/api/hide";
        private const string UnhideUrl = "/api/unhide";
        private string SetFlairUrl => $"/r/{SubredditName}/api/flair";
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
        public new string AuthorName { get; private set; }

        //TODO Discuss
        public IObservable<Comment> Comments => GetComments();

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
        /// Parent subreddit.
        /// </summary>
        [JsonIgnore]
        public Subreddit Subreddit =>
          Task.Run(async () => {
              return await Reddit.GetSubredditAsync("/r/" + SubredditName).ConfigureAwait(false);
            }).Result;

        /// <summary>
        /// Post uri.
        /// </summary>
        [JsonProperty("url")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Url { get; private set; }

        /// <summary>
        /// Comment on this post.
        /// </summary>
        /// <param name="message">Markdown text.</param>
        /// <returns></returns>
        public async Task<Comment> CommentAsync(string message)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var json = await WebAgent.Post(CommentUrl, new
            {
                text = message,
                thing_id = FullName,
                uh = Reddit.User.Modhash,
                api_type = "json"
            }).ConfigureAwait(false);
            if (json["json"]["ratelimit"] != null)
                throw new RateLimitException(TimeSpan.FromSeconds(json["json"]["ratelimit"].ValueOrDefault<double>()));
            return new Comment(Reddit, json["json"]["data"]["things"][0], this);
        }

        private async Task<JToken> SimpleActionToggleAsync(string endpoint, bool value, bool requiresModAction = false)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");

            var mods = await Subreddit.GetModeratorsAsync().ConfigureAwait(false);
            var modNameList = mods.Select(b => b.Name).ToList();

            if (requiresModAction && !modNameList.Contains(Reddit.User.Name))
                throw new AuthenticationException(
                    string.Format(
                        @"User {0} is not a moderator of subreddit {1}.",
                        Reddit.User.Name,
                        this.Subreddit.Name));

            return await WebAgent.Post(endpoint, new
            {
                id = FullName,
                state = value,
                uh = Reddit.User.Modhash
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
            if (Reddit.User == null)
                throw new Exception("No user logged in.");
            if (!IsSelfPost)
                throw new Exception("Submission to edit is not a self-post.");

            var json = await WebAgent.Post(EditUserTextUrl, new
            {
                api_type = "json",
                text = newText,
                thing_id = FullName,
                uh = Reddit.User.Modhash
            }).ConfigureAwait(false);
            if (json["json"].ToString().Contains("\"errors\": []"))
                SelfText = newText;
            else
                throw new Exception("Error editing text.");
        }

        /// <summary>
        /// Update this post.
        /// </summary>
        public async Task UpdateAsync() => Reddit.PopulateObject(GetJsonData(await Reddit.GetTokenAsync(Url)), this);

        /// <summary>
        /// Sets your claim
        /// </summary>
        /// <param name="flairText">Text to set your flair</param>
        /// <param name="flairClass">class of the flair</param>
        public async Task SetFlairAsync(string flairText, string flairClass)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");

            await WebAgent.Post(SetFlairUrl, new
            {
                api_type = "json",
                css_class = flairClass,
                link = FullName,
                name = Reddit.User.Name,
                text = flairText,
                uh = Reddit.User.Modhash
            }).ConfigureAwait(false);
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
            if (limit.HasValue)
            {
                var query = WebUtility.UrlEncode("limit="+limit.Value.ToString());
                url = string.Format("{0}?{1}", url, query);
            }
            var json = await WebAgent.Get(url).ConfigureAwait(false);
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
        public IEnumerable<Comment> EnumerateCommentsAsync() => GetComments().ToEnumerable();

        /// <summary>
        /// Enumerate more comments.
        /// </summary>
        /// <returns></returns>
        public IObservable<Comment> GetComments()
        {
            return Observable.Create<Comment>(async obs =>
            {
                var json = await WebAgent.Get(GetCommentsUrl).ConfigureAwait(false);
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
                    var baseThings = await moreComments.GetThingsAsync().ConfigureAwait(false);
                    IEnumerator<Thing> things = baseThings.GetEnumerator();
                    things.MoveNext();
                    Thing currentThing = null;
                    while (currentThing != things.Current)
                    {
                        var comment = things.Current as Comment;
                        var more = things.Current as More;
                        if (comment != null)
                        {
                            Comment next = comment.PopulateComments(things);
                            obs.OnNext(next);
                        }
                        if (more != null)
                        {
                            if (more.ParentId != FullName) break;
                            var moreThings = await more.GetThingsAsync().ConfigureAwait(false);
                            things = moreThings.GetEnumerator();
                            things.MoveNext();
                        }
                    }
                }

                obs.OnCompleted();
            });
        }
    }
}
