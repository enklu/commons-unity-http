using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.DataStructures;
using JetBrains.Annotations;

namespace CreateAR.Commons.Unity.Http
{
    public class UrlBuilder
    {
        public string BaseUrl = "localhost";
        public string Protocol = "http";
        public int Port = 80;
        public string Version = "";

        public readonly List<Tuple<string, string>> Replacements = new List<Tuple<string, string>>();

        public string Url(string endpoint)
        {
            if (string.IsNullOrEmpty(Version))
            {
                return string.Format("{0}{1}:{2}/{3}",
                    FormatProtocol(),
                    FormatBaseUrl(),
                    FormatPort(),
                    FormatEndpoint(endpoint));
            }

            return string.Format("{0}{1}:{2}/{3}/{4}",
                FormatProtocol(),
                FormatBaseUrl(),
                FormatPort(),
                FormatVersion(),
                FormatEndpoint(endpoint));
        }

        private string FormatProtocol()
        {
            var protocol = string.IsNullOrEmpty(Protocol)
                ? "http://"
                : Protocol;

            if (!protocol.EndsWith("://"))
            {
                protocol += "://";
            }

            return protocol;
        }

        private string FormatBaseUrl()
        {
            var baseUrl = string.IsNullOrEmpty(BaseUrl)
                ? "localhost"
                : BaseUrl;

            baseUrl = baseUrl.Trim('/');

            return baseUrl;
        }

        private int FormatPort()
        {
            return Port > 0
                ? Port
                : 80;
        }

        private string FormatVersion()
        {
            return Version.Trim('/');
        }

        private string FormatEndpoint(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint))
            {
                return "";
            }

            endpoint = endpoint.Trim('/');

            // replacements
            for (int i = 0, len = Replacements.Count; i < len; i++)
            {
                var replacement = Replacements[i];

                endpoint = endpoint.Replace(
                    "{" + replacement.Item1 + "}",
                    replacement.Item2);
            }

            return endpoint;
        }
    }

    /// <summary>
    /// Verbs.
    /// </summary>
    public enum HttpVerb
    {
        Get,
        Post,
        Put,
        Patch,
        Delete
    }

    /// <summary>
    /// Defines an HTTP service.
    /// </summary>
    public interface IHttpService
    {
        /// <summary>
        /// Sends a GET request to an endpoint.
        /// </summary>
        /// <typeparam name="T">The type to expect from the endpoint.</typeparam>
        /// <param name="url">The URL to hit, including GET parameters.</param>
        /// <param name="headers">Header information.</param>
        /// <returns>An IAsyncScope to listen to.</returns>
        /// <exception cref="NullReferenceException"></exception>
        IAsyncToken<HttpResponse<T>> Get<T>(
            string url,
            List<Tuple<string, string>> headers);

        /// <summary>
        /// Sends a POST request to an endpoint.
        /// </summary>
        /// <typeparam name="T">The type to expect from the endpoint.</typeparam>
        /// <param name="url">The URL to hit.</param>
        /// <param name="payload">The resource to send to this endpoint.</param>
        /// <param name="headers">Header information.</param>
        /// <returns>An IAsyncScope to listen to.</returns>
        /// <exception cref="NullReferenceException"></exception>
        IAsyncToken<HttpResponse<T>> Post<T>(
            string url,
            object payload,
            List<Tuple<string, string>> headers);
    }
}