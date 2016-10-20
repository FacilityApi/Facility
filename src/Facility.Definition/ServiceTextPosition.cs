using System;

namespace Facility.Definition
{
	/// <summary>
	/// A position in a named text source.
	/// </summary>
	public sealed class ServiceTextPosition
	{
		/// <summary>
		/// Creates a position.
		/// </summary>
		public ServiceTextPosition(string sourceName, int lineNumber = 0, int columnNumber = 0)
		{
			if (sourceName == null)
				throw new ArgumentNullException(nameof(sourceName));

			SourceName = sourceName;
			LineNumber = lineNumber;
			ColumnNumber = columnNumber;
		}

		/// <summary>
		/// The source name.
		/// </summary>
		public string SourceName { get; }

		/// <summary>
		/// The line number.
		/// </summary>
		public int LineNumber { get; }

		/// <summary>
		/// The column number.
		/// </summary>
		public int ColumnNumber { get; }

		/// <summary>
		/// The position as a source name, line number, and column number.
		/// </summary>
		public override string ToString()
		{
			if (ColumnNumber > 0)
				return $"{SourceName}({LineNumber},{ColumnNumber})";
			else if (LineNumber > 0)
				return $"{SourceName}({LineNumber})";
			else
				return SourceName;
		}
	}
}
