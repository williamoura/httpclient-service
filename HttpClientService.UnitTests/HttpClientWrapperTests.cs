using FluentAssertions;
using HttpClientService.Exceptions;
using HttpClientService.Interfaces;
using HttpClientService.Models;
using HttpClientService.UnitTests.AutoData;
using HttpClientService.UnitTests.Common;
using HttpClientService.UnitTests.Common.MessageHandlers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Net;
using System.Net.Mime;

namespace HttpClientService.UnitTests
{
    public class HttpClientWrapperTests
    {
        [Fact]
        public void Constructor_WhenMissingHttpClientDependency_ShouldThrowException()
        {
            var result =
                Assert.Throws<ArgumentNullException>(() =>
                new HttpClientWrapper(null, Substitute.For<ISerialization>()));

            result.Message.Should().Contain("httpClient");
        }

        [Fact]
        public void Constructor_WhenMissingSerialization_ShouldThrowException()
        {
            var result =
                Assert.Throws<ArgumentNullException>(() =>
                new HttpClientWrapper(Substitute.For<HttpClient>(), null));

            result.Message.Should().Contain("serializer");
        }

        [Fact]
        public void Constructor_WhenAllDependenciesAreProvided_ShouldConstructInstanceAsExpected()
        {
            var result = new HttpClientWrapper(Substitute.For<HttpClient>(), Substitute.For<ISerialization>());

            result.Should().NotBeNull();
        }

        [Theory, AutoNSubstituteData]
        public async void SendAsync_WhenResponseHasData_ShouldReturnIt(HttpRequestMessage message)
        {
            var sut = new HttpClientWrapper(new HttpClient(new SuccessMessageHandler()), Substitute.For<ISerialization>());

            sut.Serializer.Deserialize(Arg.Any<string>(), typeof(Foo)).Returns(new Foo
            {
                Field1 = "value1"
            });

            var result = await sut.SendAsync<Foo>(message);

            result.Success.Should().BeTrue();
            result.Response.Field1.Should().Be("value1");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.OriginalBody.Should().Be("{\"field1\":\"value1\"}");
        }

        [Theory, AutoNSubstituteData]
        public async void SendAsyncWithoutGenericType_WhenResponseHasData_ShouldReturnIt(HttpRequestMessage message)
        {
            var sut = new HttpClientWrapper(new HttpClient(new SuccessMessageHandler()), Substitute.For<ISerialization>());

            sut.Serializer.Deserialize(Arg.Any<string>(), typeof(Foo)).Returns(new Foo
            {
                Field1 = "value1"
            });

            var result = await sut.SendAsync(message);

            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.OriginalBody.Should().Be("{\"field1\":\"value1\"}");


            var response = result.GetResponse<Foo>(sut.Serializer);
            response.Field1.Should().Be("value1");

        }

        [Theory, AutoNSubstituteData]
        public async void SendAsync_WhenHttpMethodIsGET_ResultShouldBeAsExpected(HttpRequestMessage message)
        {
            var sut = new HttpClientWrapper(new HttpClient(new SuccessMessageHandler()), Substitute.For<ISerialization>());

            sut.Serializer.Deserialize(Arg.Any<string>(), typeof(Foo)).Returns(new Foo
            {
                Field1 = "value1"
            });

            message.Content = null;
            message.Method = HttpMethod.Get;

            var result = await sut.SendAsync<Foo>(message);

            result.Success.Should().BeTrue();
            result.Response.Field1.Should().Be("value1");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.OriginalBody.Should().Be("{\"field1\":\"value1\"}");
        }

        [Theory, AutoNSubstituteData]
        public async void SendAsync_WhenDeserializeThrows_ResultShouldBeAsExpected(HttpRequestMessage message)
        {
            var sut = new HttpClientWrapper(new HttpClient(new SuccessMessageHandler()), Substitute.For<ISerialization>());

            var exception = new Exception();
            sut.Serializer.Deserialize(Arg.Any<string>(), typeof(Foo)).Throws(exception);

            message.Content = null;
            message.Method = HttpMethod.Get;

            Func<Task<HttpResponse<Foo>>> action = async () => await sut.SendAsync<Foo>(message);

            await action.Should().ThrowAsync<DeserializationException>();
        }

        [Theory, AutoNSubstituteData]
        public async void SendAsync_WhenResponseContentIsEmpty_ResultShouldBeAsExpected(HttpRequestMessage message)
        {
            var sut = new HttpClientWrapper(new HttpClient(new SuccessEmptyMessageHandler()), Substitute.For<ISerialization>());

            var result = await sut.SendAsync<Foo>(message);

            result.Success.Should().BeTrue();
            result.Response.Should().BeNull();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.OriginalBody.Should().BeNullOrEmpty();
            sut.Serializer.DidNotReceive().Deserialize(Arg.Any<string>(), typeof(Foo));
        }

        [Theory, AutoNSubstituteData]
        public async void SendAsync_WhenResponseContentTypeIsNotJson_ResultShouldBeAsExpected(HttpRequestMessage message)
        {
            var sut = new HttpClientWrapper(new HttpClient(new SuccessEmptyMessageHandler(MediaTypeNames.Text.Html)), Substitute.For<ISerialization>());

            var result = await sut.SendAsync<Foo>(message);

            result.Success.Should().BeTrue();
            result.Response.Should().BeNull();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.OriginalBody.Should().BeNullOrWhiteSpace();
            sut.Serializer.DidNotReceive().Deserialize(Arg.Any<string>(), typeof(Foo));
        }

        [Theory, AutoNSubstituteData]
        public async void SendAsync_WhenUnsuccessfulResponse_ResultShouldBeAsExpected(HttpRequestMessage message)
        {
            var sut = new HttpClientWrapper(new HttpClient(new FailedMessageHandler()), Substitute.For<ISerialization>());

            sut.Serializer.Deserialize(Arg.Any<string>(), typeof(Foo)).Returns(new Foo
            {
                Field1 = "value1"
            });

            var result = await sut.SendAsync<Foo>(message);

            result.Success.Should().BeFalse();
            result.Response.Should().NotBeNull();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.OriginalBody.Should().Be("{\"field1\":\"value1\"}");
        }

        [Theory, AutoNSubstituteData]
        public async void SendAsync_WhenUnsuccessfulResponseIsNotFound_ResultShouldBeAsExpected(HttpRequestMessage message)
        {
            var sut = new HttpClientWrapper(new HttpClient(new FailedMessageHandler()), Substitute.For<ISerialization>());

            var result = await sut.SendAsync<Foo>(message);

            result.Success.Should().BeFalse();
            result.Response.Should().BeNull();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.OriginalBody.Should().Be("{\"field1\":\"value1\"}");
            sut.Serializer.Received(1).Deserialize(Arg.Any<string>(), typeof(Foo));
        }
    }
}
