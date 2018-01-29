using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RedditSharp
{
    /// <summary>
    /// Caching class to manage WebAgents for multiple users
    /// </summary>
    public class RefreshTokenWebAgentPool
    {
        /// <summary>
        /// Number of minutes to use for cache's sliding expiration.
        /// </summary>
        public int SlidingExpirationMinutes { get; set; }
        /// <summary>
        /// Default user agent to apply to all WebAgents unless otherwise specified.
        /// </summary>
        public string DefaultUserAgent { get; set; }
        /// <summary>
        /// Default <see cref="RateLimitMode"/> to apply to all WebAgents unless otherwise specified.
        /// </summary>
        public RateLimitMode DefaultRateLimitMode { get; set; }

        private List<RefreshTokenPoolEntry> poolEntries = new List<RefreshTokenPoolEntry>();
        private MemoryCache activeAgentsCache = new MemoryCache(new MemoryCacheOptions() { CompactOnMemoryPressure = true });
        private static readonly SemaphoreSlim cacheLock = new SemaphoreSlim(1, 1);

        private string ClientID;
        private string ClientSecret;
        private string RedirectURI;

        /// <summary>
        /// Create new pool. Requires app info to get new tokens.
        /// </summary>
        /// <param name="clientID">Reddit App ClientID</param>
        /// <param name="clientSecret">Reddit App Client Secret</param>
        /// <param name="redirectURI">Reddit App Redirect URI</param>
        public RefreshTokenWebAgentPool(string clientID, string clientSecret, string redirectURI)
        {
            ClientID = clientID;
            ClientSecret = clientSecret;
            RedirectURI = redirectURI;
            SlidingExpirationMinutes = 15;
        }
        /// <summary>
        /// Return <see cref="IWebAgent"/> from cache, or creates a <see cref="RefreshTokenWebAgent"/>, adds it to the cache, and returns it
        /// </summary>
        /// <param name="username">Username of Reddit user attached to WebAgent</param>
        /// <returns></returns>
        public async Task<IWebAgent> GetWebAgentAsync(string username, int requestsPerMinuteWithOAuth = 60, int requestsPerMinuteWithoutOAuth = 30)
        {
            var poolEnt = poolEntries.SingleOrDefault(a => a.Username.ToLower() == username.ToLower());
            return await GetWebAgentAsync(poolEnt, requestsPerMinuteWithOAuth, requestsPerMinuteWithoutOAuth);
        }

        private async Task<IWebAgent> GetWebAgentAsync(RefreshTokenPoolEntry poolEnt, int requestsPerMinuteWithOAuth, int requestsPerMinuteWithoutOAuth) {
            IWebAgent toReturn = null;
            if(poolEnt != null) {
                toReturn = activeAgentsCache.Get<IWebAgent>(poolEnt.WebAgentID);
                if(toReturn == null) {
                    await cacheLock.WaitAsync();
                    RefreshTokenWebAgent agent = null;
                    try {
                        //check if someone else wrote it while waiting for lock.
                        agent = activeAgentsCache.Get<RefreshTokenWebAgent>(poolEnt.WebAgentID);

                        if(agent != null) return agent;
                        var opts = new MemoryCacheEntryOptions() { AbsoluteExpiration = null, SlidingExpiration = new TimeSpan(0, SlidingExpirationMinutes, 0) };
                        opts.RegisterPostEvictionCallback(UpdateAgentInfoOnCacheRemove);

                        agent = new RefreshTokenWebAgent(poolEnt.RefreshToken, ClientID, ClientSecret, RedirectURI, poolEnt.UserAgentString, poolEnt.AccessToken, poolEnt.TokenExpires, new RateLimitManager(poolEnt.RateLimiterMode, requestsPerMinuteWithOAuth, requestsPerMinuteWithoutOAuth));

                        activeAgentsCache.Set(poolEnt.WebAgentID, agent, opts);
                        return agent;
                    }
                    finally {
                        cacheLock.Release();
                    }
                }
            }
            return toReturn;
        }

        /// <summary>
        /// Gets the <see cref="IWebAgent"/> corresponding to the current user, or creates one if it doesn't exist.
        /// </summary>
        /// <param name="username">Username of Reddit user of <see cref="IWebAgent"/></param>
        /// <param name="createAsync">Async function to return a new <see cref="RefreshTokenPoolEntry"/>. Parameters (<see cref="string"/> username, <see cref="string"/> default useragent, <see cref="RateLimitMode"/> default rate limit mode)</param>
        /// <returns></returns>
        public async Task<IWebAgent> GetOrCreateWebAgentAsync(string username, Func<string, string, RateLimitMode, Task<RefreshTokenPoolEntry>> createAsync, int requestsPerMinuteWithOAuth = 60, int requestsPerMinuteWithoutOAuth = 30)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("username cannot be null or empty");

            var poolEnt = poolEntries.SingleOrDefault(a => a.Username.ToLower() == username.ToLower());

            if (poolEnt == null)
            {
                await cacheLock.WaitAsync();
                RefreshTokenWebAgent agent = null;
                try {
                    //check if someone else wrote it while waiting for lock.
                    poolEnt = poolEntries.SingleOrDefault(a => a.Username.ToLower() == username.ToLower());

                    if(poolEnt != null) return await GetWebAgentAsync(poolEnt, requestsPerMinuteWithOAuth, requestsPerMinuteWithoutOAuth);

                    poolEnt = await createAsync(username, DefaultUserAgent, DefaultRateLimitMode);
                    poolEntries.Add(poolEnt);
                    var opts = new MemoryCacheEntryOptions() { AbsoluteExpiration = null, SlidingExpiration = new TimeSpan(0, SlidingExpirationMinutes, 0) };
                    opts.RegisterPostEvictionCallback(UpdateAgentInfoOnCacheRemove);

                    agent = new RefreshTokenWebAgent(poolEnt.RefreshToken, ClientID, ClientSecret, RedirectURI, poolEnt.UserAgentString, poolEnt.AccessToken, poolEnt.TokenExpires, new RateLimitManager(poolEnt.RateLimiterMode));

                    activeAgentsCache.Set(poolEnt.WebAgentID, agent, opts);
                    return agent;
                }
                finally {
                    cacheLock.Release();
                }
                
            }
           
            return await GetWebAgentAsync(poolEnt, requestsPerMinuteWithOAuth, requestsPerMinuteWithoutOAuth);
        }

        /// <summary>
        /// Sets the cache entry for an ID (if it exists it overwrites it meaning current rate limit etc is lost).
        /// If <paramref name="agent"/> is an instance of <see cref="RefreshTokenWebAgent"/> and expries, it will try and update the data to be able to recreate the <see cref="IWebAgent"/> as specified by this method call.
        /// </summary>
        /// <param name="agentID"><see cref="Guid"/> id of entry to set</param>
        /// <param name="agent"><see cref="IWebAgent"/> to set as value</param>
        /// <param name="absoluteExpiration"><see cref="DateTime"/> that if provided will set an absolute expiration date of cache object</param>
        /// <param name="noSlidingExpiration">If false, cache entry will exist until <paramref name="absoluteExpiration"/> or permanently if none is provided. If true, uses <see cref="SlidingExpirationMinutes"/> as sliding expiration time</param>
        public void SetWebAgentCache(Guid agentID, IWebAgent agent, DateTimeOffset? absoluteExpiration = null, bool noSlidingExpiration = false)
        {
            var opts = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = absoluteExpiration,
                SlidingExpiration = noSlidingExpiration ? null : new Nullable<TimeSpan>(new TimeSpan(0, SlidingExpirationMinutes, 0)),

            }.RegisterPostEvictionCallback(UpdateAgentInfoOnCacheRemove);
            activeAgentsCache.Set(agentID, agent, opts);
        }
        /// <summary>
        /// Removes all traces of agent and cache for <paramref name="username"/>. If <paramref name="revokeRefreshToken"/> is true, will first revoke the Refresh Token currently assigned to the user's <see cref="RefreshTokenWebAgent"/>.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="revokeRefreshToken"></param>
        /// <returns></returns>
        public async Task RemoveWebAgentAsync(string username, bool revokeRefreshToken = false)
        {
            var poolEnt = poolEntries.SingleOrDefault(a => a.Username.ToLower() == username.ToLower());
            if(poolEnt != null)
            {
                poolEntries.Remove(poolEnt);
                if (revokeRefreshToken)
                {
                    var agent = await GetOrCreateWebAgentAsync(username, (uname, uagent, rl) => { return Task.FromResult(poolEnt); }) as RefreshTokenWebAgent;
                    await agent.RevokeRefreshTokenAsync();
                }
                activeAgentsCache.Remove(poolEnt.WebAgentID);
                
            }
        }

        private void UpdateAgentInfoOnCacheRemove(object key, object value, EvictionReason reason, object state)
        {
            Guid webAgentID = (Guid)key;
            var poolEnt = poolEntries.SingleOrDefault(a => a.WebAgentID == webAgentID);
            RefreshTokenWebAgent agent = value as RefreshTokenWebAgent;
            if (agent == null || poolEnt == null) return;

            poolEnt.TokenExpires = agent.TokenValidTo;
            poolEnt.AccessToken = agent.AccessToken;
            poolEnt.UserAgentString = agent.UserAgent;
            poolEnt.RefreshToken = agent.RefreshToken;
            poolEnt.RateLimiterMode = agent.RateLimiter.Mode;
        }
    }




}
