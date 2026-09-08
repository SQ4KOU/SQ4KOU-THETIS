using System;

namespace Microsoft.CodeAnalysis.Emit;

internal readonly struct SynthesizedDelegateKey(string name) : IEquatable<SynthesizedDelegateKey>
{
	public readonly string Name = name;

	public override bool Equals(object? obj)
	{
		if (obj is SynthesizedDelegateKey other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(SynthesizedDelegateKey other)
	{
		return Name.Equals(other.Name, StringComparison.Ordinal);
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}
}
