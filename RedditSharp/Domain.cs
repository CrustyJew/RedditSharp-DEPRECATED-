using Newtonsoft.Json;
using RedditSharp.Things;
using System;

namespace RedditSharp
{
    public class Domain
    {
        private const string DomainPostUrl = "/domain/{0}.json";
        private const string DomainNewUrl = "/domain/{0}/new.json?sort=new";
        private const string DomainHotUrl = "/domain/{0}/hot.json";
        private const string FrontPageUrl = "/.json";

        [JsonIgnore]
        private Reddit Reddit { get; set; }

        [JsonIgnore]
        private IWebAgent WebAgent => Reddit?.WebAgent;

        /// <summary>
        /// Domain name
        /// </summary>
        [JsonIgnore]
        public string Name { get; set; }

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of posts made for this domain.
        /// </summary>
        public Listing<Post> Posts
        {
            get
            {
                return new Listing<Post>(Reddit, string.Format(DomainPostUrl, Name));
            }
        }

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of posts made for this domain that are in the new queue.
        /// </summary>
        public Listing<Post> New
        {
            get
            {
                return new Listing<Post>(Reddit, string.Format(DomainNewUrl, Name));
            }
        }

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of posts made for this domain that are in the hot queue.
        /// </summary>
        public Listing<Post> Hot
        {
            get
            {
                return new Listing<Post>(Reddit, string.Format(DomainHotUrl, Name));
            }
        }

        protected internal Domain(Reddit reddit, Uri domain)
        {
            Reddit = reddit;
            Name = domain.Host;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "/domain/" + Name;
        }
    }
}

