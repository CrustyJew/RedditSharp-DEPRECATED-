using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedditSharp
{
    /// <summary>
    /// A web agent to talk to the reddit api.
    /// </summary>
    public interface IWebAgent
    {
        /// <summary>
        /// Used to make calls against Reddit's API using OAuth2
        /// </summary>
        string AccessToken { get; set; }

        /// <summary>
        /// Execute a GET request against the reddit api.
        /// </summary>
        /// <param name="url">Endpoint.</param>
        /// <returns></returns>
        Task<JToken> Get(string url);

        /// <summary>
        /// Execute a POST request against the reddit api.
        /// </summary>
        /// <param name="url">Endpoint.</param>
        /// <param name="data">Post body.</param>
        /// <param name="additionalFields">Additional fields to pass.</param>
        /// <returns></returns>
        Task<JToken> Post(string url, object data, params string[] additionalFields);

        /// <summary>
        /// Execute a PUT request against the reddit api.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<JToken> Put(string url, object data);

        /// <summary>
        /// Create a <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="url">target  uri</param>
        /// <param name="method">http method</param>
        /// <returns></returns>
        HttpRequestMessage CreateRequest(string url, string method);

        /// <summary>
        /// Executes the web request and handles errors in the response
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<JToken> ExecuteRequestAsync(Func<HttpRequestMessage> request);

        /// <summary>
        /// Executes a web request against the reddit api.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage request);

        /// <summary>
        /// Write a post body to a request.
        /// </summary>
        /// <param name="request">Http request to use.</param>
        /// <param name="data">post body.</param>
        /// <param name="additionalFields">additional fields to pass.</param>
        void WritePostBody(HttpRequestMessage request, object data, params string[] additionalFields);

        /// <summary>
        /// <see cref="RateLimitManager"/> for this instance of IWebAgent
        /// </summary>
        IRateLimiter RateLimiter { get; set; }
    }
}
