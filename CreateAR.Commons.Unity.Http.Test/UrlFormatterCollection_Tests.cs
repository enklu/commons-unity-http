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
    }
}