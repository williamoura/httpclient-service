using HttpClientService.Interfaces;
using HttpClientService.Models;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;
using System.Dynamic;
using System.Text;

namespace HttpClientService
{
    public class HttpClientWithLog : IHttpClientWrapper
    {
        public HttpClientWithLog(
            IHttpClientWrapper httpClient,
            ILogger logger)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IHttpClientWrapper HttpClient { get; }
        public ILogger Logger { get; }

        public async Task<HttpResponse<TSuccessResponse>> SendAsync<TSuccessResponse>(HttpRequestMessage message) =>
            await Execute(message, async requestMessage => await HttpClient.SendAsync<TSuccessResponse>(message));

        public async Task<HttpResponse> SendAsync(HttpRequestMessage message) =>
            await Execute(message, async requestMessage => await HttpClient.SendAsync(message));

        private static object GetHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            if (headers == null)
            {
                return null;
            }

            var obj = new ExpandoObject();
            var dicObject = (IDictionary<string, object>)obj;
            foreach (var header in headers)
            {
                dicObject.Add(header.Key, string.Join(",", header.Value));
            }

            return obj;
        }

        private async Task<T> Execute<T>(HttpRequestMessage message, Func<HttpRequestMessage, Task<T>> fn)
            where T : HttpResponse
        {
            var requestContent = string.Empty;
            if (message.Content != null)
            {
                requestContent = await message.Content.ReadAsStringAsync();
            }

            var requestHeaders = GetHeaders(message.Headers);
            var host = message.RequestUri?.Host ?? "Unknown";

            var requestLog = new StringBuilder();
            requestLog.AppendLine("ExternalService - Request:");
            requestLog.AppendLine($"Host: {host}");
            requestLog.AppendLine($"Path: {message.RequestUri?.ToString()}");
            requestLog.AppendLine($"Method: {message.Method.Method}");
            requestLog.AppendLine($"Headers: {JsonConvert.SerializeObject(requestHeaders)}");
            requestLog.AppendLine($"Content: {requestContent}");

            Logger.Information(requestLog.ToString());

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            T response;
            try
            {
                response = await fn(message);
            }
            finally
            {
                stopwatch.Stop();
            }

            if (response.Errors != null)
            {
                var errorLog = new StringBuilder();
                errorLog.AppendLine("ExternalService - Error:");
                errorLog.AppendLine($"Host: {host}");
                errorLog.AppendLine($"Path: {message.RequestUri?.ToString()}");
                errorLog.AppendLine($"Method: {message.Method.Method}");
                errorLog.AppendLine($"Status Code: {response.StatusCode}");
                errorLog.AppendLine($"Error: {response.Errors.GetBaseException()}");
                errorLog.AppendLine($"Response Content: {JsonConvert.SerializeObject(new { response.Success, response.StatusCode, response.OriginalBody })}");

                Logger.Error(errorLog.ToString());


                return response;
            }

            var responseHeaders = GetHeaders(response.ResponseMessage.Headers);
            var responseContent = response.OriginalBody;

            var responseLog = new StringBuilder();
            responseLog.AppendLine("ExternalService - Response:");
            responseLog.AppendLine($"Host: {host}");
            responseLog.AppendLine($"Elapsed Time: {stopwatch.Elapsed}");
            responseLog.AppendLine($"Status Code: {response.StatusCode}");
            responseLog.AppendLine($"Response Content: {responseContent}");
            responseLog.AppendLine($"Headers: {JsonConvert.SerializeObject(responseHeaders)}");

            Logger.Information(responseLog.ToString());

            return response;
        }
    }
}
