using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace RedditSharp
{
    /// <summary>
    /// Generic memory cache wrapper for storing and retrieving <see cref="IWebAgent"/>s. Items are stored in memory until explicitly removed.
    /// </summary>
    /// <typeparam name="TKey">Key type to store agents</typeparam>
    /// <typeparam name="TAgent">Web Agent type. Must inherit from <see cref="IWebAgent"/></typeparam>
    public class WebAgentPool<TKey, TAgent>
        where TAgent : IWebAgent
    {
        private MemoryCache activeAgentsCache = new MemoryCache(new MemoryCacheOptions() { CompactOnMemoryPressure = false });

        /// <summary>
        /// Returns the <typeparamref name="TAgent"/> corresponding to the <paramref name="key"/>
        /// </summary>
        /// <param name="key">Key of Web Agent to return</param>
        /// <returns><typeparamref name="TAgent"/></returns>
        public TAgent GetAgent(TKey key) {
            return activeAgentsCache.Get<TAgent>(key);
        }

        /// <summary>
        /// Gets or Creates the <typeparamref name="TAgent"/> with the given key.
        /// </summary>
        /// <param name="key">Key of Web Agent to return or create</param>
        /// <param name="create">Function that returns the <typeparamref name="TAgent" /> corresponding to the <paramref name="key"/></param>
        /// <returns></returns>
        public Task<TAgent> GetOrCreateAgentAsync(TKey key, Func<Task<TAgent>> create)
        {
            return activeAgentsCache.GetOrCreateAsync<TAgent>(key, (c) =>
            {
                c.AbsoluteExpiration = null;
                c.SlidingExpiration = null;
                return create();
            });
        }
        /// <summary>
        /// Sets the web agent at the given key to be equal to the given value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="agent"></param>
        public void SetAgent(TKey key, TAgent agent)
        {
            activeAgentsCache.Set(key, agent);
        }
        /// <summary>
        /// Removes the web agent at the given key from the cache
        /// </summary>
        /// <param name="key"></param>
        public void RemoveAgent(TKey key)
        {
            activeAgentsCache.Remove(key);
        }
    }
}
