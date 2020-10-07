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
	public sealed class FsdParser : ServiceParser
	{
		/// <summary>
		/// Implements TryParseDefinition.
		/// </summary>
		protected override bool TryParseDefinitionCore(ServiceDefinitionText source, out ServiceInfo? service, out IReadOnlyList<ServiceDefinitionError> errors)
		{
			var errorList = new List<ServiceDefinitionError>();
			var definitionLines = new List<string>();
			var remarksSectionsByName = new Dictionary<string, FsdRemarksSection>(StringComparer.OrdinalIgnoreCase);
			var interleavedRemarksSections = new List<FsdRemarksSection>();

			if (!s_interleavedMarkdown.IsMatch(source.Text))
				ReadRemarksAfterDefinition(source, definitionLines, remarksSectionsByName, errorList);
			else
				ReadInterleavedRemarks(source, definitionLines, interleavedRemarksSections);

			source = new ServiceDefinitionText(source.Name, string.Join("\n", definitionLines));
			service = null;

			try
			{
				service = FsdParsers.ParseDefinition(source, remarksSectionsByName);
				errorList.AddRange(service.GetValidationErrors());

				// check for unused remarks sections
				foreach (var remarksSectionPair in remarksSectionsByName)
				{
					var sectionName = remarksSectionPair.Key;
					if (service.Name != sectionName && service.FindMember(sectionName) == null)
						errorList.Add(new ServiceDefinitionError($"Unused remarks heading: {sectionName}", remarksSectionPair.Value.Position));
				}

				// check for interleaved remarks sections
				foreach (var remarksSection in interleavedRemarksSections)
				{
					var remarksLineNumber = remarksSection.Position.LineNumber;
					if (remarksLineNumber > service.GetPart(ServicePartKind.Name)!.Position.LineNumber &&
						remarksLineNumber < service.GetPart(ServicePartKind.End)!.Position.LineNumber)
					{
						ServiceMemberInfo targetMember = service;
						var targetLineNumber = 0;
						foreach (var member in service.Members)
						{
							var memberLineNumber = member.GetPart(ServicePartKind.Name)!.Position.LineNumber;
							if (remarksLineNumber > memberLineNumber && memberLineNumber > targetLineNumber)
							{
								targetMember = member;
								targetLineNumber = memberLineNumber;
							}
						}

						if (targetMember.Remarks.Count == 0)
							targetMember.Remarks = remarksSection.Lines;
						else
							targetMember.Remarks = targetMember.Remarks.Concat(new[] { "" }).Concat(remarksSection.Lines).ToList();
					}
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

				int GetExpectationNameRank(string name) => name == "')'" || name == "']'" || name == "'}'" || name == "';'" ? 1 : 2;
				errorList.Add(new ServiceDefinitionError(
					"expected " + string.Join(" or ", expectation.Names.Distinct().OrderBy(GetExpectationNameRank).ThenBy(x => x, StringComparer.Ordinal)),
					new ServiceDefinitionPosition(source.Name, expectation.LineColumn.LineNumber, expectation.LineColumn.ColumnNumber)));
			}

			errors = errorList;
			return errorList.Count == 0;
		}

		private static void ReadInterleavedRemarks(ServiceDefinitionText source, List<string> definitionLines, List<FsdRemarksSection> remarksSections)
		{
			using var reader = new StringReader(source.Text);

			var remarksLines = new List<string>();
			var inFsdCode = false;

			while (true)
			{
				var line = reader.ReadLine();
				if (line == null)
				{
					AddRemarksSection();
					break;
				}

				if (inFsdCode)
				{
					if (line.StartsWith("```", StringComparison.Ordinal))
					{
						inFsdCode = false;
						definitionLines.Add("");
					}
					else
					{
						definitionLines.Add(line);
					}
				}
				else
				{
					if (s_interleavedMarkdown.IsMatch(line))
					{
						AddRemarksSection();
						inFsdCode = true;
					}
					else
					{
						remarksLines.Add(line);
					}

					definitionLines.Add("");
				}
			}

			void AddRemarksSection()
			{
				while (remarksLines.Count != 0 && string.IsNullOrWhiteSpace(remarksLines[0]))
					remarksLines.RemoveAt(0);

				var remarksLineNumber = definitionLines.Count - remarksLines.Count;

				while (remarksLines.Count != 0 && string.IsNullOrWhiteSpace(remarksLines[remarksLines.Count - 1]))
					remarksLines.RemoveAt(remarksLines.Count - 1);

				if (remarksLines.Count != 0)
				{
					var position = new ServiceDefinitionPosition(source.Name, remarksLineNumber, 1);
					remarksSections.Add(new FsdRemarksSection(remarksLines, position));
					remarksLines = new List<string>();
				}
			}
		}

		private static void ReadRemarksAfterDefinition(ServiceDefinitionText source, List<string> definitionLines, Dictionary<string, FsdRemarksSection> remarksSections, List<ServiceDefinitionError> errorList)
		{
			using var reader = new StringReader(source.Text);

			string? name = null;
			var lines = new List<string>();
			var lineNumber = 0;
			var headingLineNumber = 0;

			while (true)
			{
				var line = reader.ReadLine();
				lineNumber++;

				var match = line == null ? null : s_markdownHeading.Match(line);
				if (match == null || match.Success)
				{
					if (name == null)
					{
						definitionLines.AddRange(lines);
					}
					else
					{
						while (lines.Count != 0 && string.IsNullOrWhiteSpace(lines[0]))
							lines.RemoveAt(0);
						while (lines.Count != 0 && string.IsNullOrWhiteSpace(lines[lines.Count - 1]))
							lines.RemoveAt(lines.Count - 1);

						var position = new ServiceDefinitionPosition(source.Name, headingLineNumber, 1);
						if (remarksSections.ContainsKey(name))
							errorList.Add(new ServiceDefinitionError("Duplicate remarks heading: " + name, position));
						else
							remarksSections.Add(name, new FsdRemarksSection(lines, position));
					}

					if (match == null)
						break;

					name = line!.Substring(match.Index + match.Length).Trim();
					lines = new List<string>();
					headingLineNumber = lineNumber;
				}
				else
				{
					lines.Add(line!);
				}
			}
		}

		private static readonly Regex s_interleavedMarkdown = new Regex(@"^```fsd\b", RegexOptions.Multiline);
		private static readonly Regex s_markdownHeading = new Regex(@"^#\s+");
	}
}
