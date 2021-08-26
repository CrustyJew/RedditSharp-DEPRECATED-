namespace RedditSharp.Data.Collections
{
    internal class AddOrRemovePostsFromCollectionParams
    {
        /// <summary>
        /// UUID of a collection
        /// </summary>
        [RedditAPIName("collection_id")]
        internal string CollectionId { get; set; }

        /// <summary>
        /// Full name of link, e.g. t3_xyz
        /// </summary>
        [RedditAPIName("link_fullname")]
        internal string LinkFullName { get; set; }
    }
}