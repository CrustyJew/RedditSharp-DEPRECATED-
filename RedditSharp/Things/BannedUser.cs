using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace RedditSharp.Things
{
    /// <summary>
    /// A user that is banned in a subreddit.
    /// </summary>
    public class BannedUser : Thing
    {
        #pragma warning disable 1591
        public BannedUser(Reddit reddit, JToken json) : base(reddit, json) {
        }
        #pragma warning restore 1591

        /// <summary>
        /// Date the user was banned.
        /// </summary>
        [JsonProperty("date")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? TimeStamp { get; }

        /// <summary>
        /// Ban note.
        /// </summary>
        [JsonProperty("note")]
        public string Note { get; }

        /// <summary>
        /// User name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; }
    }
}
