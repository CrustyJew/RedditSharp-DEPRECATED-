using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    public class WikiPageRevision : Thing
    {
        /// <summary>
        /// Revision id.
        /// </summary>
        [JsonProperty("id")]
        new public string Id { get; private set; }

        /// <summary>
        /// DateTimeOffset of the revision.
        /// </summary>
        [JsonProperty("timestamp")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTimeOffset? TimeStamp { get; set; }

        /// <summary>
        /// Reason for the revision.
        /// </summary>
        [JsonProperty("reason")]
        public string Reason { get; private set; }

        /// <summary>
        /// Page
        /// </summary>
        [JsonProperty("page")]
        public string Page { get; private set; }

        /// <summary>
        /// User who made the revision.
        /// </summary>
        [JsonIgnore]
        public RedditUser Author { get; set; }

        protected internal WikiPageRevision() { }

        internal async Task<WikiPageRevision> InitAsync(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(reddit, json, webAgent);
            JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }
        internal WikiPageRevision Init(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(reddit, json, webAgent);
            JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }
        private void CommonInit(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            base.Init(json);
            Author = new RedditUser().Init(reddit, json["author"], webAgent);
        }
    }
}