using HttpClientService.Models;

namespace HttpClientService.Interfaces
{
    public interface IHttpClientWrapper
    {
        Task<HttpResponse<TSuccessResponse>> SendAsync<TSuccessResponse>(HttpRequestMessage message);

        Task<HttpResponse> SendAsync(HttpRequestMessage message);
    }
}
