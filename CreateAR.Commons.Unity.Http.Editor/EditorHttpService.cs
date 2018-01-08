using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.DataStructures;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.Commons.Unity.Http.Editor
{
    /// <summary>
    /// Editor implementation of <c>IHttpService</c>.
    /// </summary>
    public class EditorHttpService : IHttpService
    {
        /// <summary>
        /// How to deserialize.
        /// </summary>
        protected enum SerializationType
        {
            Json,
            Raw
        }

        /// <summary>
        /// State passed along with the <c>HttpWebRequest</c> async API.
        /// </summary>
        protected class HttpState
        {
            /// <summary>
            /// Url.
            /// </summary>
            public string Url;

            /// <summary>
            /// Request.
            /// </summary>
            public HttpWebRequest Request;

            /// <summary>
            /// Callback.
            /// </summary>
            public Action<object> Resolve;

            /// <summary>
            /// Serialization strategy.
            /// </summary>
            public SerializationType Serialization;
        }

        /// <summary>
        /// Specifies content types.
        /// </summary>
        private const string CONTENT_TYPE_JSON = "application/json";

        /// <summary>
        /// Serializer.
        /// </summary>
        private readonly ISerializer _serializer;

        /// <summary>
        /// For moving actions to the main thread.
        /// </summary>
        private readonly List<Action> _synchronizedActions = new List<Action>();

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
            bootstrapper.BootstrapCoroutine(RunActions());

            UrlBuilder = new UrlBuilder();
            Headers = new List<Tuple<string, string>>();
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> Get<T>(string url)
        {
            return SendJsonRequest<T>(HttpVerb.Get, url, null);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> Post<T>(
            string url,
            object payload)
        {
            return SendJsonRequest<T>(HttpVerb.Post, url, payload);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> Put<T>(string url, object payload)
        {
            return SendJsonRequest<T>(HttpVerb.Put, url, payload);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> Delete<T>(string url)
        {
            return SendJsonRequest<T>(HttpVerb.Delete, url, null);
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
            var token = new AsyncToken<HttpResponse<byte[]>>();

            var request = (HttpWebRequest)WebRequest.Create(url);

            ApplyHeaders(Headers, request);

            request.BeginGetResponse(
                Request_OnBeginGetResponse<byte[]>,
                new HttpState
                {
                    Url = url,
                    Request = request,
                    Serialization = SerializationType.Raw,
                    Resolve = response =>
                    {
                        var exception = response as Exception;
                        if (exception != null)
                        {
                            token.Fail(exception);
                        }
                        else
                        {
                            token.Succeed((HttpResponse<byte[]>)response);
                        }
                    }
                });

            return token;
        }

        /// <summary>
        /// Sends a JSON request.
        /// </summary>
        /// <typeparam name="T">Type of response.</typeparam>
        /// <param name="verb">HTTP verb.</param>
        /// <param name="url">Complete endpoint url.</param>
        /// <param name="payload">Object to send.</param>
        /// <returns></returns>
        private IAsyncToken<HttpResponse<T>> SendJsonRequest<T>(
            HttpVerb verb,
            string url,
            object payload)
        {
            var token = new AsyncToken<HttpResponse<T>>();
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = CONTENT_TYPE_JSON;
            request.Method = verb.ToString().ToUpperInvariant();

            ApplyHeaders(Headers, request);
            ApplyJsonPayload(payload, request);

            request.BeginGetResponse(
                Request_OnBeginGetResponse<T>,
                new HttpState
                {
                    Url = url,
                    Request = request,
                    Serialization = SerializationType.Json,
                    Resolve = response =>
                    {
                        var exception = response as Exception;
                        if (exception != null)
                        {
                            token.Fail(exception);
                        }
                        else
                        {
                            token.Succeed((HttpResponse<T>)response);
                        }
                    }
                });

            return token;
        }

        /// <summary>
        /// Called by the C# API _on a thread from a threadpool_, NOT the main
        /// thread.
        /// </summary>
        /// <typeparam name="T">Type of response.</typeparam>
        /// <param name="result">Result object.</param>
        private void Request_OnBeginGetResponse<T>(IAsyncResult result)
        {
            var state = (HttpState)result.AsyncState;

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)state.Request.EndGetResponse(result);
            }
            catch (Exception exception)
            {
                Synchronize(() => state.Resolve(exception));

                return;
            }

            var httpResponse = new HttpResponse<T>
            {
                Headers = FormatHeaders(response.Headers),
                StatusCode = (long)response.StatusCode
            };

            using (var stream = response.GetResponseStream())
            {
                if (null == stream)
                {
                    Synchronize(() => state.Resolve(new Exception("Null response.")));

                    return;
                }

                // TODO: reuse buffers
                var bytes = new byte[16384];
                var index = 0;
                var bytesRead = 0;

                while ((bytesRead = stream.Read(bytes, index, bytes.Length - index)) > 0)
                {
                    index += bytesRead;

                    if (index == bytes.Length)
                    {
                        var newBuffer = new byte[bytes.Length * 2];
                        Array.Copy(bytes, 0, newBuffer, 0, index);
                        bytes = newBuffer;
                    }
                }

                object value = null;
                switch (state.Serialization)
                {
                    case SerializationType.Json:
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
                            break;
                        }
                    default:
                        {
                            value = bytes;
                            break;
                        }
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
            }

            Synchronize(() => state.Resolve(httpResponse));

            response.Close();
        }

        /// <summary>
        /// Applies headers to request.
        /// </summary>
        /// <param name="headers">Headers to add to request.</param>
        /// <param name="request">Request to add headers to.</param>
        protected void ApplyHeaders(
            List<Tuple<string, string>> headers,
            HttpWebRequest request)
        {
            if (headers == null)
            {
                return;
            }

            for (int i = 0, len = headers.Count; i < len; i++)
            {
                var kv = headers[i];
                if (!WebHeaderCollection.IsRestricted(kv.Item1))
                {
                    request.Headers.Add(kv.Item1, kv.Item2);
                }
                else
                {
                    Log.Warning(this, "Header [{0}] is restricted.", kv.Item1);
                }
            }

            request.ContentType = CONTENT_TYPE_JSON;
            request.Accept = CONTENT_TYPE_JSON;
        }

        /// <summary>
        /// Applies json payload to request.
        /// </summary>
        /// <param name="payload">The payload to add to the request.</param>
        /// <param name="request">The request.</param>
        protected void ApplyJsonPayload(
            object payload,
            HttpWebRequest request)
        {
            if (null == payload)
            {
                return;
            }

            byte[] payloadBytes;

            // let serialization errors bubble up
            _serializer.Serialize(payload, out payloadBytes);

            using (var stream = request.GetRequestStream())
            {
                stream.Write(payloadBytes, 0, payloadBytes.Length);
            }
        }

        /// <summary>
        /// Adds an action to the sync queue.
        /// </summary>
        /// <param name="action">Action to add.</param>
        private void Synchronize(Action action)
        {
            lock (_synchronizedActions)
            {
                _synchronizedActions.Add(action);
            }
        }

        /// <summary>
        /// Long-running coroutine to execute actions on main thread.
        /// </summary>
        /// <returns></returns>
        private IEnumerator RunActions()
        {
            while (true)
            {
                lock (_synchronizedActions)
                {
                    for (int i = 0, len = _synchronizedActions.Count; i < len; i++)
                    {
                        _synchronizedActions[i]();
                    }
                    _synchronizedActions.Clear();
                }

                yield return null;
            }
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

        /// <summary>
        /// Converts headers to a list of CreateAR.Commons.Unity.DataStructures.Tuples.
        /// </summary>
        /// <param name="headers">The header collection.</param>
        /// <returns>A list of CreateAR.Commons.Unity.DataStructures.Tuples that represent the headers.</returns>
        private static List<Tuple<string, string>> FormatHeaders(WebHeaderCollection headers)
        {
            var tuples = new List<Tuple<string, string>>();

            if (null != headers)
            {
                foreach (var key in headers.AllKeys)
                {
                    tuples.Add(Tuple.Create(
                        key,
                        headers[key]));
                }
            }

            return tuples;
        }
    }
}
