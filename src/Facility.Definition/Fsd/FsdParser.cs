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
		/// <exception cref="ServiceDefinitionException">Thrown if the parse fails.</exception>
		public ServiceInfo ParseDefinition(NamedText source)
		{
			ServiceInfo service;
			IReadOnlyList<ServiceDefinitionError> errors;
			if (TryParseDefinition(source, out service, out errors))
				return service;
			else
				throw errors.First().CreateException();
		}

		/// <summary>
		/// Parses an FSD file into a service definition.
		/// </summary>
		/// <returns>true if the parse succeeds.</returns>
		public bool TryParseDefinition(NamedText source, out ServiceInfo service, out IReadOnlyList<ServiceDefinitionError> errorList)
		{
			var errors = new List<ServiceDefinitionError>();
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

							var position = new NamedTextPosition(source.Name, headingLineNumber, 1);
							if (remarksSections.ContainsKey(name))
								errors.Add(new ServiceDefinitionError("Duplicate remarks heading: " + name, position));
							else
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

			source = new NamedText(source.Name, string.Join("\n", definitionLines));
			service = null;

			try
			{
				service = FsdParsers.ParseDefinition(source, remarksSections, shouldValidate: false);
				errors.AddRange(service.Validate());

				// check for unused remarks sections
				foreach (var remarksSection in remarksSections.Values)
				{
					string sectionName = remarksSection.Name;
					if (service.Name != sectionName && service.FindMember(sectionName) == null)
						errors.Add(new ServiceDefinitionError($"Unused remarks heading: {sectionName}", remarksSection.Position));
				}
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

				errors.Add(new ServiceDefinitionError(
					"expected " + string.Join(" or ", expectation.Names.Distinct().OrderBy(GetExpectationNameRank).ThenBy(x => x, StringComparer.Ordinal)),
					new NamedTextPosition(source.Name, expectation.LineColumn.LineNumber, expectation.LineColumn.ColumnNumber),
					exception));
			}
			catch (ServiceDefinitionException exception)
			{
				errors.Add(new ServiceDefinitionError(exception.Error, exception.Position, exception));
			}

			errorList = errors;
			return errors.Count == 0;
		}

		private static int GetExpectationNameRank(string name)
		{
			return name == "')'" || name == "']'" || name == "'}'" || name == "';'" ? 1 : 2;
		}

		static readonly Regex s_markdownHeading = new Regex(@"^#\s+");
	}
}
