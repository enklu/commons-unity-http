using System.Collections.Generic;

namespace CreateAR.Commons.Unity.Http
{
    /// <summary>
    /// This type manages headers for specific services and endpoints. 
    /// </summary>
    public class HttpHeaders
    {
        /// <summary>
        /// Contains service headers by name.
        /// </summary>
        private readonly Dictionary<string, HttpServiceHeaders> _serviceHeaders;

        /// <summary>
        /// Empty header dictionary.
        /// </summary>
        private readonly Dictionary<string, string> _empty;

        /// <summary>
        /// Creates a new <see cref="HttpHeaders"/> instance.
        /// </summary>
        public HttpHeaders()
        {
            _serviceHeaders = new Dictionary<string, HttpServiceHeaders>();
            _empty = new Dictionary<string, string>();
        }

        /// <summary>
        /// Sets a specific header for a specific service.
        /// </summary>
        /// <param name="service">The name of the service to update the headers for.</param>
        /// <param name="key">The key of the header.</param>
        /// <param name="value">The value of the header.</param>
        public void Set(string service, string key, string value)
        {
            if (!_serviceHeaders.ContainsKey(service))
            {
                _serviceHeaders[service] = new HttpServiceHeaders(service);
            }

            _serviceHeaders[service].Headers[key] = value;
        }

        /// <summary>
        /// Sets a specific header for a specific service.
        /// </summary>
        /// <param name="service">The name of the service to update the headers for.</param>
        /// <param name="endpoint">The name of the endpoint to update the headers for.</param>
        /// <param name="key">The key of the header.</param>
        /// <param name="value">The value of the header.</param>
        public void Set(string service, string endpoint, string key, string value)
        {
            if (!_serviceHeaders.ContainsKey(service))
            {
                _serviceHeaders[service] = new HttpServiceHeaders(service);
            }

            _serviceHeaders[service].Set(endpoint, key, value);
        }

        /// <summary>
        /// Removes the header from any requests made to this endpoint
        /// </summary>
        public void Remove(string service, string key)
        {
            if (!_serviceHeaders.ContainsKey(service))
            {
                return;
            }

            _serviceHeaders[service].Headers.Remove(key);
        }

        /// <summary>
        /// Removes the header from any requests made to this endpoint
        /// </summary>
        public void Remove(string service, string endpoint, string key)
        {
            if (!_serviceHeaders.ContainsKey(service))
            {
                return;
            }

            _serviceHeaders[service].Remove(endpoint, key);
        }

        /// <summary>
        /// Remove all headers from the provided service.
        /// </summary>
        public void Clear(string service)
        {
            if (!_serviceHeaders.ContainsKey(service))
            {
                return;
            }

            _serviceHeaders[service].Headers.Clear();
        }

        /// <summary>
        /// Remove all headers from the service endpoint.
        /// </summary>
        public void Clear(string service, string endpoint)
        {
            if (!_serviceHeaders.ContainsKey(service))
            {
                return;
            }

            _serviceHeaders[service].Clear(endpoint);
        }

        /// <summary>
        /// Gets the headers used to execute a request.
        /// </summary>
        public Dictionary<string, string> GetHeaders(string service)
        {
            if (string.IsNullOrEmpty(service) || !_serviceHeaders.TryGetValue(service, out var serviceHeaders))
            {
                return _empty;
            }

            return serviceHeaders.Headers;
        }

        /// <summary>
        /// Gets the headers used to execute a request.
        /// </summary>
        public Dictionary<string, string> GetHeaders(string service, string endpoint)
        {
            if (string.IsNullOrEmpty(service) || !_serviceHeaders.TryGetValue(service, out var serviceHeaders))
            {
                return _empty;
            }

            return serviceHeaders.GetHeaders(endpoint);
        }
    }

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

    /// <summary>
    /// This type contains a service endpoint specific set of headers.
    /// </summary>
    public class HttpServiceEndpointHeaders
    {
        /// <summary>
        /// The name of the endpoint to use the included headers for.
        /// </summary>
        public string EndpointName { get; }

        /// <summary>
        /// Headers used for a specific endpoint.
        /// </summary>
        public Dictionary<string, string> Headers { get; }

        /// <summary>
        /// Creates a new <see cref="HttpServiceHeaders"/> instance.
        /// </summary>
        /// <param name="serviceName"></param>
        public HttpServiceEndpointHeaders(string endpointName)
        {
            EndpointName = endpointName;
            Headers = new Dictionary<string, string>();
        }
    }
}