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
		private const string MoreUrl = "/api/morechildren.json?link_id={0}&children={1}&api_type=json";

		[JsonIgnore]
		private Reddit Reddit { get; set; }

		[JsonIgnore]
		private IWebAgent WebAgent { get; set; }

		[JsonProperty("children")]
		public string[] Children { get; set; }

		[JsonProperty("parent_id")]
		public string ParentId { get; set; }

		public More Init(Reddit reddit, JToken more, IWebAgent webAgent)
		{
			CommonInit(reddit, more, webAgent);
			JsonConvert.PopulateObject(more["data"].ToString(), this, reddit.JsonSerializerSettings);
			return this;
		}
		private void CommonInit(Reddit reddit, JToken more, IWebAgent webAgent)
		{
			base.Init(more);
			Reddit = reddit;
			WebAgent = webAgent;
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
				Thing parsed = Thing.Parse(Reddit, token, WebAgent);

                toReturn.Add(parsed);
			}
            return toReturn;
		}

		internal async Task<Thing> InitAsync(Reddit reddit, JToken json, IWebAgent webAgent)
		{
			CommonInit(reddit, json, webAgent);
            await Task.Factory.StartNew(() => JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings));
			return this;	
		}
	}
}
