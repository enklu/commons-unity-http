using System;
using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.DataStructures;
using UnityEngine.Networking;

namespace CreateAR.Commons.Unity.Http
{
    /// <summary>
    /// An implementation of IHttpService on top of UnityWebRequest.
    /// </summary>
    public class HttpService : IHttpService
    {
        /// <summary>
        /// Specifies content types.
        /// </summary>
        private const string CONTENT_TYPE_JSON = "application/json";
        private const string CONTENT_TYPE_OCTET_STREAM = "application/octet-stream";

        /// <summary>
        /// Serializer!
        /// </summary>
        private readonly ISerializer _serializer;

        /// <summary>
        /// Bootstrapper implementation.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <inheritdoc cref="IHttpService"/>
        public UrlBuilder UrlBuilder { get; }

        /// <inheritdoc cref="IHttpService"/>
        public List<Tuple<string, string>> Headers { get; }

        /// <summary>
        /// Creates an HttpService.
        /// 
        /// TODO: This class enforces a json contenttype, but accepts any
        /// TODO: ISerializer.
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="bootstrapper"></param>
        public HttpService(ISerializer serializer, IBootstrapper bootstrapper)
        {
            _serializer = serializer;
            _bootstrapper = bootstrapper;

            UrlBuilder = new UrlBuilder();
            Headers = new List<Tuple<string, string>>();
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> Get<T>(string url)
        {
            return SendJsonRequest<T>(
                HttpVerb.Get,
                url,
                null);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> Post<T>(
            string url,
            object payload)
        {
            return SendJsonRequest<T>(HttpVerb.Post, url, payload);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> PostRaw<T>(string url, ref byte[] payload)
        {
            return SendRawRequest<T>(HttpVerb.Post, url, ref payload);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> Put<T>(string url, object payload)
        {
            return SendJsonRequest<T>(HttpVerb.Put, url, payload);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> PutRaw<T>(string url, ref byte[] payload)
        {
            return SendRawRequest<T>(HttpVerb.Put, url, ref payload);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> Delete<T>(string url)
        {
            return SendJsonRequest<T>(HttpVerb.Delete, url, null);
        }

        /// <summary>
        /// Sends a json request.
        /// </summary>
        /// <typeparam name="T">The type of response we expect.</typeparam>
        /// <param name="verb">The http verb to use.</param>
        /// <param name="url"></param>
        /// <param name="payload"></param>
        /// <returns>An IAsyncScope to listen to.</returns>
        /// <exception cref="NullReferenceException"></exception>
        protected IAsyncToken<HttpResponse<T>> SendJsonRequest<T>(
            HttpVerb verb,
            string url,
            object payload)
        {
            var scope = new AsyncToken<HttpResponse<T>>();

            var request = new UnityWebRequest(
                url,
                verb.ToString().ToUpperInvariant())
            {
                downloadHandler = new DownloadHandlerBuffer(),
                disposeDownloadHandlerOnDispose = true,
                disposeUploadHandlerOnDispose = true
            };
            
            ApplyHeaders(Headers, request);
            ApplyJsonPayload(payload, request);
            
            _bootstrapper.BootstrapCoroutine(Wait(request, scope));
            
            return scope;
        }

        /// <summary>
        /// Sends a json request.
        /// </summary>
        /// <typeparam name="T">The type of response we expect.</typeparam>
        /// <param name="verb">The http verb to use.</param>
        /// <param name="url"></param>
        /// <param name="bytes"></param>
        /// <returns>An IAsyncScope to listen to.</returns>
        /// <exception cref="NullReferenceException"></exception>
        protected IAsyncToken<HttpResponse<T>> SendRawRequest<T>(
            HttpVerb verb,
            string url,
            ref byte[] bytes)
        {
            var scope = new AsyncToken<HttpResponse<T>>();

            var request = new UnityWebRequest(
                url,
                verb.ToString().ToUpperInvariant())
            {
                downloadHandler = new DownloadHandlerBuffer(),
                disposeDownloadHandlerOnDispose = true,
                disposeUploadHandlerOnDispose = true
            };

            ApplyHeaders(Headers, request);
            ApplyRawPayload(ref bytes, request);

            _bootstrapper.BootstrapCoroutine(Wait(request, scope));

            return scope;
        }

        /// <summary>
        /// Applies headers to request.
        /// </summary>
        /// <param name="headers">Headers to add to request.</param>
        /// <param name="request">Request to add headers to.</param>
        protected static void ApplyHeaders(
            List<Tuple<string, string>> headers,
            UnityWebRequest request)
        {
            if (headers == null)
            {
                return;
            }

            for (int i = 0, len = headers.Count; i < len; i++)
            {
                var kv = headers[i];
                request.SetRequestHeader(kv.Item1, kv.Item2);
            }
        }

        /// <summary>
        /// Applies json payload to request.
        /// </summary>
        /// <param name="payload">The payload to add to the request.</param>
        /// <param name="request">The request.</param>
        protected void ApplyJsonPayload(object payload, UnityWebRequest request)
        {
            if (payload == null)
            {
                return;
            }

            byte[] payloadBytes;

            // let serialization errors bubble up
            _serializer.Serialize(payload, out payloadBytes);

            request.uploadHandler = new UploadHandlerRaw(payloadBytes)
            {
                contentType = CONTENT_TYPE_JSON
            };

            request.SetRequestHeader("Content-Type", CONTENT_TYPE_JSON);
            request.SetRequestHeader("Accept", CONTENT_TYPE_JSON);
        }

        /// <summary>
        /// Applies payload to request.
        /// </summary>
        /// <param name="payload">The payload to add to the request.</param>
        /// <param name="request">The request.</param>
        protected void ApplyRawPayload(ref byte[] payload, UnityWebRequest request)
        {
            if (payload == null)
            {
                return;
            }
            
            request.uploadHandler = new UploadHandlerRaw(payload)
            {
                contentType = CONTENT_TYPE_OCTET_STREAM
            };

            request.SetRequestHeader("Content-Type", CONTENT_TYPE_OCTET_STREAM);
            request.SetRequestHeader("Accept", CONTENT_TYPE_OCTET_STREAM);
        }

        /// <summary>
        /// Waits on the request and resolves the scope.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to.</typeparam>
        /// <param name="request">The UnityWebRequest to send and wait on.</param>
        /// <param name="scope">The scope to resolve.</param>
        /// <returns>The coroutines IEnumerator.</returns>
        protected IEnumerator Wait<T>(
            UnityWebRequest request,
            AsyncToken<HttpResponse<T>> scope)
        {
            yield return request.Send();

            var response = new HttpResponse<T>
            {
                Headers = FormatHeaders(request.GetResponseHeaders()),
                StatusCode = request.responseCode
            };
            
            ProcessResponse(request, response);

            // must kill the request
            request.Dispose();

            scope.Succeed(response);
        }

        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <typeparam name="T">The type to expect the response payload to be.</typeparam>
        /// <param name="request">The request to inspect.</param>
        /// <param name="response">The response object.</param>
        protected void ProcessResponse<T>(
            UnityWebRequest request,
            HttpResponse<T> response)
        {
            if (request.isNetworkError)
            {
                response.NetworkSuccess = false;
                response.NetworkError = request.error;
            }
            else
            {
                var bytes = request.downloadHandler.data;
                try
                {
                    object value;
                    _serializer.Deserialize(typeof(T), ref bytes, out value);
                    response.Payload = (T)value;

                    if (Successful(request))
                    {
                        response.NetworkSuccess = true;
                    }
                    else
                    {
                        response.NetworkSuccess = false;
                        response.NetworkError = request.downloadHandler.text;
                    }
                }
                catch (Exception exception)
                {
                    response.NetworkSuccess = false;
                    response.NetworkError = $"Could not deserialize {request.downloadHandler.text} : {exception.Message}.";
                }
            }
        }

        /// <summary>
        /// Returns true iff status code indicates a success.
        /// </summary>
        /// <param name="request">The web request.</param>
        /// <returns></returns>
        protected bool Successful(UnityWebRequest request)
        {
            // any 200 is a success
            return !request.isNetworkError
                   && request.responseCode >= 200
                   && request.responseCode < 300;
        }

        /// <summary>
        /// Converts headers in a Dictionary to a list of CreateAR.Commons.Unity.DataStructures.Tuples.
        /// </summary>
        /// <param name="headers">The headers in a dictionary.</param>
        /// <returns>A list of CreateAR.Commons.Unity.DataStructures.Tuples that represent the headers.</returns>
        private static List<Tuple<string, string>> FormatHeaders(Dictionary<string, string> headers)
        {
            var tuples = new List<Tuple<string, string>>();

            if (null != headers)
            {
                foreach (var kvPair in headers)
                {
                    tuples.Add(Tuple.Create(
                        kvPair.Key,
                        kvPair.Value));
                }
            }

            return tuples;
        }
    }
}