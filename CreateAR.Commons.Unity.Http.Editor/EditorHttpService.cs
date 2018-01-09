using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.DataStructures;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.Commons.Unity.Http.Editor
{
    /// <summary>
    /// Editor implementation of <c>IHttpService</c>.
    /// </summary>
    public class EditorHttpService : IHttpService
    {
        /// <summary>
        /// Specifies content types.
        /// </summary>
        private const string CONTENT_TYPE_JSON = "application/json";

        /// <summary>
        /// Serializer.
        /// </summary>
        private readonly ISerializer _serializer;

        /// <summary>
        /// Bootstrapper.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <inheritdoc cref="IHttpService"/>
        public UrlBuilder UrlBuilder { get; private set; }

        /// <inheritdoc cref="IHttpService"/>
        public List<Tuple<string, string>> Headers { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public EditorHttpService(
            ISerializer serializer,
            IBootstrapper bootstrapper)
        {
            _serializer = serializer;
            _bootstrapper = bootstrapper;

            UrlBuilder = new UrlBuilder();
            Headers = new List<Tuple<string, string>>();
        }

        /// <inheritdoc cref="IHttpService"/>
        public void Abort()
        {

        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> Get<T>(string url)
        {
            return SendRequest<T>(HttpVerb.Get, url, null);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> Post<T>(
            string url,
            object payload)
        {
            return SendRequest<T>(HttpVerb.Post, url, payload);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> Put<T>(string url, object payload)
        {
            return SendRequest<T>(HttpVerb.Put, url, payload);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> Delete<T>(string url)
        {
            return SendRequest<T>(HttpVerb.Delete, url, null);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> PostFile<T>(string url, IEnumerable<Tuple<string, string>> fields, ref byte[] file)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> PutFile<T>(string url, IEnumerable<Tuple<string, string>> fields, ref byte[] file)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<byte[]>> Download(string url)
        {
            return SendRequest<byte[]>(
                HttpVerb.Get,
                url,
                null);
        }

        /// <summary>
        /// Sends a JSON request.
        /// </summary>
        /// <typeparam name="T">Type of response.</typeparam>
        /// <param name="verb">HTTP verb.</param>
        /// <param name="url">Complete endpoint url.</param>
        /// <param name="payload">Object to send.</param>
        /// <returns></returns>
        private IAsyncToken<HttpResponse<T>> SendRequest<T>(
            HttpVerb verb,
            string url,
            object payload)
        {
            Log.Info(this,
                "{0} {1}",
                verb.ToString().ToUpperInvariant(),
                url);

            var token = new AsyncToken<HttpResponse<T>>();

            byte[] bytes = null;
            if (null != payload)
            {
                _serializer.Serialize(payload, out bytes);
            }

            var request = new WWW(
                url,
                bytes,
                HeaderDictionary());

            _bootstrapper.BootstrapCoroutine(Wait(request, token));

            return token;
        }

        /// <summary>
        /// Sycnhronously process response.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">WWW request.</param>
        /// <param name="token">Token to resolve.</param>
        private IEnumerator Wait<T>(
            WWW request,
            AsyncToken<HttpResponse<T>> token)
        {
            while (!request.isDone)
            {
                yield return null;
            }

            var httpResponse = new HttpResponse<T>
            {
                StatusCode = GetStatusCode(request),
                Headers = null == request.responseHeaders
                    ? new List<Tuple<string, string>>()
                    : request
                        .responseHeaders
                        .Select(pair => Tuple.Create(pair.Key, pair.Value))
                        .ToList(),
                Raw = request.bytes
            };

            var bytes = request.bytes;

            object value = null;
            if (typeof(T) != typeof(byte[]))
            {
                try
                {
                    _serializer.Deserialize(typeof(T), ref bytes, out value);
                }
                catch (Exception exception)
                {
                    httpResponse.NetworkSuccess = false;
                    httpResponse.NetworkError = string.Format("Could not deserialize : {0}.", exception.Message);
                }
            }
            else
            {
                value = bytes;
            }

            httpResponse.Payload = (T)value;

            if (Successful((int)httpResponse.StatusCode))
            {
                httpResponse.NetworkSuccess = true;
            }
            else
            {
                httpResponse.NetworkSuccess = false;
                httpResponse.NetworkError = Encoding.UTF8.GetString(bytes);
            }

            token.Succeed(httpResponse);
        }

        /// <summary>
        /// Figures out the status code.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns></returns>
        private int GetStatusCode(WWW request)
        {
            var headers = request.responseHeaders;
            if (null == headers)
            {
                return 0;
            }

            if (headers.ContainsKey("STATUS"))
            {
                var status = headers["STATUS"];
                var substrings = status.Split(' ');
                if (substrings.Length < 3)
                {
                    Log.Warning(this, "Invalid response status: {0}.", status);
                }
                else
                {
                    int returnValue;
                    if (!int.TryParse(substrings[1], out returnValue))
                    {
                        Log.Error("Invalid response code: {0}.", substrings[1]);
                        return 0;
                    }

                    return returnValue;
                }
            }

            return 0;
        }

        /// <summary>
        /// Creates a dictionary from headers.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> HeaderDictionary()
        {
            var dictionary = new Dictionary<string, string>();

            foreach (var pair in Headers)
            {
                dictionary[pair.Item1] = pair.Item2;
            }

            dictionary["Content-Type"] = CONTENT_TYPE_JSON;
            dictionary["Accept"] = CONTENT_TYPE_JSON;

            return dictionary;
        }

        /// <summary>
        /// Returns true iff status code indicates a success.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <returns></returns>
        private static bool Successful(int statusCode)
        {
            // any 200 is a success
            return statusCode >= 200 && statusCode < 300;
        }
    }
}
