using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;

namespace Microsoft.CodeAnalysis.Emit;

internal readonly struct AddedOrChangedMethodInfo(DebugId methodId, ImmutableArray<EncLocalInfo> locals, ImmutableArray<EncLambdaInfo> lambdaDebugInfo, ImmutableArray<EncClosureInfo> closureDebugInfo, string? stateMachineTypeName, ImmutableArray<EncHoistedLocalInfo> stateMachineHoistedLocalSlotsOpt, ImmutableArray<ITypeReference?> stateMachineAwaiterSlotsOpt, StateMachineStatesDebugInfo stateMachineStates)
{
	public readonly DebugId MethodId = methodId;

	public readonly ImmutableArray<EncLocalInfo> Locals = locals;

	public readonly ImmutableArray<EncLambdaInfo> LambdaDebugInfo = lambdaDebugInfo;

	public readonly ImmutableArray<EncClosureInfo> ClosureDebugInfo = closureDebugInfo;

	public readonly string? StateMachineTypeName = stateMachineTypeName;

	public readonly ImmutableArray<EncHoistedLocalInfo> StateMachineHoistedLocalSlotsOpt = stateMachineHoistedLocalSlotsOpt;

	public readonly ImmutableArray<ITypeReference?> StateMachineAwaiterSlotsOpt = stateMachineAwaiterSlotsOpt;

	public readonly StateMachineStatesDebugInfo StateMachineStates = stateMachineStates;

	public AddedOrChangedMethodInfo MapTypes(SymbolMatcher map)
	{
		ImmutableArray<EncLocalInfo> locals = ImmutableArray.CreateRange(Locals, MapLocalInfo, map);
		ImmutableArray<EncHoistedLocalInfo> stateMachineHoistedLocalSlotsOpt = (StateMachineHoistedLocalSlotsOpt.IsDefault ? default(ImmutableArray<EncHoistedLocalInfo>) : ImmutableArray.CreateRange(StateMachineHoistedLocalSlotsOpt, MapHoistedLocalSlot, map));
		ImmutableArray<ITypeReference> stateMachineAwaiterSlotsOpt = (StateMachineAwaiterSlotsOpt.IsDefault ? default(ImmutableArray<ITypeReference>) : ImmutableArray.CreateRange<ITypeReference, SymbolMatcher, ITypeReference>(StateMachineAwaiterSlotsOpt, (ITypeReference typeRef, SymbolMatcher symbolMatcher) => (typeRef != null) ? symbolMatcher.MapReference(typeRef) : null, map));
		return new AddedOrChangedMethodInfo(MethodId, locals, LambdaDebugInfo, ClosureDebugInfo, StateMachineTypeName, stateMachineHoistedLocalSlotsOpt, stateMachineAwaiterSlotsOpt, StateMachineStates);
	}

	private static EncLocalInfo MapLocalInfo(EncLocalInfo info, SymbolMatcher map)
	{
		if (info.Type == null)
		{
			return info;
		}
		ITypeReference type = map.MapReference(info.Type);
		return new EncLocalInfo(info.SlotInfo, type, info.Constraints, info.Signature);
	}

	private static EncHoistedLocalInfo MapHoistedLocalSlot(EncHoistedLocalInfo info, SymbolMatcher map)
	{
		if (info.Type == null)
		{
			return info;
		}
		ITypeReference type = map.MapReference(info.Type);
		return new EncHoistedLocalInfo(info.SlotInfo, type);
	}
}
