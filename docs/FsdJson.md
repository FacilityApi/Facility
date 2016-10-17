# FSD JSON File Format

The FSD JSON file format is an alternative to the [FSD file format](FsdFile.md). It uses [JSON](http://www.json.org/) instead of a domain-specific language.

In order to simplify Facility tooling, it makes sense to create a JSON representation of a service definition, since most platforms have excellent JSON support. Having a single, standard parser that first converts an FSD file into JSON makes creating tools simpler.

As long as a JSON representation exists, it is reasonable that an API designer more comfortable with JSON than with a domain-specific language might want to use it directly.

Designers that prefer YAML to JSON should consider the [FSD YAML file format](FsdYaml.md).

## JSON File

Each FSD JSON file should contain the definition of one service. Typically, the file name will match the service name, followed by an `.fsd.json` file extension, e.g. `MyApi.fsd.json`.

## Root Object

The root document object has the following properties:

| Property | Type | Description |
| --- | --- | --- |
| `fsd` | string | The FSD version: `"1.0"`. |
| `service` | [Service Object](#service-object) | The service. |

## Service Object

The service object defines the [service](Specification.md#service).

| Property | Type | Description |
| --- | --- | --- |
| `name` | string | The service name. |
| `summary` | string | The service summary. |
| `attributes` | [ [Attribute Object](#attribute-object) ] | The service attributes. |
| `members` | [ [Member Object](#member-object) ] | The service members. |
| `remarks` | string | The service remarks. |

## Member Object

The member object defines a [method](Specification.md#methods), [data transfer object](Specification.md#data-transfer-objects), [enumerated type](Specification.md#enumerated-types), or [error set](Specification.md#error-sets).

| Property | Type | Description |
| --- | --- | --- |
| `kind` | string | The kind of member: `method`, `dto`, `enum` or `errorSet`. |
| `name` | string | The member name. |
| `summary` | string | The member summary. |
| `attributes` | [ [Attribute Object](#attribute-object) ] | The member attributes. |
| `requestFields` | [ [Field Object](#field-object) ] | The method request fields (when `kind` is `method`). |
| `responseFields` | [ [Field Object](#field-object) ] | The method response fields (when `kind` is `method`). |
| `fields` | [ [Field Object](#field-object) ] | The DTO fields (when `kind` is `dto`). |
| `values` | [ [Enum Value Object](#enum-value-object) ] | The enumerated type values (when `kind` is `enum`). |
| `errors` | [ [Error Object](#error-object) ] | The error set errors (when `kind` is `errorSet`). |
| `remarks` | string | The member remarks. |

## Field Object

A field object defines a [field](Specification.md#fields).

| Property | Type | Description |
| --- | --- | --- |
| `name` | string | The Property. |
| `type` | string | The [field](Specification.md#field-types). |
| `summary` | string | The field summary. |
| `attributes` | [ [Attribute Object](#attribute-object) ] | The field attributes. |

## Enum Value Object

An enum value object defines an [enumerated type value](Specification.md#enumerated-types).

| Property | Type | Description |
| --- | --- | --- |
| `name` | string | The enumerated type value name. |
| `summary` | string | The enumerated type value summary. |
| `attributes` | [ [Attribute Object](#attribute-object) ] | The enumerated type value attributes. |

## Error Object

An error object defines an [error set error](Specification.md#error-sets).

| Property | Type | Description |
| --- | --- | --- |
| `name` | string | The error name. |
| `summary` | string | The error summary. |
| `attributes` | [ [Attribute Object](#attribute-object) ] | The error attributes. |

## Attribute Object

An attribute object defines an [attribute](Specification.md#attributes).

| Property | Type | Description |
| --- | --- | --- |
| `name` | string | The attribute name. |
| `parameters` | [ [Attribute Parameter Object](#attribute-parameter-object) ] | The attribute parameters. |

## Attribute Parameter Object

An attribute parameter object defines an [attribute parameter](Specification.md#attributes).

| Property | Type | Description |
| --- | --- | --- |
| `name` | string | The attribute parameter name. |
| `value` | string | The attribute parameter value. |

## Example

The following is an example service definition in JSON. It is equivalent to the [FSD example](FsdFile.md#example) and the [FSD YAML example](FsdYaml.md#example).

### ExampleApi.fsd.json

```
...
```
