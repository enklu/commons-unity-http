using System;
using System.Collections.Generic;
using System.Linq;

namespace CreateAR.Commons.Unity.Http
{
    /// <summary>
    /// Collection of UrlFormatters, organized by protocol.
    /// </summary>
    public class UrlFormatterCollection
    {
        /// <summary>
        /// Formatter used to parse specific components from urls.
        /// </summary>
        private readonly UrlFormatter _parser = new UrlFormatter();

        /// <summary>
        /// Lookup from protocol to formatter.
        /// </summary>
        private readonly Dictionary<string, UrlFormatter> _formatters = new Dictionary<string, UrlFormatter>();

        /// <summary>
        /// Default protocol.
        /// </summary>
        public string Default { get; set; }

        /// <summary>
        /// Registers a new formatter for a protocol.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <param name="formatter">The formatter.</param>
        public void Register(string protocol, UrlFormatter formatter)
        {
            protocol = FormatProtocol(protocol);

            _formatters[protocol] = formatter;

            if (string.IsNullOrEmpty(Default))
            {
                Default = protocol;
            }
        }

        /// <summary>
        /// Unregisters a formatter.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        public void Unregister(string protocol)
        {
            protocol = FormatProtocol(protocol);

            if (_formatters.Remove(protocol))
            {
                if (Default == protocol)
                {
                    Default = _formatters.Keys.FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// Attempts to extract the protocol name from a url. If there is a formatter registered
        /// for that protocol, then we return the service name. If not, we assume that the
        /// url may already be resolved and must reverse lookup the service with a matching
        /// protocol, base url, port.
        /// </summary>
        public string FormatterName(string url)
        {
            url = RemoveParams(url);
            var formatterName = ProtocolName(url);

            // Failed to parse protocol name from url
            if (string.IsNullOrEmpty(formatterName))
            {
                return null;
            }

            // Check name for a formatter. If there's a registered formatter, return the name
            var formatter = Formatter(formatterName);
            if (null != formatter)
            {
                return formatterName;
            }

            // Parse url into sub-components. Failure results in no formatter name being resolved.
            if (!_parser.FromUrl(url))
            {
                return null;
            }

            // Check parsed url against existing formatters
            var keys = _formatters.Keys.ToList();
            for (var i = 0; i < keys.Count; ++i)
            {
                var key = keys[i];
                var f = _formatters[key];

                // Formatter has the same protocol, base url, and port. Match
                if (IsSameBaseFormat(_parser, f))
                {
                    return key;
                }
            }

            // Failed to resolve the formatter name
            return null;
        }
        
        /// <summary>
        /// Extracts the protocol text from the url. 
        /// </summary>
        public string ProtocolName(string url)
        {
            var index = url.IndexOf("://", StringComparison.Ordinal);
            if (-1 != index)
            {
                return url.Substring(0, index);
            }

            return null;
        }

        /// <summary>
        /// Retrieves a formatter for a protocol, or null if no formatters are
        /// explicitly bound to the protocol.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <returns></returns>
        public UrlFormatter Formatter(string protocol)
        {
            UrlFormatter formatter;
            if (_formatters.TryGetValue(FormatProtocol(protocol), out formatter))
            {
                return formatter;
            }

            return null;
        }

        /// <summary>
        /// Generates a URL.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <returns></returns>
        public string Url(string endpoint)
        {
            UrlFormatter formatter;
            string formattedEndpoint;
            if (Formatter(endpoint, out formatter, out formattedEndpoint))
            {
                return formatter.Url(
                    formattedEndpoint,
                    formatter.Version,
                    formatter.Port, 
                    formatter.Protocol);
            }

            return endpoint;
        }

        /// <summary>
        /// Generates a URL.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="version">Version override.</param>
        /// <returns></returns>
        public string Url(string endpoint, string version)
        {
            UrlFormatter formatter;
            string formattedEndpoint;
            if (Formatter(endpoint, out formatter, out formattedEndpoint))
            {
                return formatter.Url(
                    formattedEndpoint,
                    version,
                    formatter.Port,
                    formatter.Protocol);
            }

            return endpoint;
        }

        /// <summary>
        /// Generates a URL.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="version">Version override.</param>
        /// <param name="port">Port override.</param>
        /// <returns></returns>
        public string Url(string endpoint, string version, int port)
        {
            UrlFormatter formatter;
            string formattedEndpoint;
            if (Formatter(endpoint, out formatter, out formattedEndpoint))
            {
                return formatter.Url(
                    formattedEndpoint,
                    version,
                    port,
                    formatter.Protocol);
            }

            return endpoint;
        }

        /// <summary>
        /// Generates a URL.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="version">Version override.</param>
        /// <param name="port">Port override.</param>
        /// <param name="protocol">Protocol override.</param>
        /// <returns></returns>
        public string Url(string endpoint, string version, int port, string protocol)
        {
            UrlFormatter formatter;
            string formattedEndpoint;
            if (Formatter(endpoint, out formatter, out formattedEndpoint))
            {
                return formatter.Url(formattedEndpoint, version, port, protocol);
            }

            return endpoint;
        }

        /// <summary>
        /// Generates a URL.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="version">Version override.</param>
        /// <param name="port">Port override.</param>
        /// <param name="protocol">Protocol override.</param>
        /// <param name="replacements">Optional replacements in url.</param>
        /// <returns></returns>
        public string Url(
            string endpoint,
            string version,
            int port,
            string protocol,
            Dictionary<string, string> replacements)
        {
            UrlFormatter formatter;
            string formattedEndpoint;
            if (Formatter(endpoint, out formatter, out formattedEndpoint))
            {
                return formatter.Url(formattedEndpoint, version, port, protocol, replacements);
            }

            return endpoint;
        }

        /// <summary>
        /// Cleans up a protocol.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <returns></returns>
        private string FormatProtocol(string protocol)
        {
            // trim protocol
            var index = protocol.IndexOf("://", StringComparison.Ordinal);
            if (-1 != index)
            {
                protocol = protocol.Substring(0, index);
            }

            return protocol;
        }

        /// <summary>
        /// Forwards to formatter.
        /// </summary>
        private string Url(
            UrlFormatter formatter,
            string formattedEndpoint,
            string version,
            int port,
            string protocol,
            Dictionary<string, string> replacements)
        {
            return formatter.Url(formattedEndpoint, version, port, protocol, replacements);
        }

        /// <summary>
        /// Finds a formatter.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="formatter">The formatter.</param>
        /// <param name="formattedEndpoint">The formatted endpoint.</param>
        /// <returns></returns>
        private bool Formatter(
            string endpoint,
            out UrlFormatter formatter,
            out string formattedEndpoint)
        {
            var index = endpoint.IndexOf("://", StringComparison.Ordinal);
            if (-1 == index)
            {
                if (string.IsNullOrEmpty(Default))
                {
                    formatter = null;
                    formattedEndpoint = string.Empty;
                    return false;
                }

                if (!_formatters.TryGetValue(Default, out formatter))
                {
                    formattedEndpoint = string.Empty;

                    return false;
                }

                formattedEndpoint = endpoint;
            }
            else
            {
                var key = endpoint.Substring(0, index);
                formattedEndpoint = endpoint.Substring(index + 3);

                if (!_formatters.TryGetValue(key, out formatter))
                {
                    formattedEndpoint = string.Empty;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Removes any URL parameters from a url.
        /// </summary>
        private static string RemoveParams(string url)
        {
            var index = url.IndexOf('?');
            if (-1 == index)
            {
                return url;
            }

            return url.Substring(0, index);
        }

        /// <summary>
        /// Checks to see if protocol, base url, and port are equal.
        /// </summary>
        private static bool IsSameBaseFormat(UrlFormatter f1, UrlFormatter f2)
        {
            return string.Equals(f1.Protocol, f2.Protocol)
                   && string.Equals(f1.BaseUrl, f2.BaseUrl)
                   && f1.Port == f2.Port;
        }
    }
}