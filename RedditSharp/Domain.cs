using Newtonsoft.Json;
using RedditSharp.Things;
using System;

namespace RedditSharp
{
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
        public Listing<Post> Posts => new Listing<Post>(Reddit, DomainPostUrl);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of posts made for this domain that are in the new queue.
        /// </summary>
        public Listing<Post> New => new Listing<Post>(Reddit, DomainNewUrl);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of posts made for this domain that are in the hot queue.
        /// </summary>
        public Listing<Post> Hot => new Listing<Post>(Reddit, DomainHotUrl);

        protected internal Domain(Reddit reddit, Uri domain) : base(reddit)
        {
            Name = domain.Host;
        }

        /// <inheritdoc/>
        public override string ToString() => "/domain/" + Name;
    }
}

