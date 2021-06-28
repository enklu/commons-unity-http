using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CreateAR.Commons.Unity.Async;
using UnityEngine;
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
        
        protected enum SerializationType
        {
            Json,
            Raw
        }

        /// <summary>
        /// Authentication is handled externally through headers, so the HttpService has to operate on a vague stance.
        /// </summary>
        protected enum AuthStance
        {
            /// <summary>
            /// Assumes everything is valid, requests pass through with no problems. Auth issues trigger a stance change.
            /// </summary>
            AssumeConfigured,
            
            /// <summary>
            /// Updated auth is required. Non authentication typed requests are pooled until stance is changed.
            /// </summary>
            RenegotiationRequired,
            
            /// <summary>
            /// Auth has failed, with no chance of renegotiation. Requests aren't pooled, and auth issues fail without triggering a stance change.
            /// </summary>
            Failed
        }

        /// <summary>
        /// A stored request to be handled later.
        /// </summary>
        protected struct RequestPoolRecord
        {
            /// <summary>
            /// Underlying Unity request.
            /// </summary>
            public UnityWebRequest Request;
            
            /// <summary>
            /// The response payload's type.
            /// </summary>
            public Type Type;
            
            /// <summary>
            /// The token to succeed/fail at renegotiation.
            /// </summary>
            public AsyncToken<HttpResponseTypeless> Token;
            
            /// <summary>
            /// The response serializtion type.
            /// </summary>
            public SerializationType Serialization;
        }

        /// <summary>
        /// Serializer!
        /// </summary>
        private readonly ISerializer _serializer;

        /// <summary>
        /// Bootstrapper implementation.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Requests.
        /// </summary>
        private readonly List<UnityWebRequest> _requestsOut = new List<UnityWebRequest>();

        /// <summary>
        /// Queue of would-fail requests while auth renegotiation is occuring.
        /// </summary>
        private readonly List<RequestPoolRecord> _authFailureQueue = new List<RequestPoolRecord>();

        /// <summary>
        /// Current auth stance.
        /// </summary>
        private AuthStance _authStance = AuthStance.AssumeConfigured;
        
        /// <inheritdoc/>
        public HttpServiceManager Services { get; }
        
        /// <inheritdoc />
        public long TimeoutMs { get; set; }

        /// <inheritdoc />
        public event Action<string, string, Dictionary<string, string>, object> OnRequest;

        /// <inheritdoc />
        public event Action<string> OnAuthenticationFailure;

        /// <summary>
        /// Creates an HttpService.
        /// 
        /// TODO: This class enforces a json contenttype, but accepts any
        /// TODO: ISerializer.
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="bootstrapper"></param>
        /// <param name="urls"></param>
        public HttpService(
            ISerializer serializer,
            IBootstrapper bootstrapper,
            UrlFormatterCollection urls)
        {
            _serializer = serializer;
            _bootstrapper = bootstrapper;

            Services = new HttpServiceManager(urls);
            TimeoutMs = 10000;
        }

        /// <inheritdoc />
        public void MarkAuthenticationUpdated()
        {
            _authStance = AuthStance.AssumeConfigured;
            
            // Push pooled calls back through the pipeline.
            var scratch = new List<RequestPoolRecord>(_authFailureQueue);
            _authFailureQueue.Clear();
            for (int i = 0, len = scratch.Count; i < len; i++)
            {
                var record = scratch[i];
                _bootstrapper.BootstrapCoroutine(Wait(record.Request, record.Type, record.Token, record.Serialization));
            }
        }

        /// <inheritdoc />
        public void MarkAuthenticationFailed()
        {
            _authStance = AuthStance.Failed;   
            
            // Fail existing queue.
            var scratch = new List<RequestPoolRecord>(_authFailureQueue);
            _authFailureQueue.Clear();
            for (int i = 0, len = scratch.Count; i < len; i++)
            {
                scratch[i].Token.Fail(new Exception("Unauthorized."));
            }
        }

        /// <inheritdoc />
        public void Abort()
        {
            foreach (var request in _requestsOut)
            {
                request.Abort();
                request.Dispose();
            }
            _requestsOut.Clear();
        }

        /// <inheritdoc />
        public IAsyncToken<HttpResponse<T>> Get<T>(string url)
        {
            return SendJsonRequest<T>(HttpVerb.Get, url, null);
        }

        /// <inheritdoc />
        public IAsyncToken<HttpResponse<T>> Post<T>(string url, object payload, RequestType type = RequestType.General)
        {
            return SendJsonRequest<T>(HttpVerb.Post, url, payload, type);
        }

        /// <inheritdoc />
        public IAsyncToken<HttpResponse<T>> Put<T>(string url, object payload)
        {
            return SendJsonRequest<T>(HttpVerb.Put, url, payload);
        }

        /// <inheritdoc />
        public IAsyncToken<HttpResponse<T>> Delete<T>(string url)
        {
            return SendJsonRequest<T>(HttpVerb.Delete, url, null);
        }

        /// <inheritdoc />
        public IAsyncToken<HttpResponse<T>> PostFile<T>(
            string url,
            IEnumerable<Tuple<string, string>> fields,
            ref byte[] file,
            int offset,
            int count,
            Action<float> progress = null)
        {
            return SendFile<T>(HttpVerb.Post, url, fields, ref file);
        }

        /// <inheritdoc />
        public IAsyncToken<HttpResponse<T>> PostFile<T>(
            string url, 
            IEnumerable<Tuple<string, string>> fields, 
            Stream file,
            Action<float> progress = null)
        {
            // Attempt to grab all data. Might not work in every situation.
            var data = new byte[file.Length];
            file.Read(data, 0, data.Length);
            return SendFile<T>(HttpVerb.Post, url, fields, ref data);
        }

        /// <inheritdoc />
        public IAsyncToken<HttpResponse<T>> PutFile<T>(
            string url,
            IEnumerable<Tuple<string, string>> fields,
            ref byte[] file,
            int offset,
            int count,
            Action<float> progressCallback = null)
        {
            return SendFile<T>(HttpVerb.Put, url, fields, ref file);
        }

        /// <inheritdoc />
        public IAsyncToken<HttpResponse<T>> PutFile<T>(
            string url, 
            IEnumerable<Tuple<string, string>> fields, 
            Stream file,
            Action<float> progressCallback = null)
        {
            // Attempt to grab all data. Might not work in every situation.
            var data = new byte[file.Length];
            file.Read(data, 0, data.Length);
            return SendFile<T>(HttpVerb.Put, url, fields, ref data);
        }

        /// <inheritdoc />
        public IAsyncToken<HttpResponse<byte[]>> Download(string url)
        {
            var token = new AsyncToken<HttpResponseTypeless>();

            var request = UnityWebRequest.Get(url);
            _requestsOut.Add(request);

            var service = Services.Process(request);

            // Log after Processing
            Log("GET", url, service);

            _bootstrapper.BootstrapCoroutine(Wait(
                request,
                typeof(byte[]),
                token,
                SerializationType.Raw));

            return GenerateTypedToken<byte[]>(token);
        }

        /// <summary>
        /// Sends a json request.
        /// </summary>
        /// <typeparam name="T">The type of response we expect.</typeparam>
        /// <param name="verb">The http verb to use.</param>
        /// <param name="url">The url to send the request to.</param>
        /// <param name="payload">The object that will be serialized into json.</param>
        /// <param name="service">Optional parameter which uses headers for a specific service instead of defaults.</param>
        /// <returns>An IAsyncToken to listen to.</returns>
        /// <exception cref="NullReferenceException"></exception>
        protected IAsyncToken<HttpResponse<T>> SendJsonRequest<T>(
            HttpVerb verb,
            string url,
            object payload,
            RequestType requestType = RequestType.General)
        {
            var token = new AsyncToken<HttpResponseTypeless>();

            var request = new UnityWebRequest(
                url,
                verb.ToString().ToUpperInvariant())
            {
                downloadHandler = new DownloadHandlerBuffer(),
                disposeDownloadHandlerOnDispose = true,
                disposeUploadHandlerOnDispose = true
            };

            var service = Services.Process(request);
            ApplyJsonPayload(payload, request);

            // Log after Processing
            Log(verb.ToString(), request.url, service, payload);

            _bootstrapper.BootstrapCoroutine(Wait(request, typeof(T), token, SerializationType.Json, requestType));
            
            return GenerateTypedToken<T>(token);
        }

        /// <summary>
        /// Sends a file!
        /// </summary>
        /// <typeparam name="T">The type of response we expect.</typeparam>
        /// <param name="verb">The http verb to use.</param>
        /// <param name="url">The url to send the request to.</param>
        /// <param name="fields">Optional fields that will _precede_ the file.</param>
        /// <param name="file">The file, which will be named "file".</param>
        /// <param name="service">Optional parameter which uses headers for a specific service instead of defaults.</param>
        /// <returns></returns>
        private IAsyncToken<HttpResponse<T>> SendFile<T>(
            HttpVerb verb,
            string url,
            IEnumerable<Tuple<string, string>> fields,
            ref byte[] file)
        {
            var token = new AsyncToken<HttpResponseTypeless>();

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
            request.useHttpContinue = false;
            request.downloadHandler = new DownloadHandlerBuffer();
            request.disposeDownloadHandlerOnDispose = true;
            request.disposeUploadHandlerOnDispose = true;

            var service = Services.Process(request);

            // Log after Processing
            Log(verb.ToString(), url, service);

            _bootstrapper.BootstrapCoroutine(Wait(request, typeof(T), token));

            return GenerateTypedToken<T>(token);
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
        /// <param name="type">The type expected of the payload.</param>
        /// <param name="token">The token to resolve.</param>
        /// <param name="serialization"></param>
        /// <returns>The coroutines IEnumerator.</returns>
        protected IEnumerator Wait(
            UnityWebRequest request,
            Type type,
            AsyncToken<HttpResponseTypeless> token,
            SerializationType serialization = SerializationType.Json,
            RequestType requestType = RequestType.General)
        {
            // Queue requests while an auth failure is on-going.
            if (_authStance == AuthStance.RenegotiationRequired && requestType != RequestType.Authentication)
            {
                _authFailureQueue.Add(new RequestPoolRecord
                {
                    Request = request,
                    Token = token,
                    Type = type,
                    Serialization = serialization
                });
                yield break;
            }
            
            var start = DateTime.Now;

            request.SendWebRequest();

            while (!request.isDone)
            {
                if (TimeoutMs > 0 && DateTime.Now.Subtract(start).TotalMilliseconds > TimeoutMs)
                {
                    // Handles cases where a request is outbound while auth fails.
                    if (_authStance == AuthStance.RenegotiationRequired && requestType != RequestType.Authentication)
                    {
                        _authFailureQueue.Add(new RequestPoolRecord
                        {
                            Request = request,
                            Token = token,
                            Type = type,
                            Serialization = serialization
                        });
                        yield break;
                    }
                    
                    // request timed out
                    request.Dispose();

                    token.Fail(new Exception("Request timed out."));

                    yield break;
                }

                yield return null;
            }

            // Race condition for multiple in-flight requests resolving while a failure first occurs.
            if (_authStance == AuthStance.RenegotiationRequired && requestType != RequestType.Authentication)
            {
                _authFailureQueue.Add(new RequestPoolRecord
                {
                    Request = request,
                    Token = token,
                    Type = type,
                    Serialization = serialization
                });
                    
                yield break;
            }
            
            // Auth failure? Start future request pooling.
            if (_authStance == AuthStance.AssumeConfigured && request.responseCode == 403)
            {
                _authStance = AuthStance.RenegotiationRequired;
                _authFailureQueue.Add(new RequestPoolRecord
                {
                    Request = request,
                    Token = token,
                    Type = type,
                    Serialization = serialization
                });
                OnAuthenticationFailure?.Invoke(request.error);
                yield break;
            }

            var response = new HttpResponseTypeless
            {
                Headers = FormatHeaders(request.GetResponseHeaders()),
                Type = type,
                StatusCode = request.responseCode
            };
            
            ProcessResponse(request, response, serialization);

            // must kill the request
            request.Dispose();
            _requestsOut.Remove(request);

            if (response.NetworkSuccess)
            {
                token.Succeed(response);
            }
            else
            {
                token.Fail(new Exception(response.NetworkError));
            }
        }

        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <typeparam name="T">The type to expect the response payload to be.</typeparam>
        /// <param name="request">The request to inspect.</param>
        /// <param name="response">The response object.</param>
        /// <param name="serialization">Describes how to serialize.</param>
        protected void ProcessResponse(
            UnityWebRequest request,
            HttpResponseTypeless response,
            SerializationType serialization)
        {
            if (request.isNetworkError)
            {
                response.NetworkSuccess = false;
                response.NetworkError = request.error;
            }
            else
            {
                var bytes = response.Raw = request.downloadHandler.data;
                try
                {
                    object value;
                    switch (serialization)
                    {
                        case SerializationType.Json:
                        {
                            _serializer.Deserialize(response.Type, ref bytes, out value);
                            break;
                        }
                        default:
                        {
                            value = bytes;
                            break;
                        }
                    }
                    
                    response.Payload = value;

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
        private List<Tuple<string, string>> FormatHeaders(IDictionary<string, string> headers)
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

        /// <summary>
        /// Outputs a log, if necessary.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <param name="uri">The uri.</param>
        /// <param name="payload">The payload.</param>
        private void Log(string verb, string uri, string service, object payload = null)
        {
            if (null != OnRequest)
            {
                OnRequest(verb, uri, Services.GetHeaders(service), payload);
            }
        }

        /// <summary>
        /// Generates a HttpResponse<T> token from a Typeless token.
        /// </summary>
        private AsyncToken<HttpResponse<T>> GenerateTypedToken<T>(IAsyncToken<HttpResponseTypeless> httpToken)
        {
            var rtnToken = new AsyncToken<HttpResponse<T>>();
            httpToken.OnFailure(rtnToken.Fail);
            httpToken.OnSuccess(response => rtnToken.Succeed(new HttpResponse<T>
            {
                Payload = (T) response.Payload,
                Raw = response.Raw,
                Headers = response.Headers,
                StatusCode = response.StatusCode,
                NetworkSuccess = response.NetworkSuccess,
                NetworkError = response.NetworkError
            }));
            return rtnToken;
        }
    }
}