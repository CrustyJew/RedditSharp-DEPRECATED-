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
        public string Body { get; private set; }

        [JsonProperty("body_html")]
        public string BodyHtml { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("mobile_embeds")]
        public ICollection<MobileEmbed> MobileEmbeds { get; private set; }

        [JsonProperty("author")]
        public string Author { get; private set; }

        [JsonProperty("embeds")]
        public ICollection<Embed> Embeds { get; private set; }

        [JsonProperty("stricken")]
        public bool IsStricken { get; private set; }

        public Task StrikeAsync() => SimpleActionAsync(StrikeUpdateUrl);

        public Task DeleteAsync() => SimpleActionAsync(DeleteUpdateUrl);

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
    }
}
