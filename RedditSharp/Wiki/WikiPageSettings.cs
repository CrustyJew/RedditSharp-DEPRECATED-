using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using RedditSharp.Things;

namespace RedditSharp
{
    public class WikiPageSettings
    {
        [JsonProperty("listed")]
        public bool Listed { get; }

        [JsonProperty("permlevel")]
        public int PermLevel { get; }

        [JsonIgnore]
        public IEnumerable<RedditUser> Editors { get; private set; }

        protected internal WikiPageSettings(Reddit reddit, JToken json)
        {
            Editors = json["editors"].Select(x => new RedditUser(reddit, x)).ToArray();
            reddit.PopulateObject(json, this);
        }
    }
}
