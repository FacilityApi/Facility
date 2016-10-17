# FSD YAML File Format

The FSD YAML file format is an alternative to the [FSD file format](FsdFile.md). It uses [YAML](http://yaml.org/) instead of a domain-specific language.

Some API designers prefer YAML to JSON, and since YAML is a superset of JSON, it is simple to support YAML as well.

## YAML File

Each FSD YAML file should contain the definition of one service. Typically, the file name will match the service name, followed by an `.fsd.yaml` file extension, e.g. `MyApi.fsd.yaml`.

The object definitions are exactly the same as [FSD JSON](FsdJson.md), so they are not repeated here.

## Example

The following is an example service definition in YAML. It is equivalent to the [FSD example](FsdFile.md#example) and the [FSD JSON example](FsdJson.md#example).

### ExampleApi.fsd.yaml

```
...
```
