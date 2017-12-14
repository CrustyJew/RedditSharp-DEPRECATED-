using System;
using System.Security.Authentication;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using RedditSharp.Extensions;

namespace RedditSharp.Things
{
    public class Thing
    {
        protected Reddit Reddit { get; set; }

        protected IWebAgent WebAgent { get; set; }

        internal void Init(JToken json)
        {
            if (json == null)
                return;
            var data = json["name"] == null ? json["data"] : json;
            FullName = data["name"].ValueOrDefault<string>();
            Id = data["id"].ValueOrDefault<string>();
            Kind = json["kind"].ValueOrDefault<string>();
            FetchedAt = DateTimeOffset.Now;
        }


        /// <summary>
        /// Base36 id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// reddit full name.  Kind_Base36 id.  Example.  t1_a1b2c3
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Thing kind.  t1, t2, t3 etc
        /// </summary>
        public string Kind { get; set; }

        /// <summary>
        /// The time at which this object was fetched from reddit servers.
        /// </summary>
        public DateTimeOffset FetchedAt { get; private set; }

        /// <summary>
        /// Gets the time since last fetch from reddit servers.
        /// </summary>
        public TimeSpan TimeSinceFetch
        {
            get
            {
                return DateTimeOffset.Now - FetchedAt;
            }
        }
        // Awaitables don't have to be called asyncronously

        /// <summary>
        /// Parses what it is, based on the t(number) attribute
        /// </summary>
        /// <param name="reddit">Reddit you're using</param>
        /// <param name="json">Json Token</param>
        /// <param name="webAgent">WebAgent</param>
        /// <returns>A "Thing", such as a comment, user, post, etc.</returns>
        public static async Task<Thing> ParseAsync(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            var kind = json["kind"].ValueOrDefault<string>();
            switch (kind)
            {
                case "t1":
                    return await new Comment().InitAsync(reddit, json, webAgent, null);
                case "t2":
                    return await new RedditUser().InitAsync(reddit, json, webAgent);
                case "t3":
                    return await new Post().InitAsync(reddit, json, webAgent);
                case "t4":
                    return await new PrivateMessage().InitAsync(reddit, json, webAgent);
                case "t5":
                    return await new Subreddit().InitAsync(reddit, json, webAgent);
                case "modaction":
                    return await new ModAction().InitAsync(reddit, json, webAgent);
                case "more":
                    return await new More().InitAsync(reddit, json, webAgent);
                case "LiveUpdate":
                    return await new LiveUpdate().InitAsync(reddit, json, webAgent);
                case "LiveUpdateEvent":
                    return await new LiveUpdateEvent().InitAsync(reddit, json, webAgent);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Parses what it is, based on the t(number) attribute
        /// </summary>
        /// <param name="reddit">Reddit you're using</param>
        /// <param name="json">Json Token</param>
        /// <param name="webAgent">WebAgent</param>
        /// <returns>A "Thing", such as a comment, user, post, etc.</returns>
        public static Thing Parse(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            var kind = json["kind"].ValueOrDefault<string>();
            switch (kind)
            {
                case "t1":
                    return new Comment().Init(reddit, json, webAgent, null);
                case "t2":
                    return new RedditUser().Init(reddit, json, webAgent);
                case "t3":
                    return new Post().Init(reddit, json, webAgent);
                case "t4":
                    return new PrivateMessage().Init(reddit, json, webAgent);
                case "t5":
                    return new Subreddit().Init(reddit, json, webAgent);
                case "modaction":
                    return new ModAction().Init(reddit, json, webAgent);
                case "more":
                    return new More().Init(reddit, json, webAgent);
                case "LiveUpdate":
                    return new LiveUpdate().Init(reddit, json, webAgent);
                case "LiveUpdateEvent":
                    return new LiveUpdateEvent().Init(reddit, json, webAgent);
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
        /// <param name="webAgent"></param>
        /// <returns>The "Thing"</returns>
        public static async Task<Thing> ParseAsync<T>(Reddit reddit, JToken json, IWebAgent webAgent) where T : Thing
        {
            Thing result = await ParseAsync(reddit, json, webAgent);
            if (result == null)
            {
                if (typeof(T) == typeof(WikiPageRevision))
                {
                    return await new WikiPageRevision().InitAsync(reddit, json, webAgent);
                }
                else if (typeof(T) == typeof(ModAction))
                {
                    return await new ModAction().InitAsync(reddit, json, webAgent);
                }
                else if (typeof(T) == typeof(Contributor))
                {
                    return await new Contributor().InitAsync(reddit, json, webAgent);
                }
                else if (typeof(T) == typeof(BannedUser))
                {
                    return await new BannedUser().InitAsync(reddit, json, webAgent);
                }
                else if (typeof(T) == typeof(More))
                {
                    return await new More().InitAsync(reddit, json, webAgent);
                }
                else if (typeof(T) == typeof(LiveUpdate))
                {
                    return await new LiveUpdate().InitAsync(reddit, json, webAgent);
                }
                else if (typeof(T) == typeof(LiveUpdateEvent))
                {
                    return await new LiveUpdateEvent().InitAsync(reddit, json, webAgent);
                }
            }
            return result;
        }

        /// <summary>
        /// Tries to find the "Thing" you are looking for
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reddit"></param>
        /// <param name="json"></param>
        /// <param name="webAgent"></param>
        /// <returns>The "Thing"</returns>
        public static Thing Parse<T>(Reddit reddit, JToken json, IWebAgent webAgent) where T : Thing
        {
            Thing result = Parse(reddit, json, webAgent);
            if (result == null)
            {
                if (typeof(T) == typeof(WikiPageRevision))
                {
                    return new WikiPageRevision().Init(reddit, json, webAgent);
                }
                else if (typeof(T) == typeof(ModAction))
                {
                    return new ModAction().Init(reddit, json, webAgent);
                }
                else if (typeof(T) == typeof(Contributor))
                {
                    return new Contributor().Init(reddit, json, webAgent);
                }
                else if (typeof(T) == typeof(BannedUser))
                {
                    return new BannedUser().Init(reddit, json, webAgent);
                }
                else if (typeof(T) == typeof(More))
                {
                    return new More().Init(reddit, json, webAgent);
                }
                else if (typeof(T) == typeof(LiveUpdate))
                {
                    return new LiveUpdate().Init(reddit, json, webAgent);
                }
                else if (typeof(T) == typeof(LiveUpdateEvent))
                {
                    return new LiveUpdateEvent().Init(reddit, json, webAgent);
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
        protected virtual string SimpleAction(string endpoint)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = WebAgent.CreatePost(endpoint);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            return data;
        }
    }
}
