using Newtonsoft.Json;

namespace RedditSharp.Interfaces
{
    internal interface ISettableWebAgent
    {
        [JsonIgnore]
        IWebAgent WebAgent { set; }
    }
}