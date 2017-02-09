using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;

namespace RedditSharp
{
    public class WebAgent : IWebAgent
    {

        private const string OAuthDomainUrl = "oauth.reddit.com";
        private static HttpClient _httpClient;
        private object rateLimitLock = new object();
        /// <summary>
        /// Additional values to append to the default RedditSharp user agent.
        /// </summary>
        public static string UserAgent { get; set; }

        /// <summary>
        /// It is strongly advised that you leave this enabled. Reddit bans excessive
        /// requests with extreme predjudice.
        /// </summary>
        public static bool EnableRateLimit { get; set; }

        /// <summary>
        /// web protocol "http", "https"
        /// </summary>
        public static string Protocol { get; set; }

        /// <summary>
        /// It is strongly advised that you leave this set to Burst or Pace. Reddit bans excessive
        /// requests with extreme predjudice.
        /// </summary>
        public static RateLimitMode RateLimit { get; set; }

        /// <summary>
        /// The method by which the WebAgent will limit request rate
        /// </summary>
        public enum RateLimitMode
        {
            /// <summary>
            /// Limits requests to one every two seconds (one if OAuth)
            /// </summary>
            Pace,
            /// <summary>
            /// Restricts requests to five per ten seconds (ten if OAuth)
            /// </summary>
            SmallBurst,
            /// <summary>
            /// Restricts requests to thirty per minute (sixty if OAuth)
            /// </summary>
            Burst,
            /// <summary>
            /// Does not restrict request rate. ***NOT RECOMMENDED***
            /// </summary>
            None
        }

        /// <summary>
        /// The root domain RedditSharp uses to address Reddit.
        /// www.reddit.com by default
        /// </summary>
        public static string RootDomain { get; set; }

        /// <summary>
        /// Used to make calls against Reddit's API using OAuth2
        /// </summary>
        public string AccessToken { get; set; }

        private static DateTime _lastRequest;
        private static DateTime _burstStart;
        private static int _requestsThisBurst;
        /// <summary>
        /// UTC DateTime of last request made to Reddit API
        /// </summary>
        public DateTime LastRequest
        {
            get { return _lastRequest; }
        }
        /// <summary>
        /// UTC DateTime of when the last burst started
        /// </summary>
        public DateTime BurstStart
        {
            get { return _burstStart; }
        }
        /// <summary>
        /// Number of requests made during the current burst
        /// </summary>
        public int RequestsThisBurst
        {
            get { return _requestsThisBurst; }
        }


        static WebAgent()
        {
            //Static constructors are dumb, no likey -Meepster23
            UserAgent = string.IsNullOrWhiteSpace( UserAgent ) ? "" : UserAgent ;
            Protocol = string.IsNullOrWhiteSpace(Protocol) ? "https" : Protocol;
            RootDomain = string.IsNullOrWhiteSpace(RootDomain) ? "www.reddit.com" : RootDomain;
            _httpClient = new HttpClient();
        }
        public WebAgent() {

        }
        /// <summary>
        /// Intializes a WebAgent with a specified access token and sets the default url to the oauth api address
        /// </summary>
        /// <param name="accessToken">Valid access token</param>
        public WebAgent( string accessToken ) {
            RootDomain = OAuthDomainUrl;
            AccessToken = accessToken;
        }

        /// <summary>
        /// Execute a request and return a <see cref="JToken"/>
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public virtual async Task<JToken> CreateAndExecuteRequestAsync(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                if (!Uri.TryCreate(string.Format("{0}://{1}{2}", Protocol, RootDomain, url), UriKind.Absolute, out uri))
                    throw new Exception("Could not parse Uri");
            }
            var request = CreateRequest(uri.ToString(), "GET");
            try { return await ExecuteRequestAsync(request); }
            //What the hell is going on here?! Why is this a thing? -Meepster23
            catch (Exception)
            {
                var tempProtocol = Protocol;
                var tempRootDomain = RootDomain;
                Protocol = "http";
                RootDomain = "www.reddit.com";
                var retval = await CreateAndExecuteRequestAsync(url);
                Protocol = tempProtocol;
                RootDomain = tempRootDomain;
                return retval;
            }
        }

        /// <summary>
        /// Executes the web request and handles errors in the response
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual async Task<JToken> ExecuteRequestAsync(HttpRequestMessage request)
        {
            EnforceRateLimit();
            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            JToken json;
            if (!string.IsNullOrEmpty(result))
            {
                json = JToken.Parse(result);
                try
                {
                    if (json["json"] != null)
                    {
                        json = json["json"]; //get json object if there is a root node
                    }
                    if (json["error"] != null)
                    {
                        switch (json["error"].ToString())
                        {
                            case "404":
                                throw new Exception("File Not Found");
                            case "403":
                                throw new Exception("Restricted");
                            case "invalid_grant":
                                //Refresh authtoken
                                //AccessToken = authProvider.GetRefreshToken();
                                //ExecuteRequest(request);
                                break;
                        }
                    }
                }
                catch
                {
                }
            }
            else
            {
                json = JToken.Parse("{'method':'" + response.RequestMessage.Method + "','uri':'" + response.RequestMessage.RequestUri.AbsoluteUri + "','status':'" + response.StatusCode.ToString() + "'}");
            }
            return json;

        }

        /// <summary>
        /// Enforce the api throttle.
        /// </summary>
        protected virtual void EnforceRateLimit()
        {
            lock (rateLimitLock)
            {
                var limitRequestsPerMinute = IsOAuth() ? 60.0 : 30.0;
                switch (RateLimit)
                {
                    case RateLimitMode.Pace:
                        while ((DateTime.UtcNow - _lastRequest).TotalSeconds < 60.0 / limitRequestsPerMinute)// Rate limiting
                            Thread.Sleep(250);
                        _lastRequest = DateTime.UtcNow;
                        break;
                    case RateLimitMode.SmallBurst:
                        if (_requestsThisBurst == 0 || (DateTime.UtcNow - _burstStart).TotalSeconds >= 10) //this is first request OR the burst expired
                        {
                            _burstStart = DateTime.UtcNow;
                            _requestsThisBurst = 0;
                        }
                        if (_requestsThisBurst >= limitRequestsPerMinute / 6.0) //limit has been reached
                        {
                            while ((DateTime.UtcNow - _burstStart).TotalSeconds < 10)
                                Thread.Sleep(250);
                            _burstStart = DateTime.UtcNow;
                            _requestsThisBurst = 0;
                        }
                        _lastRequest = DateTime.UtcNow;
                        _requestsThisBurst++;
                        break;
                    case RateLimitMode.Burst:
                        if (_requestsThisBurst == 0 || (DateTime.UtcNow - _burstStart).TotalSeconds >= 60) //this is first request OR the burst expired
                        {
                            _burstStart = DateTime.UtcNow;
                            _requestsThisBurst = 0;
                        }
                        if (_requestsThisBurst >= limitRequestsPerMinute) //limit has been reached
                        {
                            while ((DateTime.UtcNow - _burstStart).TotalSeconds < 60)
                                Thread.Sleep(250);
                            _burstStart = DateTime.UtcNow;
                            _requestsThisBurst = 0;
                        }
                        _lastRequest = DateTime.UtcNow;
                        _requestsThisBurst++;
                        break;
                }
            }
        }

        /// <summary>
        /// Create a <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="url">target  uri</param>
        /// <param name="method">http method</param>
        /// <returns></returns>
        public virtual HttpRequestMessage CreateRequest(string url, string method)
        {
            EnforceRateLimit();
            bool prependDomain;
            // IsWellFormedUristring returns true on Mono for some reason when using a string like "/api/me"
            if (Type.GetType("Mono.Runtime") != null)
                prependDomain = !url.StartsWith("http://") && !url.StartsWith("https://");
            else
                prependDomain = !Uri.IsWellFormedUriString(url, UriKind.Absolute);

            HttpRequestMessage request = new HttpRequestMessage();
            if (prependDomain)
            {
                request.RequestUri = new Uri(string.Format("{0}://{1}{2}", Protocol, RootDomain, url));
            }
            else
            {
                request.RequestUri = new Uri(url);
            }

            if (IsOAuth())// use OAuth
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", AccessToken);//Must be included in OAuth calls
            }

            request.Method = new HttpMethod(method);
            request.Headers.UserAgent.ParseAdd(UserAgent + " - with RedditSharp by meepster23");
            return request;
        }

        /// <summary>
        /// Create a <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="uri">target  uri</param>
        /// <param name="method">http method</param>
        /// <returns></returns>
        protected virtual HttpRequestMessage CreateRequest(Uri uri, string method)
        {
            EnforceRateLimit();
            var request = new HttpRequestMessage();
            request.RequestUri = uri;
            if (IsOAuth())// use OAuth

            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", AccessToken);//Must be included in OAuth calls
            }

            request.Method = new HttpMethod(method);
            request.Headers.UserAgent.ParseAdd(UserAgent + " - with RedditSharp by /u/meepster23");
            return request;
        }

        public async Task<JToken> Get(string url) {
            var request = CreateRequest(url, "GET");
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) {
            }
            return JToken.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task<JToken> Post(string url, object data, params string[] additionalFields) {
            var request = CreateRequest(url, "POST");
            WritePostBody(request, data, additionalFields);
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) {
            }
            return JToken.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task<JToken> Put(string url, object data) {
            var request = CreateRequest(url, "POST");
            WritePostBody(request, data);
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) {
            }
            return JToken.Parse(await response.Content.ReadAsStringAsync());
        }

        public virtual void WritePostBody(HttpRequestMessage request, object data, params string[] additionalFields)
        {
            var type = data.GetType();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var content = new List<KeyValuePair<string, string>>();
            foreach (var property in properties)
            {
                var attr = property.GetCustomAttributes(typeof(RedditAPINameAttribute), false).FirstOrDefault() as RedditAPINameAttribute;
                string name = attr == null ? property.Name : attr.Name;
                var entry = Convert.ToString(property.GetValue(data, null));
                content.Add(new KeyValuePair<string,string>(name, entry));
            }
            for (int i = 0; i < additionalFields.Length; i += 2)
            {
                var entry = Convert.ToString(additionalFields[i + 1]) ?? string.Empty;
                content.Add(new KeyValuePair<string, string>(additionalFields[i], entry));
            }

            request.Content = new FormUrlEncodedContent(content);
        }

        public Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage message) {
          return _httpClient.SendAsync(message);
        }

        private static bool IsOAuth() => RootDomain == "oauth.reddit.com";
    }
}
