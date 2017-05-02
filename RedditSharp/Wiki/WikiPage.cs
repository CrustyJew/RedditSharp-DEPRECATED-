using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using RedditSharp.Things;

namespace RedditSharp
{
    /// <summary>
    /// An individual wiki page.
    /// </summary>
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

        #pragma warning disable 1591
        protected internal WikiPage(IWebAgent agent, JToken json)
        {
            if (json["revision_by"].HasValues) {
                RevisionBy = new RedditUser(agent, json["revision_by"]);
            }
            Helpers.PopulateObject(json, this);
        }
        #pragma warning restore 1591
    }
}
