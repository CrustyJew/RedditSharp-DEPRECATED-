using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    /// <summary>
    /// An item that is created.
    /// </summary>
    public class CreatedThing : Thing
    {
        #pragma warning disable 1591
        public CreatedThing(Reddit reddit, JToken json) : base(reddit, json) {
        }
        #pragma warning restore 1591

        /// <inheritdoc />
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
