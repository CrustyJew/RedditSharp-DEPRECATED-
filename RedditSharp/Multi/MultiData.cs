using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        public string Kind { get; private set; }

        /// <summary>
        /// Internal Model Data of the Multi Class
        /// </summary>
        [JsonIgnore]
        public MData Data { get; private set; }

        /// <summary>
        /// Creates an implementation of MultiData
        /// </summary>
        /// <param name="agent">IWebAgent Object to use</param>
        /// <param name="json">Json Token containing the information for the Multi</param>
        /// <param name="subs">Whether there are subs</param>
        protected internal MultiData(IWebAgent agent, JToken json, bool subs = true)
        {
            Data = new MData(agent, json["data"], subs);
            Helpers.PopulateObject(json, this);
        }
    }




}
