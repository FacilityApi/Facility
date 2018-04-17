using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Facility.Definition.CodeGen;
using Newtonsoft.Json;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Facility.Definition.Swagger
{
	/// <summary>
	/// Parses Swagger (OpenAPI) 2.0.
	/// </summary>
	public sealed class SwaggerParser
	{
		/// <summary>
		/// The service name (defaults to 'info/x-identifier' or 'info/title').
		/// </summary>
		public string ServiceName { get; set; }

		/// <summary>
		/// Parses Swagger (OpenAPI) 2.0 into a service definition.
		/// </summary>
		/// <exception cref="ServiceDefinitionException">Thrown if parsing fails or the service would be invalid.</exception>
		public ServiceInfo ParseDefinition(ServiceDefinitionText source)
		{
			if (TryParseDefinition(source, out var service, out var errors))
				return service;
			else
				throw new ServiceDefinitionException(errors);
		}

		/// <summary>
		/// Parses Swagger (OpenAPI) 2.0 into a service definition.
		/// </summary>
		/// <returns>True if parsing succeeds and the service is valid, i.e. there are no errors.</returns>
		/// <remarks>Even if parsing fails, an invalid service may be returned.</remarks>
		public bool TryParseDefinition(ServiceDefinitionText source, out ServiceInfo service, out IReadOnlyList<ServiceDefinitionError> errors)
		{
			service = null;

			if (string.IsNullOrWhiteSpace(source.Text))
			{
				errors = new[] { new ServiceDefinitionError("Service definition is missing.", new ServiceDefinitionPosition(source.Name, 1, 1)) };
				return false;
			}

			SwaggerService swaggerService;
			SwaggerParserContext context;

			if (!s_detectJsonRegex.IsMatch(source.Text))
			{
				// parse YAML
				var yamlDeserializer = new DeserializerBuilder()
					.IgnoreUnmatchedProperties()
					.WithNamingConvention(new OurNamingConvention())
					.Build();
				using (var stringReader = new StringReader(source.Text))
				{
					try
					{
						swaggerService = yamlDeserializer.Deserialize<SwaggerService>(stringReader);
					}
					catch (YamlException exception)
					{
						var errorMessage = exception.InnerException?.Message ?? exception.Message;
						const string errorStart = "): ";
						int errorStartIndex = errorMessage.IndexOf(errorStart, StringComparison.OrdinalIgnoreCase);
						if (errorStartIndex != -1)
							errorMessage = errorMessage.Substring(errorStartIndex + errorStart.Length);

						errors = new[] { new ServiceDefinitionError(errorMessage, new ServiceDefinitionPosition(source.Name, exception.End.Line, exception.End.Column)) };
						return false;
					}
				}

				if (swaggerService == null)
				{
					errors = new[] { new ServiceDefinitionError("Service definition is missing.", new ServiceDefinitionPosition(source.Name, 1, 1)) };
					return false;
				}

				context = SwaggerParserContext.FromYaml(source);
			}
			else
			{
				// parse JSON
				using (var stringReader = new StringReader(source.Text))
				using (var jsonTextReader = new JsonTextReader(stringReader))
				{
					try
					{
						swaggerService = JsonSerializer.Create(SwaggerUtility.JsonSerializerSettings).Deserialize<SwaggerService>(jsonTextReader);
					}
					catch (JsonException exception)
					{
						errors = new[] { new ServiceDefinitionError(exception.Message, new ServiceDefinitionPosition(source.Name, jsonTextReader.LineNumber, jsonTextReader.LinePosition)) };
						return false;
					}

					context = SwaggerParserContext.FromJson(source);
				}
			}

			var conversion = SwaggerConversion.Create(swaggerService, ServiceName, context);
			service = conversion.Service;
			errors = conversion.Errors;
			return errors.Count == 0;
		}

		/// <summary>
		/// Converts Swagger (OpenAPI) 2.0 into a service definition.
		/// </summary>
		/// <exception cref="ServiceDefinitionException">Thrown if the service would be invalid.</exception>
		public ServiceInfo ConvertSwaggerService(SwaggerService swaggerService)
		{
			if (TryConvertSwaggerService(swaggerService, out var service, out var errors))
				return service;
			else
				throw new ServiceDefinitionException(errors);
		}

		/// <summary>
		/// Attempts to convert Swagger (OpenAPI) 2.0 into a service definition.
		/// </summary>
		public bool TryConvertSwaggerService(SwaggerService swaggerService, out ServiceInfo service, out IReadOnlyList<ServiceDefinitionError> errors)
		{
			var conversion = SwaggerConversion.Create(swaggerService, ServiceName, SwaggerParserContext.None);
			service = conversion.Service;
			errors = conversion.Errors;
			return errors.Count == 0;
		}

		private sealed class OurNamingConvention : INamingConvention
		{
			public string Apply(string value)
			{
				if (value[0] >= 'A' && value[0] <= 'Z')
					value = CodeGenUtility.ToCamelCase(value);
				return value;
			}
		}

		static readonly Regex s_detectJsonRegex = new Regex(@"^\s*[{/]", RegexOptions.Singleline);
	}
}
