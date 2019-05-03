using System.Collections.Generic;
using CreateAR.Commons.Unity.DataStructures;

namespace CreateAR.Commons.Unity.Http
{
    /// <summary>
    /// This type is used as a facade for <see cref="UrlFormatterCollection"/> and <see cref="HttpHeaders"/>
    /// by adapting the url protocol name to specific services within a single <see cref="HttpService"/> instance.
    /// </summary>
    public class HttpServiceManager
    {
        /// <summary>
        /// The url formatter collection used to parse urls and associate headers to specific
        /// requests.
        /// </summary>
        public UrlFormatterCollection Urls { get; }

        /// <summary>
        /// Service and endpoint based request headers.
        /// </summary>
        public HttpHeaders Headers { get; }

        /// <summary>
        /// Creates a new <see cref="HttpServiceManager"/> instance
        /// </summary>
        public HttpServiceManager(UrlFormatterCollection urls)
        {
            Urls = urls;
            Headers = new HttpHeaders();
        }

        /// <summary>
        /// Registers a <see cref="UrlFormatter"/> instance for a specific service. 
        /// </summary>
        public void Register(string name, UrlFormatter urlFormatter)
        {
            Urls.Register(name, urlFormatter);
        }

        /// <summary>
        /// Gets the url formatter for a specific service.
        /// </summary>
        public UrlFormatter GetUrlFormatter(string service)
        {
            return Urls.Formatter(service);
        }

        /// <summary>
        /// Gets the headers for a specific service.
        /// </summary>
        public Dictionary<string, string> GetHeaders(string service)
        {
            return Headers.GetHeaders(service);
        }

        /// <summary>
        /// Gets the headers for a specific endpoint.
        /// </summary>
        public Dictionary<string, string> GetHeaders(string service, string endpoint)
        {
            return Headers.GetHeaders(service, endpoint);
        }

        /// <summary>
        /// Adds a header for a specific service.
        /// </summary>
        /// <param name="service">The specific service name to add headers for.</param>
        /// <param name="key">The header key.</param>
        /// <param name="value">The header value.</param>
        public void AddHeader(string service, string key, string value)
        {
            Headers.Set(service, key, value);
        }

        /// <summary>
        /// Adds a header for a specific service endpoint. 
        /// </summary>
        /// <param name="service">The specific service name to add headers for.</param>
        /// <param name="endpoint">The specific endpoint to add headers for.</param>
        /// <param name="key">The header key.</param>
        /// <param name="value">The header value.</param>
        public void AddHeader(string service, string endpoint, string key, string value)
        {
            Headers.Set(service, endpoint, key, value);
        }

        /// <summary>
        /// Removes a header for a specific service.
        /// </summary>
        /// <param name="service">The specific service name to remove headers for.</param>
        /// <param name="key">The header key.</param>
        public void RemoveHeader(string service, string key)
        {
            Headers.Remove(service, key);
        }

        /// <summary>
        /// Removes a header for a specific service endpoint. 
        /// </summary>
        /// <param name="service">The specific service name to remove headers for.</param>
        /// <param name="endpoint">The specific endpoint to remove headers for.</param>
        /// <param name="key">The header key.</param>
        public void RemoveHeader(string service, string endpoint, string key)
        {
            Headers.Remove(service, endpoint, key);
        }

        /// <summary>
        /// Clears headers for a specific service.
        /// </summary>
        public void ClearHeaders(string service)
        {
            Headers.Clear(service);
        }

        /// <summary>
        /// Clears headers for a specific service endpoint.
        /// </summary>
        public void ClearHeaders(string service, string endpoint)
        {
            Headers.Clear(service, endpoint);
        }

        /// <summary>
        /// Resolves the service name, url, and headers for a specific url.
        /// </summary>
        /// <returns>A tuple containing the service name, resolved url, and headers.</returns>
        public Tuple<string, string, Dictionary<string, string>> ResolveServiceData(string url)
        {
            var service = Urls.FormatterName(url);

            return Tuple.Create(
                service,
                Urls.Url(url),
                Headers.GetHeaders(service));
        }
    }
}