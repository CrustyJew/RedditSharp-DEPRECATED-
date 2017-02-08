using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditSharp.Things
{
    public class ModAction : Thing
    {
        public ModAction(Reddit reddit, JToken json) : base(reddit, json) {
        }

        /// <summary>
        /// Type of action.
        /// </summary>
        [JsonProperty("action")]
        [JsonConverter(typeof(ModActionTypeConverter))]
        public ModActionType Action { get; set; }

        /// <summary>
        /// DateTime of the action.
        /// </summary>
        [JsonProperty("created_utc")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? TimeStamp { get; set; }

        /// <summary>
        /// Populated when <see cref="Action"/> is WikiBan, BanUser, or UnBanUser.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Action details.
        /// </summary>
        [JsonProperty("details")]
        public string Details { get; set; }

        /// <summary>
        /// Base 36 id of the moderator who performed the action.
        /// </summary>
        [JsonProperty("mod_id36")]
        public string ModeratorId { get; set; }

        /// <summary>
        /// Name of moderator who performed the action.
        /// </summary>
        [JsonProperty("mod")]
        public string ModeratorName { get; set; }

        /// <summary>
        /// Target author name of the item against which this moderation action was performed.
        /// </summary>
        [JsonProperty("target_author")]
        public string TargetAuthorName { get; set; }

        /// <summary>
        /// Target full name of the item against which this moderation action was performed.
        /// </summary>
        [JsonProperty("target_fullname")]
        public string TargetThingFullname { get; set; }

        /// <summary>
        /// Permalink of the item against which this moderation action was performed.
        /// </summary>
        [JsonProperty("target_permalink")]
        public string TargetThingPermalink { get; set; }

        /// <summary>
        /// Base 36 id of the subreddit.
        /// </summary>
        [JsonProperty("sr_id36")]
        public string SubredditId { get; set; }

        /// <summary>
        /// Subreddit name.
        /// </summary>
        [JsonProperty("subreddit")]
        public string SubredditName { get; set; }

        /// <summary>
        /// Populated when target is a comment.
        /// </summary>
        [JsonProperty("target_body")]
        public string TargetBody { get; set; }

        /// <summary>
        /// Populated when target is a post.
        /// </summary>
        [JsonProperty("target_title")]
        public string TargetTitle { get; set; }

        /// <summary>
        /// Author of the item against which this moderation action was performed.
        /// </summary>
        [JsonIgnore]
        public RedditUser TargetAuthor
        {
            get
            {
                //TODO discuss
                return Task.Run(async () => { return await Reddit.GetUserAsync(TargetAuthorName); }).Result;
            }
        }

        /// <summary>
        /// Item against which this moderation action was performed.
        /// </summary>
        [JsonIgnore]
        public Thing TargetThing
        {
            get
            {
                //TODO discuss
                return Task.Run(async () => { return await Reddit.GetThingByFullnameAsync(TargetThingFullname); }).Result ;
            }
        }

        protected override JToken GetJsonData(JToken json) {
            return json["data"];
        }

    }
}
