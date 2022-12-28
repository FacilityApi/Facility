using System.Text.RegularExpressions;

namespace Facility.Definition;

internal static partial class Regexes
{
	private const string c_wordPattern = "[A-Z]([A-Z]*(?![a-z])|[a-z]*)|[a-z]+|[0-9]+";
	private const RegexOptions c_wordOptions = RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;
#if NET7_0_OR_GREATER
	[GeneratedRegex(c_wordPattern, c_wordOptions)]
	public static partial Regex Word();
#else
	private static readonly Regex s_wordRegex = new(c_wordPattern, c_wordOptions);
	public static Regex Word() => s_wordRegex;
#endif
}
