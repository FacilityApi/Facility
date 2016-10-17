# Facility API Framework Goals

The Facility API Framework is designed and built with the following goals in mind:

**Concise API specification.** A clear and concise specification is ideal for API designers, implementors, and consumers.

**Flexible API formats.** A Facility Service Definition (FSD) can be expressed in JSON, YAML, or with the FSD file format, which uses a domain-specific language in an effort to make definitions easier to read and write, especially for developers comfortable with C-style languages. Definitions can easily be converted among the supported formats.

**Interoperability with other API languages.** Service definitions can imported from other API platforms such as [Open API](https://openapis.org/specification), which allows developers to leverage Facility tools without having to adopt a different specification format. We can also export definitions for those platforms so that developers can leverage the tools supported by those platforms.

**Keep it simple.** The FSD specification will be kept as simple as possible, even though that means that many APIs cannot be represented by it. Every new capability adds complexity to every tool that leverages an FSD, and a complex definition language can hinder the creation of new tools. Constraints can also free API designers from the paralysis of too many choices.

**Multiple languages, platforms, and frameworks.** A Facility Service Definition can be used to generate code in any programming language (C#, JavaScript, etc.) on any platform (Windows, Linux, etc.) for use with any API framework (ASP.NET, Node, etc.).

**Easy-to-write tooling.** The standard tools and libraries use .NET Core, which runs on Windows, Linux, and Mac. They work well with ASP.NET but can be used to generate code for any programming language, platform, or framework. Facility tools and libraries can be written in other languages as well. Any service definition can be converted to JSON, which is easily leveraged by code generators or at run time.

**Don't repeat yourself.** As much as possible, the details of an API should not have to be entered more than once. By generating data, code, and documentation from an FSD, we minimize the amount of repetition needed.

**Ongoing integration.** APIs change over time, so tools that permit ongoing integration of changes to an FSD are better than one-time scaffolding generators.

**Semantic versioning.** Integration of non-breaking changes to the API should not result in breaking changes to clients or service implementations. Tools can detect breaking and non-breaking changes to help version the API properly.

**Various API methodologies.** Facility can be used to build RESTful services, RPC services, and everything in between.

**Flexible transport and encoding.** While optimized for JSON over HTTP, an FSD can be used with other encodings (e.g. Protocol Buffers) and transports (e.g. WebSockets), or even for simple in-process communication.
