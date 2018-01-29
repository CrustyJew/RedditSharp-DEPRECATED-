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
        public Listing<Post> GetPosts(int max = -1) => Listing<Post>.Create(WebAgent, DomainPostUrl, max, 100);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of posts made for this domain that are in the new queue.
        /// </summary>
        public Listing<Post> GetNew(int max = -1) => Listing<Post>.Create(WebAgent, DomainNewUrl, max, 100);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of posts made for this domain that are in the hot queue.
        /// </summary>
        public Listing<Post> GetHot(int max = -1) => Listing<Post>.Create(WebAgent, DomainHotUrl, max, 100);

        protected internal Domain(IWebAgent agent, Uri domain) : base(agent)
        {
            Name = domain.Host;
        }

        /// <inheritdoc/>
        public override string ToString() => "/domain/" + Name;
    }
}

