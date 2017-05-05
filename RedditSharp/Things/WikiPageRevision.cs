using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    /// <summary>
    /// A revision to a wiki page.
    /// </summary>
    public class WikiPageRevision : Thing
    {
        #pragma warning disable 1591
        protected internal WikiPageRevision(IWebAgent agent, JToken json) : base(agent, json) {
            Author = new RedditUser(agent, json["author"]);
        }
        #pragma warning restore 1591

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
        /// Username of the user who made this revision.
        /// </summary>
        [JsonIgnore]
        public string AuthorName => Author.Name;

        /// <summary>
        /// User who made the revision.
        /// </summary>
        [JsonIgnore]
        public RedditUser Author { get; private set; }

    }
}
