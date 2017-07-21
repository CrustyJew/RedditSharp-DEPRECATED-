using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.WebSockets;
using System.Security.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    public class LiveUpdate : CreatedThing
    {
        private const string StrikeUpdateUrl = "/api/live/{0}/strike_update";
        private const string DeleteUpdateUrl = "/api/live/{0}/delete_update";

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("body_html")]
        public string BodyHtml { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mobile_embeds")]
        public ICollection<MobileEmbed> MobileEmbeds { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("embeds")]
        public ICollection<Embed> Embeds { get; set; }

        [JsonProperty("stricken")]
        public bool IsStricken { get; set; }


        public void Strike()
        {
            SimpleAction(StrikeUpdateUrl);
        }

        public void Delete()
        {
            SimpleAction(DeleteUpdateUrl);
        }

        public async Task<LiveUpdate> InitAsync(Reddit reddit, JToken post, IWebAgent webAgent)
        {
            CommonInit(reddit, post, webAgent);
            JsonConvert.PopulateObject(post["data"].ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

        public LiveUpdate Init(Reddit reddit, JToken post, IWebAgent webAgent)
        {
            CommonInit(reddit, post, webAgent);
            JsonConvert.PopulateObject(post["data"].ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

        private void CommonInit(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            base.Init(json);
            Reddit = reddit;
            WebAgent = webAgent;
        }

        public new void SimpleAction(string url)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = WebAgent.CreatePost(String.Format(url, Name));
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                api_type = "json",
                id = Name,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
        }

        public class MobileEmbed
        {
            [JsonProperty("provider_url")]
            public string ProviderUrl { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("original_url")]
            public string Original_Url { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("thumbnail_width")]
            public int ThumbnailWidth { get; set; }

            [JsonProperty("thumbnail_height")]
            public int ThumbnailHeight { get; set; }

            [JsonProperty("thumbnail_url")]
            public string ThumbnailUrl { get; set; }

            [JsonProperty("author_name")]
            public string AuthorName { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("provider_name")]
            public string ProviderName { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("author_url")]
            public string AuthorUrl { get; set; }
        }

        public class Embed
        {
            [JsonProperty("url")]
            public string AuthorUrl { get; set; }

            [JsonProperty("width")]
            public int Width { get; set; }

            [JsonProperty("height")]
            public int Height { get; set; }
        }
    }
}
