using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    public class LiveUpdate : CreatedThing
    {
        public LiveUpdate(Reddit reddit, JToken json) : base(reddit, json) {
        }

        private const string StrikeUpdateUrl = "/api/live/{0}/strike_update";
        private const string DeleteUpdateUrl = "/api/live/{0}/delete_update";

        [JsonProperty("body")]
        public string Body { get; }

        [JsonProperty("body_html")]
        public string BodyHtml { get; }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("mobile_embeds")]
        public ICollection<MobileEmbed> MobileEmbeds { get; }

        [JsonProperty("author")]
        public string Author { get; }

        [JsonProperty("embeds")]
        public ICollection<Embed> Embeds { get; }

        [JsonProperty("stricken")]
        public bool IsStricken { get; }

        public Task StrikeAsync() => SimpleActionAsync(StrikeUpdateUrl);

        public Task DeleteAsync() => SimpleActionAsync(DeleteUpdateUrl);

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
    }
}
