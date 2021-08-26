namespace RedditSharp.Data.Collections
{
    internal class CollectionCreationParams
    {
        /// <summary>
        /// Title of the submission. Maximum 300 characters.
        /// </summary>
        [RedditAPIName("title")]
        internal string Title { get; set; }

        /// <summary>
        /// Description of the collection. Maximum of 500 characters.
        /// </summary>
        [RedditAPIName("description")]
        internal string Description { get; set; }

        /// <summary>
        /// Name of the subreddit to which you are submitting.
        /// </summary>
        [RedditAPIName("sr_fullname")]
        internal string Subreddit { get; set; }
    }
}