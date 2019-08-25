using Newtonsoft.Json.Linq;
using RedditSharp.Extensions;
using System;
using System.Threading.Tasks;

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

        public JToken RawJson { get; private set; }

        /// <summary>
        /// Gets a property of this Thing without any automatic conversion,
        /// even to a <see cref="String"/>.
        /// </summary>
        /// <param name="property">The reddit API name of the property</param>
        /// <returns>The property's value as a <see cref="String"/> or null if the property 
        /// doesn't exist or is null.</returns>
        public JToken this[String property] => RawJson[property];
        #endregion


        /// <summary>
        /// Create new Thing from given JSON data.
        /// </summary>
        /// <param name="agent">An <see cref="IWebAgent"/>to make requests with</param>
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
            RawJson = data;
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
        /// Supports endpoints that require only id as parameter.
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
        /// <summary>
        /// Execute a simple POST request against the reddit api.
        /// Supports endpoints that require only id as parameter.
        /// </summary>
        /// <param name="agent"><see cref="IWebAgent"/> used to execute post</param>
        /// <param name="fullname">FullName of thing to act on. eg. t1_66666</param>
        /// <param name="endpoint">URL to post to</param>
        /// <returns></returns>
        protected static Task<JToken> SimpleActionAsync(IWebAgent agent, string fullname, string endpoint)
        {
            return agent.Post(endpoint, new
            {
                id = fullname
            });
        }
    }
}
