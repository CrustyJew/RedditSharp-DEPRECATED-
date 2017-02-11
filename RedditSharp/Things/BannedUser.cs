using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace RedditSharp.Things
{
    public class BannedUser : Thing
    {
        public BannedUser(Reddit reddit, JToken json) : base(reddit, json) {
        }

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
