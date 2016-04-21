using System.Threading.Tasks;

namespace Blueclass.Wunderlist
{
    public interface IWunderlistConnector
    {
        Task<T> Get<T>(string request);
        Task<WunderlistResponse<T>> Post<T>(string request, T content);
        Task<WunderlistResponse<TResponse>> Post<TRequest, TResponse>(string request, TRequest content);
        Task<bool> Patch<T>(string request, T content);
        Task<bool> PostUpload(string url, byte[] content, string authorization, string date);
    }
}