using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;

namespace Microsoft.CodeAnalysis;

internal sealed class DotNetCoreGlobalAssemblyCache : GlobalAssemblyCache
{
	public override IEnumerable<AssemblyIdentity> GetAssemblyIdentities(AssemblyName partialName, ImmutableArray<ProcessorArchitecture> architectureFilter = default(ImmutableArray<ProcessorArchitecture>))
	{
		return ImmutableArray<AssemblyIdentity>.Empty;
	}

	public override IEnumerable<AssemblyIdentity> GetAssemblyIdentities(string? partialName = null, ImmutableArray<ProcessorArchitecture> architectureFilter = default(ImmutableArray<ProcessorArchitecture>))
	{
		return ImmutableArray<AssemblyIdentity>.Empty;
	}

	public override IEnumerable<string> GetAssemblySimpleNames(ImmutableArray<ProcessorArchitecture> architectureFilter = default(ImmutableArray<ProcessorArchitecture>))
	{
		return ImmutableArray<string>.Empty;
	}

	public override AssemblyIdentity? ResolvePartialName(string displayName, out string? location, ImmutableArray<ProcessorArchitecture> architectureFilter = default(ImmutableArray<ProcessorArchitecture>), CultureInfo? preferredCulture = null)
	{
		location = null;
		return null;
	}
}
