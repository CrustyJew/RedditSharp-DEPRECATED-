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

            if (uwa == null)
            {
                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    uwa = new UserWebAgent()
                    {
                        Username = username,
                        AccessToken = accessToken,
                        TokenExpires = accessTokenExpires ?? DateTime.UtcNow.AddMinutes(45),
                        RefreshToken = refreshToken,
                        WebAgentID = new Guid()
                    };

                    userWebAgents.Add(uwa);
                    var agent = new RefreshTokenWebAgent(refreshToken, ClientID, ClientSecret, RedirectURI, uwa.AccessToken, uwa.TokenExpires, new RateLimitManager(DefaultRateLimitMode));
                    activeAgentsCache.Set(uwa.WebAgentID, agent, new MemoryCacheEntryOptions() { SlidingExpiration = new TimeSpan(0, SlidingExpirationMinutes, 0) });
                    return agent;
                }
                return null;
            }
            var toReturn = activeAgentsCache.GetOrCreate(uwa.WebAgentID, (i) =>
            {
                i.SlidingExpiration = new TimeSpan(0, SlidingExpirationMinutes, 0);
                var agent = new RefreshTokenWebAgent(refreshToken, ClientID, ClientSecret, RedirectURI, uwa.AccessToken, uwa.TokenExpires, new RateLimitManager(DefaultRateLimitMode));
                return agent;
            });
            return toReturn;
        }

        //Dont think I like how this is working
        public void UpdateWebAgent(string username, string accessToken, DateTime? accessTokenExpires, string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("username cannot be null or empty");
            if (string.IsNullOrWhiteSpace(refreshToken)) throw new ArgumentException("refreshToken cannot be null or empty");

            var uwa = userWebAgents.SingleOrDefault(a => a.Username.ToLower() == username.ToLower());

            if (uwa == null)
            {
                uwa = new UserWebAgent()
                {
                    Username = username,
                    AccessToken = accessToken,
                    TokenExpires = accessTokenExpires ?? DateTime.UtcNow.AddMinutes(45),
                    RefreshToken = refreshToken,
                    WebAgentID = new Guid()
                };

                userWebAgents.Add(uwa);
                var agent = new RefreshTokenWebAgent(refreshToken, ClientID, ClientSecret, RedirectURI, uwa.AccessToken, uwa.TokenExpires, new RateLimitManager(DefaultRateLimitMode));
                activeAgentsCache.Set(uwa.WebAgentID, agent, new MemoryCacheEntryOptions() { SlidingExpiration = new TimeSpan(0, SlidingExpirationMinutes, 0) });

            }
            var existing = activeAgentsCache.Get<RefreshTokenWebAgent>(uwa.WebAgentID);
            if(existing == null)
            {
                existing = new RefreshTokenWebAgent(refreshToken, ClientID, ClientSecret, RedirectURI, accessToken, accessTokenExpires, new RateLimitManager(DefaultRateLimitMode));
            }
            else
            {
                existing.AccessToken = accessToken;
                existing.SetRefreshToken(refreshToken);
                existing.TokenValidTo = accessTokenExpires ?? new DateTime().ToUniversalTime() ;
            }
            return toReturn;
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
