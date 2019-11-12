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
    /// Post preview images.
    /// </summary>
    public class Preview
    {
        public Preview(IWebAgent agent, JToken json)
        {
        }

        /// <summary>
        /// List of post image in various resolutions.
        /// </summary>
        [JsonProperty("images")]
        public List<Images> Images { get; private set; }

        /// <summary>
        /// Is there a preview enabled for this post.
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; private set; }
    }
}
