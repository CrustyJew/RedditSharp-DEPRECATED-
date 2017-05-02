using System;
using System.Security.Authentication;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using RedditSharp.Extensions;

namespace RedditSharp.Things
{
    /// <summary>
    /// The base reddit class.
    /// </summary>
    public class Thing : RedditObject
    {
        
        #region Properties

        /// <summary>
        /// Current user
        /// </summary>
        public AuthenticatedUser User { get; set; }

        /// <summary>
        /// Shortlink to the item
        /// </summary>
        public virtual string Shortlink => "http://redd.it/" + Id;

        /// <summary>
        /// Base36 id.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// reddit full name.  Kind_Base36 id.  Example.  t1_a1b2c3
        /// </summary>
        public string FullName { get; internal set; }

        /// <summary>
        /// Thing kind.  t1, t2, t3 etc
        /// </summary>
        public string Kind { get; internal set; }

        /// <summary>
        /// The time at which this object was fetched from reddit servers.
        /// </summary>
        public DateTime FetchedAt { get; private set; }

        /// <summary>
        /// Gets the time since last fetch from reddit servers.
        /// </summary>
        public TimeSpan TimeSinceFetch => DateTime.Now - FetchedAt;
        #endregion


        /// <summary>
        /// Create new Thing from given JSON data.
        /// </summary>
        /// <param name="agent">WebAgent for requests</param>
        /// <param name="json">JSON data containing thing's info</param>
        /// <param name="user">Optional authenticated user</param>
        public Thing(IWebAgent agent, JToken json, AuthenticatedUser user = null) : base(agent)
        {
            User = user;
            Populate(json);
        }
        internal virtual JToken GetJsonData(JToken json)
        {
            return json;
        }
        internal void Populate(JToken json)
        {
            if (json == null)
                return;
            var data = json["name"] == null ? json["data"] : json;
            FullName = data["name"].ValueOrDefault<string>();
            Id = data["id"].ValueOrDefault<string>();
            Kind = json["kind"].ValueOrDefault<string>();
            FetchedAt = DateTime.Now;
            Helpers.PopulateObject(GetJsonData(json), this);
        }

        // Awaitables don't have to be called asyncronously

        /// <summary>
        /// Parses what it is, based on the t(number) attribute
        /// </summary>
        /// <param name="agent">IWebAgent you're using</param>
        /// <param name="json">Json Token</param>
        /// <returns>A "Thing", such as a comment, user, post, etc.</returns>
        public static Thing Parse(IWebAgent agent, JToken json)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            var kind = json["kind"].ValueOrDefault<string>();
            switch (kind)
            {
                case "t1":
                    return new Comment(agent, json, null);
                case "t2":
                    return new RedditUser(agent, json);
                case "t3":
                    return new Post(agent, json);
                case "t4":
                    return new PrivateMessage(agent, json);
                case "t5":
                    return new Subreddit(agent, json);
                case "modaction":
                    return new ModAction(agent, json);
                case "more":
                    return new More(agent, json);
                case "LiveUpdate":
                    return new LiveUpdate(agent, json);
                case "LiveUpdateEvent":
                    return new LiveUpdateEvent(agent, json);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Tries to find the "Thing" you are looking for
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="agent"></param>
        /// <param name="json"></param>
        /// <returns>The "Thing"</returns>
        public static T Parse<T>(IWebAgent agent, JToken json) where T : Thing
        {
            Thing result = Parse(agent, json);
            if (result == null)
            {
                if (typeof(T) == typeof(WikiPageRevision))
                {
                    result = new WikiPageRevision(agent, json);
                }
                else if (typeof(T) == typeof(ModAction))
                {
                    result = new ModAction(agent, json);
                }
                else if (typeof(T) == typeof(Contributor))
                {
                    result = new Contributor(agent, json);
                }
                else if (typeof(T) == typeof(BannedUser))
                {
                    result = new BannedUser(agent, json);
                }
                else if (typeof(T) == typeof(More))
                {
                    result = new More(agent, json);
                }
                else if (typeof(T) == typeof(LiveUpdate))
                {
                    result = new LiveUpdate(agent, json);
                }
                else if (typeof(T) == typeof(LiveUpdateEvent))
                {
                    result = new LiveUpdateEvent(agent, json);
                }
            }
            return result as T;
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
            return await WebAgent.Post(endpoint, new
            {
                id = FullName
            }).ConfigureAwait(false);
        }
    }
}
