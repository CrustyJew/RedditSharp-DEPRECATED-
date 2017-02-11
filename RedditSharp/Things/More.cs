using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
  public class More : Thing
  {
    public More(Reddit reddit, JToken json) : base(reddit, json) {
    }

    private const string MoreUrl = "/api/morechildren.json?link_id={0}&children={1}&api_type=json";

    [JsonProperty("children")]
    public string[] Children { get; }

    [JsonProperty("parent_id")]
    public string ParentId { get; }

    protected override JToken GetJsonData(JToken json) => json["data"];

    public async Task<List<Thing>> GetThingsAsync()
    {
      var url = string.Format(MoreUrl, ParentId, string.Join(",", Children));
      var json = await WebAgent.Get(url).ConfigureAwait(false);
      if (json["errors"].Count() != 0)
        throw new AuthenticationException("Incorrect login.");
      var moreJson = json["data"]["things"];
      return moreJson.Select(t => Thing.Parse(Reddit, t)).ToList();
    }

  }
}
