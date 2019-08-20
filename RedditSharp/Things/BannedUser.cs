using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp.Things.User;
using System;

namespace RedditSharp.Things
{
    /// <summary>
    /// A user that is banned in a subreddit.
    /// </summary>
    public class BannedUser : NotedUser
    {
        /// <inheritdoc />
        public BannedUser(IWebAgent agent, JToken json) : base(agent, json) {
        }

        /// <summary>
        /// Date the user was banned.
        /// </summary>
        [Obsolete("User RelUser.Date")]
        public DateTime? BanDate { get => DateUTC; private set => DateUTC = value; }

        /// <summary>
        /// Ban note.
        /// </summary>
        [JsonProperty("note")]
        public string Note { get; private set; }
    }
}
