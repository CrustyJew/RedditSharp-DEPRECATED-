namespace RedditSharp
{
    internal abstract class SubmitData
    {
        /// <summary>
        /// Should be set to "json"
        /// </summary>
        [RedditAPIName("api_type")]
        internal string APIType { get; set; }

        /// <summary>
        /// One of "link", "self" or "image"
        /// </summary>
        [RedditAPIName("kind")]
        internal string Kind { get; set; }

        /// <summary>
        /// Name of the subreddit to which you are submitting.
        /// </summary>
        [RedditAPIName("sr")]
        internal string Subreddit { get; set; }

        /// <summary>
        /// Logged in users modhash.
        /// </summary>
        [RedditAPIName("uh")]
        internal string UserHash { get; set; }

        /// <summary>
        /// Title of the submission.  Maximum 300 characters.
        /// </summary>
        [RedditAPIName("title")]
        internal string Title { get; set; }

        /// <summary>
        /// Captcha ident.
        /// </summary>
        [RedditAPIName("iden")]
        internal string Iden { get; set; }

        /// <summary>
        /// Captcha.
        /// </summary>
        [RedditAPIName("captcha")]
        internal string Captcha { get; set; }

        /// <summary>
        /// If a link with the same URL has already been submitted to the specified
        /// subreddit an error will be returned unless resubmit is true.
        /// </summary>
        [RedditAPIName("resubmit")]
        internal bool Resubmit { get; set; }

        protected SubmitData()
        {
            APIType = "json";
        }
    }
}
