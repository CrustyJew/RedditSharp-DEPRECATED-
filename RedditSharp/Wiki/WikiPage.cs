using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using RedditSharp.Things;

namespace RedditSharp
{
    public class WikiPage
    {

        /// <summary>
        /// May revise.
        /// </summary>
        [JsonProperty("may_revise")]
        public string MayRevise { get; set; }

        /// <summary>
        /// Revision date.
        /// </summary>
        [JsonProperty("revision_date")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTimeOffset? RevisionDate { get; set; }

        /// <summary>
        /// Content of the page.
        /// </summary>
        [JsonProperty("content_html")]
        public string HtmlContent { get; set; }

        /// <summary>
        /// Markdown content of the page.
        /// </summary>
        [JsonProperty("content_md")]
        public string MarkdownContent { get; set; }

        /// <summary>
        /// Lst revision by this user.
        /// </summary>
        [JsonIgnore]
        public RedditUser RevisionBy { get; set; }

        protected internal WikiPage(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            RevisionBy = new RedditUser().Init(reddit, json["revision_by"], webAgent);
            JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
        }
    }
}