using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;

namespace Microsoft.CodeAnalysis.Emit;

internal readonly struct EncClosureInfo(ClosureDebugInfo debugInfo, DebugId? parentDebugId, ImmutableArray<string> structCaptures)
{
	public readonly ClosureDebugInfo DebugInfo = debugInfo;

	public readonly DebugId? ParentDebugId = parentDebugId;

	public readonly ImmutableArray<string> StructCaptures = structCaptures;
}
