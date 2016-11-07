using System;

namespace Facility.Definition.CodeGen
{
	/// <summary>
	/// A pattern for generated output.
	/// </summary>
	public sealed class CodeGenPattern
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public CodeGenPattern(string namePattern, string requiredSubstring)
		{
			if (namePattern == null)
				throw new ArgumentNullException(nameof(namePattern));
			if (requiredSubstring == null)
				throw new ArgumentNullException(nameof(requiredSubstring));

			NamePattern = namePattern;
			RequiredSubstring = requiredSubstring;
		}

		/// <summary>
		/// The name pattern.
		/// </summary>
		public string NamePattern { get; }

		/// <summary>
		/// The required substring (empty if none).
		/// </summary>
		public string RequiredSubstring { get; }
	}
}
