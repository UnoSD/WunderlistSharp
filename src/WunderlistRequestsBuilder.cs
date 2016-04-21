using System.Net.Http;
using System.Text;
using Blueclass.Web;
using Newtonsoft.Json;

namespace Blueclass.Wunderlist
{
    public class WunderlistRequestsBuilder : IHttpRequestsBuilder
    {
        const string ApiEndpoint = "https://a.wunderlist.com/api/v1";

        readonly IWunderlistAuthenticator _authenticator;

        public WunderlistRequestsBuilder(IWunderlistAuthenticator authenticator)
        {
            _authenticator = authenticator;
        }

        public HttpRequestMessage BuildHttpRequestMessage(string request, HttpMethod method)
        {
            var url = $"{ApiEndpoint}/{request}";

            var httpRequestMessage = new HttpRequestMessage(method, url);

            httpRequestMessage.Headers.Add("X-Access-Token", _authenticator.GetAccessToken());
            httpRequestMessage.Headers.Add("X-Client-ID", _authenticator.GetClientId());

            return httpRequestMessage;
        }

        public HttpRequestMessage BuildHttpRequestMessage<T>(string request, HttpMethod method, T content)
        {
            HttpContent stringContent = null;

            var jsonContent = JsonConvert.SerializeObject(content);

            if (jsonContent != null && jsonContent != "null")
                stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var requestMessage = this.BuildHttpRequestMessage(request, method);

            requestMessage.Content = stringContent;

            return requestMessage;
        }

        public HttpRequestMessage BuildUploadHttpRequestMessage(string url, HttpMethod method, byte[] content, string authorization, string date)
        {
            var httpRequestMessage = new HttpRequestMessage(method, url) { Content = new ByteArrayContent(content) };

            httpRequestMessage.Headers.Add("Authorization", authorization);
            httpRequestMessage.Headers.Add("x-amz-date", date);

            return httpRequestMessage;
        }
    }
}