using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CreateAR.Commons.Unity.Http
{
    public class UrlFormatterCollection_Tests
    {
        private UrlFormatterCollection _formatters;

        [SetUp]
        public void Setup()
        {
            _formatters = new UrlFormatterCollection();

            var formatter = new UrlFormatter();
            formatter.FromUrl("https://assets.com:9110/v1");

            _formatters.Register("assets", formatter);
        }

        [Test]
        public void Url()
        {
            Assert.AreEqual(
                "https://assets.com:9110/v1/foo",
                _formatters.Url("assets://foo"));
        }

        [Test]
        public void UrlNoProtocol()
        {
            Assert.AreEqual(
                "https://assets.com:9110/v1/foo",
                _formatters.Url("foo"));
        }

        [Test]
        public void UrlVersionOverride()
        {
            Assert.AreEqual(
                "https://assets.com:9110/v2/foo",
                _formatters.Url("foo", "v2"));
        }

        [Test]
        public void UrlPortOverride()
        {
            Assert.AreEqual(
                "https://assets.com:8008/v2/foo",
                _formatters.Url("foo", "v2", 8008));
        }

        [Test]
        public void UrlProtocolOverride()
        {
            Assert.AreEqual(
                "ftp://assets.com:8008/v2/foo",
                _formatters.Url("foo", "v2", 8008, "ftp"));
        }

        [Test]
        public void UrlReplacementsOverride()
        {
            Assert.AreEqual(
                "ftp://assets.com:8008/v2/foo",
                _formatters.Url("{userId}", "v2", 8008, "ftp", new Dictionary<string, string>
                {
                    { "userId", "foo" }
                }));
        }

        [Test]
        public void NormalUrl()
        {
            Assert.AreEqual(
                "http://www.google.com",
                _formatters.Url("http://www.google.com"));
        }

        [TestCase("trellis://user/12345/foo", "trellis")]
        [TestCase("ftp://test/me", "ftp")]
        [TestCase("helloWorld", null)]
        public void ParseProtocolName(string url, string expectedResult)
        {
            var protoName = _formatters.ProtocolName(url);
            Assert.AreEqual(expectedResult, protoName);
        }

        [Test]
        public void Reverse()
        {
            const string TRELLIS_URL = "https://cloud.enklu.com:10001/v1/";
            const string STARGAZER_URL = "https://cloud.enklu.com:9973/v1/";
            const string GOOGLE_URL = "http://www.google.com/v3/";

            var formatters = new UrlFormatterCollection();
            
            var trellisFormatter = FormatterFor(TRELLIS_URL);
            var stargazerFormatter = FormatterFor(STARGAZER_URL);
            var googleFormatter = FormatterFor(GOOGLE_URL);
            
            // Register formatters
            formatters.Register("trellis", trellisFormatter);
            formatters.Register("stargazer", stargazerFormatter);
            formatters.Register("google", googleFormatter);

            // Resolve names from specific URLs
            Assert.AreEqual("stargazer", formatters.FormatterName($"{STARGAZER_URL}users/13423-abcdsa-53214/snaps?type=still"));
            Assert.AreEqual("trellis", formatters.FormatterName($"{TRELLIS_URL}/foo/man/chu"));
        }

        private static UrlFormatter FormatterFor(string url)
        {
            var urlFormatter = new UrlFormatter();
            urlFormatter.FromUrl(url);
            return urlFormatter;
        }
    }
}