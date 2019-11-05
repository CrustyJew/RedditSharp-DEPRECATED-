using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditSharp.Things
{
    /// <summary>
    /// Post images provided for the post.
    /// </summary>
    public class Images
    {
        public Images(IWebAgent agent, JToken json)
        {
        }

        /// <summary>
        /// Source image for the post.
        /// </summary>
        [JsonProperty("source")]
        public Image Source { get; private set; }

        /// <summary>
        /// Different resolutions of the source image for the post.
        /// </summary>
        [JsonProperty("resolutions")]
        public List<Image> Resolutions { get; private set; }

        /// <summary>
        /// Post preview id.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; private set; }
    }
}
