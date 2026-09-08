using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class ProgrammaticSuppressionInfo : IEquatable<ProgrammaticSuppressionInfo?>
{
	public ImmutableArray<Suppression> Suppressions { get; }

	internal ProgrammaticSuppressionInfo(ImmutableArray<Suppression> suppressions)
	{
		Suppressions = suppressions;
	}

	public bool Equals(ProgrammaticSuppressionInfo? other)
	{
		if (this == other)
		{
			return true;
		}
		if (other != null)
		{
			return Suppressions.SetEquals(other.Suppressions, EqualityComparer<Suppression>.Default);
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ProgrammaticSuppressionInfo);
	}

	public override int GetHashCode()
	{
		return Suppressions.Length;
	}
}
