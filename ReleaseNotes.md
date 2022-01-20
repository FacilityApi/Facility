# Release Notes

## 2.7.1

* Fix build that didn't work properly on .NET 5 or .NET Core 3.1.

## 2.7.0

* Add .NET 6 support to `fsdgenfsd`.
* Drop .NET Core 2.1 support from `fsdgenfsd`.

## 2.6.1

* Fixed range-valued `[validate]` parsing
* Fixed an error message for invalid `[validate]` use on collections

## 2.6.0

* Introduce attribute based validation via `[validate]`

## 2.5.0

* Support content type on body fields via `type` property of `http` attribute.

## 2.4.3

* Remove unused package reference to `Newtonsoft.Json`.

## 2.4.2

* Update dependencies.

## 2.4.1

* Fix exception when console app output path has no directory information.

## 2.4.0

* Introduce and throw `CodeGeneratorException`.
* Rename overridden method parameter.
* Add .NET 5 support to `fsdgenfsd`.

## 2.3.0

* Support custom parser in code generator app.

## 2.2.2

* Add .NET Core App 3.1 support to `fsdgenfsd`.
* A few nullable reference improvements.

## 2.2.1

* Introduce common `HttpFieldInfo` base class.

## 2.2.0

* Support shorthand for required attribute, e.g. `string!`.

## 2.1.0

* Support nullable references.
* Only ignore newlines when verifying codegen.

## 2.0.2

* **Breaking:** Drop support for arbitrary HTTP methods (to help detect typos).
* **Breaking:** Upgrade to .NET Standard 2.0 and .NET 4.7. Upgrade NuGet dependencies.
* **Breaking:** Stop using System.Net.Http.HttpMethod.
* **Breaking:** Change `fsdgenfsd` to .NET Core tool.
* **Breaking:** Refactor code generator app framework.
* Support tags via attribute: `[tags(name: shiny)]`
* Support tag exclusion via command-line: `--exclude-tags shiny`
* Report multiple definition errors from command-line tools.
* Improve http attribute errors.
* Allow arrays in path and query fields.
* Allow non-strings in header fields.
* Allow bytes and strings in body fields.
* Prohibit duplicate file names in output.
* Support definitions interleaved within Markdown.
* Braces are optional around service members.
* Support static `FsdGenerator.GenerateFsd` for C# build scripts.
* Use kebab case for multi-word command-line options.
* Support `[required]` field attribute.

## 1.5.0

* Start tracking version history.
