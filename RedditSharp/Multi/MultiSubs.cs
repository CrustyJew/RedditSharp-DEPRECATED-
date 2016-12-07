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

        /// <summary>
        /// Generic Constructor
        /// </summary>
        public MultiSubs(string name)
        {
            Name = name;
        }

    }
}
