using HttpClientService.Interfaces;
using System.Text;

namespace HttpClientService
{
    public class HttpRequestMessageBuilder
    {
        private HttpRequestMessage _requestMessage;

        public ISerialization Serialization { get; }

        public HttpRequestMessageBuilder(ISerialization serialization)
        {
            Serialization = serialization ?? throw new ArgumentNullException(nameof(serialization));
        }

        public HttpRequestMessageBuilder CreateRequestMessage(string baseUrl, string relativeUrl, HttpMethod httpMethod)
        {
            _requestMessage = new HttpRequestMessage
            {
                Method = httpMethod,
                RequestUri = new Uri(new Uri(baseUrl), relativeUrl)
            };

            return this;
        }

        public HttpRequestMessageBuilder CreateRequestMessage(string url, HttpMethod httpMethod)
        {
            _requestMessage = new HttpRequestMessage
            {
                Method = httpMethod,
                RequestUri = new Uri(url)
            };

            return this;
        }

        public HttpRequestMessageBuilder WithContent<TRequest>(TRequest data, string contentType, Encoding encoding)
        {
            ValidateCall();

            _requestMessage.Content = new StringContent(Serialization.Serialize(data), encoding, contentType);
            return this;
        }

        public HttpRequestMessageBuilder WithFormData(Dictionary<string, string> formData)
        {
            ValidateCall();

            _requestMessage.Content = new FormUrlEncodedContent(formData);
            return this;
        }

        public HttpRequestMessageBuilder WithHeaders(Dictionary<string, string> headers)
        {
            ValidateCall();

            foreach (var header in headers)
            {
                _requestMessage.Headers.Add(header.Key, header.Value);
            }

            return this;
        }

        public HttpRequestMessage Build()
        {
            ValidateCall();
            return _requestMessage;
        }

        private void ValidateCall()
        {
            if (_requestMessage == null)
                throw new InvalidOperationException("CreateRequestMessage must be called first");
        }
    }
}
