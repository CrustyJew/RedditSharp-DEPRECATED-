using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class ModeratorUser
    {
        public ModeratorUser(Reddit reddit, JToken json)
        {
            reddit.PopulateObject(json, this);
        }

        /// <summary>
        /// Moderator username.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// base36 Id of the moderator.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>
        /// Permissions the moderator has in the subreddit.
        /// </summary>
        [JsonProperty("mod_permissions")]
        [JsonConverter(typeof (ModeratorPermissionConverter))]
        public ModeratorPermission Permissions { get; private set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }
}
