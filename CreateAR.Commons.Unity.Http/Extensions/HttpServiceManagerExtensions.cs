using CreateAR.Commons.Unity.Http;
using UnityEngine.Networking;

/// <summary>
/// Helper extension methods for working with Unity requests.
/// </summary>
public static class HttpServiceManagerExtensions
{
    /// <summary>
    /// Processes the <see cref="UnityWebRequest"/> by formatting the endpoint url and applying
    /// any headers necessary.
    /// </summary>
    /// <returns>The name of the service used to process the request.</returns>
    public static string Process(this HttpServiceManager services, UnityWebRequest request)
    {
        var url = request.url;
        var service = services.Urls.FormatterName(url);

        // Format URL and Apply Headers
        request.url = services.Urls.Url(url);
        services.Headers.Apply(service, request);

        return service;
    }
}