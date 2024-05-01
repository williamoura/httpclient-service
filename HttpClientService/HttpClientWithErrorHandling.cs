using HttpClientService.Exceptions;
using HttpClientService.Interfaces;
using HttpClientService.Models;

namespace HttpClientService
{
    public class HttpClientWithErrorHandling : IHttpClientWrapper
    {
        public HttpClientWithErrorHandling(IHttpClientWrapper inner)
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public IHttpClientWrapper Inner { get; }

        public async Task<HttpResponse<TSuccessResponse>> SendAsync<TSuccessResponse>(HttpRequestMessage message)
        {
            try
            {
                return await Inner.SendAsync<TSuccessResponse>(message);
            }
            catch (DeserializationException ex)
            {
                ex.Response.Errors = ex;
                ex.Response.Success = false;
                return (HttpResponse<TSuccessResponse>)ex.Response;
            }
            catch (Exception ex)
            {
                return new HttpResponse<TSuccessResponse>
                {
                    Errors = new HttpAggregateException(ex)
                };
            }
        }

        public async Task<HttpResponse> SendAsync(HttpRequestMessage message)
        {
            try
            {
                return await Inner.SendAsync(message);
            }
            catch (DeserializationException ex)
            {
                ex.Response.Errors = ex;
                ex.Response.Success = false;
                return ex.Response;
            }
            catch (Exception ex)
            {
                return new HttpResponse
                {
                    Errors = new HttpAggregateException(ex)
                };
            }
        }
    }
}
