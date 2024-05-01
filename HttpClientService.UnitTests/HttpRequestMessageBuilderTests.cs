using AutoFixture.Idioms;
using FluentAssertions;
using HttpClientService.Interfaces;
using HttpClientService.UnitTests.AutoData;
using HttpClientService.UnitTests.Common;
using NSubstitute;
using System.Text;

namespace HttpClientService.UnitTests
{
    public class HttpRequestMessageBuilderTests
    {
        HttpRequestMessageBuilder _sut;

        public HttpRequestMessageBuilderTests()
        {
            var serializer = Substitute.For<ISerialization>();
            serializer.Serialize(Arg.Any<Foo>()).Returns("{\"field1\":\"value1\"");

            _sut = new HttpRequestMessageBuilder(serializer);
        }

        [Theory, AutoNSubstituteData]
        public void Constructor_GuardTests(GuardClauseAssertion guard)
        {
            guard.Verify(typeof(HttpRequestMessageBuilder).GetConstructors());
        }

        [Fact]
        public void CreateRequestMessage_WhenUrlIsProvided_ShouldCreateMessageAsExpected()
        {
            var result = _sut.CreateRequestMessage("http://testurl.com/relative/test", HttpMethod.Get).Build();

            result.RequestUri.Should().Be("http://testurl.com/relative/test");
            result.Method.Should().Be(HttpMethod.Get);
        }

        [Fact]
        public void CreateRequestMessage_WhenBaseUrlAndRelativeUrlAreProvided_ShouldCreateMessageAsExpected()
        {
            var sut = new HttpRequestMessageBuilder(Substitute.For<ISerialization>());

            var result = sut.CreateRequestMessage("http://testurl.com/", "relative/test", HttpMethod.Get).Build();

            result.RequestUri.Should().Be("http://testurl.com/relative/test");
            result.Method.Should().Be(HttpMethod.Get);
        }

        [Fact]
        public async void CreateRequestMessage_WhenContentIsProvided_ShouldCreateMessageAsExpected()
        {
            var foo = new Foo() { Field1 = "value1" };

            var result = _sut.CreateRequestMessage("http://testurl.com/relative/test", HttpMethod.Post)
                .WithContent(foo, "application/json", Encoding.UTF8)
                .Build();

            var content = await result.Content.ReadAsStringAsync();

            content.Should().Be("{\"field1\":\"value1\"");
            result.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            result.Content.Headers.ContentType.CharSet.Should().Be("utf-8");
        }

        [Fact]
        public async void CreateRequestMessage_WhenFormDataIsProvided_ShouldCreateMessageAsExpected()
        {
            var foo = new Dictionary<string, string>
            {
                {"field1", "value1" },
                {"field2", "value2" }
            };

            var result = _sut.CreateRequestMessage("http://testurl.com/relative/test", HttpMethod.Post)
                .WithFormData(foo)
                .Build();

            var content = await result.Content.ReadAsStringAsync();

            content.Should().Be("field1=value1&field2=value2");
        }

        [Fact]
        public void CreateRequestMessage_WhenHeadersAreProvided_ShouldCreateMessageAsExpected()
        {
            var result = _sut.CreateRequestMessage("http://testurl.com/relative/test", HttpMethod.Get)
                .WithHeaders(new Dictionary<string, string>
                {
                    { "x-api-key", "ABC123" },
                    { "someID", "123" }
                }).Build();

            result.Headers.GetValues("x-api-key").FirstOrDefault().Should().Be("ABC123");
            result.Headers.GetValues("someID").FirstOrDefault().Should().Be("123");
        }

        [Fact]
        public void Build_WhenBuildIsCalledWithoutCreateMessage_ShouldThrowAnInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => _sut.Build());
        }

        [Fact]
        public void Build_WhenWithContentIsCalledWithoutCreateMessage_ShouldThrowAnInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => _sut.WithContent("dsad", "application/xml", Encoding.ASCII));
        }

        [Fact]
        public void Build_WhenWithHeadersIsCalledWithoutCreateMessage_ShouldThrowAnInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => _sut.WithHeaders(new Dictionary<string, string> { { "key", "value" } }));
        }
    }
}
