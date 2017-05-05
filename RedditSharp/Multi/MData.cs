using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedditSharp.Multi
{
    /// <summary>
    /// Contains the innner information of the Multi
    /// </summary>
    public class MData : RedditObject
    {
        /// <summary>
        /// Can the Multi be edited
        /// </summary>
        [JsonProperty("can_edit")]
        public bool CanEdit { get; private set; }

        /// <summary>
        /// Display name for the Multi
        /// </summary>
        [JsonProperty("display_name")]
        public string DisplayName { get; private set; }

        /// <summary>
        /// Actual name of the Multi
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Description of the Multi in HTML format
        /// </summary>
        [JsonProperty("description_html")]
        public string DescriptionHTML { get; private set; }

        /// <summary>
        /// When the multi was created
        /// </summary>
        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? Created { get; private set; }

        /// <summary>
        /// Where the multi was copied from if it was copied
        /// </summary>
        [JsonProperty("copied_from")]
        public string CopiedFrom { get; private set; }

        /// <summary>
        /// URL of the icon to use.
        /// </summary>
        [JsonProperty("icon_url")]
        public string IconUrl { get; private set; }

        /// <summary>
        /// List of the Subreddits in the multi
        /// </summary>
        [JsonIgnore]
        public List<MultiSubs> Subreddits { get; private set; }

        /// <summary>
        /// When the multi was created in UTC
        /// </summary>
        [JsonProperty("created_utc")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? CreatedUTC { get; private set; }

        /// <summary>
        /// Hex Code of the color for the multi
        /// </summary>
        [JsonProperty("key_color")]
        public string KeyColor { get; private set; }

        /// <summary>
        /// Visiblity property for the Multi
        /// </summary>
        [JsonProperty("visibility")]
        public string Visibility { get; private set; }

        /// <summary>
        /// Name of the icon corresponding to the URL
        /// </summary>
        [JsonProperty("icon_name")]
        public string IconName { get; private set; }

        /// <summary>
        /// Weighting scheme of the Multi
        /// </summary>
        [JsonProperty("weighting_scheme")]
        public string WeightingScheme { get; private set; }

        /// <summary>
        /// Path to navigate to the multi
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; private set; }

        /// <summary>
        /// Description of the multi in text format.
        /// </summary>
        [JsonProperty("description_md")]
        public string DescriptionMD { get; private set; }

        /// <summary>
        /// Creates a new mData implementation
        /// </summary>
        /// <param name="agent">WebAgent object to use</param>
        /// <param name="json">Token to use with parameters for the different members</param>
        /// <param name="subs">Whether or not subs exist</param>
        protected internal MData(IWebAgent agent, JToken json, bool subs) : base(agent)
        {
            Subreddits = new List<MultiSubs>();
            if (subs)
            {
                //Get Subreddit List
                for (int i = 0; i < json["subreddits"].Count(); i++)
                {
                    Subreddits.Add(new MultiSubs(agent, json["subreddits"][i]));
                }
            }
            Helpers.PopulateObject(json, this);
        }

    }
}
