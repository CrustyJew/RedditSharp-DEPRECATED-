using System;

namespace RedditSharp
{
    /// <summary>
    /// Exception that gets thrown if you try and submit a duplicate link to a SubReddit
    /// </summary>
    public class DuplicateLinkException : RedditException
    {
        #pragma warning disable 1591
        public DuplicateLinkException()
        {
        }

        public DuplicateLinkException(string message)
            : base(message)
        {
        }

        public DuplicateLinkException(string message, Exception inner)
            : base(message, inner)
        {
        }
        #pragma warning restore 1591
    }
}

