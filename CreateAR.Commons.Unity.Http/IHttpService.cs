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
        /// Object for building Urls.
        /// </summary>
        UrlBuilder UrlBuilder { get; }

        /// <summary>
        /// Header information.
        /// </summary>
        List<Tuple<string, string>> Headers { get; }

        /// <summary>
        /// Sends a GET request to an endpoint.
        /// </summary>
        /// <typeparam name="T">The type to expect from the endpoint.</typeparam>
        /// <param name="url">The URL to hit, including GET parameters.</param>
        /// <returns>An IAsyncScope to listen to.</returns>
        /// <exception cref="NullReferenceException"></exception>
        IAsyncToken<HttpResponse<T>> Get<T>(string url);

        /// <summary>
        /// Sends a POST request to an endpoint.
        /// </summary>
        /// <typeparam name="T">The type to expect from the endpoint.</typeparam>
        /// <param name="url">The URL to hit.</param>
        /// <param name="payload">The resource to send to this endpoint.</param>
        /// <returns>An IAsyncScope to listen to.</returns>
        /// <exception cref="NullReferenceException"></exception>
        IAsyncToken<HttpResponse<T>> Post<T>(
            string url,
            object payload);

        /// <summary>
        /// Sends a POST request to an endpoint.
        /// </summary>
        /// <typeparam name="T">The type to expect from the endpoint.</typeparam>
        /// <param name="url">The URL to hit.</param>
        /// <param name="payload">The resource to send to this endpoint.</param>
        /// <returns>An IAsyncScope to listen to.</returns>
        IAsyncToken<HttpResponse<T>> PostRaw<T>(string url, ref byte[] payload);

        /// <summary>
        /// Sends a PUT request to an endpoint.
        /// </summary>
        /// <typeparam name="T">The type to expect from the endpoint.</typeparam>
        /// <param name="url">The URL to hit.</param>
        /// <param name="payload">The resource to send to this endpoint.</param>
        /// <returns>An IAsyncScope to listen to.</returns>
        /// <exception cref="NullReferenceException"></exception>
        IAsyncToken<HttpResponse<T>> Put<T>(
            string url,
            object payload);

        /// <summary>
        /// Sends a PUT request to an endpoint.
        /// </summary>
        /// <typeparam name="T">The type to expect from the endpoint.</typeparam>
        /// <param name="url">The URL to hit.</param>
        /// <param name="payload">The resource to send to this endpoint.</param>
        /// <returns>An IAsyncScope to listen to.</returns>
        /// <exception cref="NullReferenceException"></exception>
        IAsyncToken<HttpResponse<T>> PutRaw<T>(string url, ref byte[] payload);

        /// <summary>
        /// Sends a DELETE request to an endpoint.
        /// </summary>
        /// <typeparam name="T">The type to expect from the endpoint.</typeparam>
        /// <param name="url">The URL to hit.</param>
        /// <returns>An IAsyncScope to listen to.</returns>
        /// <exception cref="NullReferenceException"></exception>
        IAsyncToken<HttpResponse<T>> Delete<T>(
            string url);
    }
}