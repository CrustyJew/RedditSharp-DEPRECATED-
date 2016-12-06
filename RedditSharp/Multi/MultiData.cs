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
        public string Kind { get; set; }

        /// <summary>
        /// Internal Model Data of the Multi Class
        /// </summary>
        [JsonIgnore]
        public mData Data { get; set; }

        /// <summary>
        /// Creates an implementation of MultiData
        /// </summary>
        /// <param name="reddit">Reddit Object to use</param>
        /// <param name="json">Json Token containing the information for the Multi</param>
        /// <param name="webAgent">Web Agent to use</param>
        /// <param name="subs">Whether there are subs</param>
        protected internal MultiData(Reddit reddit, JToken json, IWebAgent webAgent, bool subs = true)
        {
            Data = new mData(reddit, json["data"], webAgent, subs);
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
        public bool CanEdit { get; set; }

        /// <summary>
        /// Display name for the Multi
        /// </summary>
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Actual name of the Multi
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Description of the Multi in HTML format
        /// </summary>
        [JsonProperty("description_html")]
        public string DescriptionHTML { get; set; }

        /// <summary>
        /// When the multi was created
        /// </summary>
        [JsonProperty("created")]
        public double Created { get; set; }

        /// <summary>
        /// Where the multi was copied from if it was copied
        /// </summary>
        [JsonProperty("copied_from")]
        public string CopiedFrom { get; set; }

        /// <summary>
        /// URL of the icon to use. 
        /// </summary>
        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        /// <summary>
        /// List of the Subreddits in the multi
        /// </summary>
        [JsonIgnore]
        public List<MultiSubs> Subreddits { get; set; }

        /// <summary>
        /// When the multi was created in UTC
        /// </summary>
        [JsonProperty("created_utc")]
        public double CreatedUTC { get; set; }

        /// <summary>
        /// Hex Code of the color for the multi
        /// </summary>
        [JsonProperty("key_color")]
        public string KeyColor { get; set; }

        /// <summary>
        /// Visiblity property for the Multi
        /// </summary>
        [JsonProperty("visibility")]
        public string Visibility { get; set; }

        /// <summary>
        /// Name of the icon corresponding to the URL
        /// </summary>
        [JsonProperty("icon_name")]
        public string IconName { get; set; }

        /// <summary>
        /// Weighting scheme of the Multi
        /// </summary>
        [JsonProperty("weighting_scheme")]
        public string WeightingScheme { get; set; }

        /// <summary>
        /// Path to navigate to the multi
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }

        /// <summary>
        /// Description of the multi in text format.
        /// </summary>
        [JsonProperty("description_md")]
        public string DescriptionMD { get; set; }

        /// <summary>
        /// Creates a new mData implementation
        /// </summary>
        /// <param name="reddit">Reddit object to use</param>
        /// <param name="json">Token to use with parameters for the different members</param>
        /// <param name="webAgent">Web Agent to use</param>
        /// <param name="subs">Whether or not subs exist</param>
        protected internal mData(Reddit reddit, JToken json, IWebAgent webAgent, bool subs)
        {
            Subreddits = new List<MultiSubs>();
            if (subs)
            {
                //Get Subreddit List
                for (int i = 0; i < json["subreddits"].Count(); i++)
                {
                    Subreddits.Add(new MultiSubs(reddit, json["subreddits"][i], webAgent));
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
        public string Name { get; set; }

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
