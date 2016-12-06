using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditSharp.Multi
{
    /// <summary>
    /// Master Multi Class that contains the all of the Multi Data
    /// </summary>
    public class MultiData
    {
        /// <summary>
        /// Kind of Multi
        /// </summary>
        [JsonProperty("kind")]
        public string kind { get; set; }

        /// <summary>
        /// Internal Model Data of the Multi Class
        /// </summary>
        [JsonIgnore]
        public mData data { get; set; }

        /// <summary>
        /// Creates an implementation of MultiData
        /// </summary>
        /// <param name="reddit">Reddit Object to use</param>
        /// <param name="json">Json Token containing the information for the Multi</param>
        /// <param name="webAgent">Web Agent to use</param>
        /// <param name="subs">Whether there are subs</param>
        protected internal MultiData(Reddit reddit, JToken json, IWebAgent webAgent, bool subs = true)
        {
            data = new mData(reddit, json["data"], webAgent, subs);
            JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
        }
    }

    /// <summary>
    /// Contains the innner information of the Multi
    /// </summary>
    public class mData
    {
        /// <summary>
        /// Can the Multi be edited
        /// </summary>
        [JsonProperty("can_edit")]
        public bool canEdit { get; set; }

        /// <summary>
        /// Display name for the Multi
        /// </summary>
        [JsonProperty("display_name")]
        public string displayName { get; set; }

        /// <summary>
        /// Actual name of the Multi
        /// </summary>
        [JsonProperty("name")]
        public string name { get; set; }

        /// <summary>
        /// Description of the Multi in HTML format
        /// </summary>
        [JsonProperty("description_html")]
        public string descriptionHTML { get; set; }

        /// <summary>
        /// When the multi was created
        /// </summary>
        [JsonProperty("created")]
        public double created { get; set; }

        /// <summary>
        /// Where the multi was copied from if it was copied
        /// </summary>
        [JsonProperty("copied_from")]
        public string copiedFrom { get; set; }

        /// <summary>
        /// URL of the icon to use. 
        /// </summary>
        [JsonProperty("icon_url")]
        public string iconUrl { get; set; }

        /// <summary>
        /// List of the Subreddits in the multi
        /// </summary>
        [JsonIgnore]
        public List<MultiSubs> subreddits { get; set; }

        /// <summary>
        /// When the multi was created in UTC
        /// </summary>
        [JsonProperty("created_utc")]
        public double createdUTC { get; set; }

        /// <summary>
        /// Hex Code of the color for the multi
        /// </summary>
        [JsonProperty("key_color")]
        public string keyColor { get; set; }

        /// <summary>
        /// Visiblity property for the Multi
        /// </summary>
        [JsonProperty("visibility")]
        public string visibility { get; set; }

        /// <summary>
        /// Name of the icon corresponding to the URL
        /// </summary>
        [JsonProperty("icon_name")]
        public string iconName { get; set; }

        /// <summary>
        /// Weighting scheme of the Multi
        /// </summary>
        [JsonProperty("weighting_scheme")]
        public string weightingScheme { get; set; }

        /// <summary>
        /// Path to navigate to the multi
        /// </summary>
        [JsonProperty("path")]
        public string path { get; set; }

        /// <summary>
        /// Description of the multi in text format.
        /// </summary>
        [JsonProperty("description_md")]
        public string descriptionMD { get; set; }

        /// <summary>
        /// Creates a new mData implementation
        /// </summary>
        /// <param name="reddit">Reddit object to use</param>
        /// <param name="json">Token to use with parameters for the different members</param>
        /// <param name="webAgent">Web Agent to use</param>
        /// <param name="subs">Whether or not subs exist</param>
        protected internal mData(Reddit reddit, JToken json, IWebAgent webAgent, bool subs)
        {
            subreddits = new List<MultiSubs>();
            if (subs)
            {
                //Get Subreddit List
                for (int i = 0; i < json["subreddits"].Count(); i++)
                {
                    subreddits.Add(new MultiSubs(reddit, json["subreddits"][i], webAgent));
                }
            }
                JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
        }
    }

    /// <summary>
    /// Class to contain the information for a single subreddit in a multi
    /// </summary>
    public class MultiSubs
    {
        /// <summary>
        /// Name of the subreddit
        /// </summary>
        [JsonProperty("name")]
        public string name { get; set; }

        /// <summary>
        /// Creates a new MultiSubs implementation
        /// </summary>
        /// <param name="reddit">Reddit object to use</param>
        /// <param name="json">Token to use for the name</param>
        /// <param name="webAgent">Web Agent to implement the creation</param>
        protected internal MultiSubs(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
        }


    }
}
