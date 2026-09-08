using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Debugging;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class MethodBody : IMethodBody
{
	private readonly IMethodDefinition _parent;

	private readonly ImmutableArray<byte> _ilBits;

	private readonly ushort _maxStack;

	private readonly ImmutableArray<ILocalDefinition> _locals;

	private readonly ImmutableArray<ExceptionHandlerRegion> _exceptionHandlers;

	private readonly bool _areLocalsZeroed;

	private readonly ImmutableArray<SequencePoint> _sequencePoints;

	private readonly ImmutableArray<LocalScope> _localScopes;

	private readonly Microsoft.Cci.IImportScope? _importScopeOpt;

	private readonly string? _stateMachineTypeNameOpt;

	private readonly ImmutableArray<StateMachineHoistedLocalScope> _stateMachineHoistedLocalScopes;

	private readonly bool _hasDynamicLocalVariables;

	private readonly StateMachineMoveNextBodyDebugInfo? _stateMachineMoveNextDebugInfoOpt;

	private readonly DebugId _methodId;

	private readonly ImmutableArray<EncHoistedLocalInfo> _stateMachineHoistedLocalSlots;

	private readonly ImmutableArray<EncLambdaInfo> _lambdaDebugInfo;

	private readonly ImmutableArray<LambdaRuntimeRudeEditInfo> _orderedLambdaRuntimeRudeEdits;

	private readonly ImmutableArray<EncClosureInfo> _closureDebugInfo;

	private readonly StateMachineStatesDebugInfo _stateMachineStatesDebugInfo;

	private readonly ImmutableArray<ITypeReference?> _stateMachineAwaiterSlots;

	private readonly ImmutableArray<SourceSpan> _codeCoverageSpans;

	private readonly bool _isPrimaryConstructor;

	ImmutableArray<SourceSpan> IMethodBody.CodeCoverageSpans => _codeCoverageSpans;

	ImmutableArray<ExceptionHandlerRegion> IMethodBody.ExceptionRegions => _exceptionHandlers;

	bool IMethodBody.AreLocalsZeroed => _areLocalsZeroed;

	ImmutableArray<ILocalDefinition> IMethodBody.LocalVariables => _locals;

	IMethodDefinition IMethodBody.MethodDefinition => _parent;

	StateMachineMoveNextBodyDebugInfo? IMethodBody.MoveNextBodyInfo => _stateMachineMoveNextDebugInfoOpt;

	ushort IMethodBody.MaxStack => _maxStack;

	public ImmutableArray<byte> IL => _ilBits;

	public ImmutableArray<SequencePoint> SequencePoints => _sequencePoints;

	ImmutableArray<LocalScope> IMethodBody.LocalScopes => _localScopes;

	Microsoft.Cci.IImportScope? IMethodBody.ImportScope => _importScopeOpt;

	string? IMethodBody.StateMachineTypeName => _stateMachineTypeNameOpt;

	ImmutableArray<StateMachineHoistedLocalScope> IMethodBody.StateMachineHoistedLocalScopes => _stateMachineHoistedLocalScopes;

	ImmutableArray<EncHoistedLocalInfo> IMethodBody.StateMachineHoistedLocalSlots => _stateMachineHoistedLocalSlots;

	ImmutableArray<ITypeReference?> IMethodBody.StateMachineAwaiterSlots => _stateMachineAwaiterSlots;

	bool IMethodBody.HasDynamicLocalVariables => _hasDynamicLocalVariables;

	public DebugId MethodId => _methodId;

	public ImmutableArray<EncLambdaInfo> LambdaDebugInfo => _lambdaDebugInfo;

	public ImmutableArray<LambdaRuntimeRudeEditInfo> OrderedLambdaRuntimeRudeEdits => _orderedLambdaRuntimeRudeEdits;

	public ImmutableArray<EncClosureInfo> ClosureDebugInfo => _closureDebugInfo;

	public StateMachineStatesDebugInfo StateMachineStatesDebugInfo => _stateMachineStatesDebugInfo;

	public bool HasStackalloc { get; }

	public bool IsPrimaryConstructor => _isPrimaryConstructor;

	public MethodBody(ImmutableArray<byte> ilBits, ushort maxStack, IMethodDefinition parent, DebugId methodId, ImmutableArray<ILocalDefinition> locals, SequencePointList? sequencePoints, DebugDocumentProvider? debugDocumentProvider, ImmutableArray<ExceptionHandlerRegion> exceptionHandlers, bool areLocalsZeroed, bool hasStackalloc, ImmutableArray<LocalScope> localScopes, bool hasDynamicLocalVariables, Microsoft.Cci.IImportScope? importScopeOpt, ImmutableArray<EncLambdaInfo> lambdaDebugInfo, ImmutableArray<LambdaRuntimeRudeEditInfo> orderedLambdaRuntimeRudeEdits, ImmutableArray<EncClosureInfo> closureDebugInfo, string? stateMachineTypeNameOpt, ImmutableArray<StateMachineHoistedLocalScope> stateMachineHoistedLocalScopes, ImmutableArray<EncHoistedLocalInfo> stateMachineHoistedLocalSlots, ImmutableArray<ITypeReference?> stateMachineAwaiterSlots, StateMachineStatesDebugInfo stateMachineStatesDebugInfo, StateMachineMoveNextBodyDebugInfo? stateMachineMoveNextDebugInfoOpt, ImmutableArray<SourceSpan> codeCoverageSpans, bool isPrimaryConstructor)
	{
		_ilBits = ilBits;
		_maxStack = maxStack;
		_parent = parent;
		_methodId = methodId;
		_locals = locals;
		_exceptionHandlers = exceptionHandlers;
		_areLocalsZeroed = areLocalsZeroed;
		HasStackalloc = hasStackalloc;
		_localScopes = localScopes;
		_hasDynamicLocalVariables = hasDynamicLocalVariables;
		_importScopeOpt = importScopeOpt;
		_lambdaDebugInfo = lambdaDebugInfo;
		_orderedLambdaRuntimeRudeEdits = orderedLambdaRuntimeRudeEdits;
		_closureDebugInfo = closureDebugInfo;
		_stateMachineTypeNameOpt = stateMachineTypeNameOpt;
		_stateMachineHoistedLocalScopes = stateMachineHoistedLocalScopes;
		_stateMachineHoistedLocalSlots = stateMachineHoistedLocalSlots;
		_stateMachineAwaiterSlots = stateMachineAwaiterSlots;
		_stateMachineStatesDebugInfo = stateMachineStatesDebugInfo;
		_stateMachineMoveNextDebugInfoOpt = stateMachineMoveNextDebugInfoOpt;
		_codeCoverageSpans = codeCoverageSpans;
		_sequencePoints = GetSequencePoints(sequencePoints, debugDocumentProvider);
		_isPrimaryConstructor = isPrimaryConstructor;
	}

	private static ImmutableArray<SequencePoint> GetSequencePoints(SequencePointList? sequencePoints, DebugDocumentProvider? debugDocumentProvider)
	{
		if (sequencePoints == null || sequencePoints.IsEmpty)
		{
			return ImmutableArray<SequencePoint>.Empty;
		}
		ArrayBuilder<SequencePoint> instance = ArrayBuilder<SequencePoint>.GetInstance();
		sequencePoints.GetSequencePoints(debugDocumentProvider, instance);
		return instance.ToImmutableAndFree();
	}
}
