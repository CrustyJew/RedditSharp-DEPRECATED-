using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    public class WikiPageRevision : Thing
    {
        protected internal WikiPageRevision(Reddit reddit, JToken json) : base(reddit, json) {
            Author = new RedditUser(Reddit, json["author"]);
        }

        /// <summary>
        /// Revision id.
        /// </summary>
        [JsonProperty("id")]
        new public string Id { get; private set; }

        /// <summary>
        /// DateTime of the revision.
        /// </summary>
        [JsonProperty("timestamp")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? TimeStamp { get; private set; }

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
        public RedditUser Author { get; private set; }

    }
}
