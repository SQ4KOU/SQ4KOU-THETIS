using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CodeGen;

namespace Microsoft.CodeAnalysis.Emit;

internal readonly struct EncLambdaMapValue(DebugId id, int closureOrdinal, ImmutableArray<DebugId> structClosureIds)
{
	public readonly DebugId Id = id;

	public readonly int ClosureOrdinal = closureOrdinal;

	public readonly ImmutableArray<DebugId> StructClosureIds = structClosureIds;

	public bool IsCompatibleWith(int closureOrdinal, ImmutableArray<DebugId> structClosureIds)
	{
		if (ClosureOrdinal == closureOrdinal)
		{
			return StructClosureIds.SequenceEqual(structClosureIds);
		}
		return false;
	}
}
