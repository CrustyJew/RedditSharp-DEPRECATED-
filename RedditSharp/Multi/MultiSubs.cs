using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Multi
{
    /// <summary>
    /// Class to contain the information for a single subreddit in a multi
    /// </summary>
    public class MultiSubs : RedditObject
    {
        /// <summary>
        /// Name of the subreddit
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Creates a new MultiSubs implementation
        /// </summary>
        /// <param name="reddit">Reddit object to use</param>
        /// <param name="json">Token to use for the name</param>
        protected internal MultiSubs(Reddit reddit, JToken json) : base(reddit)
        {
            reddit.PopulateObject(json, this);
        }

    }
}
