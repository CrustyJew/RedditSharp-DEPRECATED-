#pragma warning disable 1591
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Extensions
{
    public static class Extensions
    {
        public static T ValueOrDefault<T>(this IEnumerable<JToken> enumerable)
        {
            if (enumerable == null)
                return default(T);
            return enumerable.Value<T>();
        }
    }
}
#pragma warning restore 1591