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

        public DateTime TokenValidTo { get; set; }

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

        public override HttpWebRequest CreateRequest(string url, string method)
        {
            //add 5 minutes for clock skew to ensure requests succeed 
            if (url != AuthProvider.AccessUrl && DateTime.UtcNow.AddMinutes(5) > TokenValidTo)
            {
                GetNewToken();
            }
            return base.CreateRequest(url, method);
        }

        protected override HttpWebRequest CreateRequest(Uri uri, string method)
        {
            //add 5 minutes for clock skew to ensure requests succeed
            if (uri.ToString() != AuthProvider.AccessUrl && DateTime.UtcNow.AddMinutes(5) > TokenValidTo)
            {
                GetNewToken();
            }
            return base.CreateRequest(uri, method);
        }

        private void GetNewToken()
        {
            AccessToken = TokenProvider.GetOAuthToken(Username, Password);
            TokenValidTo = DateTime.UtcNow.AddHours(1);
        }
    }

}
