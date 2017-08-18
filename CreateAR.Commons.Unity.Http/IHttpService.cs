using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.DataStructures;

namespace CreateAR.Commons.Unity.Http
{
    /// <summary>
    /// Verbs.
    /// </summary>
    public enum HttpVerb
    {
        Get,
        Post,
        Put,
        Patch,
        Delete
    }

    /// <summary>
    /// Defines an HTTP service.
    /// </summary>
    public interface IHttpService
    {
        /// <summary>
        /// Sends a GET request to an endpoint.
        /// </summary>
        /// <typeparam name="T">The type to expect from the endpoint.</typeparam>
        /// <param name="url">The URL to hit, including GET parameters.</param>
        /// <param name="headers">Header information.</param>
        /// <returns>An IAsyncScope to listen to.</returns>
        /// <exception cref="NullReferenceException"></exception>
        IAsyncToken<HttpResponse<T>> Get<T>(
            string url,
            List<Tuple<string, string>> headers);

        /// <summary>
        /// Sends a POST request to an endpoint.
        /// </summary>
        /// <typeparam name="T">The type to expect from the endpoint.</typeparam>
        /// <param name="url">The URL to hit.</param>
        /// <param name="payload">The resource to send to this endpoint.</param>
        /// <param name="headers">Header information.</param>
        /// <returns>An IAsyncScope to listen to.</returns>
        /// <exception cref="NullReferenceException"></exception>
        IAsyncToken<HttpResponse<T>> Post<T>(
            string url,
            object payload,
            List<Tuple<string, string>> headers);
    }
}