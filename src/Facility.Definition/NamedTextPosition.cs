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
			m_lineNumber = lineNumber;
			m_columnNumber = columnNumber;
		}

		/// <summary>
		/// Creates a position.
		/// </summary>
		public NamedTextPosition(string name, Func<Tuple<int, int>> getLineColumn)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			Name = name;
			m_getLineColumn = getLineColumn;
		}

		/// <summary>
		/// The source name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The line number.
		/// </summary>
		public int LineNumber
		{
			get
			{
				EnsureLineColumn();
				return m_lineNumber;
			}
		}

		/// <summary>
		/// The column number.
		/// </summary>
		public int ColumnNumber
		{
			get
			{
				EnsureLineColumn();
				return m_columnNumber;
			}
		}

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

		private void EnsureLineColumn()
		{
			if (m_getLineColumn != null)
			{
				var tuple = m_getLineColumn();
				m_lineNumber = tuple.Item1;
				m_columnNumber = tuple.Item2;
				m_getLineColumn = null;
			}
		}

		int m_lineNumber;
		int m_columnNumber;
		Func<Tuple<int, int>> m_getLineColumn;
	}
}
