using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    /// <summary>
    /// A live update.
    /// </summary>
    public class LiveUpdate : CreatedThing
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reddit">Reddit.</param>
        /// <param name="json">Json payload.</param>
        public LiveUpdate(Reddit reddit, JToken json) : base(reddit, json) {
        }

        private const string StrikeUpdateUrl = "/api/live/{0}/strike_update";
        private const string DeleteUpdateUrl = "/api/live/{0}/delete_update";

        /// <summary>
        /// Body of the update.
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; }

        /// <summary>
        /// Body of the update in Html.
        /// </summary>
        [JsonProperty("body_html")]
        public string BodyHtml { get; }

        /// <summary>
        /// Name of the update.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; }

        /// <summary>
        /// Embeded items for mobile.
        /// </summary>
        [JsonProperty("mobile_embeds")]
        public ICollection<MobileEmbed> MobileEmbeds { get; }

        /// <summary>
        /// Name of the redditor who made the update.
        /// </summary>
        [JsonProperty("author")]
        public string Author { get; }

        /// <summary>
        /// Embeded items.
        /// </summary>
        [JsonProperty("embeds")]
        public ICollection<Embed> Embeds { get; }

        /// <summary>
        /// Returns true is the update is stricken.
        /// </summary>
        [JsonProperty("stricken")]
        public bool IsStricken { get; }

        /// <summary>
        /// Strike this update.
        /// </summary>
        /// <returns></returns>
        public Task StrikeAsync() => SimpleActionAsync(StrikeUpdateUrl);

        /// <summary>
        /// Delete this update.
        /// </summary>
        /// <returns></returns>
        public Task DeleteAsync() => SimpleActionAsync(DeleteUpdateUrl);

#pragma warning disable 1591
        public class MobileEmbed
        {
            [JsonProperty("provider_url")]
            public string ProviderUrl { get; }

            [JsonProperty("description")]
            public string Description { get; }

            [JsonProperty("original_url")]
            public string Original_Url { get; }

            [JsonProperty("url")]
            public string Url { get; }

            [JsonProperty("title")]
            public string Title { get; }

            [JsonProperty("thumbnail_width")]
            public int ThumbnailWidth { get; }

            [JsonProperty("thumbnail_height")]
            public int ThumbnailHeight { get; }

            [JsonProperty("thumbnail_url")]
            public string ThumbnailUrl { get; }

            [JsonProperty("author_name")]
            public string AuthorName { get; }

            [JsonProperty("version")]
            public string Version { get; }

            [JsonProperty("provider_name")]
            public string ProviderName { get; }

            [JsonProperty("type")]
            public string Type { get; }

            [JsonProperty("author_url")]
            public string AuthorUrl { get; }
        }

        public class Embed
        {
            [JsonProperty("url")]
            public string AuthorUrl { get; }

            [JsonProperty("width")]
            public int Width { get; }

            [JsonProperty("height")]
            public int Height { get; }
        }
#pragma warning restore 1591
    }
}
