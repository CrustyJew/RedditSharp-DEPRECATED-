using System;

namespace RedditSharp
{
    /// <summary>
    /// A captcha challenge.
    /// </summary>
    public struct Captcha
    {
        private const string UrlFormat = "http://www.reddit.com/captcha/{0}";

        /// <summary>
        /// Captcha Id.
        /// </summary>
        public readonly string Id;

        /// <summary>
        /// Captcha url.
        /// </summary>
        public readonly Uri Url;

        internal Captcha(string id)
        {
            Id = id;
            Url = new Uri(string.Format(UrlFormat, Id), UriKind.Absolute);
        }
    }
}