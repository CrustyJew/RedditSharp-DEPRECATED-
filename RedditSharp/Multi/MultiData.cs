using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditSharp.Multi
{
    public class MultiData
    {
        [JsonProperty("kind")]
        public string kind { get; set; }

        [JsonIgnore]
        public mData data { get; set; }

        protected internal MultiData(Reddit reddit, JToken json, IWebAgent webAgent, bool subs = true)
        {
            data = new mData(reddit, json["data"], webAgent, subs);
            JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
        }
    }

    public class mData
    {
        [JsonProperty("can_edit")]
        public bool canEdit { get; set; }

        [JsonProperty("display_name")]
        public string displayName { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("description_html")]
        public string descriptionHTML { get; set; }

        [JsonProperty("created")]
        public double created { get; set; }

        [JsonProperty("copied_from")]
        public string copiedFrom { get; set; }

        [JsonProperty("icon_url")]
        public string iconUrl { get; set; }

        [JsonIgnore]
        public List<MultiSubs> subreddits { get; set; }

        [JsonProperty("created_utc")]
        public double createdUTC { get; set; }

        [JsonProperty("key_color")]
        public string keyColor { get; set; }

        [JsonProperty("visibility")]
        public string visibility { get; set; }

        [JsonProperty("icon_name")]
        public string iconName { get; set; }

        [JsonProperty("weighting_scheme")]
        public string weightingScheme { get; set; }

        [JsonProperty("path")]
        public string path { get; set; }

        [JsonProperty("description_md")]
        public string descriptionMD { get; set; }

        protected internal mData(Reddit reddit, JToken json, IWebAgent webAgent, bool subs)
        {
            subreddits = new List<MultiSubs>();
            if (subs)
            {
                //Get Subreddit List
                for (int i = 0; i < json["subreddits"].Count(); i++)
                {
                    subreddits.Add(new MultiSubs(reddit, json["subreddits"][i], webAgent));
                }
            }
                JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
        }
    }

    public class MultiSubs
    {
        [JsonProperty("name")]
        public string name { get; set; }

        protected internal MultiSubs(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
        }


    }
}
