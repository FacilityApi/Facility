using System;

namespace Facility.Definition.CodeGen
{
	/// <summary>
	/// An exception thrown by a code generator.
	/// </summary>
	public sealed class CodeGeneratorException : Exception
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public CodeGeneratorException(string message)
			: base(message)
		{
		}
	}
}
