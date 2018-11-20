using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

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
        None = 0x00,
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
        Flair = 0x04,
        /// <summary>
        /// Modmail.
        /// </summary>
        Mail = 0x08,
        /// <summary>
        /// Moderate posts.
        /// </summary>
        Posts = 0x10,
        /// <summary>
        /// Edit / view protected wiki paes.
        /// </summary>
        Wiki = 0x20,
        /// <summary>
        /// Configure chat groups
        /// </summary>
        ChatConfig = 0x40,
        /// <summary>
        /// Moderate users in subreddit chatrooms
        /// </summary>
        ChatOperator = 0x80,
        /// <summary>
        /// Has the "all" permission / super user status in sub
        /// </summary>
        SuperUser = 0x1000000 | All, //give some padding for other permissions
        /// <summary>
        /// All permissions.
        /// </summary>
        All = Access | Config | Flair | Mail | Posts | Wiki | ChatConfig | ChatOperator
    }

    internal class ModeratorPermissionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var perms = JArray.Load(reader).Select(t => t.ToString());
            ModeratorPermission result = ModeratorPermission.None;
            foreach (var perm in perms)
            {
                switch (perm.ToLower())
                {
                    case "all":
                        {
                            return ModeratorPermission.SuperUser;
                        }
                    case "access":
                        {
                            result = result | ModeratorPermission.Access;
                            break;
                        }
                    case "config":
                        {
                            result = result | ModeratorPermission.Config;
                            break;
                        }
                    case "flair":
                        {
                            result = result | ModeratorPermission.Flair;
                            break;
                        }
                    case "mail":
                        {
                            result = result | ModeratorPermission.Mail;
                            break;
                        }
                    case "posts":
                        {
                            result = result | ModeratorPermission.Posts;
                            break;
                        }
                    case "wiki":
                        {
                            result = result | ModeratorPermission.Wiki;
                            break;
                        }
                    case "chat_config":
                        {
                            result = result | ModeratorPermission.ChatConfig;
                            break;
                        }
                    case "chat_operator":
                        {
                            result = result | ModeratorPermission.ChatOperator;
                            break;
                        }
                }
            }

            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            // NOTE: Not sure if this is what is supposed to be returned
            // This method wasn't called in my (Sharparam) tests so unsure what it does
            return objectType == typeof(ModeratorPermission);
        }
    }
}
