using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using RedditSharp.Interfaces;

namespace RedditSharp
{
    public partial class Helpers
    {
        internal static List<T> PopulateObjects<T>(JToken json, IWebAgent webAgent)
            where T : ISettableWebAgent, new()
        {
            if (json.Type != JTokenType.Array)
                throw new ArgumentException("must be of type array", nameof(json));

            var objects = new List<T>();

            for (var i = 0; i < json.Count(); i++)
            {
                var item = new T();
                PopulateObject(json[i], item);
                item.WebAgent = webAgent;
                objects.Add(item);
            }

            return objects;
        }
    }
}