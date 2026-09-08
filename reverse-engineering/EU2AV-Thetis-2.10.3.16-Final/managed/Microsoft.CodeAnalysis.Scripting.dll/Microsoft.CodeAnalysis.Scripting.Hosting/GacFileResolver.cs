using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal sealed class GacFileResolver : IEquatable<GacFileResolver>
{
	public static bool IsAvailable
	{
		get
		{
			if (!typeof(object).Assembly.GlobalAssemblyCache)
			{
				return PlatformInformation.IsRunningOnMono;
			}
			return true;
		}
	}

	public ImmutableArray<ProcessorArchitecture> Architectures { get; }

	public CultureInfo PreferredCulture { get; }

	public GacFileResolver(ImmutableArray<ProcessorArchitecture> architectures = default(ImmutableArray<ProcessorArchitecture>), CultureInfo preferredCulture = null)
	{
		if (!IsAvailable)
		{
			throw new PlatformNotSupportedException();
		}
		if (architectures.IsDefault)
		{
			architectures = GlobalAssemblyCache.CurrentArchitectures;
		}
		Architectures = architectures;
		PreferredCulture = preferredCulture;
	}

	public string Resolve(string assemblyName)
	{
		GlobalAssemblyCache.Instance.ResolvePartialName(assemblyName, out string location, Architectures, PreferredCulture);
		if (!File.Exists(location))
		{
			return null;
		}
		return location;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(PreferredCulture, Hash.CombineValues(Architectures));
	}

	public bool Equals(GacFileResolver other)
	{
		if (this != other)
		{
			if (other != null && Architectures.SequenceEqual(other.Architectures))
			{
				return PreferredCulture == other.PreferredCulture;
			}
			return false;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as GacFileResolver);
	}
}
