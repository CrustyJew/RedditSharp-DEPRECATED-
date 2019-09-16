using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things.User
{
    /// <summary>
    /// Represents a user that has a relationship formed with another thing, 
    /// usually with a <see cref="Subreddit"/> or another <see cref="RedditUser"/>.
    /// </summary>
    public class RelatedUser : PartialUser
    {
        public RelatedUser(IWebAgent agent, JToken json) : base(agent, json)
        {
        }

        /// <summary>
        /// The base-36 fullname of the relationship
        /// </summary>
        [JsonProperty("rel_id")]
        public String RelationFullName { get; internal set; }

        /// <summary>
        /// UTC time of when this user was added to the list
        /// </summary>
        [JsonProperty("date")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? DateUTC { get; internal set; }
    }
}
