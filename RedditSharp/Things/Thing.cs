using System;
using System.Security.Authentication;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using RedditSharp.Extensions;

namespace RedditSharp.Things
{
    public class Thing : RedditObject
    {
        public Thing(Reddit reddit, JToken json) : base(reddit) {
          Populate(json);
        }

        protected void Populate(JToken json) {
            if (json == null)
              return;
            var data = json["name"] == null ? json["data"] : json;
            FullName = data["name"].ValueOrDefault<string>();
            Id = data["id"].ValueOrDefault<string>();
            Kind = json["kind"].ValueOrDefault<string>();
            FetchedAt = DateTime.Now;
            Reddit.PopulateObject(GetJsonData(json), this);
        }

        protected virtual JToken GetJsonData(JToken json) {
          return json.ToString();
        }

        /// <summary>
        /// Shortlink to the item
        /// </summary>
        public virtual string Shortlink =>  "http://redd.it/" + Id;

        /// <summary>
        /// Base36 id.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// reddit full name.  Kind_Base36 id.  Example.  t1_a1b2c3
        /// </summary>
        public string FullName { get; internal set; }

        /// <summary>
        /// Thing kind.  t1, t2, t3 etc
        /// </summary>
        public string Kind { get; private set; }

        /// <summary>
        /// The time at which this object was fetched from reddit servers.
        /// </summary>
        public DateTime FetchedAt { get; private set; }

        /// <summary>
        /// Gets the time since last fetch from reddit servers.
        /// </summary>
        public TimeSpan TimeSinceFetch => DateTime.Now - FetchedAt;

        // Awaitables don't have to be called asyncronously

        /// <summary>
        /// Parses what it is, based on the t(number) attribute
        /// </summary>
        /// <param name="reddit">Reddit you're using</param>
        /// <param name="json">Json Token</param>
        /// <returns>A "Thing", such as a comment, user, post, etc.</returns>
        public static Thing Parse(Reddit reddit, JToken json)
        {
            var kind = json["kind"].ValueOrDefault<string>();
            switch (kind)
            {
                case "t1":
                    return new Comment(reddit, json, null);
                case "t2":
                    return new RedditUser(reddit, json);
                case "t3":
                    return new Post(reddit, json);
                case "t4":
                    return new PrivateMessage(reddit, json);
                case "t5":
                    return new Subreddit(reddit, json);
                case "modaction":
                    return new ModAction(reddit, json);
                case "more":
                    return new More(reddit, json);
                case "LiveUpdate":
                    return new LiveUpdate(reddit, json);
                case "LiveUpdateEvent":
                    return new LiveUpdateEvent(reddit, json);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Tries to find the "Thing" you are looking for
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reddit"></param>
        /// <param name="json"></param>
        /// <returns>The "Thing"</returns>
        public static Thing Parse<T>(Reddit reddit, JToken json) where T : Thing
        {
            Thing result = Parse(reddit, json);
            if (result == null)
            {
                if (typeof(T) == typeof(WikiPageRevision))
                {
                    return new WikiPageRevision(reddit, json);
                }
                else if (typeof(T) == typeof(ModAction))
                {
                    return new ModAction(reddit, json);
                }
                else if (typeof(T) == typeof(Contributor))
                {
                    return new Contributor(reddit, json);
                }
                else if (typeof(T) == typeof(BannedUser))
                {
                    return new BannedUser(reddit, json);
                }
                else if (typeof(T) == typeof(More))
                {
                    return new More(reddit, json);
                }
                else if (typeof(T) == typeof(LiveUpdate))
                {
                    return new LiveUpdate(reddit, json);
                }
                else if (typeof(T) == typeof(LiveUpdateEvent))
                {
                    return new LiveUpdateEvent(reddit, json);
                }
            }
            return result;
        }

        /// <summary>
        /// Execute a simple POST request against the reddit api.
        /// Supports endpoints that require only id and modhash as
        /// parameters.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        protected virtual async Task<JToken> SimpleActionAsync(string endpoint)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            return await WebAgent.Post(endpoint, new
            {
                id = FullName,
                uh = Reddit.User.Modhash
            }).ConfigureAwait(false);
        }
    }
}
