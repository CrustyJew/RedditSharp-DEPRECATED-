using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    /// <summary>
    /// Permissions granted to a moderator.
    /// </summary>
    [Flags]
    public enum ModeratorPermission
    {
        /// <summary>
        /// No permissions.
        /// </summary>
        None   = 0x00,
        /// <summary>
        /// access permissions.
        /// </summary>
        Access = 0x01,
        /// <summary>
        /// Subreddit config.
        /// </summary>
        Config = 0x02,
        /// <summary>
        /// Flair management.
        /// </summary>
        Flair  = 0x04,
        /// <summary>
        /// Modmail.
        /// </summary>
        Mail   = 0x08,
        /// <summary>
        /// Moderate posts.
        /// </summary>
        Posts  = 0x10,
        /// <summary>
        /// Edit / view protected wiki paes.
        /// </summary>
        Wiki   = 0x20,
        /// <summary>
        /// All permissions.
        /// </summary>
        All    = Access | Config | Flair | Mail | Posts | Wiki
    }

    internal class ModeratorPermissionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var data = string.Join(",", JArray.Load(reader).Select(t => t.ToString()));
            ModeratorPermission result;
            var valid = Enum.TryParse(data, true, out result);

            if (!valid)
                result = ModeratorPermission.None;

            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            // NOTE: Not sure if this is what is supposed to be returned
            // This method wasn't called in my (Sharparam) tests so unsure what it does
            return objectType == typeof (ModeratorPermission);
        }
    }
}
