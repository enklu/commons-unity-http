using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.DataStructures;

namespace CreateAR.Commons.Unity.Http.Editor
{
    public class EditorHttpService : IHttpService
    {
        protected enum SerializationType
        {
            Json,
            Raw
        }

        protected class HttpState
        {
            public HttpWebRequest Request;
            public Action<object> Resolve;
            public SerializationType Serialization;
        }

        /// <summary>
        /// Specifies content types.
        /// </summary>
        private const string CONTENT_TYPE_JSON = "application/json";

        private readonly ISerializer _serializer;

        public UrlBuilder UrlBuilder { get; }

        public List<Tuple<string, string>> Headers { get; }

        public EditorHttpService(ISerializer serializer)
        {
            _serializer = serializer;

            UrlBuilder = new UrlBuilder();
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

        public IAsyncToken<HttpResponse<T>> PostFile<T>(string url, IEnumerable<Tuple<string, string>> fields, ref byte[] file)
        {
            throw new System.NotImplementedException();
        }

        public IAsyncToken<HttpResponse<T>> PutFile<T>(string url, IEnumerable<Tuple<string, string>> fields, ref byte[] file)
        {
            throw new System.NotImplementedException();
        }

        public IAsyncToken<HttpResponse<byte[]>> Download(string url)
        {
            throw new System.NotImplementedException();
        }

        private IAsyncToken<HttpResponse<T>> SendJsonRequest<T>(
            HttpVerb verb,
            string url,
            object payload)
        {
            var token = new AsyncToken<HttpResponse<T>>();
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.ContentType = CONTENT_TYPE_JSON;
            request.Method = verb.ToString().ToUpperInvariant();
            
            ApplyHeaders(Headers, request);
            ApplyJsonPayload(payload, request);

            request.BeginGetResponse(
                Request_OnBeginGetResponse<T>,
                new HttpState
                {
                    Request = request,
                    Serialization = SerializationType.Json,
                    Resolve = response => token.Succeed((HttpResponse<T>) response)
                });

            return token;
        }

        private void Request_OnBeginGetResponse<T>(IAsyncResult result)
        {
            var state = (HttpState) result.AsyncState;
            var response = (HttpWebResponse) state.Request.EndGetResponse(result);

            var httpResponse = new HttpResponse<T>
            {
                Headers = FormatHeaders(response.Headers),
                StatusCode = (long) response.StatusCode
            };

            using (var stream = response.GetResponseStream())
            {
                var len = (int) stream.Length;
                var bytes = new byte[len];
                stream.Read(bytes, 0, len);

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

                httpResponse.Payload = (T) value;

                if (Successful((int) httpResponse.StatusCode))
                {
                    httpResponse.NetworkSuccess = true;
                }
                else
                {
                    httpResponse.NetworkSuccess = false;
                    httpResponse.NetworkError = Encoding.UTF8.GetString(bytes);
                }
            }

            state.Resolve(httpResponse);

            response.Close();
        }

        /// <summary>
        /// Applies headers to request.
        /// </summary>
        /// <param name="headers">Headers to add to request.</param>
        /// <param name="request">Request to add headers to.</param>
        protected static void ApplyHeaders(
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
                request.Headers.Add(kv.Item1, kv.Item2);
            }

            request.Headers.Add("Content-Type", CONTENT_TYPE_JSON);
            request.Headers.Add("Accept", CONTENT_TYPE_JSON);
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
