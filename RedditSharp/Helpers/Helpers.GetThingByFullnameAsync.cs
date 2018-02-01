using RedditSharp.Things;
using System.Threading.Tasks;

namespace RedditSharp
{
    partial class Helpers
    {
        private const string GetThingUrl = "/api/info.json?id={0}";
        /// <summary>
        /// Get a <see cref="Thing"/> by full name.
        /// </summary>
        /// <param name="agent">IWebAgent to use to make request</param>
        /// <param name="fullname">fullname including kind + underscore. EG ("t1_######")</param>
        /// <returns></returns>
        public static async Task<Thing> GetThingByFullnameAsync(IWebAgent agent, string fullname)
        {
            var json = await agent.Get(string.Format(GetThingUrl, fullname)).ConfigureAwait(false);
            return Thing.Parse(agent, json["data"]["children"][0]);
        }

    }
}
