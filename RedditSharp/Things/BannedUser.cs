using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace RedditSharp.Things
{
    public class BannedUser : Thing
    {
        [JsonProperty("date")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? TimeStamp { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public BannedUser Init(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(json);
            JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

        public async Task<BannedUser> InitAsync(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(json);
            await JsonConvert.PopulateObjectAsync(json.ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

        private void CommonInit(JToken json)
        {
            base.Init(json);
        }
    }
}
