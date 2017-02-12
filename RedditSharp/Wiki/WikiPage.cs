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
        public string MayRevise { get; private set; }

        /// <summary>
        /// Revision date.
        /// </summary>
        [JsonProperty("revision_date")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? RevisionDate { get; private set; }

        /// <summary>
        /// Content of the page.
        /// </summary>
        [JsonProperty("content_html")]
        public string HtmlContent { get; private set; }

        /// <summary>
        /// Markdown content of the page.
        /// </summary>
        [JsonProperty("content_md")]
        public string MarkdownContent { get; private set; }

        /// <summary>
        /// Lst revision by this user.
        /// </summary>
        [JsonIgnore]
        public RedditUser RevisionBy { get; private set; }

        protected internal WikiPage(Reddit reddit, JToken json)
        {
            RevisionBy = new RedditUser(reddit, json["revision_by"]);
            reddit.PopulateObject(json, this);
        }
    }
}
