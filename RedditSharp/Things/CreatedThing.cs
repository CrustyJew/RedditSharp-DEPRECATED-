using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    public class CreatedThing : Thing
    {
        public CreatedThing(Reddit reddit, JToken json) : base(reddit, json) {
        }

        protected override JToken GetJsonData(JToken json) => json["data"];

        /// <summary>
        /// DateTime when the item was created.
        /// </summary>
        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime Created { get; }

        /// <summary>
        /// DateTime when the item was created in UTC.
        /// </summary>
        [JsonProperty("created_utc")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime CreatedUTC { get; }
    }
}
