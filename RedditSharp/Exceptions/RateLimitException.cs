#pragma warning disable 1591
using System;

namespace RedditSharp
{
    public class RateLimitException : Exception
    {
        public TimeSpan TimeToReset { get; set; }

        public RateLimitException(TimeSpan timeToReset)
        {
            TimeToReset = timeToReset;
        }
    }
}
#pragma warning restore 1591