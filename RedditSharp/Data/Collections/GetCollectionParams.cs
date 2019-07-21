namespace RedditSharp.Data.Collections
{
    internal class CollectionBaseParams
    {
        /// <summary>
        /// UUID of a collection
        /// </summary>
        [RedditAPIName("collection_id")]
        internal string CollectionId { get; set; }
    }

    internal class GetCollectionParams : CollectionBaseParams
    {
        /// <summary>
        /// Should include all the links
        /// </summary>
        [RedditAPIName("include_links")]
        internal bool IncludeLinks { get; set; }
    }
}