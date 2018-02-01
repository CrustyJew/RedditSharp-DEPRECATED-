using System.Net.Http;
using System.Threading.Tasks;

namespace RedditSharp
{
    public interface IRateLimiter
    {
        /// <summary>
        /// It is strongly advised that you leave this set to Burst or Pace. Reddit bans excessive
        /// requests with extreme predjudice.
        /// </summary>
        RateLimitMode Mode { get; set; }

        /// <summary>
        /// Enforces the configured rate limit
        /// </summary>
        /// <param name="oauth">Set to true if authentication is through OAuth. Reddit allows a higher rate limit when using OAuth.</param>
        /// <returns></returns>
        Task CheckRateLimitAsync(bool oauth);

        /// <summary>
        /// Called after the response is received to parse the rate limit headers
        /// </summary>
        /// <param name="response">The HTTP response from the previous Reddit call</param>
        /// <returns></returns>
        Task ReadHeadersAsync(HttpResponseMessage response);
    }
}
