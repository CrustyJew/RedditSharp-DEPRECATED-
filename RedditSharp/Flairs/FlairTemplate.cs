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
        /// <summary>
        /// Flair text.
        /// </summary>
        [JsonProperty("flair_text")]
        public string Text { get; set; }

        /// <summary>
        /// Flair clss class.
        /// </summary>
        [JsonProperty("flair_css_class")]
        public string CssClass { get; set; }

        /// <summary>
        /// Flair template id.
        /// </summary>
        [JsonProperty("flair_template_id")]
        public string TemplateId { get; set; }

        /// <summary>
        /// Set to true if this is user editable.
        /// </summary>
        [JsonProperty("flair_text_editable")]
        public bool IsEditable { get; set; }

        /// <summary>
        /// Position of the flair left or right.
        /// </summary>
        [JsonProperty("flair_position")]
        [JsonConverter(typeof(StringEnumConverter))]
        public FlairPosition FlairPosition { get; set; }
    }
}