using System;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ResumableStateMachineStateAllocator
{
	private readonly VariableSlotAllocator? _slotAllocator;

	private readonly bool _increasing;

	private readonly StateMachineState _firstState;

	private StateMachineState _nextState;

	private int _matchedStateCount;

	public bool HasMissingStates => _matchedStateCount < Math.Abs((_slotAllocator?.GetFirstUnusedStateMachineState(_increasing) ?? _firstState) - _firstState);

	public ResumableStateMachineStateAllocator(VariableSlotAllocator? slotAllocator, StateMachineState firstState, bool increasing)
	{
		_increasing = increasing;
		_slotAllocator = slotAllocator;
		_matchedStateCount = 0;
		_firstState = firstState;
		_nextState = slotAllocator?.GetFirstUnusedStateMachineState(increasing) ?? firstState;
	}

	public StateMachineState AllocateState(SyntaxNode awaitOrYieldReturnSyntax, AwaitDebugId awaitId)
	{
		int num = (_increasing ? 1 : (-1));
		VariableSlotAllocator? slotAllocator = _slotAllocator;
		if (slotAllocator != null && slotAllocator.TryGetPreviousStateMachineState(awaitOrYieldReturnSyntax, awaitId, out var state))
		{
			_matchedStateCount++;
		}
		else
		{
			state = _nextState;
			_nextState += num;
		}
		return state;
	}

	public BoundStatement? GenerateThrowMissingStateDispatch(SyntheticBoundNodeFactory f, BoundExpression cachedState, HotReloadExceptionCode errorCode)
	{
		if (!HasMissingStates)
		{
			return null;
		}
		return f.If(f.Binary(_increasing ? BinaryOperatorKind.IntGreaterThanOrEqual : BinaryOperatorKind.IntLessThanOrEqual, f.SpecialType(SpecialType.System_Boolean), cachedState, f.Literal(_firstState)), f.Throw(f.New((MethodSymbol)f.ModuleBuilderOpt.GetOrCreateHotReloadExceptionConstructorDefinition(), f.StringLiteral(ConstantValue.Create(errorCode.GetExceptionMessage())), f.Literal(errorCode.GetExceptionCodeValue()))));
	}
}
