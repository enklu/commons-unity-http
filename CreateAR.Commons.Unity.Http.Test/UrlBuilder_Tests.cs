using NUnit.Framework;

namespace CreateAR.Commons.Unity.Http
{
    [TestFixture]
    public class UrlBuilder_Tests
    {
        private const string ENDPOINT = "user/messages";

        private UrlFormatter _formatter;

        [SetUp]
        public void Setup()
        {
            _formatter = new UrlFormatter();
        }

        [Test]
        public void Run()
        {
            // basic stuff
            Assert.AreEqual("http://localhost:80/user/messages", _formatter.Url(ENDPOINT));
            Assert.AreEqual("http://localhost:80/user/messages", _formatter.Url("/" + ENDPOINT + "/"));
        }

        [Test]
        public void Ports()
        {
            // ports
            _formatter.Port = 1024;
            Assert.AreEqual("http://localhost:1024/user/messages", _formatter.Url(ENDPOINT));
            _formatter.Port = -10;
            Assert.AreEqual("http://localhost:80/user/messages", _formatter.Url(ENDPOINT));
        }

        [Test]
        public void BaseUrl()
        {
            // base url
            _formatter.BaseUrl = "createar.co";
            Assert.AreEqual("http://createar.co:80/user/messages", _formatter.Url(ENDPOINT));
            _formatter.BaseUrl = "createar.co/";
            Assert.AreEqual("http://createar.co:80/user/messages", _formatter.Url(ENDPOINT));
            _formatter.BaseUrl = "https://createar.co/";
            Assert.AreEqual("http://createar.co:80/user/messages", _formatter.Url(ENDPOINT));
            _formatter.BaseUrl = "https://createar.co/endpoint";
            Assert.AreEqual("http://createar.co:80/user/messages", _formatter.Url(ENDPOINT));
        }

        [Test]
        public void Protocol()
        {
            // protocol
            _formatter.Protocol = "https";
            Assert.AreEqual("https://localhost:80/user/messages", _formatter.Url(ENDPOINT));
            _formatter.Protocol = "https://";
            Assert.AreEqual("https://localhost:80/user/messages", _formatter.Url(ENDPOINT));
        }

        [Test]
        public void Replacements()
        {
            // replacements
            _formatter.Replacements["REPLACE"] = "user";
            Assert.AreEqual(
                "http://localhost:80/user/messages",
                _formatter.Url("{REPLACE}/messages"));
        }

        [Test]
        public void Version()
        {
            // version
            _formatter.Version = "v2";
            Assert.AreEqual(
                "http://localhost:80/v2/user/messages",
                _formatter.Url(ENDPOINT));
        }

        [Test]
        public void FromSimple()
        {
            Assert.IsTrue(_formatter.FromUrl("http://localhost:1234/v1"));

            Assert.AreEqual("localhost", _formatter.BaseUrl);
            Assert.AreEqual("http://", _formatter.Protocol);
            Assert.AreEqual(1234, _formatter.Port);
            Assert.AreEqual("/v1", _formatter.Version);
        }

        [Test]
        public void FromEc2()
        {
            Assert.IsTrue(_formatter.FromUrl("http://ec2-34-213-184-152.us-west-2.compute.amazonaws.com:1234/v1"));

            Assert.AreEqual("ec2-34-213-184-152.us-west-2.compute.amazonaws.com", "ec2-34-213-184-152.us-west-2.compute.amazonaws.com");
            Assert.AreEqual("http://", _formatter.Protocol);
            Assert.AreEqual(1234, _formatter.Port);
            Assert.AreEqual("/v1", _formatter.Version);
        }

        [Test]
        public void FromDefaultPort()
        {
            Assert.IsTrue(_formatter.FromUrl("http://ec2-34-213-184-152.us-west-2.compute.amazonaws.com/v1"));

            Assert.AreEqual("ec2-34-213-184-152.us-west-2.compute.amazonaws.com", "ec2-34-213-184-152.us-west-2.compute.amazonaws.com");
            Assert.AreEqual("http://", _formatter.Protocol);
            Assert.AreEqual(80, _formatter.Port);
            Assert.AreEqual("/v1", _formatter.Version);
        }

        [Test]
        public void FromDefaultVersion()
        {
            Assert.IsTrue(_formatter.FromUrl("http://ec2-34-213-184-152.us-west-2.compute.amazonaws.com"));

            Assert.AreEqual("ec2-34-213-184-152.us-west-2.compute.amazonaws.com", "ec2-34-213-184-152.us-west-2.compute.amazonaws.com");
            Assert.AreEqual("http://", _formatter.Protocol);
            Assert.AreEqual(80, _formatter.Port);
            Assert.AreEqual("", _formatter.Version);
        }

        [Test]
        public void FromDefaultProtocol()
        {
            Assert.IsTrue(_formatter.FromUrl("ec2-34-213-184-152.us-west-2.compute.amazonaws.com"));

            Assert.AreEqual("ec2-34-213-184-152.us-west-2.compute.amazonaws.com", "ec2-34-213-184-152.us-west-2.compute.amazonaws.com");
            Assert.AreEqual("http", _formatter.Protocol);
            Assert.AreEqual(80, _formatter.Port);
            Assert.AreEqual("", _formatter.Version);
        }

        [Test]
        public void FromFailed()
        {
            Assert.IsFalse(_formatter.FromUrl("http//localhost"));
            Assert.IsFalse(_formatter.FromUrl("http://?localhost"));
            Assert.IsFalse(_formatter.FromUrl("http://localhost:d"));
            Assert.IsFalse(_formatter.FromUrl("http://?localhost:/"));
            Assert.IsFalse(_formatter.FromUrl("http://?localhost:"));
            Assert.IsFalse(_formatter.FromUrl("http://?localhost/:"));
            Assert.IsFalse(_formatter.FromUrl("http://?localhost/v:"));
        }

        [Test]
        public void Overrides()
        {
            Assert.AreEqual("http://localhost:80/v2/foo", _formatter.Url("foo", "v2"));
            Assert.AreEqual("http://localhost:8080/v2/foo", _formatter.Url("foo", "v2", 8080));
            Assert.AreEqual("https://localhost:8080/v2/foo", _formatter.Url("foo", "v2", 8080, "https"));
        }
    }
}