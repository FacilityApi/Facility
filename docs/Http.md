# HTTP

Every service has a default HTTP mapping. The HTTP mapping can be customized by using the `http` attribute, which can be applied to services, methods, request fields, response fields, and errors, as documented below.

The `http` attribute is always optional. When the attribute is omitted, the defaults are used, as documented below.

The examples below use the [FSD file format](FsdFile.md).

## Services

On a service, the `url` parameter of the `http` attribute indicates the base URL where the HTTP service lives, e.g. `https://api.example.com/v1/`. The trailing slash is optional. Clients should still be able to use a different base URL; if this parameter is omitted, clients will be required to provide the base URL.

The `version` parameter indicates the version of the API. This specification does not indicate the format, though [semantic versioning](http://semver.org/) is recommended.

```
[http(url: "https://api.example.com/v1/", version: 1.0.4)]
service MyApi
{
  ...
}
```

## Methods

On a method, the `method` parameter of the `http` attribute indicates the HTTP method that is used, e.g. `GET`, `POST`, `PUT`, `DELETE`, or `PATCH`. If omitted, the default is `POST`.

The `path`  parameter indicates the HTTP path of the method (relative to the base URI). The path must start with a slash. A single slash indicates that the method is at the base URL itself.

For example, if a method uses `[http(method: GET, path: "/widgets"]` in a service that uses `[http(url: "https://api.example.com/v1/"]`, the full HTTP method and path for that method would be `GET https://api.example.com/v1/widgets`.

If the `path` parameter is not specified, it defaults to the method name, e.g. `/getWidgets` for a method named `getWidgets`. This default would not be appropriate for a RESTful API, but may be acceptable for an RPC-style API.

The `code` parameter indicates the HTTP status code used if the method is successful (but see also *body fields* below). If omitted, it defaults to `200` (OK), or to `204` (No Content) if the response has no normal or body fields.

```
  [http(method: POST, path: "/widgets", code: 201)]
  method createWidget
  {
    ...
  }:
  {
    ...
  }
```

## Request/Response Fields

On a request or response field, the `from` parameter of the `http` attribute indicates where the field comes from. It can be set to `path`, `query`, `body`, `header`, or `normal`.

The `http` attribute should not be used on a DTO field.

### Path Fields

If `from: path` is used on a request field, the field comes from the method path, which must contain the field name in braces. (Response fields cannot be path fields.)

The `name` parameter of the `http` attribute can be used to indicate the name of the field as found in the path; if omitted, it defaults to the field name.

If the name of a request field without a `from` parameter is found in the path, it is assumed to be a path field, so `from: path` is rarely used explicitly.

For example, a `getWidget` method might have a `/widgets/{id}` path and a corresponding `id` request field.

```
  [http(method: GET, path: "/widgets/{id}")]
  method getWidget
  {
    id: string;
  }:
  {
    ...
  }
```

### Query Fields

If `from: query` is used on a request field, that field value comes from the query string. (Response fields cannot be query fields.)

The `name` parameter of the `http` attribute can be used to indicate the name of the field as found in the query string; if omitted, it defaults to the field name.

If a request field of an `method: GET` method is not a path field and it has no other `http` attributes, it is assumed to be a query field. For non-`GET` methods like `POST`, `from: query` is always needed to identify query fields.

The following example uses two query fields, e.g. `GET https://api.example.com/v1/widgets?q=blue&limit=10`.

```
  [http(method: GET, path: "/widgets")]
  method getWidgets
  {
    [http(name: "q")] query: string;
    limit: int32;
  }:
  {
    ...
  }
```

### Body Fields

If `from: body` is used on a request or response field, the field value comprises the entire request or response body. The name of the field is not used by the HTTP mapping.

The `code` parameter of the `http` attribute can be used on a body field of a response to indicate the HTTP status code used if the method is successful. If omitted, it defaults to the status code of the method.

In the response, multiple fields can use `from: body` to indicate multiple possible response bodies. Each field should use a different `code`.

The field type of a body field should generally be a DTO. A `boolean` body field can be used to indicate an empty response; when used, it is set to `true`.

```
  [http(path: "/widgets", code: 201)]
  method createWidget
  {
    [http(from: body)]
    widget: Widget;
  }:
  {
    [http(from: body)]
    widget: Widget;
  }
```

### Header Fields

If `from: header` is used on a request or response field, the field is transmitted via HTTP header.

The `name` parameter of the `http` attribute can be used to indicate the name of the HTTP header; if omitted, it defaults to the field name.

Header fields should be of type `string`. The header value is not transformed in any way and must conform to the HTTP requirements for that header.

Headers commonly used by all service methods (`Authorization`, `User Agent`, etc.) are generally outside the scope of the FSD and not explicitly included with each request and/or response.

```
  [http(method: GET, path: "/widgets/{id}")]
  method getWidget
  {
    id: string;

    [http(from: header, name: If-None-Match)]
    ifNotETag: string;
  }:
  {
    [http(from: header)]
    eTag: string;

    ...
  }
```

### Normal Fields

If `from: normal` is used on a request or response field, the field is a normal part of the request or response body.

Except as indicated above, request and response fields are assumed to be normal fields, so `from: normal` is rarely used explicitly.

A method response may have both normal fields and body fields, in which case the set of normal fields is used by a different status code than any of the body fields.

```
  [http(method: POST, path: "/widgets/search")]
  method searchWidgets
  {
    query: Query;
    limit: int32;
    offset: int32;
  }:
  {
    items: Widget[];
    more: boolean;
  }
```

## Errors

On an error of an error set, the `code` parameter of the `http` attribute is used to specify the HTTP status code that should be used when that error is returned, e.g. `404`.

If the `code` parameter is missing from an error, `500` (Internal Server Error) is used.

The standard error codes already have reasonable status codes:

* `InvalidRequest`: 400
* `InternalError`: 500
* `InvalidResponse`: 500
* `ServiceUnavailable`: 503
* `Timeout`: 500
* `NotAuthenticated`: 401
* `NotAuthorized`: 403
* `NotFound`: 404
* `NotModified`: 304
* `Conflict`: 409
* `TooManyRequests`: 429
* `RequestTooLarge`: 413

```
  errors MyErrors
  {
    [http(code: 503)]
    OutToLunch
  }
```
