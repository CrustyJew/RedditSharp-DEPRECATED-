using System;
using System.Collections.Generic;
using System.Text;

namespace RedditSharp
{
    public class RefreshTokenPoolEntry
    {
        public string Username { get; set; }
        internal string AccessToken { get; set; }
        internal string RefreshToken { get; set; }
        public DateTime TokenExpires { get; set; }
        public string UserAgentString { get; set; }
        public RateLimitMode RateLimiterMode { get; set; }
        public Guid WebAgentID { get; set; }

        public RefreshTokenPoolEntry(string username, string refreshToken, RateLimitMode rateLimiterMode = RateLimitMode.Burst, string userAgentString = "")
        {
            Username = username;
            RefreshToken = refreshToken;
            RateLimiterMode = rateLimiterMode;
            UserAgentString = userAgentString;
            WebAgentID = Guid.NewGuid();
        }
    }
}
