using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace RedditSharp
{
    public class RefreshTokenWebAgentPool
    {
        public int SlidingExpirationMinutes { get; set; }
        public string DefaultUserAgent { get; set; }
        public RateLimitMode DefaultRateLimitMode { get; set; }

        private List<UserWebAgent> userWebAgents = new List<UserWebAgent>();
        private MemoryCache activeAgentsCache = new MemoryCache(new MemoryCacheOptions() { CompactOnMemoryPressure = true });

        private string ClientID;
        private string ClientSecret;
        private string RedirectURI;

        public RefreshTokenWebAgentPool(string clientID, string clientSecret, string redirectURI)
        {
            ClientID = clientID;
            ClientSecret = clientSecret;
            RedirectURI = redirectURI;
        }


        public IWebAgent GetWebAgent(string username, string accessToken = "", DateTime? accessTokenExpires = null, string refreshToken = "")
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("username cannot be null or empty");

            var uwa = userWebAgents.SingleOrDefault(a => a.Username.ToLower() == username.ToLower());
            
            if(uwa == null)
            {
                if(!string.IsNullOrWhiteSpace(refreshToken))
                {
                    uwa = new UserWebAgent() {
                        Username = username,
                        AccessToken = accessToken,
                        TokenExpires = accessTokenExpires ?? DateTime.UtcNow.AddMinutes(45),
                        RefreshToken = refreshToken,
                        WebAgentID = new Guid()
                    };

                    userWebAgents.Add(uwa);
                    var agent = new RefreshTokenWebAgent(refreshToken, ClientID, ClientSecret, RedirectURI, uwa.AccessToken, uwa.TokenExpires, new RateLimitManager(DefaultRateLimitMode));
                    activeAgentsCache.Set(uwa.WebAgentID, agent );
                    return agent;
                }
                return null;
            }
            activeAgentsCache;
        }

    }

    internal class UserWebAgent
    {
        public string Username { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime TokenExpires { get; set; }

        public Guid WebAgentID { get; set; }


    }
}
