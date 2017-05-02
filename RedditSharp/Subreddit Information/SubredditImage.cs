using System;
using System.Threading.Tasks;

namespace RedditSharp
{
    /// <summary>
    /// Subreddit image logo.
    /// </summary>
    public class SubredditImage : RedditObject
    {
        private const string DeleteImageUrl = "/api/delete_sr_img";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subredditStyle">Subreddit style.</param>
        /// <param name="cssLink">Css link.</param>
        /// <param name="name">Name of the image.</param>
        public SubredditImage(SubredditStyle subredditStyle,
            string cssLink, string name) : base(subredditStyle?.WebAgent)
        {
            SubredditStyle = subredditStyle;
            Name = name;
            CssLink = cssLink;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subreddit">Subreddit style.</param>
        /// <param name="cssLink">Css link.</param>
        /// <param name="name">Name of the image.</param>
        /// <param name="url">Url of the image.</param>
        public SubredditImage(SubredditStyle subreddit,
            string cssLink, string name, string url)
            : this(subreddit, cssLink, name)
        {

            int discarded;
            if (int.TryParse(url, out discarded))
            {
                Url = new Uri(string.Format("http://thumbs.reddit.com/{0}_{1}.png", subreddit.Subreddit.FullName, url), UriKind.Absolute);
            }
            else
            {
                Url = new Uri(url);
            }
            // Handle legacy image urls
            // http://thumbs.reddit.com/FULLNAME_NUMBER.png

        }

        /// <summary>
        /// css link.
        /// </summary>
        public string CssLink { get; private set; }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Url.
        /// </summary>
        public Uri Url { get; private set; }

        /// <summary>
        /// Subreddit style.
        /// </summary>
        public SubredditStyle SubredditStyle { get; private set; }

        /// <summary>
        /// Delete this subreddit image.
        /// </summary>
        public async Task Delete()
        {
            await WebAgent.Post(DeleteImageUrl, new
            {
                img_name = Name,
                r = SubredditStyle.Subreddit.Name
            }).ConfigureAwait(false);
            SubredditStyle.Images.Remove(this);
        }

        /// <summary>
        /// Delete this subreddit image.
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAsync()
        {
            await WebAgent.Post(DeleteImageUrl, new
            {
                img_name = Name,
                r = SubredditStyle.Subreddit.Name
            }).ConfigureAwait(false);
            SubredditStyle.Images.Remove(this);
        }
    }
}
