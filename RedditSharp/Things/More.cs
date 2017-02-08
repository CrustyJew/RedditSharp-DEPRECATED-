using System;
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
    public string[] Children { get; set; }

    [JsonProperty("parent_id")]
    public string ParentId { get; set; }

    protected override JToken GetJsonData(JToken json) {
      return json["data"];
    }

    public async Task<List<Thing>> GetThingsAsync()
    {
      var url = string.Format(MoreUrl, ParentId, string.Join(",", Children));
      var request = WebAgent.CreateGet(url);
            var response = await WebAgent.GetResponseAsync(request);
            var data = await request.Content.ReadAsStringAsync();
            List<Thing> toReturn = new List<Things.Thing>();

      var json = JObject.Parse(data)["json"];
      if (json["errors"].Count() != 0)
        throw new AuthenticationException("Incorrect login.");
      var moreJson = json["data"]["things"];

      foreach (JToken token in moreJson)
      {
        Thing parsed = Thing.Parse(Reddit, token);
        toReturn.Add(parsed);
      }
      return toReturn;
    }

  }
}
