using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.Emit;

internal sealed class EncVariableSlotAllocator : VariableSlotAllocator
{
	private readonly SymbolMatcher _symbolMap;

	private readonly EncMappedMethod _mappedMethod;

	private readonly DebugId? _methodId;

	private readonly IReadOnlyDictionary<EncLocalInfo, int> _previousLocalSlots;

	private readonly ImmutableArray<EncLocalInfo> _previousLocals;

	private readonly string? _stateMachineTypeName;

	private readonly int _hoistedLocalSlotCount;

	private readonly IReadOnlyDictionary<EncHoistedLocalInfo, int>? _hoistedLocalSlots;

	private readonly int _awaiterCount;

	private readonly IReadOnlyDictionary<ITypeReference, int>? _awaiterMap;

	private readonly IReadOnlyDictionary<(int syntaxOffset, AwaitDebugId awaitId), StateMachineState>? _stateMachineStateMap;

	private readonly StateMachineState? _firstUnusedDecreasingStateMachineState;

	private readonly StateMachineState? _firstUnusedIncreasingStateMachineState;

	private readonly IReadOnlyDictionary<int, EncLambdaMapValue>? _lambdaMap;

	private readonly IReadOnlyDictionary<int, EncClosureMapValue>? _closureMap;

	private readonly LambdaSyntaxFacts _lambdaSyntaxFacts;

	public override DebugId? MethodId => _methodId;

	public override string? PreviousStateMachineTypeName => _stateMachineTypeName;

	public override int PreviousHoistedLocalSlotCount => _hoistedLocalSlotCount;

	public override int PreviousAwaiterSlotCount => _awaiterCount;

	public EncVariableSlotAllocator(SymbolMatcher symbolMap, EncMappedMethod mappedMethod, DebugId? methodId, ImmutableArray<EncLocalInfo> previousLocals, IReadOnlyDictionary<int, EncLambdaMapValue>? lambdaMap, IReadOnlyDictionary<int, EncClosureMapValue>? closureMap, string? stateMachineTypeName, int hoistedLocalSlotCount, IReadOnlyDictionary<EncHoistedLocalInfo, int>? hoistedLocalSlots, int awaiterCount, IReadOnlyDictionary<ITypeReference, int>? awaiterMap, IReadOnlyDictionary<(int syntaxOffset, AwaitDebugId awaitId), StateMachineState>? stateMachineStateMap, StateMachineState? firstUnusedIncreasingStateMachineState, StateMachineState? firstUnusedDecreasingStateMachineState, LambdaSyntaxFacts lambdaSyntaxFacts)
	{
		_symbolMap = symbolMap;
		_mappedMethod = mappedMethod;
		_previousLocals = previousLocals;
		_methodId = methodId;
		_hoistedLocalSlots = hoistedLocalSlots;
		_hoistedLocalSlotCount = hoistedLocalSlotCount;
		_stateMachineTypeName = stateMachineTypeName;
		_awaiterCount = awaiterCount;
		_awaiterMap = awaiterMap;
		_stateMachineStateMap = stateMachineStateMap;
		_lambdaMap = lambdaMap;
		_closureMap = closureMap;
		_lambdaSyntaxFacts = lambdaSyntaxFacts;
		_firstUnusedIncreasingStateMachineState = firstUnusedIncreasingStateMachineState;
		_firstUnusedDecreasingStateMachineState = firstUnusedDecreasingStateMachineState;
		Dictionary<EncLocalInfo, int> dictionary = new Dictionary<EncLocalInfo, int>();
		for (int i = 0; i < previousLocals.Length; i++)
		{
			EncLocalInfo key = previousLocals[i];
			if (!key.IsUnused)
			{
				dictionary.Add(key, i);
			}
		}
		_previousLocalSlots = dictionary;
	}

	private int CalculateSyntaxOffsetInPreviousMethod(SyntaxNode node)
	{
		return _mappedMethod.PreviousMethod.CalculateLocalSyntaxOffset(_lambdaSyntaxFacts.GetDeclaratorPosition(node), node.SyntaxTree);
	}

	public override void AddPreviousLocals(ArrayBuilder<ILocalDefinition> builder)
	{
		builder.AddRange(_previousLocals.Select((EncLocalInfo info, int index) => new SignatureOnlyLocalDefinition(info.Signature, index)));
	}

	private bool TryGetPreviousLocalId(SyntaxNode currentDeclarator, LocalDebugId currentId, out LocalDebugId previousId)
	{
		if (_mappedMethod.SyntaxMap == null)
		{
			previousId = currentId;
			return true;
		}
		SyntaxNode syntaxNode = _mappedMethod.SyntaxMap(currentDeclarator);
		if (syntaxNode == null)
		{
			previousId = default(LocalDebugId);
			return false;
		}
		int syntaxOffset = CalculateSyntaxOffsetInPreviousMethod(syntaxNode);
		previousId = new LocalDebugId(syntaxOffset, currentId.Ordinal);
		return true;
	}

	public override LocalDefinition? GetPreviousLocal(ITypeReference currentType, ILocalSymbolInternal currentLocalSymbol, string? name, SynthesizedLocalKind kind, LocalDebugId id, LocalVariableAttributes pdbAttributes, LocalSlotConstraints constraints, ImmutableArray<bool> dynamicTransformFlags, ImmutableArray<string> tupleElementNames)
	{
		if (id.IsNone)
		{
			return null;
		}
		if (!TryGetPreviousLocalId(currentLocalSymbol.GetDeclaratorSyntax(), id, out var previousId))
		{
			return null;
		}
		ITypeReference typeReference = _symbolMap.MapReference(currentType);
		if (typeReference == null)
		{
			return null;
		}
		EncLocalInfo key = new EncLocalInfo(new LocalSlotDebugInfo(kind, previousId), typeReference, constraints, null);
		if (!_previousLocalSlots.TryGetValue(key, out var value))
		{
			return null;
		}
		return new LocalDefinition(currentLocalSymbol, name, currentType, value, kind, id, pdbAttributes, constraints, dynamicTransformFlags, tupleElementNames);
	}

	public override bool TryGetPreviousHoistedLocalSlotIndex(SyntaxNode currentDeclarator, ITypeReference currentType, SynthesizedLocalKind synthesizedKind, LocalDebugId currentId, DiagnosticBag diagnostics, out int slotIndex)
	{
		if (_hoistedLocalSlots == null)
		{
			slotIndex = -1;
			return false;
		}
		if (!TryGetPreviousLocalId(currentDeclarator, currentId, out var previousId))
		{
			slotIndex = -1;
			return false;
		}
		ITypeReference typeReference = _symbolMap.MapReference(currentType);
		if (typeReference == null)
		{
			slotIndex = -1;
			return false;
		}
		EncHoistedLocalInfo key = new EncHoistedLocalInfo(new LocalSlotDebugInfo(synthesizedKind, previousId), typeReference);
		return _hoistedLocalSlots.TryGetValue(key, out slotIndex);
	}

	public override bool TryGetPreviousAwaiterSlotIndex(ITypeReference currentType, DiagnosticBag diagnostics, out int slotIndex)
	{
		if (_awaiterMap == null)
		{
			slotIndex = -1;
			return false;
		}
		ITypeReference key = _symbolMap.MapReference(currentType);
		return _awaiterMap.TryGetValue(key, out slotIndex);
	}

	private bool TryGetPreviousSyntaxOffset(SyntaxNode currentSyntax, out int previousSyntaxOffset)
	{
		SyntaxNode syntaxNode = _mappedMethod.SyntaxMap?.Invoke(currentSyntax);
		if (syntaxNode == null)
		{
			previousSyntaxOffset = 0;
			return false;
		}
		previousSyntaxOffset = CalculateSyntaxOffsetInPreviousMethod(syntaxNode);
		return true;
	}

	private bool TryGetPreviousLambdaSyntaxOffset(SyntaxNode lambdaOrLambdaBodySyntax, bool isLambdaBody, out int previousSyntaxOffset)
	{
		SyntaxNode arg = (isLambdaBody ? _lambdaSyntaxFacts.GetLambda(lambdaOrLambdaBodySyntax) : lambdaOrLambdaBodySyntax);
		SyntaxNode syntaxNode = _mappedMethod.SyntaxMap?.Invoke(arg);
		if (syntaxNode == null)
		{
			previousSyntaxOffset = 0;
			return false;
		}
		SyntaxNode syntaxNode2;
		if (isLambdaBody)
		{
			syntaxNode2 = _lambdaSyntaxFacts.TryGetCorrespondingLambdaBody(syntaxNode, lambdaOrLambdaBodySyntax);
			if (syntaxNode2 == null)
			{
				previousSyntaxOffset = 0;
				return false;
			}
		}
		else
		{
			syntaxNode2 = syntaxNode;
		}
		previousSyntaxOffset = CalculateSyntaxOffsetInPreviousMethod(syntaxNode2);
		return true;
	}

	public override bool TryGetPreviousClosure(SyntaxNode scopeSyntax, DebugId? parentClosureId, ImmutableArray<string> structCaptures, out DebugId closureId, out RuntimeRudeEdit? runtimeRudeEdit)
	{
		if (_closureMap != null && TryGetPreviousSyntaxOffset(scopeSyntax, out var previousSyntaxOffset))
		{
			if (_closureMap.TryGetValue(previousSyntaxOffset, out var value) && value.IsCompatibleWith(parentClosureId, structCaptures))
			{
				closureId = value.Id;
				runtimeRudeEdit = _mappedMethod.RuntimeRudeEdit?.Invoke(scopeSyntax);
				return true;
			}
			closureId = default(DebugId);
			runtimeRudeEdit = new RuntimeRudeEdit(HotReloadExceptionCode.UnsupportedChangeToCapturedVariables);
			return false;
		}
		closureId = default(DebugId);
		runtimeRudeEdit = null;
		return false;
	}

	public override bool TryGetPreviousLambda(SyntaxNode lambdaOrLambdaBodySyntax, bool isLambdaBody, int closureOrdinal, ImmutableArray<DebugId> structClosureIds, out DebugId lambdaId, out RuntimeRudeEdit? runtimeRudeEdit)
	{
		if (_lambdaMap != null && TryGetPreviousLambdaSyntaxOffset(lambdaOrLambdaBodySyntax, isLambdaBody, out var previousSyntaxOffset))
		{
			if (_lambdaMap.TryGetValue(previousSyntaxOffset, out var value) && value.IsCompatibleWith(closureOrdinal, structClosureIds))
			{
				runtimeRudeEdit = _mappedMethod.RuntimeRudeEdit?.Invoke(isLambdaBody ? _lambdaSyntaxFacts.GetLambda(lambdaOrLambdaBodySyntax) : lambdaOrLambdaBodySyntax);
				lambdaId = value.Id;
				return true;
			}
			lambdaId = default(DebugId);
			runtimeRudeEdit = new RuntimeRudeEdit(HotReloadExceptionCode.UnsupportedChangeToCapturedVariables);
			return false;
		}
		lambdaId = default(DebugId);
		runtimeRudeEdit = null;
		return false;
	}

	public override StateMachineState? GetFirstUnusedStateMachineState(bool increasing)
	{
		if (!increasing)
		{
			return _firstUnusedDecreasingStateMachineState;
		}
		return _firstUnusedIncreasingStateMachineState;
	}

	public override bool TryGetPreviousStateMachineState(SyntaxNode syntax, AwaitDebugId awaitId, out StateMachineState state)
	{
		if (_stateMachineStateMap != null && TryGetPreviousSyntaxOffset(syntax, out var previousSyntaxOffset) && _stateMachineStateMap.TryGetValue((previousSyntaxOffset, awaitId), out state))
		{
			return true;
		}
		state = StateMachineState.FirstUnusedState;
		return false;
	}
}
