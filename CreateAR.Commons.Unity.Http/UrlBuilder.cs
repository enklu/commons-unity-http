using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CreateAR.Commons.Unity.DataStructures;

namespace CreateAR.Commons.Unity.Http
{
    /// <summary>
    /// A utility object for building URLs.
    /// </summary>
    public class UrlBuilder
    {
        /// <summary>
        /// Gets and sets the Base URL.
        /// </summary>
        public string BaseUrl = "localhost";

        /// <summary>
        /// The protocol.
        /// </summary>
        public string Protocol = "http";

        /// <summary>
        /// Port number.
        /// </summary>
        public int Port = 80;

        /// <summary>
        /// Api version. Eg - 'v1'.
        /// </summary>
        public string Version = "";

        /// <summary>
        /// A list of replacements. For each (A, B), we replace "{A}" with "B".
        /// 
        /// Eg. -
        /// 
        /// baseUrl.Url("/users/{userId}");
        /// </summary>
        public readonly List<Tuple<string, string>> Replacements = new List<Tuple<string, string>>();

        /// <summary>
        /// Creates a url from the endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to make into a URL.</param>
        /// <returns>The formatted URL.</returns>
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

        /// <summary>
        /// Parses url and fills out information.
        /// </summary>
        /// <param name="url">The URL to parse.</param>
        public bool FromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            
            var regex = new Regex(@"^(\w+://)?([a-zA-Z0-9_\-\.]+)(:\d+)?(/[a-zA-Z0-9_\-]+)?/?$");
            var match = regex.Match(url);
            if (match.Success)
            {
                var groups = match.Groups;
                Protocol = groups[1].Success ? groups[1].Value : "http";
                BaseUrl = groups[2].Value;
                int.TryParse(
                    groups[3].Success
                        ? groups[3].Value.Substring(1)
                        : "80",
                    out Port);
                Version = groups[4].Success ? groups[4].Value : "";

                return true;
            }

            return false;
        }

        /// <summary>
        /// Correctly formats a protocol.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Correctly formats the base url.
        /// </summary>
        /// <returns></returns>
        private string FormatBaseUrl()
        {
            var baseUrl = string.IsNullOrEmpty(BaseUrl)
                ? "localhost"
                : BaseUrl;

            // trim trailing slashes
            baseUrl = baseUrl.Trim('/');

            // trim protocol
            var index = baseUrl.IndexOf("://");
            if (-1 != index)
            {
                baseUrl = baseUrl.Substring(index + 3);
            }

            // trim endpoints off
            index = baseUrl.IndexOf("/");
            while (-1 != index)
            {
                baseUrl = baseUrl.Substring(0, index);

                index = baseUrl.IndexOf("/");
            }

            return baseUrl;
        }

        /// <summary>
        /// Correctly formats the port.
        /// </summary>
        /// <returns></returns>
        private int FormatPort()
        {
            return Port > 0
                ? Port
                : 80;
        }

        /// <summary>
        /// Correctly formats the version.
        /// </summary>
        /// <returns></returns>
        private string FormatVersion()
        {
            return Version.Trim('/');
        }

        /// <summary>
        /// Correctly formats the endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <returns></returns>
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
}