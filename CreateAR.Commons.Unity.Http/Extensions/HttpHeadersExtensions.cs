using CreateAR.Commons.Unity.Http;
using UnityEngine.Networking;

/// <summary>
/// Extension methods for <see cref="HttpHeaders"/> to assist with Unity requests.
/// </summary>
public static class HttpHeadersExtensions
{
    /// <summary>
    /// Applies the headers from default and the specific service to the web request.
    /// </summary>
    /// <param name="service">The name of the service to resolve headers for.</param>
    /// <param name="request">The web request to apply the headers to.</param>
    public static void Apply(this HttpHeaders @this, string service, UnityWebRequest request)
    {
        var headers = @this.GetHeaders(service);

        // Apply Service Specific Headers
        foreach (var entry in headers)
        {
            request.SetRequestHeader(entry.Key, entry.Value);
        }
    }

    /// <summary>
    /// Applies the headers from default and the specific service to the web request.
    /// </summary>
    /// <param name="service">The name of the service to resolve headers for.</param>
    /// <param name="endpoint">The name of the endpoint to resolve headers for.</param>
    /// <param name="request">The web request to apply the headers to.</param>
    public static void Apply(this HttpHeaders @this, string service, string endpoint, UnityWebRequest request)
    {
        var headers = @this.GetHeaders(service, endpoint);

        // Apply Service.Endpoint Specific Headers
        foreach (var entry in headers)
        {
            request.SetRequestHeader(entry.Key, entry.Value);
        }
    }
}