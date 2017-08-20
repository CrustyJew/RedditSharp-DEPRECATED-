using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace RedditSharp.Things 
{
    public class Contributor : Thing 
    {
        /// <summary>
        /// Contributor name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Date contributor was added.
        /// </summary>
        [JsonProperty("date")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTimeOffset DateAdded { get; set; }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="json"></param>
        /// <param name="webAgent"></param>
        /// <returns></returns>
        public Contributor Init(Reddit reddit, JToken json, IWebAgent webAgent) 
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
        public async Task<Contributor> InitAsync(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(json);
            JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

        private void CommonInit(JToken json) 
        {
            Init(json);
        }
    }
}
