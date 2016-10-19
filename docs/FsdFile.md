# FSD File Format

The FSD file format uses a domain-specific language in an effort to make [Facility Service Definitions](Specification.md) easier to read and write, especially for developers comfortable with C-style languages.

## FSD File

Each FSD file should contain the definition of one service. Frequently the file name will match the service name followed by an `.fsd` file extension, e.g. `MyApi.fsd`.

An FSD file should use UTF-8 with no BOM.

## Service

The `service` keyword starts the definition of a [service](Specification.md#service). It is followed by the service name and optionally preceded by service attributes (see below). The methods and other definitions follow the service name, enclosed in braces.

```
service MyApi
{
  method myMethod { ... }: { ... }
  data MyData { ... }
  enum MyEnum : string { ... }
  ...
}
```

## Methods

The `method` keyword starts the definition of a [method](Specification.md#methods). It is followed by the method name and optionally preceded by method attributes.

The request and response follow the method name, each enclosed in braces and separated by a colon (`:`). The request and response fields are listed within the braces.

```
  method myMethod
  {
    name: string;
  }:
  {
    value: string;
  }
```

## Fields

A [field](Specification.md#fields) is represented by a name and a [field type](Specification.md#field-types), which are separated by a colon `:` and followed by a semicolon (`;`). Fields can also be preceded by field attributes.

## Data Transfer Objects

The `data` keyword starts the definition of a [data transfer object](Specification.md#data-transfer-objects) (DTO). It is followed by the name of the DTO and optionally preceded by data attributes.

```
  data MyData
  {
    id: int32;
    name: string;
  }
```

The fields of the DTO follow the name, enclosed in braces.

## Enumerated Types

The `enum` keyword starts the definition of an [enumerated type](Specification.md#enumerated-types). It is optionally preceded by enum attributes.

The enumerated values are comma-delimited alphanumeric names surrounded by braces. A final trailing comma is permitted.

Enumerated values are always transmitted as strings, not integers.

```
  enum MyEnum
  {
    first,
    second,
  }
```

## Error Sets

The `errors` keyword starts an [error set](Specification.md#error-sets). It is followed by the name of the error set and optionally preceded by attributes.

The error values are comma-delimited alphanumeric names surrounded by braces. A final trailing comma is permitted.

```
  errors MyErrors
  {
    NoBigDeal,
    EpicFail
  }
```

## Attributes

One or more [attributes](Specification.md#attributes) can be added before a service, method, DTO, field, enumerated type, enumerated value, error set, or error value.

Each attribute has an alphanumeric name and may optionally include one or more parameters. Each parameter has also has a name as well as a value, which can be a ASCII token or a JSON-style double-quoted string. An ASCII token can consist of numbers, letters, periods, hyphens, plus signs, and/or underscores. An ASCII token (such as an integer) is not semantically different than a double-quoted string containing that token.

An attribute is surrounded by square brackets, and its optional parameters are comma-delimited and specified in parentheses.

```
[myService] // no parameters
[something(name: q)] // one parameter
[meaning(of: "life", is: 42)] // two parameters
```

Multiple attributes can be comma-delimited within one set of square brackets and/or specified in separate sets of square brackets.

```
[attr1, attr2]
[attr3]
data MyData
{
}
```

## Comments

To add a comment to an FSD file, use `// this syntax`.

```
data MyData // this comment is ignored
{
  name: string; // so is this
  // and this
}
```

## Summary

A [summary](Specification.md#summary) is a special comment that appears in generated code and documentation. Comments with summaries use three slashes instead of two.

Multiple summary comments can be used for a single element of a service; newlines are automatically replaced with spaces.

Summaries are supported by services, methods, DTOs, fields, enumerated types, enumerated values, error sets, and error values.

```
/// My awesome data.
data MyData
{
  ...
}
```

## Remarks

Additional [remarks](Specification.md#remarks) using Markdown may be added after the end of the closing bracket of the `service`.

The first non-blank line immediately following the closing bracket must be a top-level [GitHub Flavored Markdown](https://guides.github.com/features/mastering-markdown/) heading (e.g. `# myMethod`).

That first heading as well as any additional top-level headings must match the name of the service or a method, DTO, enumerated type, or error set. Any text under that heading represents additional documentation for that part of the service.

```
service MyApi
{
  method myMethod { ... }: { ... }
  data MyData { ... }
  ...
}

# MyApi

These are the remarks for the entire service.

# myMethod

Here are the remarks for one of the service methods.

# MyData

Here are the remarks for one of the service DTOs.
```
