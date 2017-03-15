using Newtonsoft.Json;
using RedditSharp.Things;
using System;

namespace RedditSharp
{
    /// <summary>
    /// A domain submitted to reddit.
    /// </summary>
    public class Domain : RedditObject
    {
        private string DomainPostUrl => $"/domain/{Name}.json";
        private string DomainNewUrl => $"/domain/{Name}/new.json?sort=new";
        private string DomainHotUrl => $"/domain/{Name}/hot.json";
        private const string FrontPageUrl = "/.json";

        /// <summary>
        /// Domain name
        /// </summary>
        [JsonIgnore]
        public string Name { get; set; }

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of posts made for this domain.
        /// </summary>
        /// <param name="max">Maximum number of records to return.  -1 for unlimited.</param>
        public Listing<Post> GetPosts(int max = -1) => Listing<Post>.Create(Reddit, DomainPostUrl, max, 100);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of posts made for this domain that are in the new queue.
        /// </summary>
        public Listing<Post> GetNew(int max = -1) => Listing<Post>.Create(Reddit, DomainNewUrl, max, 100);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of posts made for this domain that are in the hot queue.
        /// </summary>
        public Listing<Post> GetHot(int max = -1) => Listing<Post>.Create(Reddit, DomainHotUrl, max, 100);

#pragma warning disable 1591
        protected internal Domain(Reddit reddit, Uri domain) : base(reddit)
        {
            Name = domain.Host;
        }
        #pragma warning restore 1591

        /// <inheritdoc/>
        public override string ToString() => "/domain/" + Name;
    }
}

