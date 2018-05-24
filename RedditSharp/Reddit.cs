using RedditSharp.Search;
using RedditSharp.Things;
using System;
using System.Linq;
using System.Linq.Expressions;
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
        private IAdvancedSearchFormatter _searchFormatter;
        private IAdvancedSearchFormatter SearchFormatter
        {
            get
            {
                if(_searchFormatter == null)
                {
                    _searchFormatter = new DefaultSearchFormatter();
                }
                return _searchFormatter;
            }
            set => _searchFormatter = value;
        }

        #region Properties
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
        public RateLimitMode RateLimit
        {
            get { return WebAgent.RateLimiter.Mode; }
            set { WebAgent.RateLimiter.Mode = value; }
        }

        /// <summary>
        /// Gets the FrontPage using the current Reddit instance.
        /// </summary>
        public Subreddit FrontPage
        {
            get { return Subreddit.GetFrontPage(WebAgent); }
        }

        /// <summary>
        /// Gets /r/All using the current Reddit instance.
        /// </summary>
        public Subreddit RSlashAll
        {
            get { return Subreddit.GetRSlashAll(WebAgent); }
        }
        #endregion

#pragma warning disable 1591
        public Reddit()
            : this(true) { }

        public Reddit(bool useSsl)
        {
            DefaultWebAgent defaultAgent = new DefaultWebAgent();
            
            DefaultWebAgent.Protocol = useSsl ? "https" : "http";
            WebAgent = defaultAgent;
            CaptchaSolver = new ConsoleCaptchaSolver();
        }
#pragma warning restore 1591

        /// <summary>
        ///
        /// </summary>
        /// <param name="limitMode">Rate limit</param>
        /// <param name="useSsl">use ssl.  Defaults to true.</param>
        public Reddit(RateLimitMode limitMode, bool useSsl = true)
            : this(useSsl)
        {
            DefaultWebAgent.DefaultUserAgent = "";
            DefaultWebAgent.DefaultRateLimiter.Mode = limitMode;
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
        public Reddit(IWebAgent agent) : this(agent, false)
        {
        }

        /// <summary>
        /// Creates a Reddit instance with the given WebAgent implementation
        /// </summary>
        /// <param name="agent">Implementation of IWebAgent interface. Used to generate requests.</param>
        /// <param name="initUser">Whether to run InitOrUpdateUser, requires <paramref name="agent"/> to have credentials first.</param>
        public Reddit(IWebAgent agent, bool initUser)
        {
            WebAgent = agent;
            CaptchaSolver = new ConsoleCaptchaSolver();
            if (initUser) Task.Run(InitOrUpdateUserAsync).Wait();
        }

        /// <summary>
        /// Get a reddit user by name.
        /// </summary>
        /// <param name="name">user name</param>
        /// <returns></returns>
        public Task<RedditUser> GetUserAsync(string name)
        {
            return RedditUser.GetUserAsync(WebAgent, name);
        }

        /// <summary>
        /// Initializes the User property if it's null,
        /// otherwise replaces the existing user object
        /// with a new one fetched from reddit servers.
        /// </summary>
        public async Task InitOrUpdateUserAsync()
        {
            var json = await WebAgent.Get(!string.IsNullOrEmpty(WebAgent.AccessToken) || WebAgent.GetType() == typeof(RefreshTokenWebAgent) ? OAuthMeUrl : MeUrl).ConfigureAwait(false);
            User = new AuthenticatedUser(WebAgent, json);
        }

        /// <summary>
        /// Get a subreddit by name.
        /// </summary>
        /// <param name="name">subreddit name with or without preceding /r/</param>
        /// <param name="validateName">Whether to validate the subreddit name.</param>
        /// <returns></returns>
        public Task<Subreddit> GetSubredditAsync(string name, bool validateName = true)
        {
            return Subreddit.GetByNameAsync(WebAgent, name, validateName);
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
            return new Domain(WebAgent, uri);
        }

        

        /// <summary>
        /// Get a <see cref="Post"/> by uri.
        /// </summary>
        /// <param name="uri">uri to fetch</param>
        /// <returns></returns>
        public async Task<Post> GetPostAsync(Uri uri)
        {
            return new Post(WebAgent, await Helpers.GetTokenAsync(WebAgent, uri).ConfigureAwait(false));
        }

        /// <summary>
        /// Create a Reddit Live thread.
        /// </summary>
        /// <param name="title">Required.</param>
        /// <param name="description">Required</param>
        /// <param name="resources"></param>
        /// <param name="nsfw"></param>
        /// <returns></returns>
        public async Task<LiveUpdateEvent> CreateLiveEventAsync(string title, string description, string resources = "", bool nsfw = false)
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

            if (json["errors"].Any())
                throw new Exception(json["errors"][0][0].ToString());

            var id = json["data"]["id"].ToString();

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

            var token = await Helpers.GetTokenAsync(WebAgent, uri,true).ConfigureAwait(false);
            return new LiveUpdateEvent(WebAgent, token);
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
                var subReddit = await GetSubredditAsync(fromSubReddit).ConfigureAwait(false);
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

            if (json["errors"].Any() && json["errors"][0][0].ToString() == "BAD_CAPTCHA" && solver != null)
            {
                captchaId = json["captcha"].ToString();
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
            return new AuthenticatedUser(WebAgent, json);
            // TODO: Error
        }

        /// <summary>
        /// Get a <see cref="Thing"/> by full name.
        /// </summary>
        /// <param name="fullname"></param>
        /// <returns></returns>
        public Task<Thing> GetThingByFullnameAsync(string fullname)
        {
            return Helpers.GetThingByFullnameAsync(WebAgent, fullname);
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
            var sender = new Post(WebAgent, json[0]["data"]["children"][0]);
            return new Comment(WebAgent, json[1]["data"]["children"][0], sender);
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
        /// <param name="max">Maximum number of records to return.  -1 for unlimited.</param>
        /// <returns></returns>
        public Listing<T> Search<T>(string query, Sorting sortE = Sorting.Relevance, TimeSorting timeE = TimeSorting.All, int max = -1) where T : Thing
        {
            string sort = sortE.ToString().ToLower();
            string time = timeE.ToString().ToLower();
            return Listing<T>.Create(this.WebAgent, string.Format(SearchUrl, query, sort, time), max, 100);
        }

        public Listing<Post> AdvancedSearch(Expression<Func<AdvancedSearchFilter, bool>> searchFilter, Sorting sortE = Sorting.Relevance, TimeSorting timeE = TimeSorting.All)
        {
            string query = SearchFormatter.Format(searchFilter);
            string sort = sortE.ToString().ToLower();
            string time = timeE.ToString().ToLower();
            string final = string.Format(SearchUrl, query, sort, time);
            return new Listing<Post>(WebAgent, final);
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
        /// <param name="max">Maximum number of records to return.  -1 for unlimited.</param>
        /// <returns></returns>
        [Obsolete("time search was discontinued by reddit",true)]
        public Listing<T> SearchByTimestamp<T>(DateTime from, DateTime to, string query = "", string subreddit = "", Sorting sortE = Sorting.Relevance, TimeSorting timeE = TimeSorting.All, int max = -1) where T : Thing
        {
            string sort = sortE.ToString().ToLower();
            string time = timeE.ToString().ToLower();

            var fromUnix = (from - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            var toUnix = (to - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

            string searchQuery = "(and+timestamp:" + fromUnix + ".." + toUnix + "+'" + query + "'+" + "subreddit:'" + subreddit + "')&syntax=cloudsearch";
            return Listing<T>.Create(this.WebAgent, string.Format(SearchUrl, searchQuery, sort, time), max, 100);
        }


        #region SubredditSearching

        /// <summary>
        /// Returns a Listing of newly created subreddits.
        /// </summary>
        /// <param name="max">Maximum number of records to return.  -1 for unlimited.</param>
        public Listing<Subreddit> GetNewSubreddits(int max = -1) => Listing<Subreddit>.Create(this.WebAgent, NewSubredditsUrl, max, 100);

        /// <summary>
        /// Returns a Listing of the most popular subreddits.
        /// </summary>
        /// <param name="max">Maximum number of records to return.  -1 for unlimited.</param>
        public Listing<Subreddit> GetPopularSubreddits(int max = -1) => Listing<Subreddit>.Create(this.WebAgent, PopularSubredditsUrl, max, 100);

        /// <summary>
        /// Returns a Listing of Gold-only subreddits. This endpoint will not return anything if the authenticated Reddit account does not currently have gold.
        /// </summary>
        /// <param name="max">Maximum number of records to return.  -1 for unlimited.</param>
        public Listing<Subreddit> GetGoldSubreddits(int max = -1) => Listing<Subreddit>.Create(this.WebAgent, GoldSubredditsUrl, max, 100);

        /// <summary>
        /// Returns the Listing of default subreddits.
        /// </summary>
        /// <param name="max">Maximum number of records to return.  -1 for unlimited.</param>
        public Listing<Subreddit> GetDefaultSubreddits(int max = -1) => Listing<Subreddit>.Create(this.WebAgent, DefaultSubredditsUrl, max, 100);

        /// <summary>
        /// Returns the Listing of subreddits related to a query.
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="max">Maximum number of records to return.  -1 for unlimited.</param>
        public Listing<Subreddit> SearchSubreddits(string query, int max = -1) => Listing<Subreddit>.Create(this.WebAgent, string.Format(SearchSubredditsUrl, query), max, 100);

        #endregion SubredditSearching

        /// <summary>
        /// Create a listing from the specified url.  Useful if you want to specify your own "before" and "after" values.
        /// </summary>
        /// <typeparam name="T">A <see cref="RedditSharp.Things.Thing"/></typeparam>
        /// <param name="url">endpoint url.</param>
        /// <param name="maxLimit">Maximum number of records to retrieve from reddit.</param>
        /// <param name="limitPerRequest">Maximum number of records to return per request.  This number is endpoint specific.</param>
        /// <returns></returns>
        public Listing<T> GetListing<T>(string url, int maxLimit = -1, int limitPerRequest = -1) where T : Thing
        {
            return new Listing<T>(this.WebAgent, url, maxLimit, limitPerRequest);
        }
    }
}
