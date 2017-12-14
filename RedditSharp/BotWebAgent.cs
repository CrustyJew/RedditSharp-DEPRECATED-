using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RedditSharp
{
    public class BotWebAgent : WebAgent
    {
        //private so it doesn't leak app secret to other code
        private AuthProvider TokenProvider;
        private string Username;
        private string Password;

        /// <summary>
        /// Date and time the token expires.
        /// </summary>
        public DateTimeOffset TokenValidTo { get; set; }

        /// <summary>
        /// A web agent using reddit's OAuth interface.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="clientId">Granted by reddit as part of app.</param>
        /// <param name="clientSecret">Granted by reddit as part of app.</param>
        /// <param name="redirectUri">Selected as part of app. Reddit will send users back here.</param>
        public BotWebAgent(string username, string password, string clientID, string clientSecret, string redirectURI)
        {
            Username = username;
            Password = password;
            EnableRateLimit = true;
            RateLimit = RateLimitMode.Burst;
            RootDomain = "oauth.reddit.com";
            TokenProvider = new AuthProvider(clientID, clientSecret, redirectURI, this);
            GetNewToken();
        }

        /// <inheritdoc/>
        public override HttpWebRequest CreateRequest(string url, string method)
        {
            //add 5 minutes for clock skew to ensure requests succeed 
            if (url != AuthProvider.AccessUrl && DateTimeOffset.UtcNow.AddMinutes(5) > TokenValidTo)
            {
                GetNewToken();
            }
            return base.CreateRequest(url, method);
        }

        /// <inheritdoc/>
        protected override HttpWebRequest CreateRequest(Uri uri, string method)
        {
            //add 5 minutes for clock skew to ensure requests succeed
            if (uri.ToString() != AuthProvider.AccessUrl && DateTimeOffset.UtcNow.AddMinutes(5) > TokenValidTo)
            {
                GetNewToken();
            }
            return base.CreateRequest(uri, method);
        }

        private void GetNewToken()
        {
            AccessToken = TokenProvider.GetOAuthToken(Username, Password);
            TokenValidTo = DateTimeOffset.UtcNow.AddHours(1);
        }
    }

}
