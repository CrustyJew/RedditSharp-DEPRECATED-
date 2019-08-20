using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things.User
{
	public class NotedUser : RelUser
	{
		public NotedUser(IWebAgent agent, JToken json) : base(agent, json)
		{
		}

		[JsonProperty("note")]
		public String Note { get; internal set; }
	}
}
