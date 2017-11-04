# Overview

`IHttpService` is the entry point into making Http requests in Unity. This interface is fairly low level, allowing you to configure every part of an Http request-- but it also provides some powerful abstractions to help make writing requests and responses quick and easy.

### Making a Request

To make a request, `IHttpService` provides methods for several Http verbs (we do not support some of the more esoteric verbs (HEAD, CONNECT, TRACE, etc). 

#### GET, DELETE

Here's an example of making a simple GET request:

```
_http
	.Get<MyResponse>(url)
	.OnSuccess(httpResponse => ...)
	.OnFailure(exception => ...);`
```

As you'll notice, the method is parameterized to the return type of the object. The `httpResponse` object returned wraps the `MyResponse` object with other information about the request, like `StatusCode` and `Headers`. The request is considered successful if the response status code is in the 200 range.

`httpResponse.Payload` is the typed response. In this case, `Payload` is a `MyResponse`.

The request is considered a failure for any other status code. This means that `OnFailure` will be called if there is some sort of network error OR if the Http transport was successful but the server returned a specialized error.

#### POST/PUT

POST and PUT requests also allow a body:

```
_http
	.Post<MyRespose>(url, new MyRequest(...))
	.OnSuccess(...)
	.OnFailure(...);
```

#### Authentication

Authentication is usually done via headers. In our case, we use the JWT standard. One a user has obtained a token, the `IHttpService` can be configured globally:

```
_http.Headers.Add(Tuple.Create("Authorization", "Bearer " + token));

// future requests will pass authentication
_http.Get(url)...
```

#### UrlBuilder

Generally, applications will not want to hard code URLs to make Http calls. Protocols, environments, ports, etc-- all of these are subject to change. Furthermore, users don't want to write the same `.Trim` methods over and over, making sure `/` are in the right place. To provide for these needs, the `IHttpService` contains a `UrlBuilder` object which automatically cleans and formats urls.

```
var builder = _http.UrlBuilder;
builder.BaseUrl = "my.baseurl.com";
builder.Port = 10206;
builder.version = "v2";
builder.Replacements.Add(Tuple.Create("userId", userId));
```

Once the builder is configured, you can use this object to build nice urls given only the endpoint.

```
_http
	.Get<MyResponse>(_http.UrlBuilder.Url("/user/{userId}/mail"))
	.OnSuccess(...)
	.OnFailiure(...);
```

# Further Reading

* [JWT](http://jwt.io)
* [Http Status Codes](https://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html)
* [Http Request Methods](https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html)

# Future Improvements

* Automatically push URL through `UrlBuilder`, but only if url does not have protocol on the front (in case you need to pass in another builder).
* Add `UrlBuilder.Url(string, replacements)`.
