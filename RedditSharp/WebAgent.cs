using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace RedditSharp
{
    /// <inheritdoc />
    public class WebAgent : IWebAgent
    {

        private const string OAuthDomainUrl = "oauth.reddit.com";
        private static HttpClient _httpClient;

        /// <summary>
        /// Additional values to append to the default RedditSharp user agent.
        /// </summary>
        public static string DefaultUserAgent { get; set; } = "RedditSharp";

        /// <summary>
        /// web protocol "http", "https"
        /// </summary>
        public static string Protocol { get; set; }

        /// <summary>
        /// The root domain RedditSharp uses to address Reddit.
        /// www.reddit.com by default
        /// </summary>
        public static string RootDomain { get; set; }

        /// <summary>
        /// Global default RateLimitManager
        /// </summary>
        public static IRateLimiter DefaultRateLimiter { get; private set; }

        /// <summary>
        /// RateLimitManager for this Reddit instance.
        /// </summary>
        public IRateLimiter RateLimiter { get; set; }

        /// <inheritdoc />
        public string AccessToken { get; set; }

        /// <summary>
        /// String to override default useragent for this instance
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Append the library to the end of the User-Agent string. Default is true.
        /// </summary>
        public bool AppendLibraryToUA { get; set; } = true;

        private static readonly bool IsMono = Type.GetType("Mono.Runtime") != null;
        private static bool IsOAuth => RootDomain == "oauth.reddit.com";

        static WebAgent() {
            //Static constructors are dumb, no likey -Meepster23
            DefaultUserAgent = string.IsNullOrWhiteSpace( DefaultUserAgent ) ? "" : DefaultUserAgent ;
            Protocol = string.IsNullOrWhiteSpace(Protocol) ? "https" : Protocol;
            RootDomain = string.IsNullOrWhiteSpace(RootDomain) ? "www.reddit.com" : RootDomain;
            DefaultRateLimiter = new RateLimitManager();
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Creates WebAgent with default rate limiter and default useragent.
        /// </summary>
        public WebAgent() {
            UserAgent = DefaultUserAgent;
            RateLimiter = WebAgent.DefaultRateLimiter;
        }

        /// <summary>
        /// Intializes a WebAgent with a specified access token and optional <see cref="RateLimitManager"/>. Sets the default url to the oauth api address
        /// </summary>
        /// <param name="accessToken">Valid access token</param>
        /// <param name="rateLimiter"><see cref="RateLimitManager"/> that controls the rate limit for this instance of the WebAgent. Defaults to the shared, static rate limiter.</param>
        /// <param name="userAgent">Optional userAgent string to override default UserAgent</param>
        public WebAgent( string accessToken, IRateLimiter rateLimiter = null, string userAgent = "") {
            RootDomain = OAuthDomainUrl;
            AccessToken = accessToken;
            if(rateLimiter == null)
            {
                RateLimiter = WebAgent.DefaultRateLimiter;
            }
            else
            {
                RateLimiter = rateLimiter;
            }
            if (string.IsNullOrEmpty(userAgent))
            {
                UserAgent = DefaultUserAgent;
            }
            else
            {
                UserAgent = userAgent;
            }
        }

        /// <inheritdoc />
        public virtual async Task<JToken> ExecuteRequestAsync(Func<HttpRequestMessage> request)
        {
            if (request == null)
              throw new ArgumentNullException(nameof(request));
            const int maxTries = 20;
            HttpResponseMessage response;
            var tries = 0;
            do {
              await RateLimiter.CheckRateLimitAsync(IsOAuth).ConfigureAwait(false);
              response = await _httpClient.SendAsync(request()).ConfigureAwait(false);
              await RateLimiter.ReadHeadersAsync(response);
              ++tries;
            } while( 
            // only retry if 500, 502, 503, or 504
                (response.StatusCode == System.Net.HttpStatusCode.InternalServerError || 
                 response.StatusCode == System.Net.HttpStatusCode.BadGateway ||
                 response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                 response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
                && tries < maxTries
            );
            if (!response.IsSuccessStatusCode)
              throw new RedditHttpException(response.StatusCode);
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

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
                    if (json["errors"] != null)
                    {
                        switch (json["errors"].ToString())
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
                            default:
                                throw new Exception($"Error return by reddit: {json["errors"][0][0].ToString()}");
                        }
                    }
                }
                catch
                {
                }
            }
            else
            {
                json = JToken.Parse($"{{'method':'{response.RequestMessage.Method}','uri':'{response.RequestMessage.RequestUri.AbsoluteUri}','status':'{response.StatusCode.ToString()}'}}");
            }
            return json;
        }

        /// <inheritdoc />
        public virtual HttpRequestMessage CreateRequest(string url, string method)
        {
            bool prependDomain;
            // IsWellFormedUristring returns true on Mono for some reason when using a string like "/api/me"
            if (IsMono)
                prependDomain = !url.StartsWith("http://") && !url.StartsWith("https://");
            else
                prependDomain = !Uri.IsWellFormedUriString(url, UriKind.Absolute);

            Uri uri;
            if (prependDomain)
                uri = new Uri(string.Format("{0}://{1}{2}", Protocol, RootDomain, url));
            else
                uri = new Uri(url);

            return CreateRequest(uri, method);
        }

        /// <inheritdoc />
        protected virtual HttpRequestMessage CreateRequest(Uri uri, string method)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = uri
            };
            if (IsOAuth)// use OAuth
                request.Headers.Authorization = new AuthenticationHeaderValue("bearer", AccessToken);//Must be included in OAuth calls

            request.Method = new HttpMethod(method);
            //request.Headers.UserAgent.ParseAdd(UserAgent);
            var userAgentString = AppendLibraryToUA ? $"{UserAgent} - with RedditSharp by meepster23" : UserAgent;
            request.Headers.TryAddWithoutValidation("User-Agent", userAgentString);
            return request;
        }

        /// <inheritdoc />
        public Task<JToken> Get(string url) => ExecuteRequestAsync(() => CreateRequest(url, "GET"));

        /// <inheritdoc />
        public Task<JToken> Post(string url, object data, params string[] additionalFields)
        {
            return ExecuteRequestAsync(() => {
                  var request = CreateRequest(url, "POST");
                  WritePostBody(request, data, additionalFields);
                  return request;
                });
        }

        /// <inheritdoc />
        public Task<JToken> Put(string url, object data)
        {
            return ExecuteRequestAsync(() => {
                  var request = CreateRequest(url, "PUT");
                  WritePostBody(request, data);
                  return request;
                });
        }

        /// <inheritdoc />
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
                var entry = Convert.ToString(additionalFields[i + 1]);
                content.Add(new KeyValuePair<string, string>(additionalFields[i], entry));
            }
            //new FormUrlEncodedContent has a limit on length which can cause issues;
            request.Content = new StringContent(string.Join("&",content.Select(c=>c.Key + "=" + System.Net.WebUtility.UrlEncode(c.Value))), System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage message) {
          return _httpClient.SendAsync(message);
        }


    }
}
