using HttpClientService.Exceptions;
using HttpClientService.Interfaces;
using System.Net;

namespace HttpClientService.Models
{
    public class HttpResponse
    {
        public HttpAggregateException Errors { get; set; }
        public string OriginalBody { get; set; }
        public HttpResponseMessage ResponseMessage { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public bool Success { get; set; }

        public virtual TResponse GetResponse<TResponse>(ISerialization serializer)
        {
            if (serializer is null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            return (TResponse)serializer.Deserialize(OriginalBody, typeof(TResponse));
        }
    }

    public class HttpResponse<T> : HttpResponse
    {
        public T Response { get; set; }
    }
}
