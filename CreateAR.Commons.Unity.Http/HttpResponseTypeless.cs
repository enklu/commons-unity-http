using System;
using System.Collections.Generic;

namespace CreateAR.Commons.Unity.Http
{
    /// <summary>
    /// Contains information about the response received from an HTTP endpoint.
    /// </summary>
    public class HttpResponseTypeless
    {
        /// <summary>
        /// The response object.
        /// </summary>
        public object Payload;

        /// <summary>
        /// The type of Payload.
        /// </summary>
        public Type Type;

        /// <summary>
        /// Raw response.
        /// </summary>
        public byte[] Raw;

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