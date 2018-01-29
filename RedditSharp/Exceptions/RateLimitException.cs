using System;

namespace RedditSharp
{
    [Serializable]
    public class RateLimitException : Exception
    {
        public TimeSpan TimeToReset { get; set; }

        public RateLimitException(TimeSpan timeToReset)
        {
            TimeToReset = timeToReset;
        }
    }
}
