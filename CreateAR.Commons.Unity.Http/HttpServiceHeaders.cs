using System.Collections.Generic;

namespace CreateAR.Commons.Unity.Http
{
    /// <summary>
    /// This type contains a service specific set of headers to use.
    /// </summary>
    public class HttpServiceHeaders
    {
        /// <summary>
        /// Endpoint specific headers
        /// </summary>
        private readonly Dictionary<string, HttpServiceEndpointHeaders> _endpointHeaders;

        /// <summary>
        /// The name of the service to use the included headers for.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// Headers used for a specific service.
        /// </summary>
        public Dictionary<string, string> Headers { get; }
        
        /// <summary>
        /// Creates a new <see cref="HttpServiceHeaders"/> instance.
        /// </summary>
        /// <param name="serviceName"></param>
        public HttpServiceHeaders(string serviceName)
        {
            ServiceName = serviceName;
            Headers = new Dictionary<string, string>();

            _endpointHeaders = new Dictionary<string, HttpServiceEndpointHeaders>();
        }
        
        /// <summary>
        /// Gets the headers used to execute a request.
        /// </summary>
        public Dictionary<string, string> GetHeaders(string endpoint)
        {
            // If we don't find any specific endpoint headers, return service headers
            if (string.IsNullOrEmpty(endpoint) || !_endpointHeaders.TryGetValue(endpoint, out var endpointHeaders))
            {
                return Headers;
            }

            return endpointHeaders.Headers;
        }

        /// <summary>
        /// Sets a specific header for a specific endpoint within the service.
        /// </summary>
        /// <param name="endpoint">The name of the endpoint to update the headers for.</param>
        /// <param name="key">The key of the header.</param>
        /// <param name="value">The value of the header.</param>
        public void Set(string endpoint, string key, string value)
        {
            if (!_endpointHeaders.ContainsKey(endpoint))
            {
                _endpointHeaders[endpoint] = new HttpServiceEndpointHeaders(endpoint);
            }

            _endpointHeaders[endpoint].Headers[key] = value;
        }

        /// <summary>
        /// Removes the header from any requests made to this endpoint
        /// </summary>
        public void Remove(string endpoint, string key)
        {
            if (!_endpointHeaders.ContainsKey(endpoint))
            {
                return;
            }

            _endpointHeaders[endpoint].Headers.Remove(key);
        }

        /// <summary>
        /// Remove all headers from this endpoint.
        /// </summary>
        public void Clear(string endpoint)
        {
            if (!_endpointHeaders.ContainsKey(endpoint))
            {
                return;
            }

            _endpointHeaders[endpoint].Headers.Clear();
        }
    }
}