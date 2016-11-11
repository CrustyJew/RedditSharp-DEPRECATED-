using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RedditSharp
{
    public enum FlairPosition
    {
        right,
        left
    }
    public class UserFlairTemplate // TODO: Consider using this class to set templates as well
    {
        [JsonProperty("flair_text")]
        public string Text { get; set; }

        [JsonProperty("flair_css_class")]
        public string CssClass { get; set; }

        [JsonProperty("flair_template_id")]
        public string TemplateId { get; set; }

        [JsonProperty("flair_text_editable")]
        public bool IsEditable { get; set; }

        [JsonProperty("flair_position")]
        [JsonConverter(typeof(StringEnumConverter))]
        public FlairPosition FlairPosition { get; set; }
    }
}