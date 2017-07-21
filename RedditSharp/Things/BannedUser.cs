using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace RedditSharp.Things
{
    public class BannedUser : Thing
    {
        /// <summary>
        /// Date the user was banned.
        /// </summary>
        [JsonProperty("date")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTimeOffset? TimeStamp { get; set; }

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

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="json"></param>
        /// <param name="webAgent"></param>
        /// <returns></returns>
        public BannedUser Init(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(json);
            JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="json"></param>
        /// <param name="webAgent"></param>
        /// <returns></returns>
        public async Task<BannedUser> InitAsync(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(json);
            JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

        private void CommonInit(JToken json)
        {
            base.Init(json);
        }
    }
}
