using System;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    /// <summary>
    /// Represents an error that occurred during accessing or manipulating data on Reddit.
    /// </summary>
    [Serializable]
    public class RedditException : Exception
    {
        public JArray Errors { get; }

        /// <summary>
        /// Initializes a new instance of the RedditException class.
        /// </summary>
        public RedditException()
        {
        
        }

        /// <summary>
        /// Initializes a new instance of the RedditException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RedditException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the RedditException class with a specified error message and a JArray of errors
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="errors">List of errors.</param>
        public RedditException(string message, JArray errors)
            : base(message)
        {
            Errors = errors;
        }

        /// <summary>
        /// Initializes a new instance of the RedditException class with a specified error message and
        /// a referenced inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null
        /// reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public RedditException(string message, Exception inner)
            : base(message, inner)
        {
            
        }
        
    }
}
