using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp.Things;
using System;
using System.Linq;
using System.Net;
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

        private const string SslLoginUrl = "https://ssl.reddit.com/api/login";
        private const string LoginUrl = "/api/login/username";
        private const string UserInfoUrl = "/user/{0}/about.json";
        private const string MeUrl = "/api/me.json";
        private const string OAuthMeUrl = "/api/v1/me.json";
        private const string SubredditAboutUrl = "/r/{0}/about.json";
        private const string ComposeMessageUrl = "/api/compose";
        private const string RegisterAccountUrl = "/api/register";
        private const string GetThingUrl = "/api/info.json?id={0}";
        private const string GetCommentUrl = "/r/{0}/comments/{1}/foo/{2}";
        private const string GetPostUrl = "{0}.json";
        private const string DomainUrl = "www.reddit.com";
        private const string OAuthDomainUrl = "oauth.reddit.com";
        private const string SearchUrl = "/search.json?q={0}&restrict_sr=off&sort={1}&t={2}";
        private const string UrlSearchPattern = "url:'{0}'";
        private const string NewSubredditsUrl = "/subreddits/new.json";
        private const string PopularSubredditsUrl = "/subreddits/popular.json";
        private const string GoldSubredditsUrl = "/subreddits/gold.json";
        private const string DefaultSubredditsUrl = "/subreddits/default.json";
        private const string SearchSubredditsUrl = "/subreddits/search.json?q={0}";


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

        internal JsonSerializerSettings JsonSerializerSettings { get; set; }

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

        public Reddit()
            : this(true) { }

        public Reddit(bool useSsl)
        {
            DefaultWebAgent defaultAgent = new DefaultWebAgent();

            JsonSerializerSettings = new JsonSerializerSettings
                {
                    CheckAdditionalContent = false,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };
            DefaultWebAgent.Protocol = useSsl ? "https" : "http";
            WebAgent = defaultAgent;
            CaptchaSolver = new ConsoleCaptchaSolver();
        }

        public Reddit(DefaultWebAgent.RateLimitMode limitMode, bool useSsl = true)
            : this(useSsl)
        {
            DefaultWebAgent.UserAgent = "";
            DefaultWebAgent.RateLimit = limitMode;
            DefaultWebAgent.RootDomain = "www.reddit.com";
        }

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
        public Reddit(IWebAgent agent)
        {
            WebAgent = agent;
            JsonSerializerSettings = new JsonSerializerSettings
            {
                CheckAdditionalContent = false,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            CaptchaSolver = new ConsoleCaptchaSolver();
        }
        /// <summary>
        /// Creates a Reddit instance with the given WebAgent implementation
        /// </summary>
        /// <param name="agent">Implementation of IWebAgent interface. Used to generate requests.</param>
        /// <param name="initUser">Whether to run InitOrUpdateUser, requires <paramref name="agent"/> to have credentials first.</param>
        public Reddit(IWebAgent agent, bool initUser)
        {
            WebAgent = agent;
            JsonSerializerSettings = new JsonSerializerSettings
            {
                CheckAdditionalContent = false,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            CaptchaSolver = new ConsoleCaptchaSolver();
            if(initUser) Task.Run(InitOrUpdateUserAsync);
        }

        public async Task<RedditUser> GetUserAsync(string name)
        {
            var request = WebAgent.CreateGet(string.Format(UserInfoUrl, name));
            var response = await WebAgent.GetResponseAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(result);
            return new RedditUser().Init(this, json, WebAgent);
        }

        /// <summary>
        /// Initializes the User property if it's null,
        /// otherwise replaces the existing user object
        /// with a new one fetched from reddit servers.
        /// </summary>
        public async Task InitOrUpdateUserAsync()
        {
            var request = WebAgent.CreateGet(string.IsNullOrEmpty(WebAgent.AccessToken) ? MeUrl : OAuthMeUrl);
            var response = await WebAgent.GetResponseAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(result);
            User = new AuthenticatedUser().Init(this, json, WebAgent);
        }

        /// <summary>
        /// Returns the subreddit. 
        /// </summary>
        /// <param name="name">The name of the subreddit</param>
        /// <returns>The Subreddit by given name</returns>
        public async Task<Subreddit> GetSubredditAsync(string name)
        {
            name = System.Text.RegularExpressions.Regex.Replace(name, "(r/|/)", "");
            return await GetThingAsync<Subreddit>(string.Format(SubredditAboutUrl, name));
        }

        public Domain GetDomain(string domain)
        {
            if (!domain.StartsWith("http://") && !domain.StartsWith("https://"))
                domain = "http://" + domain;
            var uri = new Uri(domain);
            return new Domain(this, uri, WebAgent);
        }

        public async Task<JToken> GetTokenAsync(Uri uri)
        {
            var url = uri.AbsoluteUri;

            if (url.EndsWith("/"))
                url = url.Remove(url.Length - 1);

            var request = WebAgent.CreateGet(string.Format(GetPostUrl, url));
            var response = await WebAgent.GetResponseAsync(request);
            var data = await response.Content.ReadAsStringAsync();
            var json = JToken.Parse(data);

            return json[0]["data"]["children"].First;
        }

        public async Task<Post> GetPostAsync(Uri uri)
        {
            return await new Post().InitAsync(this, await GetTokenAsync(uri), WebAgent);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="to"></param>
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
                var subReddit =await GetSubredditAsync(fromSubReddit);
                var modNameList = (await subReddit.GetModeratorsAsync()).Select(b => b.Name).ToList();

                if (!modNameList.Contains(User.Name))
                    throw new AuthenticationException(
                        string.Format(
                            @"User {0} is not a moderator of subreddit {1}.",
                            User.Name,
                            subReddit.Name));
            }

            var request = WebAgent.CreatePost(ComposeMessageUrl);
            WebAgent.WritePostBody(request, new
            {
                api_type = "json",
                subject,
                text = body,
                to,
                from_sr = fromSubReddit,
                uh = User.Modhash,
                iden = captchaId,
                captcha = captchaAnswer
            });
            var response = await WebAgent.GetResponseAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(result);

            ICaptchaSolver solver = CaptchaSolver; // Prevent race condition

            if (json["json"]["errors"].Any() && json["json"]["errors"][0][0].ToString() == "BAD_CAPTCHA" && solver != null)
            {
                captchaId = json["json"]["captcha"].ToString();
                CaptchaResponse captchaResponse = solver.HandleCaptcha(new Captcha(captchaId));

                if (!captchaResponse.Cancel) // Keep trying until we are told to cancel
                    await ComposePrivateMessageAsync(subject, body, to, fromSubReddit, captchaId, captchaResponse.Answer);
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
            var request = WebAgent.CreatePost(RegisterAccountUrl);
            WebAgent.WritePostBody(request, new
            {
                api_type = "json",
                email = email,
                passwd = passwd,
                passwd2 = passwd,
                user = userName
            });
            var response = await WebAgent.GetResponseAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(result);
            return new AuthenticatedUser().Init(this, json, WebAgent);
            // TODO: Error
        }

        public async Task<Thing> GetThingByFullnameAsync(string fullname)
        {
            var request = WebAgent.CreateGet(string.Format(GetThingUrl, fullname));
            var response = await WebAgent.GetResponseAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            var json = JToken.Parse(result);
            return Thing.Parse(this, json["data"]["children"][0], WebAgent);
        }

        public Task<Comment> GetCommentAsync(string subreddit, string name, string linkName)
        {
            
                if (linkName.StartsWith("t3_"))
                    linkName = linkName.Substring(3);
                if (name.StartsWith("t1_"))
                    name = name.Substring(3);

                var url = string.Format(GetCommentUrl, subreddit, linkName, name);
                return GetCommentAsync(new Uri(url));
            
        }

        public async Task<Comment> GetCommentAsync(Uri uri)
        {
            var url = string.Format(GetPostUrl, uri.AbsoluteUri);
            var request = WebAgent.CreateGet(url);
            var response = await WebAgent.GetResponseAsync(request);
            var data = await response.Content.ReadAsStringAsync();
            var json = JToken.Parse(data);

            var sender = new Post().Init(this, json[0]["data"]["children"][0], WebAgent);
            return new Comment().Init(this, json[1]["data"]["children"][0], WebAgent, sender);
        }

        public Listing<T> SearchByUrl<T>(string url) where T : Thing
        {
            var urlSearchQuery = string.Format(UrlSearchPattern, url);
            return Search<T>(urlSearchQuery);
        }

        public Listing<T> Search<T>(string query, Sorting sortE = Sorting.Relevance, TimeSorting timeE = TimeSorting.All) where T : Thing
        {
            string sort = sortE.ToString().ToLower();
            string time = timeE.ToString().ToLower();
            return new Listing<T>(this, string.Format(SearchUrl, query, sort, time), WebAgent);
        }

        public Listing<T> SearchByTimestamp<T>(DateTime from, DateTime to, string query = "", string subreddit = "", Sorting sortE = Sorting.Relevance, TimeSorting timeE = TimeSorting.All) where T : Thing
        {
            string sort = sortE.ToString().ToLower();
            string time = timeE.ToString().ToLower();

            var fromUnix = (from - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            var toUnix = (to - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

            string searchQuery = "(and+timestamp:" + fromUnix + ".." + toUnix + "+'" + query + "'+" + "subreddit:'" + subreddit + "')&syntax=cloudsearch";
            return new Listing<T>(this, string.Format(SearchUrl, searchQuery, sort, time), WebAgent);
        }


        #region SubredditSearching

        /// <summary>
        /// Returns a Listing of newly created subreddits.
        /// </summary>
        /// <returns></returns>
        public Listing<Subreddit> GetNewSubreddits()
        {
            return new Listing<Subreddit>(this, NewSubredditsUrl, WebAgent);
        }

        /// <summary>
        /// Returns a Listing of the most popular subreddits.
        /// </summary>
        /// <returns></returns>
        public Listing<Subreddit> GetPopularSubreddits()
        {
            return new Listing<Subreddit>(this, PopularSubredditsUrl, WebAgent);
        }

        /// <summary>
        /// Returns a Listing of Gold-only subreddits. This endpoint will not return anything if the authenticated Reddit account does not currently have gold.
        /// </summary>
        /// <returns></returns>
        public Listing<Subreddit> GetGoldSubreddits()
        {
            return new Listing<Subreddit>(this, GoldSubredditsUrl, WebAgent);
        }

        /// <summary>
        /// Returns the Listing of default subreddits.
        /// </summary>
        /// <returns></returns>
        public Listing<Subreddit> GetDefaultSubreddits()
        {
            return new Listing<Subreddit>(this, DefaultSubredditsUrl, WebAgent);
        }

        /// <summary>
        /// Returns the Listing of subreddits related to a query.
        /// </summary>
        /// <returns></returns>
        public Listing<Subreddit> SearchSubreddits(string query)
        {
            return new Listing<Subreddit>(this, string.Format(SearchSubredditsUrl, query), WebAgent);
        }

        #endregion SubredditSearching

        #region Helpers

        protected async internal Task<T> GetThingAsync<T>(string url) where T : Thing
        {
            var request = WebAgent.CreateGet(url);
            var response = await WebAgent.GetResponseAsync(request);
            var data = await response.Content.ReadAsStringAsync();
            var json = JToken.Parse(data);
            var ret = await Thing.ParseAsync(this, json, WebAgent);
            return (T)ret;
        }
        #endregion
    }
}
