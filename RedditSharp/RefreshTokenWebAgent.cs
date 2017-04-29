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
        internal string RefreshToken { get; set; }

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
        public RefreshTokenWebAgent(string refreshToken, string clientID, string clientSecret, string redirectURI, string userAgentString = "", string accessToken = "", DateTime? validTo = null, RateLimitManager rateLimiter = null):base(accessToken,rateLimiter,userAgentString)
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

        /// <summary>
        /// Set the current refresh token for this web agent. Recommended to call <see cref="GetNewTokenAsync"/> afterwards.
        /// </summary>
        /// <param name="refreshToken"></param>
        public virtual void SetRefreshToken(string refreshToken)
        {
            RefreshToken = refreshToken;
        }

        /// <summary>
        /// This will permanently revoke the OAuth Refresh Token that is currently assigned to this web agent.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> RevokeRefreshTokenAsync()
        {
            try
            {
                await TokenProvider.RevokeTokenAsync(RefreshToken, true);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Forces the web agent to get a new access token using the refresh token.
        /// </summary>
        /// <returns></returns>
        public async Task GetNewTokenAsync()
        {
            AccessToken = await TokenProvider.GetOAuthTokenAsync(RefreshToken,true).ConfigureAwait(false);
            TokenValidTo = DateTime.UtcNow.AddHours(1);
        }
    }

}
