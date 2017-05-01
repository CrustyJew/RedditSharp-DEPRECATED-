using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedditSharp
{
    /// <summary>
    /// WebAgent supporting OAuth.
    /// </summary>
    public class BotWebAgent : WebAgent
    {
        //private so it doesn't leak app secret to other code
        private AuthProvider TokenProvider;
        private string Username;
        private string Password;

        /// <summary>
        /// DateTime the token expires.
        /// </summary>
        public DateTime TokenValidTo { get; set; }

        /// <summary>
        /// A web agent using reddit's OAuth interface.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="clientID">Granted by reddit as part of app.</param>
        /// <param name="clientSecret">Granted by reddit as part of app.</param>
        /// <param name="redirectURI">Selected as part of app. Reddit will send users back here.</param>
        public BotWebAgent(string username, string password, string clientID, string clientSecret, string redirectURI)
        {
            Username = username;
            Password = password;
            RateLimiter.Mode = RateLimitMode.Burst;
            RootDomain = "oauth.reddit.com";
            TokenProvider = new AuthProvider(clientID, clientSecret, redirectURI, this);
            Task.Run(GetNewTokenAsync).Wait();
        }

        /// <inheritdoc/>
        public override HttpRequestMessage CreateRequest(string url, string method)
        {
            //add 5 minutes for clock skew to ensure requests succeed
            if (url != AuthProvider.AccessUrl && DateTime.UtcNow.AddMinutes(5) > TokenValidTo)
            {
                Task.Run(GetNewTokenAsync).Wait();
            }
            return base.CreateRequest(url, method);
        }

        /// <inheritdoc/>
        protected override HttpRequestMessage CreateRequest(Uri uri, string method)
        {
            //add 5 minutes for clock skew to ensure requests succeed
            if (uri.ToString() != AuthProvider.AccessUrl && DateTime.UtcNow.AddMinutes(5) > TokenValidTo)
            {
                Task.Run(GetNewTokenAsync).Wait();
            }
            return base.CreateRequest(uri, method);
        }

        private async Task GetNewTokenAsync()
        {
            AccessToken = await TokenProvider.GetOAuthTokenAsync(Username, Password).ConfigureAwait(false);
            TokenValidTo = DateTime.UtcNow.AddHours(1);
        }
    }

}
