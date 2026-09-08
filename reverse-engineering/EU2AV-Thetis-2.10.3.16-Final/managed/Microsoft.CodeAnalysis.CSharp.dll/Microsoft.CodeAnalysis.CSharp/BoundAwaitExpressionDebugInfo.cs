using Microsoft.CodeAnalysis.CodeGen;

namespace Microsoft.CodeAnalysis.CSharp;

internal readonly record struct BoundAwaitExpressionDebugInfo(AwaitDebugId AwaitId, byte ReservedStateMachineCount);
