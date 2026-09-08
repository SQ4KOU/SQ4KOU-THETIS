using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.CodeAnalysis;

internal abstract class GlobalAssemblyCache
{
	internal static readonly GlobalAssemblyCache Instance = CreateInstance();

	public static readonly ImmutableArray<ProcessorArchitecture> CurrentArchitectures = ((IntPtr.Size == 4) ? ImmutableArray.Create(ProcessorArchitecture.None, ProcessorArchitecture.MSIL, ProcessorArchitecture.X86) : ImmutableArray.Create(ProcessorArchitecture.None, ProcessorArchitecture.MSIL, ProcessorArchitecture.Amd64));

	private static GlobalAssemblyCache CreateInstance()
	{
		if (Type.GetType("Mono.Runtime") != null)
		{
			return new MonoGlobalAssemblyCache();
		}
		if (!RuntimeInformation.FrameworkDescription.Contains(".NET Framework"))
		{
			return new DotNetCoreGlobalAssemblyCache();
		}
		return new ClrGlobalAssemblyCache();
	}

	public abstract IEnumerable<AssemblyIdentity> GetAssemblyIdentities(AssemblyName partialName, ImmutableArray<ProcessorArchitecture> architectureFilter = default(ImmutableArray<ProcessorArchitecture>));

	public abstract IEnumerable<AssemblyIdentity> GetAssemblyIdentities(string? partialName = null, ImmutableArray<ProcessorArchitecture> architectureFilter = default(ImmutableArray<ProcessorArchitecture>));

	public abstract IEnumerable<string> GetAssemblySimpleNames(ImmutableArray<ProcessorArchitecture> architectureFilter = default(ImmutableArray<ProcessorArchitecture>));

	public AssemblyIdentity? ResolvePartialName(string displayName, ImmutableArray<ProcessorArchitecture> architectureFilter = default(ImmutableArray<ProcessorArchitecture>), CultureInfo? preferredCulture = null)
	{
		string location;
		return ResolvePartialName(displayName, out location, architectureFilter, preferredCulture);
	}

	public abstract AssemblyIdentity? ResolvePartialName(string displayName, out string? location, ImmutableArray<ProcessorArchitecture> architectureFilter = default(ImmutableArray<ProcessorArchitecture>), CultureInfo? preferredCulture = null);
}
