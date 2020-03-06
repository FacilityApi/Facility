using System;

namespace Facility.Definition
{
	/// <summary>
	/// A position in a service definition text.
	/// </summary>
	public sealed class ServiceDefinitionPosition
	{
		/// <summary>
		/// Creates a position.
		/// </summary>
		public ServiceDefinitionPosition(string name, int lineNumber = 0, int columnNumber = 0)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			m_lineColumn = (lineNumber, columnNumber);
		}

		/// <summary>
		/// Creates a position.
		/// </summary>
		public ServiceDefinitionPosition(string name, Func<(int LineNumber, int ColumnNumber)> getLineColumn)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			m_getLineColumn = getLineColumn;
		}

		/// <summary>
		/// The source name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The line number.
		/// </summary>
		public int LineNumber => GetLineColumn().LineNumber;

		/// <summary>
		/// The column number.
		/// </summary>
		public int ColumnNumber => GetLineColumn().ColumnNumber;

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

		private (int LineNumber, int ColumnNumber) GetLineColumn()
		{
			if (m_getLineColumn != null)
			{
				m_lineColumn = m_getLineColumn();
				m_getLineColumn = null;
			}

			return m_lineColumn;
		}

		private (int LineNumber, int ColumnNumber) m_lineColumn;
		private Func<(int LineNumber, int ColumnNumber)>? m_getLineColumn;
	}
}
