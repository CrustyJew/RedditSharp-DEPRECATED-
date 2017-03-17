using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedditSharp {

    public class RateLimitManager
    {
        // See https://github.com/reddit/reddit/wiki/API for more details.
        public int Used { get; private set; }
        public int Remaining { get; private set; }
        // Approximate seconds until the rate limit is reset.
        public DateTimeOffset Reset { get; private set;}

        private SemaphoreSlim rateLimitLock;

        public RateLimitManager() {
            rateLimitLock = new SemaphoreSlim(1, 1);
            Reset = DateTimeOffset.UtcNow;
        }

        public async Task CheckRateLimitAsync()
        {
              await rateLimitLock.WaitAsync().ConfigureAwait(false);
              try {
                if (Remaining <= 0 && DateTime.UtcNow < Reset) {
                    await Task.Delay(Reset - DateTime.UtcNow).ConfigureAwait(false);
                }
              } finally {
                rateLimitLock.Release();
              }
        }

        public async Task ReadHeadersAsync(HttpResponseMessage response) {
              await rateLimitLock.WaitAsync().ConfigureAwait(false);
              try {
                IEnumerable<string> values; var headers = response.Headers;
                int used, remaining;
                if (headers.TryGetValues("X-Ratelimit-Used", out values)) {
                  used = int.Parse(values.First());
                } else {
                  return;
                }
                if (headers.TryGetValues("X-Ratelimit-Remaining", out values)) {
                  remaining = (int)double.Parse(values.First());
                } else {
                  return;
                }
                // Do not update values if they the limit has not been reset and
                // the show an impossible reduction in usage
                if (DateTime.UtcNow < Reset && (used < Used || remaining > Remaining))
                  return;
                Used = used;
                Remaining = remaining;
                if (headers.TryGetValues("X-Ratelimit-Reset", out values))
                  Reset = DateTime.UtcNow + TimeSpan.FromSeconds(int.Parse(values.First()));
              } finally {
                rateLimitLock.Release();
              }

        }

    }

}
