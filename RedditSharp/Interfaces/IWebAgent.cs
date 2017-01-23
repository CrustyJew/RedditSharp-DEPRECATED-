using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net.Http;

namespace RedditSharp
{
    public interface IWebAgent
    {
        string AccessToken { get; set; }
        HttpRequestMessage CreateRequest(string url, string method);
        HttpRequestMessage CreateGet(string url);
        HttpRequestMessage CreatePost(string url);
        Task<JToken> ExecuteRequestAsync(HttpRequestMessage request);
        Task<JToken> CreateAndExecuteRequestAsync(string url);
        Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage request);
        void WritePostBody(HttpRequestMessage request, object data, params string[] additionalFields);
    }
}
