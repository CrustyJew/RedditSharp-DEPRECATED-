using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp.Things;
using System;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
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

            JsonSerializerSettings = new JsonSerializerSettings
                {
                    CheckAdditionalContent = false,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };
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
        /// DEPRECATED: Avoid use as Reddit will be removing this option eventually
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="useSsl"></param>
        [Obsolete("OAuth is recommended.", false)]
        public Reddit(string username, string password, bool useSsl = true)
            : this(useSsl)
        {
            LogIn(username, password, useSsl);
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
            InitOrUpdateUser();
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
            if(initUser) InitOrUpdateUser();
        }

        /// <summary>
        /// Logs in the current Reddit instance. DEPRECATED
        /// </summary>
        /// <param name="username">The username of the user to log on to.</param>
        /// <param name="password">The password of the user to log on to.</param>
        /// <param name="useSsl">Whether to use SSL or not. (default: true)</param>
        /// <returns></returns>
        public AuthenticatedUser LogIn(string username, string password, bool useSsl = true)
        {
            if (Type.GetType("Mono.Runtime") != null)
                ServicePointManager.ServerCertificateValidationCallback = (s, c, ch, ssl) => true;
            WebAgent.Cookies = new CookieContainer();
            HttpWebRequest request;
            if (useSsl)
                request = WebAgent.CreatePost(SslLoginUrl);
            else
                request = WebAgent.CreatePost(LoginUrl);
            var stream = request.GetRequestStream();
            if (useSsl)
            {
                WebAgent.WritePostBody(stream, new
                {
                    user = username,
                    passwd = password,
                    api_type = "json"
                });
            }
            else
            {
                WebAgent.WritePostBody(stream, new
                {
                    user = username,
                    passwd = password,
                    api_type = "json",
                    op = "login"
                });
            }
            stream.Close();
            var response = (HttpWebResponse)request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result)["json"];
            if (json["errors"].Count() != 0)
                throw new AuthenticationException("Incorrect login.");

            InitOrUpdateUser();

            return User;
        }

        /// <summary>
        /// Get a reddit user by name.
        /// </summary>
        /// <param name="name">user name</param>
        /// <returns></returns>
        public RedditUser GetUser(string name)
        {
            var request = WebAgent.CreateGet(string.Format(UserInfoUrl, name));
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);
            return new RedditUser().Init(this, json, WebAgent);
        }

        /// <summary>
        /// Initializes the User property if it's null,
        /// otherwise replaces the existing user object
        /// with a new one fetched from reddit servers.
        /// </summary>
        public void InitOrUpdateUser()
        {
            var request = WebAgent.CreateGet(string.IsNullOrEmpty(WebAgent.AccessToken) ? MeUrl : OAuthMeUrl);
            var response = (HttpWebResponse)request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);
            User = new AuthenticatedUser().Init(this, json, WebAgent);
        }

        #region Obsolete Getter Methods

        [Obsolete("Use User property instead")]
        public AuthenticatedUser GetMe()
        {
            return User;
        }

        #endregion Obsolete Getter Methods

        /// <summary>
        /// Get a subreddit by name.
        /// </summary>
        /// <param name="name">subreddit name with or without preceding /r/</param>
        /// <returns></returns>
        public Subreddit GetSubreddit(string name)
        {
            name = System.Text.RegularExpressions.Regex.Replace(name, "(r/|/)", "");
            return GetThing<Subreddit>(string.Format(SubredditAboutUrl, name));
        }

        /// <summary>
        /// Get a subreddit by name.
        /// </summary>
        /// <param name="name">subreddit name with or without preceding /r/</param>
        /// <returns></returns>
        public async Task<Subreddit> GetSubredditAsync(string name)
        {
            name = System.Text.RegularExpressions.Regex.Replace(name, "(r/|/)", "");
            return await GetThingAsync<Subreddit>(string.Format(SubredditAboutUrl, name));
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
            return new Domain(this, uri, WebAgent);
        }


        /// <summary>
        /// Get a <see cref="JToken"/> from a url.
        /// </summary>
        /// <param name="uri">uri to fetch</param>
        /// <param name="isLive">bool indicating if it's a live thread or not</param>
        /// <returns></returns>
        public JToken GetToken(Uri uri, bool isLive = false)
        {
            var url = uri.AbsoluteUri;

            if (url.EndsWith("/"))
                url = url.Remove(url.Length - 1);

            var request = WebAgent.CreateGet(string.Format(GetPostUrl, url));
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);

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
        public Post GetPost(Uri uri)
        {
            if (!String.IsNullOrEmpty(WebAgent.AccessToken) && uri.AbsoluteUri.StartsWith("https://www.reddit.com"))
                uri = new Uri(uri.AbsoluteUri.Replace("https://www.reddit.com", "https://oauth.reddit.com"));

            return new Post().Init(this, GetToken(uri), WebAgent);
        }

        /// <summary>
        /// Create a Reddit Live thread.
        /// </summary>
        /// <param name="title">Required.</param>
        /// <param name="description">Required</param>
        /// <param name="resources"></param>
        /// <param name="nsfw"></param>
        /// <returns></returns>
        public LiveUpdateEvent CreateLiveEvent(string title,string description,string resources = "", bool nsfw = false)
        {
            if (String.IsNullOrEmpty(title))
                throw new ArgumentException(nameof(title));

            if (String.IsNullOrEmpty(description))
                throw new ArgumentException(nameof(description));

            var request = WebAgent.CreatePost(CreateLiveEventUrl);
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                title = title,
                description = description,
                resources = resources,
                nsfw = nsfw
            });
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);

            if (json["json"]["errors"].Any())
                throw new Exception(json["json"]["errors"][0][0].ToString());

            var id = json["json"]["data"]["id"].ToString();

            return GetLiveEvent(new Uri(String.Format(GetLiveEventUrl, id)));
        }

        /// <summary>
        /// Get a reddit live thread.
        /// </summary>
        /// <param name="uri">Uri of the live thread.</param>
        /// <returns></returns>
        public LiveUpdateEvent GetLiveEvent(Uri uri)
        {
            if (!uri.AbsoluteUri.EndsWith("about"))
                uri = new Uri(uri.AbsoluteUri + "/about");

            var token = GetToken(uri,true);
            return new LiveUpdateEvent().Init(this, token, WebAgent);
        }


        /// <summary>
        /// 

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
        public void ComposePrivateMessage(string subject, string body, string to, string fromSubReddit = "", string captchaId = "", string captchaAnswer = "")
        {
            if (User == null)
                throw new Exception("User can not be null.");

            if (!string.IsNullOrWhiteSpace(fromSubReddit))
            {
                var subReddit = this.GetSubreddit(fromSubReddit);
                var modNameList = subReddit.Moderators.Select(b => b.Name).ToList();

                if (!modNameList.Contains(User.Name))
                    throw new AuthenticationException(
                        string.Format(
                            @"User {0} is not a moderator of subreddit {1}.",
                            User.Name,
                            subReddit.Name));
            }

            var request = WebAgent.CreatePost(ComposeMessageUrl);
            WebAgent.WritePostBody(request.GetRequestStream(), new
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
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);

            ICaptchaSolver solver = CaptchaSolver; // Prevent race condition

            if (json["json"]["errors"].Any() && json["json"]["errors"][0][0].ToString() == "BAD_CAPTCHA" && solver != null)
            {
                captchaId = json["json"]["captcha"].ToString();
                CaptchaResponse captchaResponse = solver.HandleCaptcha(new Captcha(captchaId));

                if (!captchaResponse.Cancel) // Keep trying until we are told to cancel
                    ComposePrivateMessage(subject, body, to, fromSubReddit, captchaId, captchaResponse.Answer);
            }
            else if(json["json"]["errors"].Any())
            {
                throw new Exception("Error when composing message. Error: " + json["json"]["errors"][0][0].ToString());
            }
        }

        /// <summary>
        /// Registers a new Reddit user
        /// </summary>
        /// <param name="userName">The username for the new account.</param>
        /// <param name="passwd">The password for the new account.</param>
        /// <param name="email">The optional recovery email for the new account.</param>
        /// <returns>The newly created user account</returns>
        public AuthenticatedUser RegisterAccount(string userName, string passwd, string email = "")
        {
            var request = WebAgent.CreatePost(RegisterAccountUrl);
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                email = email,
                passwd = passwd,
                passwd2 = passwd,
                user = userName
            });
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);
            return new AuthenticatedUser().Init(this, json, WebAgent);
            // TODO: Error
        }

        /// <summary>
        /// Get a <see cref="Thing"/> by full name.
        /// </summary>
        /// <param name="fullname"></param>
        /// <returns></returns>
        public Thing GetThingByFullname(string fullname)
        {
            var request = WebAgent.CreateGet(string.Format(GetThingUrl, fullname));
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            return Thing.Parse(this, json["data"]["children"][0], WebAgent);
        }

        /// <summary>
        /// Get a <see cref="Comment"/>.
        /// </summary>
        /// <param name="subreddit">subreddit name in which the comment resides</param>
        /// <param name="name">comment base36 id</param>
        /// <param name="linkName">post base36 id</param>
        /// <returns></returns>
        public Comment GetComment(string subreddit, string name, string linkName)
        {
            try
            {
                if (linkName.StartsWith("t3_"))
                    linkName = linkName.Substring(3);
                if (name.StartsWith("t1_"))
                    name = name.Substring(3);

                var url = string.Format(GetCommentUrl, subreddit, linkName, name);
                return GetComment(new Uri(url));
            }
            catch (WebException)
            {
                return null;
            }
        }

        /// <summary>
        /// Get a <see cref="Comment"/>.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public Comment GetComment(Uri uri)
        {
            var url = string.Format(GetPostUrl, uri.AbsoluteUri);
            var request = WebAgent.CreateGet(url);
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);

            var sender = new Post().Init(this, json[0]["data"]["children"][0], WebAgent);
            return new Comment().Init(this, json[1]["data"]["children"][0], WebAgent, sender);
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
        /// <param name="author">The user who submitted the post</param>
        /// <param name="flair">The text of the link flair on the post.</param>
        /// <param name="nsfw">include NSFW posts.</param>
        /// <param name="self">includ eText post.</param>
        /// <param name="selfText">For self-posts, the body of the post.</param>
        /// <param name="site">The domain of the submitted URL.</param>
        /// <param name="subreddit">The submission's subreddit.</param>
        /// <param name="title">The submission title.</param>
        /// <param name="url">The submission's URL (the website's address)</param>
        /// <remarks>https://www.reddit.com/wiki/search#wiki_field_search</remarks>
        /// <returns></returns>
        public Listing<T> Search<T>(string query, 
            Sorting sortE = Sorting.Relevance, 
            TimeSorting timeE = TimeSorting.All, 
            string author = null,
            string flair = null,
            bool? nsfw = null,
            bool? self = null,
            string selfText = null,
            string site = null,
            string subreddit = null,
            string title = null,
            string url = null

            ) where T : Thing
        {

            StringBuilder queryBuilder = new StringBuilder(query);


            if (author != null)
            {
                queryBuilder.Append($"+author:{author}");
            }

            if (flair != null)
            {
                queryBuilder.Append($"+flair:{flair}");
            }

            if (nsfw != null)
            {
                queryBuilder.Append($"+nsfw:{Convert.ToInt16(nsfw.Value)}");
            }

            if (self != null)
            {
                queryBuilder.Append($"+self:{Convert.ToInt16(self.Value)}");
            }

            if (selfText != null)
            {
                queryBuilder.Append($"+selftext:{selfText}");
            }

            if (site != null)
            {
                queryBuilder.Append($"+site:{site}");
            }

            if (subreddit != null)
            {
                queryBuilder.Append($"+subreddit:{subreddit}");
            }

            if (title != null)
            {
                queryBuilder.Append($"+title:{title}");
            }

            if (url != null)
            {
                queryBuilder.Append($"+url:{url}");
            }
            string sort = sortE.ToString().ToLower();
            string time = timeE.ToString().ToLower();
            string final = string.Format(SearchUrl, queryBuilder.ToString(), sort, time);
            return new Listing<T>(this, final, WebAgent);
        }

        /// <summary>
        /// Return a <see cref="Listing{T}"/> of items matching search with a given time period.
        /// </summary>
        /// <typeparam name="T"><see cref="Thing"/></typeparam>
        /// <param name="from">When to begin. </param>
        /// <param name="to">When to end. </param>
        /// <param name="query">string to query</param>
        /// <param name="subreddit">subreddit in which to search</param>
        /// <param name="sortE">Order by <see cref="Sorting"/></param>
        /// <param name="timeE">Order by <see cref="TimeSorting"/></param>
        /// <returns></returns>
        [Obsolete("time search was discontinued by reddit", true)]
        public Listing<T> SearchByTimestamp<T>(DateTime from, DateTime to, string query = "", string subreddit = "", Sorting sortE = Sorting.Relevance, TimeSorting timeE = TimeSorting.All) where T : Thing
        {
            return SearchByTimestamp<T>(new DateTimeOffset(from), new DateTimeOffset(to), query, subreddit, sortE, timeE);
        }

        /// <summary>
        /// Return a <see cref="Listing{T}"/> of items matching search with a given time period.
        /// </summary>
        /// <typeparam name="T"><see cref="Thing"/></typeparam>
        /// <param name="from">When to begin. </param>
        /// <param name="to">When to end. </param>
        /// <param name="query">string to query</param>
        /// <param name="subreddit">subreddit in which to search</param>
        /// <param name="sortE">Order by <see cref="Sorting"/></param>
        /// <param name="timeE">Order by <see cref="TimeSorting"/></param>
        /// <returns></returns>
        [Obsolete("time search was discontinued by reddit", true)]
        public Listing<T> SearchByTimestamp<T>(DateTimeOffset from, DateTimeOffset to, string query = "", string subreddit = "", Sorting sortE = Sorting.Relevance, TimeSorting timeE = TimeSorting.All) where T : Thing
        {
            string sort = sortE.ToString().ToLower();
            string time = timeE.ToString().ToLower();

            var fromUnix = from.ToUnixTimeSeconds();
            var toUnix = to.ToUnixTimeSeconds();

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
            var response = await request.GetResponseAsync();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            var ret = await Thing.ParseAsync(this, json, WebAgent);
            return (T)ret;
        }
        protected internal T GetThing<T>(string url) where T : Thing
        {
            var request = WebAgent.CreateGet(url);
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            var ret = Thing.Parse(this, json, WebAgent);
            return (T)ret;
        }
        #endregion
    }
}
