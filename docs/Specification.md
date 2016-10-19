# Facility Service Definition Specification

A Facility Service Definition (FSD) describes the operations supported by a service API.

This specification intentionally avoids discussion of file formats, since multiple file formats may be supported:
* [FSD File Format](FsdFile.md)

This specification also avoids discussion of transport (e.g. HTTP) and encoding (e.g. JSON), since an FSD can be used with any supported transport or encoding.
* [HTTP](Http.md)
* [JSON](Json.md)

Each Facility Service Definition defines one **service**.

## Service

Every service has a **name**. Unless otherwise noted, a name in this specification must start with an ASCII letter but may otherwise contain ASCII letters and/or numbers.

A service consists of **methods**, **data transfer objects**, **enumerated types**, and **error sets**. A service also supports **attributes**, **summary**, and **remarks** (see below).

A service with no methods is permitted. It could be used to define data transfer objects rather than the operations of an actual service.

## Methods

Each method represents an operation of the service.

Each method has a **name**, **request fields**, and **response fields**. A method also supports **attributes**, **summary**, and **remarks** (see below).

When a client invokes a service method, it provides values for some or all of the request fields. For example, a translation service could have a `translate` method with request fields named `text`, `sourceLanguage`, and `targetLanguage`.

If the method succeeds, values are returned for some or all of the response fields. A `translate` method might return the translated text in a `text` field and a confidence level in a `confidence` field.

If the method fails, an error is returned instead. An error consists of a machine-readable code, a human-readable message, and potentially other details (see below).

## Fields

A field stores data for a method request, method response, or data transfer object (see below).

Each field has a **name** and a **type**. The field type restricts type of data that can be stored in that field.

Fields are generally optional, i.e. they may or may not store a value.

## Field Types

The following primitive field types are supported:

* `string`: A string of zero or more Unicode characters.
* `boolean`: A Boolean value, i.e. true or false.
* `double`: A double-precision floating-point number.
* `int32`: A 32-bit signed integer.
* `int64`: A 64-bit signed integer.
* `bytes`: Zero or more bytes.
* `object`: An arbitrary JSON object.
* `error`: A [service error](#service-errors).

A field type can be any [data transfer object](#data-transfer-objects) or [enumerated type](#enumerated-types) in the service, referenced by name.

A field type can be a [service result](#service-results). Use `result<T>` to indicate a service result; for example, `result<Widget>` is a service result whose value is a DTO named `Widget`.

A field type can be an array, i.e. zero or more ordered items of a particular type, including primitive types, data transfer objects, enumerated types, or service results. Use `T[]` to indicate an array; for example, `int32[]` is an array of `int32`.

A field type can be a dictionary that maps strings to values of a particular type, including primitive types, data transfer objects, enumerated types, or service results. Use `map<T>` to indicate a map; for example, `map<int32>` is a map of strings to `int32`.

Arrays or maps of other arrays or maps are not permitted.

## Data Transfer Objects

Data transfer objects (DTOs) are used to combine simpler data types into a more complex data type.

Each data transfer object has a **name** and a collection of **fields**.

## Enumerated Types

An enumerated type is a string that is restricted to a set of named values.

An enumerated type has a **name** and a collection of **values**, each of which has a name.

The string stored by an enumerated type field should match the name of one of the values.

The value names of an enumerated type must be case-insensitively unique and may be matched case-insensitively but should nevertheless always be transmitted with the correct case.

## Service Errors

As mentioned above, a failed service method call returns a service error instead of a response. A service error can also be stored in an `error` field, or in a non-successful `result` field.

Each instance of a service error has a **code** and a **message**. It may also have **details** (an arbitrary JSON object) and an **inner error** (another service error instance).

The **code** is a machine-readable string that identifies the error. There are a number of standard error codes that should be used if possible:

* `InvalidRequest`: The request was invalid.
* `InternalError`: The service experienced an unexpected internal error.
* `InvalidResponse`: The service returned an unexpected response.
* `ServiceUnavailable`: The service is unavailable.
* `Timeout`: The service timed out.
* `NotAuthenticated`: The client must be authenticated.
* `NotAuthorized`: The authenticated client does not have the required authorization.
* `NotFound`: The specified item was not found.
* `NotModified`: The specified item was not modified.
* `Conflict`: A conflict occurred.
* `TooManyRequests`: The client has made too many requests.
* `RequestTooLarge`: The request is too large.

The **message** is a human-readable string that describes the error. It is usually intended for client developers, not end users.

The **details** object can be used however the service wants, though cross-service standards may emerge.

The **inner error** can be used to provide more information about what caused the error, especially if it was caused by a dependent service that failed.

## Service Results

 A service result (or an array of service results) can be used in response fields by methods that perform multiple operations and want to return separate success or failure for each one.

 An instance of a service result contains either a **value** of one of the defined DTO types or an **error** (see service errors above).

## Error Sets

A service that needs to support non-standard error codes can define its own error set, which supplements the standard error codes.

Each error set has a **name** and a collection of **values**, each of which has a name.

The name of each error set value represents a supported error code.

The documentation summary (see below) of each error set value is used as the default error message for that error code.

## Attributes

Attributes are used to attach additional information to a service and other service elements.

One or more attributes can be placed on the service, methods, DTOs, fields, enumerated types, enumerated type values, error sets, and/or error set values.

Each attribute has a **name** and can also have one or more **parameters**.

Each parameter has a **name** and a **value**, which can be a string or a 64-bit signed integer.

There are a few standard attributes:

* `obsolete`: Indicates that the service element is obsolete and/or deprecated and should no longer be used.
* `required`: Used on a field to indicate that the request, response, or DTO should be considered "invalid" if the field does not have a value.

## Summary

Most elements of a service support a **summary** string for documentation purposes: service, methods, DTOs, fields, enumerated types, enumerated type values, error sets, and error set values.

The summary should be short and consist of a single sentence or short paragraph.

## Remarks

Some elements also support **remarks**: service, methods, DTOs, enumerated types, and error sets.

The remarks can use [GitHub Flavored Markdown](https://guides.github.com/features/mastering-markdown/). They can be arbitrarily long and can include multiple paragraphs.
