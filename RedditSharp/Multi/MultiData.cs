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
        public string Kind { get; }

        /// <summary>
        /// Internal Model Data of the Multi Class
        /// </summary>
        [JsonIgnore]
        public MData Data { get; private set; }

        /// <summary>
        /// Creates an implementation of MultiData
        /// </summary>
        /// <param name="reddit">Reddit Object to use</param>
        /// <param name="json">Json Token containing the information for the Multi</param>
        /// <param name="webAgent">Web Agent to use</param>
        /// <param name="subs">Whether there are subs</param>
        protected internal MultiData(Reddit reddit, JToken json, bool subs = true)
        {
            Data = new MData(reddit, json["data"], subs);
            reddit.PopulateObject(json, this);
        }
    }




}
