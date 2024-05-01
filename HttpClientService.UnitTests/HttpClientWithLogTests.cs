using FluentAssertions;
using HttpClientService.Models;
using HttpClientService.UnitTests.AutoData;
using HttpClientService.UnitTests.Common;
using NSubstitute;

namespace HttpClientService.UnitTests
{
    public class HttpClientWithLogTests
    {
        [Theory, AutoNSubstituteData]
        public async Task SendAsync_WhenThereIsNoExceptions_ShouldLogAsExpected(HttpClientWithLog sut,
            HttpRequestMessage message,
            HttpResponse<Foo> responseWrapper)
        {
            responseWrapper.Errors = null;

            sut.HttpClient.SendAsync<Foo>(message).Returns(responseWrapper);

            var result = await sut.SendAsync<Foo>(message);

            sut.Logger.Received(2).Information(Arg.Any<string>());

            result.Should().Be(responseWrapper);
        }

        [Theory, AutoNSubstituteData]
        public async Task SendAsync_WhenThereIsExceptions_ShouldLogAsExpected(
            HttpClientWithLog sut,
            HttpRequestMessage message,
            HttpResponse<Foo> responseWrapper)
        {
            sut.HttpClient.SendAsync<Foo>(message).Returns(responseWrapper);

            var result = await sut.SendAsync<Foo>(message);

            sut.Logger.Received(1).Information(Arg.Any<string>());
            sut.Logger.Received(1).Error(Arg.Any<string>());

            result.Should().Be(responseWrapper);
        }
    }
}
