using FluentAssertions;
using MyFeed.Services;
using Xunit;

namespace MyFeed.Tests
{
    public sealed class RegexImageServiceTests
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("<sample><img ></sample>", null)]
        [InlineData("<b></b><image /><another></another>", null)]
        [InlineData("London is the capital of Great Britain!", null)]
        [InlineData("<img src='http://f.boo'/><img src='http://faa.bo'/>", "http://f.boo")]
        [InlineData("<img foo='bar' src='http://example.com' />", "http://example.com")]
        [InlineData("<bla a='42'></bla><img src='http://foo.bar'>", "http://foo.bar")]
        [InlineData("Foo \n<img src='http://example.com' />", "http://example.com")]
        [InlineData("<img src='http://a.b'/><img src='nothing'>", "http://a.b")]
        [InlineData("<img src='nothing'/><img src='http://a.b'>", "http://a.b")]
        public void ShouldExtractImageUriFromRawXmlOrReturnNull(string input, string output)
        {
            var regexImageService = new RegexImageService();
            var uri = regexImageService.ExtractImageUri(input);
            uri.Should().Be(output);
        }
    }
}