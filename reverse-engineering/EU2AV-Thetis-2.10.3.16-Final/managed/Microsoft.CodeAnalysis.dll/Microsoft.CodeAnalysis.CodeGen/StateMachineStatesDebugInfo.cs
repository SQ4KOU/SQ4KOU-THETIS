using System;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.CodeAnalysis.CodeGen;

internal readonly struct StateMachineStatesDebugInfo
{
	public readonly ImmutableArray<StateMachineStateDebugInfo> States;

	public readonly StateMachineState? FirstUnusedIncreasingStateMachineState;

	public readonly StateMachineState? FirstUnusedDecreasingStateMachineState;

	private StateMachineStatesDebugInfo(ImmutableArray<StateMachineStateDebugInfo> states, StateMachineState? firstUnusedIncreasingStateMachineState, StateMachineState? firstUnusedDecreasingStateMachineState)
	{
		States = states;
		FirstUnusedIncreasingStateMachineState = firstUnusedIncreasingStateMachineState;
		FirstUnusedDecreasingStateMachineState = firstUnusedDecreasingStateMachineState;
	}

	public static StateMachineStatesDebugInfo Create(VariableSlotAllocator? variableSlotAllocator, ImmutableArray<StateMachineStateDebugInfo> stateInfos)
	{
		StateMachineState? firstUnusedIncreasingStateMachineState = null;
		StateMachineState? firstUnusedDecreasingStateMachineState = null;
		if (variableSlotAllocator != null)
		{
			firstUnusedIncreasingStateMachineState = variableSlotAllocator.GetFirstUnusedStateMachineState(increasing: true);
			firstUnusedDecreasingStateMachineState = variableSlotAllocator.GetFirstUnusedStateMachineState(increasing: false);
			if (!stateInfos.IsDefaultOrEmpty)
			{
				StateMachineState stateMachineState = stateInfos.Max((StateMachineStateDebugInfo info) => info.StateNumber) + 1;
				StateMachineState stateMachineState2 = stateInfos.Min((StateMachineStateDebugInfo info) => info.StateNumber) - 1;
				firstUnusedIncreasingStateMachineState = (firstUnusedIncreasingStateMachineState.HasValue ? ((StateMachineState)Math.Max((int)firstUnusedIncreasingStateMachineState.Value, (int)stateMachineState)) : stateMachineState);
				if (stateMachineState2 < StateMachineState.FirstUnusedState)
				{
					firstUnusedDecreasingStateMachineState = (firstUnusedDecreasingStateMachineState.HasValue ? ((StateMachineState)Math.Min((int)firstUnusedDecreasingStateMachineState.Value, (int)stateMachineState2)) : stateMachineState2);
				}
			}
		}
		return new StateMachineStatesDebugInfo(stateInfos, firstUnusedIncreasingStateMachineState, firstUnusedDecreasingStateMachineState);
	}
}
