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
        public string MayRevise { get; }

        /// <summary>
        /// Revision date.
        /// </summary>
        [JsonProperty("revision_date")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? RevisionDate { get; }

        /// <summary>
        /// Content of the page.
        /// </summary>
        [JsonProperty("content_html")]
        public string HtmlContent { get; }

        /// <summary>
        /// Markdown content of the page.
        /// </summary>
        [JsonProperty("content_md")]
        public string MarkdownContent { get; }

        /// <summary>
        /// Lst revision by this user.
        /// </summary>
        [JsonIgnore]
        public RedditUser RevisionBy { get; }

        protected internal WikiPage(Reddit reddit, JToken json)
        {
            RevisionBy = new RedditUser(reddit, json["revision_by"]);
            reddit.PopulateObject(json, this);
        }
    }
}
