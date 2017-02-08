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
        public bool Listed { get; set; }

        [JsonProperty("permlevel")]
        public int PermLevel { get; set; }

        [JsonIgnore]
        public IEnumerable<RedditUser> Editors { get; set; }

        protected internal WikiPageSettings(Reddit reddit, JToken json)
        {
            var editors = json["editors"].ToArray();
            Editors = editors.Select(x => new RedditUser(reddit, x));
            reddit.PopulateObject(json, this);
        }
    }
}
