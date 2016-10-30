using System.Globalization;

namespace Facility.Definition.Console
{
	/// <summary>
	/// Helper methods for common command-line arguments.
	/// </summary>
	public static class CommonArgs
	{
		/// <summary>
		/// Reads the help flag.
		/// </summary>
		public static bool ReadHelpFlag(this ArgsReader args)
		{
			return args.ReadFlag("help|h|?");
		}

		/// <summary>
		/// Reads the indent option.
		/// </summary>
		public static string ReadIndentOption(this ArgsReader args)
		{
			string value = args.ReadOption("indent");
			if (value == null)
				return null;

			if (value == "tab")
				return "\t";

			int spaceCount;
			if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out spaceCount) && spaceCount >= 1 && spaceCount <= 8)
				return new string(' ', spaceCount);

			throw new ArgsReaderException($"Invalid indent '{value}'. (Should be 'tab' or the number of spaces.)");
		}

		/// <summary>
		/// Reads the new line option.
		/// </summary>
		public static string ReadNewLineOption(this ArgsReader args)
		{
			string value = args.ReadOption("newline");
			if (value == null)
				return null;

			switch (value)
			{
			case "auto":
				return null;
			case "lf":
				return "\n";
			case "crlf":
				return "\r\n";
			default:
				throw new ArgsReaderException($"Invalid new line '{value}'. (Should be 'auto', 'lf', or 'crlf'.)");
			}
		}
	}
}
