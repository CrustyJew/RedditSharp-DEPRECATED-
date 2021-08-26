using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp.Data;
using RedditSharp.Data.Collections;
using RedditSharp.Extensions.JTokenExtensions;
using RedditSharp.Interfaces;

namespace RedditSharp.Things
{
    public class Collection : ISettableWebAgent
    {
        [JsonProperty("subreddit_id")]
        public string SubredditId { get; internal set; }

        [JsonProperty("description")]
        public string Description { get; internal set; }

        [JsonProperty("author_name")]
        public string AuthorName { get; internal set; }

        [JsonProperty("collection_id")]
        public string CollectionId { get; internal set; }

        [JsonProperty("display_layout")]
        public string DisplayLayout { get; internal set; }

        [JsonProperty("permalink")]
        public string Permalink { get; internal set; }

        [JsonProperty("link_ids")]
        public string[] LinkIds { get; internal set; }

        [JsonProperty("title")]
        public string Title { get; internal set; }

        [JsonProperty("created_at_utc"), JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime CreatedAtUtc { get; internal set; }

        [JsonProperty("author_id")]
        public string AuthorId { get; internal set; }

        [JsonProperty("last_update_utc"), JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime LastUpdateUtc { get; internal set; }

        public Post[] Posts { get; }

        public IWebAgent WebAgent { private get; set; }

        public Collection()
        {
        }

        public Collection(JToken json, IWebAgent agent)
        {
            WebAgent = agent;

            Helpers.PopulateObject(json, this);
            
            var posts = new List<Post>();
            var children = json.SelectToken("sorted_links.data.children");
            if (children != null && children.Type == JTokenType.Array)
            {
                posts.AddRange(children.Select(item => new Post(WebAgent, item)));
            }

            Posts = posts.ToArray();
        }

        /// <summary>
        /// Adds a post to the collection
        /// </summary>
        /// <param name="linkFullName">Full name of link, e.g. t3_xyz</param>
        public async Task AddPostAsync(string linkFullName)
        {
            var data = new AddOrRemovePostsFromCollectionParams
            {
                CollectionId = CollectionId,
                LinkFullName = linkFullName,
            };
            var json = await WebAgent.Post(Urls.Collections.AddPost, data);
            json.ThrowIfHasErrors("Could not add post to collection.");
        }

        /// <summary>
        /// Removes a post from the collection
        /// </summary>
        /// <param name="linkFullName">Full name of link, e.g. t3_xyz</param>
        public async Task RemovePostAsync(string linkFullName)
        {
            var data = new AddOrRemovePostsFromCollectionParams
            {
                CollectionId = CollectionId,
                LinkFullName = linkFullName,
            };
            var json = await WebAgent.Post(Urls.Collections.RemovePost, data);
            json.ThrowIfHasErrors("Could not remove post from collection.");
        }

        public async Task DeleteAsync()
        {
            var json = await WebAgent.Post(Urls.Collections.Delete, null);
            json.ThrowIfHasErrors("Could not remove collection.");
        }
    }
}