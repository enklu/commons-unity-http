using System.Collections.Generic;

namespace CreateAR.Commons.Unity.Http
{
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