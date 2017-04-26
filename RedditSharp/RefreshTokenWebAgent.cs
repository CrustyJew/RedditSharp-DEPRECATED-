using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedditSharp
{
    /// <summary>
    /// WebAgent supporting OAuth.
    /// </summary>
    public class RefreshTokenWebAgent : WebAgent
    {
        /// <summary>
        /// Minutes before token expiration to get a new token. Defaults to 5
        /// </summary>
        public int RenewTokenThreshold { get; set; }
        //private so it doesn't leak app secret to other code
        private AuthProvider TokenProvider;
        private string RefreshToken;

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
        public RefreshTokenWebAgent(string refreshToken, string clientID, string clientSecret, string redirectURI, string accessToken = "", DateTime? validTo = null, RateLimitManager rateLimiter = null):base(accessToken,rateLimiter)
        {
            RefreshToken = refreshToken;
            AccessToken = accessToken;
            TokenValidTo = validTo.HasValue && !string.IsNullOrWhiteSpace(accessToken) ? validTo.Value : DateTime.UtcNow.AddMinutes(-1);
            RootDomain = "oauth.reddit.com";
            RenewTokenThreshold = 5;
            TokenProvider = new AuthProvider(clientID, clientSecret, redirectURI, this);
        }

        /// <inheritdoc/>
        public override HttpRequestMessage CreateRequest(string url, string method)
        {
            if (url != AuthProvider.AccessUrl && DateTime.UtcNow.AddMinutes(RenewTokenThreshold) > TokenValidTo)
            {
                Task.Run(GetNewTokenAsync).Wait();
            }
            return base.CreateRequest(url, method);
        }

        /// <inheritdoc/>
        protected override HttpRequestMessage CreateRequest(Uri uri, string method)
        {
            if (uri.ToString() != AuthProvider.AccessUrl && DateTime.UtcNow.AddMinutes(RenewTokenThreshold) > TokenValidTo)
            {
                Task.Run(GetNewTokenAsync).Wait();
            }
            return base.CreateRequest(uri, method);
        }

        private async Task GetNewTokenAsync()
        {
            AccessToken = await TokenProvider.GetOAuthTokenAsync(RefreshToken,true).ConfigureAwait(false);
            TokenValidTo = DateTime.UtcNow.AddHours(1);
        }
    }

}
