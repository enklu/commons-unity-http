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
            _builder.BaseUrl = "https://createar.co/endpoint";
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

        [Test]
        public void FromSimple()
        {
            Assert.IsTrue(_builder.FromUrl("http://localhost:1234/v1"));

            Assert.AreEqual("localhost", _builder.BaseUrl);
            Assert.AreEqual("http://", _builder.Protocol);
            Assert.AreEqual(1234, _builder.Port);
            Assert.AreEqual("/v1", _builder.Version);
        }

        [Test]
        public void FromEc2()
        {
            Assert.IsTrue(_builder.FromUrl("http://ec2-34-213-184-152.us-west-2.compute.amazonaws.com:1234/v1"));

            Assert.AreEqual("ec2-34-213-184-152.us-west-2.compute.amazonaws.com", "ec2-34-213-184-152.us-west-2.compute.amazonaws.com");
            Assert.AreEqual("http://", _builder.Protocol);
            Assert.AreEqual(1234, _builder.Port);
            Assert.AreEqual("/v1", _builder.Version);
        }

        [Test]
        public void FromDefaultPort()
        {
            Assert.IsTrue(_builder.FromUrl("http://ec2-34-213-184-152.us-west-2.compute.amazonaws.com/v1"));

            Assert.AreEqual("ec2-34-213-184-152.us-west-2.compute.amazonaws.com", "ec2-34-213-184-152.us-west-2.compute.amazonaws.com");
            Assert.AreEqual("http://", _builder.Protocol);
            Assert.AreEqual(80, _builder.Port);
            Assert.AreEqual("/v1", _builder.Version);
        }

        [Test]
        public void FromDefaultVersion()
        {
            Assert.IsTrue(_builder.FromUrl("http://ec2-34-213-184-152.us-west-2.compute.amazonaws.com"));

            Assert.AreEqual("ec2-34-213-184-152.us-west-2.compute.amazonaws.com", "ec2-34-213-184-152.us-west-2.compute.amazonaws.com");
            Assert.AreEqual("http://", _builder.Protocol);
            Assert.AreEqual(80, _builder.Port);
            Assert.AreEqual("", _builder.Version);
        }

        [Test]
        public void FromDefaultProtocol()
        {
            Assert.IsTrue(_builder.FromUrl("ec2-34-213-184-152.us-west-2.compute.amazonaws.com"));

            Assert.AreEqual("ec2-34-213-184-152.us-west-2.compute.amazonaws.com", "ec2-34-213-184-152.us-west-2.compute.amazonaws.com");
            Assert.AreEqual("http", _builder.Protocol);
            Assert.AreEqual(80, _builder.Port);
            Assert.AreEqual("", _builder.Version);
        }

        [Test]
        public void FromFailed()
        {
            Assert.IsFalse(_builder.FromUrl("http//localhost"));
            Assert.IsFalse(_builder.FromUrl("http://?localhost"));
            Assert.IsFalse(_builder.FromUrl("http://localhost:d"));
            Assert.IsFalse(_builder.FromUrl("http://?localhost:/"));
            Assert.IsFalse(_builder.FromUrl("http://?localhost:"));
            Assert.IsFalse(_builder.FromUrl("http://?localhost/:"));
            Assert.IsFalse(_builder.FromUrl("http://?localhost/v:"));
        }
    }
}