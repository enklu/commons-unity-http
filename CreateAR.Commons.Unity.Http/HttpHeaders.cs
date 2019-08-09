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
}