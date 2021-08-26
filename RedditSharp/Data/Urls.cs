namespace RedditSharp.Data
{
    internal static class Urls
    {
        internal static class Collections
        {
            internal const string AddPost = "/api/v1/collections/add_post_to_collection";
            internal static string Get(string collectionId, bool includeLinks) => $"/api/v1/collections/collection.json?collection_id={collectionId}&include_links={includeLinks}";
            internal const string CreateCollectionUrl = "/api/v1/collections/create_collection";
            internal const string Delete = "/api/v1/collections/delete_collection";
            internal const string RemovePost = "/api/v1/collections/remove_post_in_collection";
            internal static string SubredditCollectionsUrl(string fullName) => $"/api/v1/collections/subreddit_collections.json?sr_fullname={fullName}";
        } 
    }
}