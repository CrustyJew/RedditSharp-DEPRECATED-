using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    /// <summary>
    /// A contributor to a subreddit.
    /// </summary>
    public class Contributor : Thing
    {
        #pragma warning disable 1591
        public Contributor(Reddit reddit, JToken json) : base(reddit, json) {
        }
        #pragma warning restore 1591

        /// <summary>
        /// Contributor name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; }

        /// <summary>
        /// Date contributor was added.
        /// </summary>
        [JsonProperty("date")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime DateAdded { get; }
    }
}
