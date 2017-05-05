using Newtonsoft.Json;
using System;

namespace RedditSharp
{
    /// <summary>
    /// Wrapper class to provide <see cref="IWebAgent"/> to children.
    /// </summary>
    public abstract class RedditObject
    {
        /// <summary>
        /// WebAgent for requests
        /// </summary>
        [JsonIgnore]
        public IWebAgent WebAgent { get; }

        /// <summary>
        /// Assign <see cref="WebAgent"/>
        /// </summary>
        /// <param name="agent"></param>
        public RedditObject(IWebAgent agent)
        {
            WebAgent = agent ?? throw new ArgumentNullException(nameof(agent));
        }

    }

}