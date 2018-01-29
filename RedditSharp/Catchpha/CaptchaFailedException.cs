using System;

namespace RedditSharp
{
    [Serializable]
    public class CaptchaFailedException : RedditException
    {
        public CaptchaFailedException()
        {
            
        }

        public CaptchaFailedException(string message)
            : base(message)
        {
            
        }

        public CaptchaFailedException(string message, Exception inner)
            : base(message, inner)
        {
            
        }
    }
}