using System.Net.Http;
using System.Threading.Tasks;
using Blueclass.Web;
using Newtonsoft.Json;

namespace Blueclass.Wunderlist
{
    public class WunderlistConnector : IWunderlistConnector
    {
        readonly IHttpRequestsBuilder _wunderlistRequestsBuilder;

        public WunderlistConnector(IHttpRequestsBuilder wunderlistRequestsBuilder)
        {
            _wunderlistRequestsBuilder = wunderlistRequestsBuilder;
        }

        public async Task<T> Get<T>(string request)
        {
            var response = await this.WunderlistSend(HttpMethod.Get, request);

            if (!response.IsSuccessStatusCode)
                return default(T);

            var responseString = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(responseString);
        }

        public async Task<WunderlistResponse<TResponse>> Post<TRequest, TResponse>(string request, TRequest content)
        {
            var response = await this.WunderlistSend(HttpMethod.Post, request, content);

            var responseObject = JsonConvert.DeserializeObject<TResponse>(await response.Content.ReadAsStringAsync());

            return new WunderlistResponse<TResponse> { IsSuccessStatusCode = response.IsSuccessStatusCode, ResponseObject = responseObject };
        }

        public async Task<WunderlistResponse<T>> Post<T>(string request, T content)
        {
            var response = await this.WunderlistSend(HttpMethod.Post, request, content);

            var responseObject = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());

            return new WunderlistResponse<T> { IsSuccessStatusCode = response.IsSuccessStatusCode, ResponseObject = responseObject };
        }

        public async Task<bool> Patch<T>(string request, T content)
        {
            var response = await this.WunderlistSend(new HttpMethod("PATCH"), request, content);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PostUpload(string url, byte[] content, string authorization, string date)
        {
            var request = _wunderlistRequestsBuilder.BuildUploadHttpRequestMessage(url, HttpMethod.Put, content, authorization, date);

            var response = await WunderlistConnector.WunderlistSend(request);
            
            return response.IsSuccessStatusCode;
        }

        async Task<HttpResponseMessage> WunderlistSend(HttpMethod method, string request)
        {
            return await WunderlistConnector.WunderlistSend(_wunderlistRequestsBuilder.BuildHttpRequestMessage(request, method));
        }

        async Task<HttpResponseMessage> WunderlistSend<T>(HttpMethod method, string request, T content)
        {
            return await WunderlistConnector.WunderlistSend(_wunderlistRequestsBuilder.BuildHttpRequestMessage(request, method, content));
        }

        static async Task<HttpResponseMessage> WunderlistSend(HttpRequestMessage httpRequestMessage)
        {
            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(httpRequestMessage);

                return response;
            }
        }
    }
}