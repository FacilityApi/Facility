using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Globalization", "CA1307:Specify StringComparison", Justification = "Replace is well known to be ordinal.")]
[assembly: SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "Okay for tests.")]
