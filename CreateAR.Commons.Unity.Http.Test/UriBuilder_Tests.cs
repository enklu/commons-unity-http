using CreateAR.Commons.Unity.DataStructures;
using NUnit.Framework;

namespace CreateAR.Commons.Unity.Http
{
    [TestFixture]
    public class UriBuilder_Tests
    {
        private const string ENDPOINT = "user/messages";

        private UrlBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new UrlBuilder();
        }

        [Test]
        public void Run()
        {
            // basic stuff
            Assert.AreEqual("http://localhost:80/user/messages", _builder.Url(ENDPOINT));
            Assert.AreEqual("http://localhost:80/user/messages", _builder.Url("/" + ENDPOINT + "/"));

        }

        [Test]
        public void Ports()
        {
            // ports
            _builder.Port = 1024;
            Assert.AreEqual("http://localhost:1024/user/messages", _builder.Url(ENDPOINT));
            _builder.Port = -10;
            Assert.AreEqual("http://localhost:80/user/messages", _builder.Url(ENDPOINT));
        }

        [Test]
        public void BaseUrl()
        {
            // base url
            _builder.BaseUrl = "createar.co";
            Assert.AreEqual("http://createar.co:80/user/messages", _builder.Url(ENDPOINT));
            _builder.BaseUrl = "createar.co/";
            Assert.AreEqual("http://createar.co:80/user/messages", _builder.Url(ENDPOINT));
            _builder.BaseUrl = "https://createar.co/";
            Assert.AreEqual("http://createar.co:80/user/messages", _builder.Url(ENDPOINT));
        }

        [Test]
        public void Protocol()
        {
            // protocol
            _builder.Protocol = "https";
            Assert.AreEqual("https://localhost:80/user/messages", _builder.Url(ENDPOINT));
            _builder.Protocol = "https://";
            Assert.AreEqual("https://localhost:80/user/messages", _builder.Url(ENDPOINT));
        }

        [Test]
        public void Replacements()
        {
            // replacements
            _builder.Replacements.Add(Tuple.Create("REPLACE", "user"));
            Assert.AreEqual(
                "http://localhost:80/user/messages",
                _builder.Url("{REPLACE}/messages"));
        }

        [Test]
        public void Version()
        {
            // version
            _builder.Version = "v2";
            Assert.AreEqual(
                "http://localhost:80/v2/user/messages",
                _builder.Url(ENDPOINT));
        }
    }
}