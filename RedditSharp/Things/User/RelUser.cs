using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things.User
{
	/// <summary>
	/// Represents a user that was added to a user list or listing at a
	/// time.
	/// </summary>
	public class RelUser : PartialUser
	{
		public RelUser(IWebAgent agent, JToken json) : base(agent, json)
		{
		}

		/// <summary>
		/// The base-36 fullname of the rel item
		/// </summary>
		[JsonProperty("rel_id")]
		public String RelFullName { get; internal set; }

		/// <summary>
		/// UTC time of when this user was added to the list
		/// </summary>
		[JsonProperty("date")]
		[JsonConverter(typeof(UnixTimestampConverter))]
		public DateTime? DateUTC { get; internal set; }
	}
}
