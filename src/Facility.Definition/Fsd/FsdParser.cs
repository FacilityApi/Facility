using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Faithlife.Parsing;

namespace Facility.Definition.Fsd
{
	/// <summary>
	/// Parses FSD files.
	/// </summary>
	public sealed class FsdParser
	{
		/// <summary>
		/// Parses an FSD file into a service definition.
		/// </summary>
		public ServiceInfo ParseDefinition(ServiceTextSource source)
		{
			IReadOnlyList<string> definitionLines = null;
			var remarksSections = new Dictionary<string, FsdRemarksSection>(StringComparer.OrdinalIgnoreCase);

			// read remarks after definition
			using (var reader = new StringReader(source.Text))
			{
				string name = null;
				var lines = new List<string>();
				int lineNumber = 0;
				int headingLineNumber = 0;

				while (true)
				{
					string line = reader.ReadLine();
					lineNumber++;

					Match match = line == null ? null : s_markdownHeading.Match(line);
					if (match == null || match.Success)
					{
						if (name == null)
						{
							definitionLines = lines;
						}
						else
						{
							while (lines.Count != 0 && string.IsNullOrWhiteSpace(lines[0]))
								lines.RemoveAt(0);
							while (lines.Count != 0 && string.IsNullOrWhiteSpace(lines[lines.Count - 1]))
								lines.RemoveAt(lines.Count - 1);

							var position = new ServiceTextPosition(source.Name, headingLineNumber, 1);
							if (remarksSections.ContainsKey(name))
								throw new ServiceDefinitionException("Duplicate remarks heading: " + name, position);
							remarksSections.Add(name, new FsdRemarksSection(name, lines, position));
						}

						if (match == null)
							break;

						name = line.Substring(match.Index + match.Length).Trim();
						lines = new List<string>();
						headingLineNumber = lineNumber;
					}
					else
					{
						lines.Add(line);
					}
				}
			}

			source = new ServiceTextSource(source.Name, string.Join("\n", definitionLines));

			ServiceInfo service;
			try
			{
				service = FsdParsers.ParseDefinition(source, remarksSections);
			}
			catch (ParseException exception)
			{
				var expectation = exception
					.Result
					.GetNamedFailures()
					.Distinct()
					.GroupBy(x => x.Position)
					.Select(x => new { LineColumn = x.Key.GetLineColumn(), Names = x.Select(y => y.Name) })
					.OrderByDescending(x => x.LineColumn.LineNumber)
					.ThenByDescending(x => x.LineColumn.ColumnNumber)
					.First();

				throw new ServiceDefinitionException(
					"expected " + string.Join(" or ", expectation.Names.Distinct().OrderBy(GetExpectationNameRank).ThenBy(x => x, StringComparer.Ordinal)),
					new ServiceTextPosition(source.Name, expectation.LineColumn.LineNumber, expectation.LineColumn.ColumnNumber),
					exception);
			}

			// check for unused remarks sections
			foreach (var remarksSection in remarksSections.Values)
			{
				string sectionName = remarksSection.Name;
				if (service.Name != sectionName && service.FindMember(sectionName) == null)
					throw new ServiceDefinitionException($"Unused remarks heading: {sectionName}", remarksSection.Position);
			}

			return service;
		}

		private static int GetExpectationNameRank(string name)
		{
			return name == "')'" || name == "']'" || name == "'}'" || name == "';'" ? 1 : 2;
		}

		static readonly Regex s_markdownHeading = new Regex(@"^#\s+");
	}
}
