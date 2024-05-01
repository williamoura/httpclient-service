using System.Net;
using System.Net.Mime;
using System.Text;

namespace HttpClientService.UnitTests.Common.MessageHandlers
{
    public class SuccessEmptyMessageHandler : HttpMessageHandler
    {
        private readonly string _contentType;

        public SuccessEmptyMessageHandler(string contentType = MediaTypeNames.Application.Json)
        {
            _contentType = contentType;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            response.RequestMessage = request;
            response.Content = new StringContent("", Encoding.UTF8, _contentType);

            return Task.FromResult(response);
        }
    }
}
