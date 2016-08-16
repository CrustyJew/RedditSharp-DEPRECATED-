using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace RedditSharp.Things 
{
    public class Contributor : Thing 
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("date")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime DateAdded { get; set; }

        public Contributor Init(Reddit reddit, JToken json, IWebAgent webAgent) 
        {
            CommonInit(json);
            JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }
        public async Task<Contributor> InitAsync(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(json);
            await JsonConvert.PopulateObjectAsync(json.ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }
        private void CommonInit(JToken json) 
        {
            Init(json);
        }
    }
}
