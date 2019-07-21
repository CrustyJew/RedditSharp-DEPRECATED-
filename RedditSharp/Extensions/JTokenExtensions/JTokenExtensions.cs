using Newtonsoft.Json.Linq;

namespace RedditSharp.Extensions.JTokenExtensions
{
    public static class JTokenExtensions
    {
        public static void ThrowIfHasErrors(this JToken json, string message)
        {
            if (json["errors"].IsNonEmptyArray(out var errors))
            {
                throw new RedditException($"{message} {errors}", errors);
            }
        }

        public static bool IsNonEmptyArray(this JToken json, out JArray array)
        {
            var isArray = _IsArray(json, out array);
            return isArray && array.Count > 0;
        }

        private static bool _IsArray(JToken json, out JArray array)
        {
            if (json != null && json.Type == JTokenType.Array)
            {
                array = (JArray)json;
                return true;
            }
            array = default;
            return false;

        }
    }
}