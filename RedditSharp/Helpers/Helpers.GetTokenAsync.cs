using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedditSharp
{
    partial class Helpers
    {
        /// <summary>
        /// Get a <see cref="JToken"/> from a url.
        /// </summary>
        /// <param name="agent">IWebAgent to use to make request</param>
        /// <param name="uri">uri to fetch</param>
        /// <param name="isLive">bool indicating if it's a live thread or not</param>
        /// <returns></returns>
        public static async Task<JToken> GetTokenAsync(IWebAgent agent,Uri uri, bool isLive = false)
        {
            //TODO clean this up
            if ((!String.IsNullOrEmpty(agent.AccessToken) || agent.GetType() == typeof(RefreshTokenWebAgent)) && uri.AbsoluteUri.StartsWith("https://www.reddit.com"))
                uri = new Uri(uri.AbsoluteUri.Replace("https://www.reddit.com", "https://oauth.reddit.com"));

            var url = uri.AbsoluteUri;

            if (url.EndsWith("/"))
                url = url.Remove(url.Length - 1);

            if (!url.ToLower().EndsWith(".json"))
            {
                url += ".json";
            }

            var json = await agent.Get(url);

            if (isLive)
                return json;
            else
                return json[0]["data"]["children"].First;
        }
    }
}
