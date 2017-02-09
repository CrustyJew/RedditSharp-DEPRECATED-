namespace RedditSharp
{
    internal class LinkData : SubmitData
    {
        /// <summary>
        /// Used for redirects.
        /// </summary>
        [RedditAPIName("extension")]
        internal string Extension { get; set; }

        /// <summary>
        /// Url of the link.
        /// </summary>
        [RedditAPIName("url")]
        internal string URL { get; set; }

        internal LinkData()
        {
            Extension = "json";
            Kind = "link";
        }
    }
}
