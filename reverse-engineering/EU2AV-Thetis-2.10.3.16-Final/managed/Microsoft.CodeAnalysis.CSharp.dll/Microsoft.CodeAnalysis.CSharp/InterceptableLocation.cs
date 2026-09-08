using System;

namespace Microsoft.CodeAnalysis.CSharp;

public abstract class InterceptableLocation : IEquatable<InterceptableLocation>
{
	public abstract int Version { get; }

	public abstract string Data { get; }

	private protected InterceptableLocation()
	{
	}

	public abstract string GetDisplayLocation();

	public abstract override bool Equals(object? obj);

	public abstract override int GetHashCode();

	public abstract bool Equals(InterceptableLocation? other);
}
