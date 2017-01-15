using System;
using System.Threading.Tasks;

namespace RedditSharp
{
    public class SubredditImage
    {
        private const string DeleteImageUrl = "/api/delete_sr_img";

        private Reddit Reddit { get; set; }
        private IWebAgent WebAgent { get; set; }

        public SubredditImage(Reddit reddit, SubredditStyle subredditStyle,
            string cssLink, string name, IWebAgent webAgent)
        {
            Reddit = reddit;
            WebAgent = webAgent;
            SubredditStyle = subredditStyle;
            Name = name;
            CssLink = cssLink;
        }

        public SubredditImage(Reddit reddit, SubredditStyle subreddit,
            string cssLink, string name, string url, IWebAgent webAgent)
            : this(reddit, subreddit, cssLink, name, webAgent)
        {
            Url = new Uri(url);
            // Handle legacy image urls
            // http://thumbs.reddit.com/FULLNAME_NUMBER.png
            int discarded;
            if (int.TryParse(url, out discarded))
                Url = new Uri(string.Format("http://thumbs.reddit.com/{0}_{1}.png", subreddit.Subreddit.FullName, url), UriKind.Absolute);
        }

        /// <summary>
        /// css link.
        /// </summary>
        public string CssLink { get; set; }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Url.
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        /// Subreddit style.
        /// </summary>
        public SubredditStyle SubredditStyle { get; set; }

        /// <summary>
        /// Delete this subreddit image.
        /// </summary>
        public void Delete()
        {
            var request = WebAgent.CreatePost(DeleteImageUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                img_name = Name,
                uh = Reddit.User.Modhash,
                r = SubredditStyle.Subreddit.Name
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            SubredditStyle.Images.Remove(this);
        }

        /// <summary>
        /// Delete this subreddit image.
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAsync()
        {
            var request = WebAgent.CreatePost(DeleteImageUrl);
            var stream = await request.GetRequestStreamAsync();
            WebAgent.WritePostBody(stream, new
            {
                img_name = Name,
                uh = Reddit.User.Modhash,
                r = SubredditStyle.Subreddit.Name
            });
            stream.Close();
            var response = await request.GetResponseAsync();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            SubredditStyle.Images.Remove(this);
        }
    }
}
