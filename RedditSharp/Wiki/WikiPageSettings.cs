using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using RedditSharp.Things;

namespace RedditSharp
{
    /// <summary>
    /// The settings for an individual wiki page.
    /// </summary>
    public class WikiPageSettings
    {
        /// <summary>
        /// Indicates who can edit this page.
        /// </summary>
        public enum WikiPagePermissionLevel
        {
            /// <summary>
            /// Use subreddit wiki permissions.
            /// </summary>
            Inherit = 0,
            /// <summary>
            /// Only approved wiki contributors for this page may edit.
            /// </summary>
            Contributors = 1,
            /// <summary>
            /// Only mods may edit and view.
            /// </summary>
            Mods = 2
        }

        /// <summary>
        /// Returns true if this page appears in the list of wiki pages.
        /// </summary>
        [JsonProperty("listed")]
        public bool Listed { get; private set; }

        /// <summary>
        /// Indicates who can edit this page.
        /// </summary>
        [JsonProperty("permlevel")]
        public WikiPagePermissionLevel PermLevel { get; private set; }

        /// <summary>
        /// Users who are allowed to edit the wiki.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<RedditUser> Editors { get; private set; }

        #pragma warning disable 1591
        protected internal WikiPageSettings(IWebAgent agent, JToken json)
        {
            Editors = json["editors"].Select(x => new RedditUser(agent, x)).ToArray();
            Helpers.PopulateObject(json, this);
        }
        #pragma warning restore 1591
    }
}
