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
        public MData Data { get; set; }

        /// <summary>
        /// Creates an implementation of MultiData
        /// </summary>
        /// <param name="reddit">Reddit Object to use</param>
        /// <param name="json">Json Token containing the information for the Multi</param>
        /// <param name="webAgent">Web Agent to use</param>
        /// <param name="subs">Whether there are subs</param>
        protected internal MultiData(Reddit reddit, JToken json, IWebAgent webAgent, bool subs = true)
        {
            Data = new MData(reddit, json["data"], webAgent, subs);
            JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
        }
    }

    

   
}
