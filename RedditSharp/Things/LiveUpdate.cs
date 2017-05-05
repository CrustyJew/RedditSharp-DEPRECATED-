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
        /// <param name="agent">WebAgent.</param>
        /// <param name="json">Json payload.</param>
        public LiveUpdate(IWebAgent agent, JToken json) : base(agent, json) {
        }

        private const string StrikeUpdateUrl = "/api/live/{0}/strike_update";
        private const string DeleteUpdateUrl = "/api/live/{0}/delete_update";

        /// <summary>
        /// Body of the update.
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; private set; }

        /// <summary>
        /// Body of the update in Html.
        /// </summary>
        [JsonProperty("body_html")]
        public string BodyHtml { get; private set; }

        /// <summary>
        /// Name of the update.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Embeded items for mobile.
        /// </summary>
        [JsonProperty("mobile_embeds")]
        public ICollection<MobileEmbed> MobileEmbeds { get; private set; }

        /// <summary>
        /// Name of the redditor who made the update.
        /// </summary>
        [JsonProperty("author")]
        public string Author { get; private set; }

        /// <summary>
        /// Embeded items.
        /// </summary>
        [JsonProperty("embeds")]
        public ICollection<Embed> Embeds { get; private set; }

        /// <summary>
        /// Returns true is the update is stricken.
        /// </summary>
        [JsonProperty("stricken")]
        public bool IsStricken { get; private set; }

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
            public string ProviderUrl { get; private set; }

            [JsonProperty("description")]
            public string Description { get; private set; }

            [JsonProperty("original_url")]
            public string Original_Url { get; private set; }

            [JsonProperty("url")]
            public string Url { get; private set; }

            [JsonProperty("title")]
            public string Title { get; private set; }

            [JsonProperty("thumbnail_width")]
            public int ThumbnailWidth { get; private set; }

            [JsonProperty("thumbnail_height")]
            public int ThumbnailHeight { get; private set; }

            [JsonProperty("thumbnail_url")]
            public string ThumbnailUrl { get; private set; }

            [JsonProperty("author_name")]
            public string AuthorName { get; private set; }

            [JsonProperty("version")]
            public string Version { get; private set; }

            [JsonProperty("provider_name")]
            public string ProviderName { get; private set; }

            [JsonProperty("type")]
            public string Type { get; private set; }

            [JsonProperty("author_url")]
            public string AuthorUrl { get; private set; }
        }

        public class Embed
        {
            [JsonProperty("url")]
            public string AuthorUrl { get; private set; }

            [JsonProperty("width")]
            public int Width { get; private set; }

            [JsonProperty("height")]
            public int Height { get; private set; }
        }
#pragma warning restore 1591
    }
}
