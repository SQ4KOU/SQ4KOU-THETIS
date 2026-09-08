using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;

namespace Microsoft.CodeAnalysis.Emit;

internal readonly struct EncLambdaInfo(LambdaDebugInfo debugInfo, ImmutableArray<DebugId> structClosureIds)
{
	public readonly LambdaDebugInfo DebugInfo = debugInfo;

	public readonly ImmutableArray<DebugId> StructClosureIds = structClosureIds;
}
