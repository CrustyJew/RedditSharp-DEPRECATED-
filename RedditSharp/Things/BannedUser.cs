using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

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
        public DateTime? TimeStamp { get; set; }

        /// <summary>
        /// Ban note.
        /// </summary>
        [JsonProperty("note")]
        public string Note { get; set; }

        /// <summary>
        /// User name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
