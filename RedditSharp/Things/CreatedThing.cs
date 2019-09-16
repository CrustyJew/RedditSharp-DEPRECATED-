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
        /// <remarks>
        /// This property is broken, as it returns the timestamp with the time
        /// zone of the server that served the page. This is almost always
        /// completely useless and any new code should use <see cref="CreatedUTC"/>
        /// instead. <a href="https://www.reddit.com/comments/29991t">This 
        /// /r/redditdev post</a> explains more.
        /// </remarks>
        /// <seealso cref="CreatedUTC"/>
        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        [Obsolete("Use the CreatedUTC property. This property is buggy and is kept for backwards compatability.")]
        public DateTime Created { get; private set; }

        /// <summary>
        /// DateTime when the item was created in UTC.
        /// </summary>
        [JsonProperty("created_utc")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime CreatedUTC { get; set; }
    }
}
