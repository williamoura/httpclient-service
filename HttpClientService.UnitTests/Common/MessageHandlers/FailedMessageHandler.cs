using System.Net;
using System.Net.Mime;
using System.Text;

namespace HttpClientService.UnitTests.Common.MessageHandlers
{
    public class FailedMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

            response.RequestMessage = request;
            response.Content = new StringContent("{\"field1\":\"value1\"}", Encoding.UTF8, MediaTypeNames.Application.Json);

            return Task.FromResult(response);
        }
    }
}
