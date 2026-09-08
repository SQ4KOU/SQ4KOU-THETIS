using System;
using System.Globalization;

namespace Microsoft.CodeAnalysis.CodeGen;

internal readonly record struct DebugId : IComparable<DebugId>
{
	public const int UndefinedOrdinal = -1;

	public readonly int Ordinal;

	public readonly int Generation;

	public DebugId(int ordinal, int generation)
	{
		Ordinal = ordinal;
		Generation = generation;
	}

	public int CompareTo(DebugId other)
	{
		int ordinal = Ordinal;
		int num = ordinal.CompareTo(other.Ordinal);
		if (num == 0)
		{
			ordinal = Generation;
			return ordinal.CompareTo(other.Generation);
		}
		return num;
	}

	public override string ToString()
	{
		if (Generation <= 0)
		{
			int ordinal = Ordinal;
			return ordinal.ToString(CultureInfo.InvariantCulture);
		}
		return $"{Ordinal}{'#'}{Generation}";
	}
}
