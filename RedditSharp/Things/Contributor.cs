using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp.Extensions;

namespace RedditSharp.Things
{
    /// <summary>
    /// A contributor to a subreddit.
    /// </summary>
    public class Contributor : RedditUser
    {
        #pragma warning disable 1591
        public Contributor(IWebAgent agent, JToken json) : base(agent, json) {
            var data = json["name"] == null ? json["data"] : json;
            base.Name = data["name"].ValueOrDefault<string>();
            var id = data["id"].ValueOrDefault<string>();
            if (id.Contains("_"))
            {
                base.Id = id.Split('_')[1];
                base.FullName = id;
            }
        }
        #pragma warning restore 1591

        /// <summary>
        /// Contributor name.
        /// </summary>
        [JsonProperty("name")]
        public new string Name { get; private set; }

        /// <summary>
        /// Date contributor was added.
        /// </summary>
        [JsonProperty("date")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime DateAdded { get; private set; }

        /// <summary>
        /// This will always return 0 for Contributors
        /// </summary>
        [JsonIgnore]
        public new int CommentKarma => 0;

        /// <summary>
        /// This will always return 0 for Contributors
        /// </summary>
        [JsonIgnore]
        public new int LinkKarma => 0;
    }
}
