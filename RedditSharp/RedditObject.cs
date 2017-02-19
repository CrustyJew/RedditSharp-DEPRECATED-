#pragma warning disable 1591
using Newtonsoft.Json;
using System;

namespace RedditSharp {

  public abstract class RedditObject {

      [JsonIgnore]
      public Reddit Reddit { get; }
      [JsonIgnore]
      public IWebAgent WebAgent => Reddit?.WebAgent;

      public RedditObject(Reddit reddit) {
          if (reddit == null)
            throw new ArgumentNullException(nameof(reddit));
          Reddit = reddit;
      }

  }

}
#pragma warning restore 1591
