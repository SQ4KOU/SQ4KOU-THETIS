using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.CodeGen;

internal abstract class VariableSlotAllocator
{
	public abstract string? PreviousStateMachineTypeName { get; }

	public abstract int PreviousHoistedLocalSlotCount { get; }

	public abstract int PreviousAwaiterSlotCount { get; }

	public abstract DebugId? MethodId { get; }

	public abstract void AddPreviousLocals(ArrayBuilder<ILocalDefinition> builder);

	public abstract LocalDefinition? GetPreviousLocal(ITypeReference type, ILocalSymbolInternal symbol, string? name, SynthesizedLocalKind kind, LocalDebugId id, LocalVariableAttributes pdbAttributes, LocalSlotConstraints constraints, ImmutableArray<bool> dynamicTransformFlags, ImmutableArray<string> tupleElementNames);

	public abstract bool TryGetPreviousHoistedLocalSlotIndex(SyntaxNode currentDeclarator, ITypeReference currentType, SynthesizedLocalKind synthesizedKind, LocalDebugId currentId, DiagnosticBag diagnostics, out int slotIndex);

	public abstract bool TryGetPreviousAwaiterSlotIndex(ITypeReference currentType, DiagnosticBag diagnostics, out int slotIndex);

	public abstract bool TryGetPreviousClosure(SyntaxNode closureSyntax, DebugId? parentClosureId, ImmutableArray<string> structCaptures, out DebugId closureId, out RuntimeRudeEdit? runtimeRudeEdit);

	public abstract bool TryGetPreviousLambda(SyntaxNode lambdaOrLambdaBodySyntax, bool isLambdaBody, int closureOrdinal, ImmutableArray<DebugId> structClosureIds, out DebugId lambdaId, out RuntimeRudeEdit? runtimeRudeEdit);

	public abstract StateMachineState? GetFirstUnusedStateMachineState(bool increasing);

	public abstract bool TryGetPreviousStateMachineState(SyntaxNode syntax, AwaitDebugId awaitId, out StateMachineState state);
}
