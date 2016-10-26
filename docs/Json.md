# JSON

Since [JSON](http://www.json.org/) is currently the most commonly-used serialization format for APIs over HTTP, field definitions are designed to be trivially compatible with JSON.

In fact, to avoid complicating implementations, there is no way to customize the JSON serialization of a request body, response body, or DTO. Each field is always serialized as a JSON property with the same name.

* `string`, `boolean`, `double`, `int32`, and `int64` are encoded as JSON literals.
* `bytes` are encoded as a [Base64](https://en.wikipedia.org/wiki/Base64) string.
* `object` is encoded as a JSON object.
* `error` is encoded as a JSON object with `code`, `message`, `details`, and `innerError` properties.
* `result<T>` is encoded as a JSON object with a `value` or `error` property.
* `T[]` is encoded as a JSON array.
* `map<T>` is encoded as a JSON object.

`null` is not a valid value for any field type. If a JSON property is set to `null`, it is treated as though it were omitted. Arrays and maps are not permitted to have `null` items.

When reading JSON, conforming clients and servers may match property names case-insensitively, and they may perform type conversions for property values, e.g. strings to numbers. They may also support non-standard JSON features, such as unquoted property names, single-quoted strings, comments, etc. However, when writing JSON, conforming clients and servers must always use [standard JSON](http://www.json.org/) with no comments, correctly-cased property names, correctly-typed property values, etc.
