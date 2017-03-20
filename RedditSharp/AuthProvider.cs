using Newtonsoft.Json.Linq;
using RedditSharp.Things;
using System;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace RedditSharp
{
    /// <summary>
    /// Manages OAuth connections to reddit.
    /// </summary>
    public class AuthProvider
    {
        /// <summary>
        /// Access token request url.
        /// </summary>
        public const string AccessUrl = "https://ssl.reddit.com/api/v1/access_token";
        private const string OauthGetMeUrl = "https://oauth.reddit.com/api/v1/me";
        private const string RevokeUrl = "https://www.reddit.com/api/v1/revoke_token";

        /// <summary>
        /// OAuth2 token.
        /// </summary>
        public static string OAuthToken { get; set; }

        /// <summary>
        /// OAuth2 refresh token.
        /// </summary>
        public static string RefreshToken { get; set; }

#pragma warning disable 1591
        [Flags]
        public enum Scope
        {
            none = 0x0,
            identity = 0x1,
            edit = 0x2,
            flair = 0x4,
            history = 0x8,
            modconfig = 0x10,
            modflair = 0x20,
            modlog = 0x40,
            modposts = 0x80,
            modwiki = 0x100,
            mysubreddits = 0x200,
            privatemessages = 0x400,
            read = 0x800,
            report = 0x1000,
            save = 0x2000,
            submit = 0x4000,
            subscribe = 0x8000,
            vote = 0x10000,
            wikiedit = 0x20000,
            wikiread = 0x40000
        }
#pragma warning restore 1591

        private IWebAgent _webAgent;
        private readonly string _redirectUri;
        private readonly string _clientId;
        private readonly string _clientSecret;

        /// <summary>
        /// Allows use of reddit's OAuth interface, using an app set up at https://ssl.reddit.com/prefs/apps/.
        /// </summary>
        /// <param name="clientId">Granted by reddit as part of app.</param>
        /// <param name="clientSecret">Granted by reddit as part of app.</param>
        /// <param name="redirectUri">Selected as part of app. Reddit will send users back here.</param>
        public AuthProvider(string clientId, string clientSecret, string redirectUri)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUri = redirectUri;
            _webAgent = new WebAgent();
        }
        /// <summary>
        /// Allows use of reddit's OAuth interface, using an app set up at https://ssl.reddit.com/prefs/apps/.
        /// </summary>
        /// <param name="clientId">Granted by reddit as part of app.</param>
        /// <param name="clientSecret">Granted by reddit as part of app.</param>
        /// <param name="redirectUri">Selected as part of app. Reddit will send users back here.</param>
        /// <param name="agent">Implementation of IWebAgent to use to make requests.</param>
        public AuthProvider(string clientId, string clientSecret, string redirectUri,IWebAgent agent)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUri = redirectUri;
            _webAgent = agent;
        }

        /// <summary>
        /// Creates the reddit OAuth2 Url to redirect the user to for authorization.
        /// </summary>
        /// <param name="state">Used to verify that the user received is the user that was sent</param>
        /// <param name="scope">Determines what actions can be performed against the user.</param>
        /// <param name="permanent">Set to true for access lasting longer than one hour.</param>
        /// <returns></returns>
        public string GetAuthUrl(string state, Scope scope, bool permanent = false)
        {
            return string.Format("https://ssl.reddit.com/api/v1/authorize?client_id={0}&response_type=code&state={1}&redirect_uri={2}&duration={3}&scope={4}", _clientId, state, _redirectUri, permanent ? "permanent" : "temporary", scope.ToString().Replace(" ",""));
        }

        /// <summary>
        /// Gets the OAuth token for the user associated with the provided code.
        /// </summary>
        /// <param name="code">Sent by reddit as a parameter in the return uri.</param>
        /// <param name="isRefresh">Set to true for refresh requests.</param>
        /// <returns></returns>
        public async Task<string> GetOAuthTokenAsync(string code, bool isRefresh = false)
        {
            //TODO test mono and make sure this works without security issues. Shouldn't be handled by library, should require install of cert or at least explicit calls to ingore certs
            //if (Type.GetType("Mono.Runtime") != null)
            //    ServicePointManager.ServerCertificateValidationCallback = (s, c, ch, ssl) => true;

            var json = await _webAgent.ExecuteRequestAsync(() => {
                  var request = _webAgent.CreateRequest(AccessUrl, "POST");
                  request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(_clientId + ":" + _clientSecret)));
                  if (isRefresh)
                  {
                      _webAgent.WritePostBody(request, new
                      {
                          grant_type = "refresh_token",
                          refresh_token = code
                      });
                  }
                  else
                  {
                      _webAgent.WritePostBody(request, new
                      {
                          grant_type = "authorization_code",
                          code,
                          redirect_uri = _redirectUri
                      });
                  }
                  return request;
                }).ConfigureAwait(false);
            if (json["access_token"] != null)
            {
                if (json["refresh_token"] != null)
                    RefreshToken = json["refresh_token"].ToString();
                OAuthToken = json["access_token"].ToString();
                return json["access_token"].ToString();
            }
            throw new AuthenticationException("Could not log in.");
        }

        /// <summary>
        /// Gets the OAuth token for the user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>The access token</returns>
        public async Task<string> GetOAuthTokenAsync(string username, string password)
        {
            //TODO same as above
            //if (Type.GetType("Mono.Runtime") != null)
            //    ServicePointManager.ServerCertificateValidationCallback = (s, c, ch, ssl) => true;
            //_webAgent.Cookies = new CookieContainer();


            var json = await _webAgent.ExecuteRequestAsync(() => {
                  var request = _webAgent.CreateRequest(AccessUrl, "POST");
                  request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(_clientId + ":" + _clientSecret)));
                  _webAgent.WritePostBody(request, new
                  {
                      grant_type = "password",
                      username,
                      password,
                      redirect_uri = _redirectUri
                  });
                  return request;
                }).ConfigureAwait(false);
            if (json["access_token"] != null)
            {
                if (json["refresh_token"] != null)
                    RefreshToken = json["refresh_token"].ToString();
                OAuthToken = json["access_token"].ToString();
                return json["access_token"].ToString();
            }
            throw new AuthenticationException("Could not log in.");
        }

        /// <summary>
        /// revokes an oauth token
        /// </summary>
        /// <param name="token">The oauth token..</param>
        /// <param name="isRefresh">Set to true for refresh token.</param>
        /// <returns>The access token</returns>
        public async Task RevokeTokenAsync(string token, bool isRefresh)
        {
            string tokenType = isRefresh ? "refresh_token" : "access_token";
            await _webAgent.ExecuteRequestAsync(() => {
                  var request = _webAgent.CreateRequest(RevokeUrl, "POST");
                  request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(_clientId + ":" + _clientSecret)));
                  _webAgent.WritePostBody(request, new {
                      token = token,
                      token_type = tokenType
                  });
                  return request;
                }).ConfigureAwait(false);
        }
    }
}
