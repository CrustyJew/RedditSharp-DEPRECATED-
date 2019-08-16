using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things.User
{
	public class RelUser : PartialUser
	{
		public RelUser(IWebAgent agent, JToken json) : base(agent, json)
		{
		}

		/// <summary>
		/// UTC time of when this user was added to the list
		/// </summary>
		[JsonProperty("date")]
		[JsonConverter(typeof(UnixTimestampConverter))]
		public DateTime? DateUTC { get; set; }
	}
}
