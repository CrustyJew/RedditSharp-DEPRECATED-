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
    /// Single image for a post.
    /// </summary>
    public class Image
    {
        public Image(IWebAgent agent, JToken json)
        {
        }

        /// <summary>
        /// Url for the image.
        /// </summary>
        [JsonProperty("url")]
        public Uri Url { get; private set; }

        /// <summary>
        /// Width of the image.
        /// </summary>
        [JsonProperty("width")]
        public int? Width { get; private set; }

        /// <summary>
        /// Height of the image.
        /// </summary>
        [JsonProperty("height")]
        public int? Height { get; private set; }

    }
}
