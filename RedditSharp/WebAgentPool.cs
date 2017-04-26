using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace RedditSharp
{
    public static class WebAgentPool
    {
        public static int SlidingExpirationMinutes { get; set; }
        public static string DefaultUserAgent { get; set; }


        private static List<UserWebAgent> userWebAgents = new List<UserWebAgent>();
        private static MemoryCache activeAgentsCache = new MemoryCache(new MemoryCacheOptions() { CompactOnMemoryPressure = true });


        public static WebAgent GetWebAgentByUsername(string username, string accessToken = "", DateTime? accessTokenExpires = null, string refreshToken = "")
        {
            var uwa = userWebAgents.SingleOrDefault(a => a.Username.ToLower() == username.ToLower());
            
            if(uwa == null)
            {
                if(!string.IsNullOrWhiteSpace(accessToken) || !string.IsNullOrWhiteSpace(refreshToken))
                {
                    uwa = new UserWebAgent() {
                        Username = username,
                        AccessToken = accessToken,
                        TokenExpires = (accessTokenExpires.HasValue ? accessTokenExpires.Value : DateTime.UtcNow.AddMinutes(45)),
                        RefreshToken = refreshToken, WebAgentID = new Guid()
                    };
                    
                }
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
