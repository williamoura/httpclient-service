using HttpClientService.Exceptions;
using HttpClientService.Interfaces;
using HttpClientService.Models;

namespace HttpClientService
{
    public class HttpClientWrapper : IHttpClientWrapper
    {
        public HttpClientWrapper(HttpClient httpClient, ISerialization serializer)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public HttpClient HttpClient { get; }

        public ISerialization Serializer { get; }

        public async Task<HttpResponse<TResponse>> SendAsync<TResponse>(HttpRequestMessage message)
        {
            var result = new HttpResponse<TResponse>
            {
                StatusCode = 0,
                Success = false,
                ResponseMessage = await HttpClient.SendAsync(message)
            };

            if (result.ResponseMessage == null)
            {
                return result;
            }

            result.OriginalBody = await result.ResponseMessage.Content.ReadAsStringAsync();
            result.Success = result.ResponseMessage.IsSuccessStatusCode;
            result.StatusCode = result.ResponseMessage.StatusCode;

            if (string.IsNullOrWhiteSpace(result.OriginalBody))
            {
                return result;
            }

            try
            {
                result.Response = result.GetResponse<TResponse>(Serializer);
            }
            catch (Exception ex)
            {
                throw new DeserializationException(result, ex);
            }

            return result;
        }

        public async Task<HttpResponse> SendAsync(HttpRequestMessage message)
        {
            var result = new HttpResponse
            {
                StatusCode = 0,
                Success = false,
                ResponseMessage = await HttpClient.SendAsync(message)
            };

            if (result.ResponseMessage == null)
            {
                return result;
            }

            result.OriginalBody = await result.ResponseMessage.Content.ReadAsStringAsync();
            result.Success = result.ResponseMessage.IsSuccessStatusCode;
            result.StatusCode = result.ResponseMessage.StatusCode;

            return result;
        }
    }
}
