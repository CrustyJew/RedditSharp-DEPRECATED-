using Newtonsoft.Json;
using System;

namespace RedditSharp {

  public abstract class RedditObject {

      [JsonIgnore]
      public Reddit Reddit { get; }
      [JsonIgnore]
      public IWebAgent WebAgent => Reddit?.WebAgent;

      public RedditObject(Reddit reddit) {
          if (Reddit == null)
            throw new ArgumentNullException(nameof(reddit));
          Reddit = reddit;
      }

  }

}
