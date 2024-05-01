using AutoFixture.Idioms;
using FluentAssertions;
using HttpClientService.Exceptions;
using HttpClientService.Models;
using HttpClientService.UnitTests.AutoData;
using HttpClientService.UnitTests.Common;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HttpClientService.UnitTests
{
    public class HttpClientWithErrorHandlingTests
    {
        [Theory, AutoNSubstituteData]
        public void Constructor_WhenMissingDependencies_ShouldThrowException(GuardClauseAssertion guardClauseAssertion)
        {
            guardClauseAssertion.Verify(typeof(HttpClientWithErrorHandling).GetConstructors());
        }

        [Theory, AutoNSubstituteData]
        public async void SendAsync_WhenSendAsyncIsSuccess_ErrorsShouldBeNull(HttpClientWithErrorHandling sut, HttpRequestMessage message,
            Task<HttpResponse<Foo>> responseWrapper)
        {
            (await responseWrapper).Errors = null;

            sut.Inner.SendAsync<Foo>(message).Returns(responseWrapper);

            var result = await sut.SendAsync<Foo>(message);

            result.Errors.Should().BeNull();
        }

        [Theory, AutoNSubstituteData]
        public async void SendAsync_WhenSendAsyncThrowsException_ResultShouldBeFailed(HttpClientWithErrorHandling sut, HttpRequestMessage message)
        {
            sut.Inner.SendAsync<Foo>(message).Throws((new TimeoutException()));

            var result = await sut.SendAsync<Foo>(message);

            result.Errors.Should().BeOfType<HttpAggregateException>();
            result.Errors.InnerException.Should().BeOfType<TimeoutException>();
        }

        [Theory, AutoNSubstituteData]
        public async void SendAsync_WhenSendAsyncThrowsDeserializationException_ResultShouldBeFailed(HttpClientWithErrorHandling sut, HttpRequestMessage message, HttpResponse<Foo> response)
        {
            sut.Inner.SendAsync<Foo>(message).Throws(new DeserializationException(response, new TimeoutException()));

            var result = await sut.SendAsync<Foo>(message);

            result.Errors.Should().BeOfType<DeserializationException>();
            result.Errors.InnerException.Should().BeOfType<TimeoutException>();
        }
    }
}
