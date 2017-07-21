using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    public class CreatedThing : Thing
    {

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        protected CreatedThing Init(Reddit reddit, JToken json)
        {
            CommonInit(reddit, json);
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        protected async Task<CreatedThing> InitAsync(Reddit reddit, JToken json)
        {
            CommonInit(reddit, json);
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

        private void CommonInit(Reddit reddit, JToken json)
        {
            Init(json);
            Reddit = reddit;
        }

        /// <summary>
        /// Date and time when the item was created.
        /// </summary>
        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// Date and time when the item was created in UTC.
        /// </summary>
        [JsonProperty("created_utc")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTimeOffset CreatedUTC { get; set; }
    }
}
