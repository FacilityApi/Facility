using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.RepresentationModel;

namespace Facility.Definition.Swagger
{
	internal sealed class SwaggerParserContext
	{
		public static SwaggerParserContext None => new SwaggerParserContext(namedText: null, isYaml: false);

		public static SwaggerParserContext FromYaml(NamedText namedText) => new SwaggerParserContext(namedText, isYaml: true);

		public static SwaggerParserContext FromJson(NamedText namedText) => new SwaggerParserContext(namedText, isYaml: false);

		public SwaggerParserContext Root => new SwaggerParserContext(m_namedText, m_isYaml);

		public NamedTextPosition CreatePosition(string path = null) => m_namedText == null ? null : new NamedTextPosition(m_namedText.Name, () => FindLineColumn(path));

		public ServiceDefinitionException CreateException(string error, string path = null) => new ServiceDefinitionException(error, CreatePosition(path));

		public SwaggerParserContext CreateContext(string path) => new SwaggerParserContext(m_namedText, m_isYaml, ResolvePath(path));

		private SwaggerParserContext(NamedText namedText, bool isYaml, string path = null)
		{
			m_namedText = namedText;
			m_isYaml = isYaml;
			m_path = path;
		}

		private Tuple<int, int> FindLineColumn(string path)
		{
			if (m_isYaml)
			{
				var yamlStream = new YamlStream();
				using (var stringReader = new StringReader(m_namedText.Text))
					yamlStream.Load(stringReader);

				var node = yamlStream.Documents[0].RootNode;
				if (!string.IsNullOrEmpty(path))
				{
					foreach (var part in path.Split('/'))
					{
						if (!(node is YamlMappingNode mappingNode))
							break;

						mappingNode.Children.TryGetValue(new YamlScalarNode(part), out var childNode);
						if (childNode == null)
							break;

						node = childNode;
					}
				}

				return Tuple.Create(node.End.Line, node.End.Column);
			}
			else
			{
				JToken token = JToken.Parse(m_namedText.Text);

				if (!string.IsNullOrEmpty(path))
				{
					foreach (var part in path.Split('/'))
					{
						if (!(token is JObject jObject))
							break;

						var childToken = jObject[part];
						if (childToken == null)
							break;

						token = childToken;
					}
				}
				JToken pathToken = string.IsNullOrEmpty(path) ? token : token.SelectToken(ResolvePath(path));

				var lineInfo = (IJsonLineInfo) (pathToken ?? token);
				return Tuple.Create(lineInfo.LineNumber, lineInfo.LinePosition);
			}
		}

		private string ResolvePath(string path) => string.IsNullOrEmpty(path) ? m_path : string.IsNullOrEmpty(m_path) ? path : m_path + "." + path;

		readonly NamedText m_namedText;
		readonly bool m_isYaml;
		readonly string m_path;
	}
}
