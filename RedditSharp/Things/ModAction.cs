using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace RedditSharp.Things
{
    /// <summary>
    /// An entry in the modlog.
    /// </summary>
    public class ModAction : Thing
    {
        #pragma warning disable 1591
        public ModAction(IWebAgent agent, JToken json) : base(agent, json) {
        }
        #pragma warning restore 1591

        /// <summary>
        /// Type of action.
        /// </summary>
        [JsonProperty("action")]
        public ModActionType Action { get; private set; }

        /// <summary>
        /// DateTime of the action.
        /// </summary>
        [JsonProperty("created_utc")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? TimeStamp { get; private set; }

        /// <summary>
        /// Populated when <see cref="Action"/> is WikiBan, BanUser, or UnBanUser.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; private set; }

        /// <summary>
        /// Action details.
        /// </summary>
        [JsonProperty("details")]
        public string Details { get; private set; }

        /// <summary>
        /// Base 36 id of the moderator who performed the action.
        /// </summary>
        [JsonProperty("mod_id36")]
        public string ModeratorId { get; private set; }

        /// <summary>
        /// Name of moderator who performed the action.
        /// </summary>
        [JsonProperty("mod")]
        public string ModeratorName { get; private set; }

        /// <summary>
        /// Target author name of the item against which this moderation action was performed.
        /// </summary>
        [JsonProperty("target_author")]
        public string TargetAuthorName { get; private set; }

        /// <summary>
        /// Target full name of the item against which this moderation action was performed.
        /// </summary>
        [JsonProperty("target_fullname")]
        public string TargetThingFullname { get; private set; }

        /// <summary>
        /// Permalink of the item against which this moderation action was performed.
        /// </summary>
        [JsonProperty("target_permalink")]
        public string TargetThingPermalink { get; private set; }

        /// <summary>
        /// Base 36 id of the subreddit.
        /// </summary>
        [JsonProperty("sr_id36")]
        public string SubredditId { get; private set; }

        /// <summary>
        /// Subreddit name.
        /// </summary>
        [JsonProperty("subreddit")]
        public string SubredditName { get; private set; }

        /// <summary>
        /// Populated when target is a comment.
        /// </summary>
        [JsonProperty("target_body")]
        public string TargetBody { get; private set; }

        /// <summary>
        /// Populated when target is a post.
        /// </summary>
        [JsonProperty("target_title")]
        public string TargetTitle { get; private set; }

        /// <summary>
        /// Author of the item against which this moderation action was performed.
        /// </summary>
        //TODO discuss
        public Task<RedditUser> GetTargetAuthorAsync() => 
            RedditUser.GetUserAsync(WebAgent, TargetAuthorName);

        /// <summary>
        /// Item against which this moderation action was performed.
        /// </summary>
        //TODO discuss
        public Task<Thing> GetTargetThing() =>
            Helpers.GetThingByFullnameAsync(WebAgent,TargetThingFullname);

        /// <inheritdoc />
        internal override JToken GetJsonData(JToken json) => json["data"];
   }
}
