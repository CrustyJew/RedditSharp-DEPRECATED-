using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp.Things;
using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using DefaultWebAgent = RedditSharp.WebAgent;

namespace RedditSharp
{
    /// <summary>
    /// Class to communicate with Reddit.com
    /// </summary>
    public class Reddit
    {
        #region Constant Urls

        private const string UserInfoUrl = "/user/{0}/about.json";
        private const string MeUrl = "/api/me.json";
        private const string OAuthMeUrl = "/api/v1/me.json";
        private const string SubredditAboutUrl = "/r/{0}/about.json";
        private const string ComposeMessageUrl = "/api/compose";
        private const string RegisterAccountUrl = "/api/register";
        private const string GetThingUrl = "/api/info.json?id={0}";
        private const string GetCommentUrl = "/r/{0}/comments/{1}/foo/{2}";
        private const string GetPostUrl = "{0}.json";
        private const string OAuthDomainUrl = "oauth.reddit.com";
        private const string SearchUrl = "/search.json?q={0}&restrict_sr=off&sort={1}&t={2}";
        private const string UrlSearchPattern = "url:'{0}'";
        private const string NewSubredditsUrl = "/subreddits/new.json";
        private const string PopularSubredditsUrl = "/subreddits/popular.json";
        private const string GoldSubredditsUrl = "/subreddits/gold.json";
        private const string DefaultSubredditsUrl = "/subreddits/default.json";
        private const string SearchSubredditsUrl = "/subreddits/search.json?q={0}";
        private const string CreateLiveEventUrl = "/api/live/create";
        private const string GetLiveEventUrl = "https://www.reddit.com/live/{0}/about";

        #endregion


        internal IWebAgent WebAgent { get; set; }
        /// <summary>
        /// Captcha solver instance to use when solving captchas.
        /// </summary>
        public ICaptchaSolver CaptchaSolver;

        /// <summary>
        /// The authenticated user for this instance.
        /// </summary>
        public AuthenticatedUser User { get; set; }

        /// <summary>
        /// Sets the Rate Limiting Mode of the underlying WebAgent
        /// </summary>
        public DefaultWebAgent.RateLimitMode RateLimit
        {
            get { return DefaultWebAgent.RateLimit; }
            set { DefaultWebAgent.RateLimit = value; }
        }

        internal JsonSerializer JsonSerializer { get; }

        /// <summary>
        /// Gets the FrontPage using the current Reddit instance.
        /// </summary>
        public Subreddit FrontPage
        {
            get { return Subreddit.GetFrontPage(this); }
        }

        /// <summary>
        /// Gets /r/All using the current Reddit instance.
        /// </summary>
        public Subreddit RSlashAll
        {
            get { return Subreddit.GetRSlashAll(this); }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Reddit()
            : this(true) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Reddit(bool useSsl)
        {
            DefaultWebAgent defaultAgent = new DefaultWebAgent();

            JsonSerializer = JsonSerializer.Create(new JsonSerializerSettings {
                    CheckAdditionalContent = false,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });
            DefaultWebAgent.Protocol = useSsl ? "https" : "http";
            WebAgent = defaultAgent;
            CaptchaSolver = new ConsoleCaptchaSolver();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="limitMode">Rate limit</param>
        /// <param name="useSsl">use ssl.  Defaults to true.</param>
        public Reddit(DefaultWebAgent.RateLimitMode limitMode, bool useSsl = true)
            : this(useSsl)
        {
            DefaultWebAgent.UserAgent = "";
            DefaultWebAgent.RateLimit = limitMode;
            DefaultWebAgent.RootDomain = "www.reddit.com";
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="accessToken">oauth access token.</param>
        public Reddit(string accessToken)
            : this(true)
        {
            DefaultWebAgent.RootDomain = OAuthDomainUrl;
            WebAgent.AccessToken = accessToken;
            Task.Run(InitOrUpdateUserAsync);
        }
        /// <summary>
        /// Creates a Reddit instance with the given WebAgent implementation
        /// </summary>
        /// <param name="agent">Implementation of IWebAgent interface. Used to generate requests.</param>
        public Reddit(IWebAgent agent) : this(agent, false) {
        }

        /// <summary>
        /// Creates a Reddit instance with the given WebAgent implementation
        /// </summary>
        /// <param name="agent">Implementation of IWebAgent interface. Used to generate requests.</param>
        /// <param name="initUser">Whether to run InitOrUpdateUser, requires <paramref name="agent"/> to have credentials first.</param>
        public Reddit(IWebAgent agent, bool initUser)
        {
            WebAgent = agent;
            JsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                CheckAdditionalContent = false,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            CaptchaSolver = new ConsoleCaptchaSolver();
            if(initUser) Task.Run(InitOrUpdateUserAsync).Wait();
        }

        internal void PopulateObject(JToken json, object obj) {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            using (var reader = json.CreateReader()) {
                JsonSerializer.Populate(reader, obj);
            }
        }

        /// <summary>
        /// Get a reddit user by name.
        /// </summary>
        /// <param name="name">user name</param>
        /// <returns></returns>
        public async Task<RedditUser> GetUserAsync(string name)
        {
            var json = await WebAgent.Get(string.Format(UserInfoUrl, name)).ConfigureAwait(false);
            return new RedditUser(this, json);
        }

        /// <summary>
        /// Initializes the User property if it's null,
        /// otherwise replaces the existing user object
        /// with a new one fetched from reddit servers.
        /// </summary>
        public async Task InitOrUpdateUserAsync()
        {
            var json = await WebAgent.Get(string.IsNullOrEmpty(WebAgent.AccessToken) ? MeUrl : OAuthMeUrl).ConfigureAwait(false);
            User = new AuthenticatedUser(this, json);
        }

        /// <summary>
        /// Get a subreddit by name.
        /// </summary>
        /// <param name="name">subreddit name with or without preceding /r/</param>
        /// <returns></returns>
        public async Task<Subreddit> GetSubredditAsync(string name)
        {
            name = System.Text.RegularExpressions.Regex.Replace(name, "(r/|/)", "");
            return await GetThingAsync<Subreddit>(string.Format(SubredditAboutUrl, name)).ConfigureAwait(false);
        }

        /// <summary>
        /// Get information about a domain.
        /// </summary>
        /// <param name="domain">domain name</param>
        /// <returns></returns>
        public Domain GetDomain(string domain)
        {
            if (!domain.StartsWith("http://") && !domain.StartsWith("https://"))
                domain = "http://" + domain;
            var uri = new Uri(domain);
            return new Domain(this, uri);
        }

        /// <summary>
        /// Get a <see cref="JToken"/> from a url.
        /// </summary>
        /// <param name="uri">uri to fetch</param>
        /// <param name="isLive">bool indicating if it's a live thread or not</param>
        /// <returns></returns>
        public async Task<JToken> GetTokenAsync(Uri uri, bool isLive = false)
        {
            var url = uri.AbsoluteUri;

            if (url.EndsWith("/"))
                url = url.Remove(url.Length - 1);

            var json = await WebAgent.Get(string.Format(GetPostUrl, url));

            if (isLive)
                return json;
            else
                return json[0]["data"]["children"].First;
        }

        /// <summary>
        /// Get a <see cref="Post"/> by uri.
        /// </summary>
        /// <param name="uri">uri to fetch</param>
        /// <returns></returns>
        public async Task<Post> GetPostAsync(Uri uri)
        {
            if (!String.IsNullOrEmpty(WebAgent.AccessToken) && uri.AbsoluteUri.StartsWith("https://www.reddit.com"))
                uri = new Uri(uri.AbsoluteUri.Replace("https://www.reddit.com", "https://oauth.reddit.com"));
            return new Post(this, await GetTokenAsync(uri).ConfigureAwait(false));
        }

        /// <summary>
        /// Create a Reddit Live thread.
        /// </summary>
        /// <param name="title">Required.</param>
        /// <param name="description">Required</param>
        /// <param name="resources"></param>
        /// <param name="nsfw"></param>
        /// <returns></returns>
        public async Task<LiveUpdateEvent> CreateLiveEventAsync(string title,string description,string resources = "", bool nsfw = false)
        {
            if (String.IsNullOrEmpty(title))
                throw new ArgumentException(nameof(title));

            if (String.IsNullOrEmpty(description))
                throw new ArgumentException(nameof(description));

            var json = await WebAgent.Post(CreateLiveEventUrl, new
            {
                api_type = "json",
                title = title,
                description = description,
                resources = resources,
                nsfw = nsfw
            }).ConfigureAwait(false);

            if (json["json"]["errors"].Any())
                throw new Exception(json["json"]["errors"][0][0].ToString());

            var id = json["json"]["data"]["id"].ToString();

            return await GetLiveEvent(new Uri(String.Format(GetLiveEventUrl, id))).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a reddit live thread.
        /// </summary>
        /// <param name="uri">Uri of the live thread.</param>
        /// <returns></returns>
        public async Task<LiveUpdateEvent> GetLiveEvent(Uri uri)
        {
            if (!uri.AbsoluteUri.EndsWith("about"))
                uri = new Uri(uri.AbsoluteUri + "/about");

            var token = await GetTokenAsync(uri).ConfigureAwait(false);
            return new LiveUpdateEvent(this, token);
        }


        /// <summary>
        /// Compose a private message.
        /// </summary>
        /// <param name="subject">message subject</param>
        /// <param name="body">markdown body</param>
        /// <param name="to">target author or subreddit</param>
        /// <param name="fromSubReddit">The subreddit to send the message as (optional).</param>
        /// <param name="captchaId"></param>
        /// <param name="captchaAnswer"></param>
        /// <remarks>If <paramref name="fromSubReddit"/> is passed in then the message is sent from the subreddit. the sender must be a mod of the specified subreddit.</remarks>
        /// <exception cref="AuthenticationException">Thrown when a subreddit is passed in and the user is not a mod of that sub.</exception>
        public async Task ComposePrivateMessageAsync(string subject, string body, string to, string fromSubReddit = "", string captchaId = "", string captchaAnswer = "")
        {
            if (User == null)
                throw new Exception("User can not be null.");

            if (!string.IsNullOrWhiteSpace(fromSubReddit))
            {
                var subReddit =await GetSubredditAsync(fromSubReddit).ConfigureAwait(false);
                var modNameList = (await subReddit.GetModeratorsAsync().ConfigureAwait(false)).Select(b => b.Name).ToList();

                if (!modNameList.Contains(User.Name))
                    throw new AuthenticationException(
                        string.Format(
                            @"User {0} is not a moderator of subreddit {1}.",
                            User.Name,
                            subReddit.Name));
            }

            var json = await WebAgent.Post(ComposeMessageUrl, new
            {
                api_type = "json",
                subject,
                text = body,
                to,
                from_sr = fromSubReddit,
                uh = User.Modhash,
                iden = captchaId,
                captcha = captchaAnswer
            }).ConfigureAwait(false);

            ICaptchaSolver solver = CaptchaSolver; // Prevent race condition

            if (json["json"]["errors"].Any() && json["json"]["errors"][0][0].ToString() == "BAD_CAPTCHA" && solver != null)
            {
                captchaId = json["json"]["captcha"].ToString();
                CaptchaResponse captchaResponse = solver.HandleCaptcha(new Captcha(captchaId));

                if (!captchaResponse.Cancel) // Keep trying until we are told to cancel
                    await ComposePrivateMessageAsync(subject, body, to, fromSubReddit, captchaId, captchaResponse.Answer).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Registers a new Reddit user
        /// </summary>
        /// <param name="userName">The username for the new account.</param>
        /// <param name="passwd">The password for the new account.</param>
        /// <param name="email">The optional recovery email for the new account.</param>
        /// <returns>The newly created user account</returns>
        public async Task<AuthenticatedUser> RegisterAccountAsync(string userName, string passwd, string email = "")
        {
            var json = await WebAgent.Post(RegisterAccountUrl, new
            {
                api_type = "json",
                email = email,
                passwd = passwd,
                passwd2 = passwd,
                user = userName
            }).ConfigureAwait(false);
            return new AuthenticatedUser(this, json);
            // TODO: Error
        }

        /// <summary>
        /// Get a <see cref="Thing"/> by full name.
        /// </summary>
        /// <param name="fullname"></param>
        /// <returns></returns>
        public async Task<Thing> GetThingByFullnameAsync(string fullname)
        {
            var json = await WebAgent.Get(string.Format(GetThingUrl, fullname)).ConfigureAwait(false);
            return Thing.Parse(this, json["data"]["children"][0]);
        }

        /// <summary>
        /// Get a <see cref="Comment"/>.
        /// </summary>
        /// <param name="subreddit">subreddit name in which the comment resides</param>
        /// <param name="name">comment base36 id</param>
        /// <param name="linkName">post base36 id</param>
        /// <returns></returns>
        public Task<Comment> GetCommentAsync(string subreddit, string name, string linkName)
        {
            if (linkName.StartsWith("t3_"))
                linkName = linkName.Substring(3);
            if (name.StartsWith("t1_"))
                name = name.Substring(3);

            var url = string.Format(GetCommentUrl, subreddit, linkName, name);
            return GetCommentAsync(new Uri(url));
        }

        /// <summary>
        /// Get a <see cref="Comment"/>.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<Comment> GetCommentAsync(Uri uri)
        {
            var url = string.Format(GetPostUrl, uri.AbsoluteUri);
            var json = await WebAgent.Get(url).ConfigureAwait(false);
            var sender = new Post(this, json[0]["data"]["children"][0]);
            return new Comment(this, json[1]["data"]["children"][0], sender);
        }

        /// <summary>
        /// Return a <see cref="Listing{T}"/> of items matching url search.
        /// </summary>
        /// <typeparam name="T"><see cref="Thing"/></typeparam>
        /// <param name="url">query url</param>
        /// <returns></returns>
        public Listing<T> SearchByUrl<T>(string url) where T : Thing
        {
            var urlSearchQuery = string.Format(UrlSearchPattern, url);
            return Search<T>(urlSearchQuery);
        }

        /// <summary>
        /// Return a <see cref="Listing{T}"/> of items matching search.
        /// </summary>
        /// <typeparam name="T"><see cref="Thing"/></typeparam>
        /// <param name="query">string to query</param>
        /// <param name="sortE">Order by <see cref="Sorting"/></param>
        /// <param name="timeE">Order by <see cref="TimeSorting"/></param>
        /// <returns></returns>
        public Listing<T> Search<T>(string query, Sorting sortE = Sorting.Relevance, TimeSorting timeE = TimeSorting.All) where T : Thing
        {
            string sort = sortE.ToString().ToLower();
            string time = timeE.ToString().ToLower();
            return new Listing<T>(this, string.Format(SearchUrl, query, sort, time));
        }

        /// <summary>
        /// Return a <see cref="Listing{T}"/> of items matching search with a given time period.
        /// </summary>
        /// <typeparam name="T"><see cref="Thing"/></typeparam>
        /// <param name="from">DateTime from</param>
        /// <param name="to">DateTime to</param>
        /// <param name="query">string to query</param>
        /// <param name="subreddit">subreddit in which to search</param>
        /// <param name="sortE">Order by <see cref="Sorting"/></param>
        /// <param name="timeE">Order by <see cref="TimeSorting"/></param>
        /// <returns></returns>
        public Listing<T> SearchByTimestamp<T>(DateTime from, DateTime to, string query = "", string subreddit = "", Sorting sortE = Sorting.Relevance, TimeSorting timeE = TimeSorting.All) where T : Thing
        {
            string sort = sortE.ToString().ToLower();
            string time = timeE.ToString().ToLower();

            var fromUnix = (from - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            var toUnix = (to - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

            string searchQuery = "(and+timestamp:" + fromUnix + ".." + toUnix + "+'" + query + "'+" + "subreddit:'" + subreddit + "')&syntax=cloudsearch";
            return new Listing<T>(this, string.Format(SearchUrl, searchQuery, sort, time));
        }


        #region SubredditSearching

        /// <summary>
        /// Returns a Listing of newly created subreddits.
        /// </summary>
        /// <returns></returns>
        public Listing<Subreddit> GetNewSubreddits() => new Listing<Subreddit>(this, NewSubredditsUrl);

        /// <summary>
        /// Returns a Listing of the most popular subreddits.
        /// </summary>
        /// <returns></returns>
        public Listing<Subreddit> GetPopularSubreddits() => new Listing<Subreddit>(this, PopularSubredditsUrl);

        /// <summary>
        /// Returns a Listing of Gold-only subreddits. This endpoint will not return anything if the authenticated Reddit account does not currently have gold.
        /// </summary>
        /// <returns></returns>
        public Listing<Subreddit> GetGoldSubreddits() => new Listing<Subreddit>(this, GoldSubredditsUrl);

        /// <summary>
        /// Returns the Listing of default subreddits.
        /// </summary>
        /// <returns></returns>
        public Listing<Subreddit> GetDefaultSubreddits() => new Listing<Subreddit>(this, DefaultSubredditsUrl);

        /// <summary>
        /// Returns the Listing of subreddits related to a query.
        /// </summary>
        /// <returns></returns>
        public Listing<Subreddit> SearchSubreddits(string query) => new Listing<Subreddit>(this, string.Format(SearchSubredditsUrl, query));

        #endregion SubredditSearching

        #region Helpers

        protected async internal Task<T> GetThingAsync<T>(string url) where T : Thing
        {
            var json = await WebAgent.Get(url).ConfigureAwait(false);
            return Thing.Parse<T>(this, json);
        }
        #endregion
    }
}
