using System;
using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.DataStructures;
using UnityEngine;
using UnityEngine.Networking;

namespace CreateAR.Commons.Unity.Http
{
    /// <summary>
    /// An implementation of IHttpService on top of UnityWebRequest.
    /// </summary>
    public class HttpService : IHttpService
    {
        protected enum SerializationType
        {
            Json,
            Raw
        }

        /// <summary>
        /// Specifies content types.
        /// </summary>
        private const string CONTENT_TYPE_JSON = "application/json";

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
        public IAsyncToken<HttpResponse<T>> PostFile<T>(
            string url,
            IEnumerable<Tuple<string, string>> fields,
            ref byte[] file)
        {
            return SendFile<T>(HttpVerb.Post, url, fields, ref file);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> PutFile<T>(
            string url,
            IEnumerable<Tuple<string, string>> fields,
            ref byte[] file)
        {
            return SendFile<T>(HttpVerb.Put, url, fields, ref file);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<byte[]>> Download(string url)
        {
            var token = new AsyncToken<HttpResponse<byte[]>>();

            var request = UnityWebRequest.Get(url);

            ApplyHeaders(Headers, request);

            _bootstrapper.BootstrapCoroutine(Wait(
                request,
                token,
                SerializationType.Raw));

            return token;
        }

        /// <summary>
        /// Sends a json request.
        /// </summary>
        /// <typeparam name="T">The type of response we expect.</typeparam>
        /// <param name="verb">The http verb to use.</param>
        /// <param name="url">The url to send the request to.</param>
        /// <param name="payload">The object that will be serialized into json.</param>
        /// <returns>An IAsyncToken to listen to.</returns>
        /// <exception cref="NullReferenceException"></exception>
        protected IAsyncToken<HttpResponse<T>> SendJsonRequest<T>(
            HttpVerb verb,
            string url,
            object payload)
        {
            var token = new AsyncToken<HttpResponse<T>>();

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
            
            _bootstrapper.BootstrapCoroutine(Wait(request, token));
            
            return token;
        }
        
        /// <summary>
        /// Sends a file!
        /// </summary>
        /// <typeparam name="T">The type of response we expect.</typeparam>
        /// <param name="verb">The http verb to use.</param>
        /// <param name="url">The url to send the request to.</param>
        /// <param name="fields">Optional fields that will _precede_ the file.</param>
        /// <param name="file">The file, which will be named "file".</param>
        /// <returns></returns>
        private IAsyncToken<HttpResponse<T>> SendFile<T>(
            HttpVerb verb,
            string url,
            IEnumerable<Tuple<string, string>> fields,
            ref byte[] file)
        {
            var token = new AsyncToken<HttpResponse<T>>();

            var form = new WWWForm();

            foreach (var tuple in fields)
            {
                form.AddField(tuple.Item1, tuple.Item2);
            }

            form.AddBinaryData("file", file);

            var request = UnityWebRequest.Post(
                url,
                form);
            request.method = verb.ToString().ToUpperInvariant();
            request.downloadHandler = new DownloadHandlerBuffer();
            request.disposeDownloadHandlerOnDispose = true;
            request.disposeUploadHandlerOnDispose = true;

            ApplyHeaders(Headers, request);
            
            _bootstrapper.BootstrapCoroutine(Wait(request, token));

            return token;
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
        /// Waits on the request and resolves the token.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to.</typeparam>
        /// <param name="request">The UnityWebRequest to send and wait on.</param>
        /// <param name="token">The token to resolve.</param>
        /// <param name="serialization"></param>
        /// <returns>The coroutines IEnumerator.</returns>
        protected IEnumerator Wait<T>(
            UnityWebRequest request,
            AsyncToken<HttpResponse<T>> token,
            SerializationType serialization = SerializationType.Json)
        {
            yield return request.Send();

            var response = new HttpResponse<T>
            {
                Headers = FormatHeaders(request.GetResponseHeaders()),
                StatusCode = request.responseCode
            };
            
            ProcessResponse(request, response, serialization);

            // must kill the request
            request.Dispose();

            token.Succeed(response);
        }

        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <typeparam name="T">The type to expect the response payload to be.</typeparam>
        /// <param name="request">The request to inspect.</param>
        /// <param name="response">The response object.</param>
        /// <param name="serialization">Describes how to serialize.</param>
        protected void ProcessResponse<T>(
            UnityWebRequest request,
            HttpResponse<T> response,
            SerializationType serialization)
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
                    switch (serialization)
                    {
                        case SerializationType.Json:
                        {
                            _serializer.Deserialize(typeof(T), ref bytes, out value);
                            break;
                        }
                        default:
                        {
                            value = bytes;
                            break;
                        }
                    }
                    
                    response.Payload = (T) value;

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