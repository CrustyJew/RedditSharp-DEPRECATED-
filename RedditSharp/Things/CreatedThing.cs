using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace RedditSharp.Things
{
    /// <summary>
    /// An item that is created.
    /// </summary>
    public class CreatedThing : Thing
    {
        /// <inheritdoc />
        public CreatedThing(IWebAgent agent, JToken json) : base(agent, json) {
        }

        /// <inheritdoc />
        internal override JToken GetJsonData(JToken json) => json["data"] ?? json;

        /// <summary>
        /// DateTime when the item was created.
        /// </summary>
        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime Created { get; private set; }

        /// <summary>
        /// DateTime when the item was created in UTC.
        /// </summary>
        [JsonProperty("created_utc")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime CreatedUTC { get; set; }
    }
}
