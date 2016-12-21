using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Web;

namespace RedditSharp
{
    public class WebAgent : IWebAgent
    {
        /// <summary>
        /// Additional values to append to the default RedditSharp user agent.
        /// </summary>
        public static string UserAgent { get; set; }

        /// <summary>
        /// It is strongly advised that you leave this enabled. Reddit bans excessive
        /// requests with extreme predjudice.
        /// </summary>
        public static bool EnableRateLimit { get; set; }

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

        public CookieContainer Cookies { get; set; }
        public string AuthCookie { get; set; }

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
            UserAgent = "";
            RateLimit = RateLimitMode.Pace;
            Protocol = "https";
            RootDomain = "www.reddit.com";
        }

        public virtual JToken CreateAndExecuteRequest(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                if (!Uri.TryCreate(string.Format("{0}://{1}{2}", Protocol, RootDomain, url), UriKind.Absolute, out uri))
                    throw new Exception("Could not parse Uri");
            }
            var request = CreateGet(uri);
            try { return ExecuteRequest(request); }
            catch (Exception)
            {
                var tempProtocol = Protocol;
                var tempRootDomain = RootDomain;
                Protocol = "http";
                RootDomain = "www.reddit.com";
                var retval = CreateAndExecuteRequest(url);
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
        public virtual JToken ExecuteRequest(HttpWebRequest request)
        {
            EnforceRateLimit();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var result = GetResponseString(response.GetResponseStream());

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
                json = JToken.Parse("{'method':'" + response.Method + "','uri':'" + response.ResponseUri.AbsoluteUri + "','status':'" + response.StatusCode.ToString() + "'}");
            }
            return json;

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected virtual void EnforceRateLimit()
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

        public virtual HttpWebRequest CreateRequest(string url, string method)
        {
            EnforceRateLimit();
            bool prependDomain = !Uri.IsWellFormedUriString(url, UriKind.Absolute);
            HttpWebRequest request;
            if (prependDomain)
                request = (HttpWebRequest)WebRequest.Create(string.Format("{0}://{1}{2}", Protocol, RootDomain, url));
            else
                request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = Cookies;
            if (IsOAuth() && request.Host.ToLower() == "oauth.reddit.com")// use OAuth
            {
                request.Headers.Set("Authorization", "bearer " + AccessToken);//Must be included in OAuth calls
            }
            request.Method = method;
            request.UserAgent = UserAgent + " - with RedditSharp by /u/meepster23";
            return request;
        }

        protected virtual HttpWebRequest CreateRequest(Uri uri, string method)
        {
            EnforceRateLimit();
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.CookieContainer = Cookies;
            if (Type.GetType("Mono.Runtime") != null)
            {
                var cookieHeader = Cookies.GetCookieHeader(new Uri("http://reddit.com"));
                request.Headers.Set("Cookie", cookieHeader);
            }
            if (IsOAuth() && uri.Host.ToLower() == "oauth.reddit.com")// use OAuth
            {
                request.Headers.Set("Authorization", "bearer " + AccessToken);//Must be included in OAuth calls
            }
            request.Method = method;
            request.UserAgent = UserAgent + " - with RedditSharp by /u/meepster23";
            return request;
        }

        public virtual HttpWebRequest CreateGet(string url)
        {
            return CreateRequest(url, "GET");
        }

        private HttpWebRequest CreateGet(Uri url)
        {
            return CreateRequest(url, "GET");
        }

        public virtual HttpWebRequest CreatePost(string url)
        {
            var request = CreateRequest(url, "POST");
            request.ContentType = "application/x-www-form-urlencoded";
            return request;
        }

        public virtual HttpWebRequest CreatePut(string url)
        {
            var request = CreateRequest(url, "PUT");
            request.ContentType = "application/x-www-form-urlencoded";
            return request;
        }

        public virtual HttpWebRequest CreateDelete(string url)
        {
            var request = CreateRequest(url, "DELETE");
            request.ContentType = "application/x-www-form-urlencoded";
            return request;
        }

        public virtual string GetResponseString(Stream stream)
        {
            var data = new StreamReader(stream).ReadToEnd();
            stream.Close();
            return data;
        }

        public virtual void WritePostBody(Stream stream, object data, params string[] additionalFields)
        {
            var type = data.GetType();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            string value = "";
            foreach (var property in properties)
            {
                var attr = property.GetCustomAttributes(typeof(RedditAPINameAttribute), false).FirstOrDefault() as RedditAPINameAttribute;
                string name = attr == null ? property.Name : attr.Name;
                var entry = Convert.ToString(property.GetValue(data, null));
                value += name + "=" + HttpUtility.UrlEncode(entry).Replace(";", "%3B").Replace("&", "%26") + "&";
            }
            for (int i = 0; i < additionalFields.Length; i += 2)
            {
                var entry = Convert.ToString(additionalFields[i + 1]) ?? string.Empty;
                value += additionalFields[i] + "=" + HttpUtility.UrlEncode(entry).Replace(";", "%3B").Replace("&", "%26") + "&";
            }
            value = value.Remove(value.Length - 1); // Remove trailing &
            var raw = Encoding.UTF8.GetBytes(value);
            stream.Write(raw, 0, raw.Length);
            stream.Close();
        }

        private static bool IsOAuth()
        {
            return RootDomain == "oauth.reddit.com";
        }
    }
}
