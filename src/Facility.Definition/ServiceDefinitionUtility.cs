using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Facility.Definition
{
	internal static class ServiceDefinitionUtility
	{
		internal static void ValidateName(string name, ServiceTextPosition position)
		{
			if (!s_validNameRegex.IsMatch(name))
				throw new ServiceDefinitionException($"Invalid name '{name}'.", position);
		}

		internal static void ValidateTypeName(string name, ServiceTextPosition position)
		{
			ServiceTypeInfo.Parse(name, x => s_validNameRegex.IsMatch(x) ? new ServiceDtoInfo(x) : null, position);
		}

		internal static void ValidateNoDuplicateNames(IEnumerable<IServiceNamedInfo> infos, string description)
		{
			var duplicate = infos
				.GroupBy(x => x.Name.ToLowerInvariant())
				.Where(x => x.Count() != 1)
				.Select(x => x.Skip(1).First())
				.FirstOrDefault();
			if (duplicate != null)
				throw new ServiceDefinitionException($"Duplicate {description}: {duplicate.Name}", duplicate.Position);
		}

		internal static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> items)
		{
			return new ReadOnlyCollection<T>((items ?? Enumerable.Empty<T>()).ToList());
		}

		static readonly Regex s_validNameRegex = new Regex(@"^[a-zA-Z][a-zA-Z0-9]*$");
	}
}
