using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace RedditSharp.Things
{
    public class Contributor : Thing
    {
        public Contributor(Reddit reddit, JToken json) : base(reddit, json) {
        }

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
