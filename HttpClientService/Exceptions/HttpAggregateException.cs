namespace HttpClientService.Exceptions
{
    public class HttpAggregateException : AggregateException
    {
        public HttpAggregateException(params Exception[] innerExceptions) : base(innerExceptions)
        {
        }
    }
}
