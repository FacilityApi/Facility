# Facility API Framework

The Facility API Framework consists of tools, libraries, and specifications that facilitate designing, implementing, documenting, and consuming web APIs.

## About

The Facility API Framework centers around the design and use of a [Facility Service Definition (FSD)](docs/Specification.md), which describes the operations supported by a service API.

An FSD is similar to an [Open API](https://openapis.org/) description, but it uses a different design philosophy—see [Goals](#goals) below.

## Languages and Frameworks

* C# (.NET)
* JavaScript
* Python
* Markdown
* ASP.NET

## Goals

The Facility API Framework is designed and built with the following goals in mind:

**Concise API specification.** A clear and concise specification is ideal for API designers, implementors, and consumers.

**Familiar file format.** The Facility Service Definition (FSD) file format uses a domain-specific language in an effort to make definitions easier to read and write, especially for developers comfortable with C-style languages. Alternative standardized formats (e.g. JSON, YAML) may be supported in the future.

**Interoperability with other API platforms.** Service definitions can imported from other API platforms such as [Open API](https://openapis.org/specification), which allows developers to leverage Facility tools without having to adopt a different specification format. We can also export definitions for those platforms so that developers can leverage the tools supported by those platforms.

**Keep it simple.** The FSD specification will be kept as simple as possible, even though that means that many APIs cannot be represented by it. Every new capability adds complexity to every tool that leverages an FSD, and a complex definition language can hinder the creation of new tools. Constraints can also free API designers from the paralysis of too many choices.

**Multiple languages, platforms, and frameworks.** A Facility Service Definition can be used to generate code in any programming language (C#, JavaScript, etc.) on any platform (Windows, Linux, etc.) for use with any API framework (ASP.NET, Node, etc.).

**Easy-to-write tooling.** The standard tools and libraries use C#, which can be used on Windows, Linux, and Mac. They work well with ASP.NET but can be used to generate code for any programming language, platform, or framework. Facility tools and libraries can be written in other languages as well.

**API documentation.** Tools can be used to generate accurate documentation from service definitions.

**Don't repeat yourself.** As much as possible, the details of an API should not have to be entered more than once. By generating data, code, and documentation from an FSD, we minimize the amount of repetition needed.

**Ongoing integration.** APIs change over time, so tools that permit ongoing integration of changes to an FSD are better than one-time scaffolding generators.

**Semantic versioning.** Integration of non-breaking changes to the API should not result in breaking changes to clients or service implementations. Tools can detect breaking and non-breaking changes to help version the API properly.

**Various API methodologies.** Facility can be used to build RESTful services, RPC services, and everything in between.

**JSON over HTTP.** Using JSON over HTTP makes it possible for existing libraries and tools to be used to consume, implement, and monitor Facility APIs.

**Flexible transport and encoding.** While optimized for JSON over HTTP, an FSD can be used with other encodings (e.g. Protocol Buffers) and transports (e.g. WebSockets), or even for simple in-process communication.
