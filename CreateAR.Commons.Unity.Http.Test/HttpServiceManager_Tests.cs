using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.Networking;

namespace CreateAR.Commons.Unity.Http
{
    [TestFixture]
    public class HttpServiceManager_Tests
    {
        private HttpServiceManager _serviceManager;

        private static readonly string XContentType = "X-Content-Type";
        private static readonly string SuperXContentType = "super/awesome-stuff";

        private static readonly string TrellisUrl = "https://cloud.enklu.com:10001/v1/";
        private static readonly string StargazerUrl = "https://cloud.enklu.com:9973/v1/";

        [SetUp]
        public void Setup()
        {
            _serviceManager = new HttpServiceManager(new UrlFormatterCollection());
            _serviceManager.Register("trellis", FormatterFor(TrellisUrl));
            _serviceManager.Register("stargazer", FormatterFor(StargazerUrl));

            _serviceManager.AddHeader("stargazer", "Authorization", "Bearer sg-auth");
            _serviceManager.AddHeader("stargazer", XContentType, SuperXContentType);
            _serviceManager.AddHeader("trellis", "Authorization", "Bearer trellis-auth");
        }

        [Test]
        public void ResolveServiceData()
        {
            string snapsEndPoint = "users/15324/snaps?tag=foo";
            string sgSnapsFull = $"{StargazerUrl}{snapsEndPoint}";
            string sgSnaps = $"stargazer://{snapsEndPoint}";

            // stargazer:// protocol
            var data = _serviceManager.ResolveServiceData(sgSnaps);

            // Service Data
            var serviceName = data.Item1;
            var resolvedUrl = data.Item2;
            var headers = data.Item3;

            // 
            Assert.AreEqual("stargazer", serviceName);
            Assert.AreEqual(sgSnapsFull, resolvedUrl);
            Assert.AreEqual(SuperXContentType, headers[XContentType]);
        }

        private static UrlFormatter FormatterFor(string url)
        {
            var f = new UrlFormatter();
            if (!f.FromUrl(url))
            {
                throw new Exception("Failed to create formatter from url: " + url);
            }
            return f;
        }
    }
}
