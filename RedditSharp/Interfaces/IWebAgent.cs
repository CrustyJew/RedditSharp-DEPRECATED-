using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net.Http;

namespace RedditSharp
{
    public interface IWebAgent
    {
        string AccessToken { get; set; }
        Task<JToken> Get(string url);
        Task<JToken> Post(string url, object data, params string[] additionalFields);
        Task<JToken> Put(string url, object data);
        HttpRequestMessage CreateRequest(string url, string method);
        Task<JToken> ExecuteRequestAsync(HttpRequestMessage request);
        Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage request);
        void WritePostBody(HttpRequestMessage request, object data, params string[] additionalFields);
    }
}
