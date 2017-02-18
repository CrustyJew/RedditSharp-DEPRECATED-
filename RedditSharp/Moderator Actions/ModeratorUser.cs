using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    /// <summary>
    /// Represents a moderator.
    /// </summary>
    public class ModeratorUser
    {
        #pragma warning disable 1591
        public ModeratorUser(Reddit reddit, JToken json)
        {
            reddit.PopulateObject(json, this);
        }
        #pragma warning restore 1591

        /// <summary>
        /// Moderator username.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; }

        /// <summary>
        /// base36 Id of the moderator.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; }

        /// <summary>
        /// Permissions the moderator has in the subreddit.
        /// </summary>
        [JsonProperty("mod_permissions")]
        [JsonConverter(typeof (ModeratorPermissionConverter))]
        public ModeratorPermission Permissions { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }
}
