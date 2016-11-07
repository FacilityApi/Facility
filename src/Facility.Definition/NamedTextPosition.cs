using System;

namespace Facility.Definition
{
	/// <summary>
	/// A position in a named text.
	/// </summary>
	public sealed class NamedTextPosition
	{
		/// <summary>
		/// Creates a position.
		/// </summary>
		public NamedTextPosition(string name, int lineNumber = 0, int columnNumber = 0)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			Name = name;
			LineNumber = lineNumber;
			ColumnNumber = columnNumber;
		}

		/// <summary>
		/// The source name.
		/// </summary>
		public string Name { get; }

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
				return $"{Name}({LineNumber},{ColumnNumber})";
			else if (LineNumber > 0)
				return $"{Name}({LineNumber})";
			else
				return Name;
		}
	}
}
