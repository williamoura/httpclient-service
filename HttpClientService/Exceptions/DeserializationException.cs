using HttpClientService.Models;

namespace HttpClientService.Exceptions
{
    public class DeserializationException : HttpAggregateException
    {
        public DeserializationException(HttpResponse response, Exception exception) : base(exception)
        {
            Response = response;
        }

        public HttpResponse Response { get; }
    }
}
