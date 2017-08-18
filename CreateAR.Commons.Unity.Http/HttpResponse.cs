using System.Collections.Generic;
using CreateAR.Commons.Unity.DataStructures;

namespace CreateAR.Commons.Unity.Http
{
    /// <summary>
    /// Contains information about the response received from an HTTP endpoint.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpResponse<T>
    {
        /// <summary>
        /// The response object.
        /// </summary>
        public T Payload;

        /// <summary>
        /// Headers.
        /// </summary>
        public List<Tuple<string, string>> Headers;

        /// <summary>
        /// Http status.
        /// </summary>
        public long StatusCode;

        /// <summary>
        /// True iff network was successful-- does not judge successful request
        /// logic.
        /// </summary>
        public bool NetworkSuccess;

        /// <summary>
        /// Error returned. Populated iff NetworkSuccess is false.
        /// </summary>
        public string NetworkError;
    }
}