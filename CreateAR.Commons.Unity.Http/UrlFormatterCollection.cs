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
                protocol = protocol.Substring(index + 3);
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
    }
}