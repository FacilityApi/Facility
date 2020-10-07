using System.Collections.Generic;
using System.Globalization;
using ArgsReading;

namespace Facility.CodeGen.Console
{
	internal static class CommonArgs
	{
		public static bool ReadCleanFlag(this ArgsReader args) => args.ReadFlag("clean");

		public static bool ReadDryRunFlag(this ArgsReader args) => args.ReadFlag("dry-run");

		public static bool ReadHelpFlag(this ArgsReader args) => args.ReadFlag("help|h|?");

		public static bool ReadQuietFlag(this ArgsReader args) => args.ReadFlag("quiet");

		public static bool ReadVerifyFlag(this ArgsReader args) => args.ReadFlag("verify");

		public static string? ReadIndentOption(this ArgsReader args)
		{
			var value = args.ReadOption("indent");
			if (value == null)
				return null;

			if (value == "tab")
				return "\t";

			if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var spaceCount) && spaceCount >= 1 && spaceCount <= 8)
				return new string(' ', spaceCount);

			throw new ArgsReaderException($"Invalid indent '{value}'. (Should be 'tab' or the number of spaces.)");
		}

		public static string? ReadNewLineOption(this ArgsReader args)
		{
			var value = args.ReadOption("newline");
			return value switch
			{
				null => null,
				"auto" => null,
				"lf" => "\n",
				"crlf" => "\r\n",
				_ => throw new ArgsReaderException($"Invalid new line '{value}'. (Should be 'auto', 'lf', or 'crlf'.)"),
			};
		}

		public static IReadOnlyList<string> ReadExcludeTagOptions(this ArgsReader args)
		{
			var values = new List<string>();
			while (true)
			{
				var value = args.ReadOption("exclude-tag");
				if (value == null)
					break;
				values.Add(value);
			}

			return values;
		}
	}
}
