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
        public void Add()
        {
            Assert.AreEqual(
                "https://assets.com:9110/v1/foo",
                _formatters.Url("assets://foo"));
        }
    }
}