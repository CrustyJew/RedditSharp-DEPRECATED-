using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RedditSharp
{
    // TODO: confirm this?
    /// <summary>
    /// Justification 
    /// </summary>
    public enum FlairPosition
    {
        /// <summary>
        /// Right justified.
        /// </summary>
        right,
        /// <summary>
        /// Left justified.
        /// </summary>
        left
    }

    /// <summary>
    /// Template for a user flair.
    /// </summary>
    public class UserFlairTemplate // TODO: Consider using this class to set templates as well
    {
        /// <summary>
        /// Flair text.
        /// </summary>
        [JsonProperty("flair_text")]
        public string Text { get; private set; }

        /// <summary>
        /// Flair clss class.
        /// </summary>
        [JsonProperty("flair_css_class")]
        public string CssClass { get; private set; }

        /// <summary>
        /// Flair template id.
        /// </summary>
        [JsonProperty("flair_template_id")]
        public string TemplateId { get; private set; }

        /// <summary>
        /// Set to true if this is user editable.
        /// </summary>
        [JsonProperty("flair_text_editable")]
        public bool IsEditable { get; private set; }

        /// <summary>
        /// Position of the flair left or right.
        /// </summary>
        [JsonProperty("flair_position")]
        [JsonConverter(typeof(StringEnumConverter))]
        public FlairPosition FlairPosition { get; private set; }
    }
}
