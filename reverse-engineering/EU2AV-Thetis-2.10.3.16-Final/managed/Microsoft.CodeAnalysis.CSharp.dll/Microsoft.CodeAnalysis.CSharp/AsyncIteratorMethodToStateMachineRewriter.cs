using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class AsyncIteratorMethodToStateMachineRewriter : AsyncMethodToStateMachineRewriter
{
	private readonly AsyncIteratorInfo _asyncIteratorInfo;

	private LabelSymbol _currentDisposalLabel;

	private readonly LabelSymbol _exprReturnLabelTrue;

	private readonly ResumableStateMachineStateAllocator _iteratorStateAllocator;

	internal AsyncIteratorMethodToStateMachineRewriter(MethodSymbol method, int methodOrdinal, AsyncMethodBuilderMemberCollection asyncMethodBuilderMemberCollection, AsyncIteratorInfo asyncIteratorInfo, SyntheticBoundNodeFactory F, FieldSymbol state, FieldSymbol builder, FieldSymbol? instanceIdField, IReadOnlySet<Symbol> hoistedVariables, IReadOnlyDictionary<Symbol, CapturedSymbolReplacement> nonReusableLocalProxies, ImmutableArray<FieldSymbol> nonReusableFieldsForCleanup, SynthesizedLocalOrdinalsDispenser synthesizedLocalOrdinals, ArrayBuilder<StateMachineStateDebugInfo> stateMachineStateDebugInfoBuilder, VariableSlotAllocator? slotAllocatorOpt, int nextFreeHoistedLocalSlot, BindingDiagnosticBag diagnostics)
		: base(method, methodOrdinal, asyncMethodBuilderMemberCollection, F, state, builder, instanceIdField, hoistedVariables, nonReusableLocalProxies, nonReusableFieldsForCleanup, synthesizedLocalOrdinals, stateMachineStateDebugInfoBuilder, slotAllocatorOpt, nextFreeHoistedLocalSlot, diagnostics)
	{
		_asyncIteratorInfo = asyncIteratorInfo;
		_currentDisposalLabel = _exprReturnLabel;
		_exprReturnLabelTrue = F.GenerateLabel("yieldReturn");
		_iteratorStateAllocator = new ResumableStateMachineStateAllocator(slotAllocatorOpt, StateMachineState.FirstResumableAsyncIteratorState, increasing: false);
	}

	protected override BoundStatement? GenerateMissingStateDispatch()
	{
		BoundStatement boundStatement = base.GenerateMissingStateDispatch();
		BoundStatement boundStatement2 = _iteratorStateAllocator.GenerateThrowMissingStateDispatch(F, F.Local(cachedState), HotReloadExceptionCode.CannotResumeSuspendedIteratorMethod);
		if (boundStatement2 == null)
		{
			return boundStatement;
		}
		if (boundStatement == null)
		{
			return boundStatement2;
		}
		return F.Block(boundStatement, boundStatement2);
	}

	protected override BoundStatement GenerateCleanupForExit(ImmutableArray<StateMachineFieldSymbol> rootHoistedLocals)
	{
		return GenerateAllHoistedLocalsCleanup();
	}

	protected override BoundStatement GenerateSetResultCall()
	{
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		AddDisposeCombinedTokensIfNeeded(instance);
		instance.AddRange(GenerateClearCurrent(), GenerateCompleteOnBuilder(), generateSetResultOnPromise(result: false), F.Return(), F.Label(_exprReturnLabelTrue), generateSetResultOnPromise(result: true));
		return F.Block(instance.ToImmutableAndFree());
		BoundExpressionStatement generateSetResultOnPromise(bool result)
		{
			BoundFieldAccess receiver = F.InstanceField(_asyncIteratorInfo.PromiseOfValueOrEndField);
			return F.ExpressionStatement(F.Call(receiver, _asyncIteratorInfo.SetResultMethod, F.Literal(result)));
		}
	}

	private BoundExpressionStatement GenerateClearCurrent()
	{
		FieldSymbol currentField = _asyncIteratorInfo.CurrentField;
		return F.Assignment(F.InstanceField(currentField), F.Default(currentField.Type));
	}

	private BoundExpressionStatement GenerateCompleteOnBuilder()
	{
		return F.ExpressionStatement(F.Call(F.Field(F.This(), _asyncMethodBuilderField), _asyncMethodBuilderMemberCollection.SetResult, ImmutableArray<BoundExpression>.Empty));
	}

	private void AddDisposeCombinedTokensIfNeeded(ArrayBuilder<BoundStatement> builder)
	{
		if ((object)_asyncIteratorInfo.CombinedTokensField != null)
		{
			BoundFieldAccess boundFieldAccess = F.Field(F.This(), _asyncIteratorInfo.CombinedTokensField);
			TypeSymbol type = boundFieldAccess.Type;
			builder.Add(F.If(F.ObjectNotEqual(boundFieldAccess, F.Null(type)), F.Block(F.ExpressionStatement(F.Call(boundFieldAccess, F.WellKnownMethod(WellKnownMember.System_Threading_CancellationTokenSource__Dispose))), F.Assignment(boundFieldAccess, F.Null(type)))));
		}
	}

	protected override BoundStatement GenerateSetExceptionCall(LocalSymbol exceptionLocal)
	{
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		AddDisposeCombinedTokensIfNeeded(instance);
		instance.Add(GenerateClearCurrent());
		instance.Add(GenerateCompleteOnBuilder());
		instance.Add(F.ExpressionStatement(F.Call(F.InstanceField(_asyncIteratorInfo.PromiseOfValueOrEndField), _asyncIteratorInfo.SetExceptionMethod, F.Local(exceptionLocal))));
		return F.Block(instance.ToImmutableAndFree());
	}

	private BoundStatement GenerateJumpToCurrentDisposalLabel()
	{
		return F.If(F.InstanceField(_asyncIteratorInfo.DisposeModeField), F.Goto(_currentDisposalLabel));
	}

	private BoundStatement AppendJumpToCurrentDisposalLabel(BoundStatement node)
	{
		return F.Block(node, GenerateJumpToCurrentDisposalLabel());
	}

	protected override BoundBinaryOperator ShouldEnterFinallyBlock()
	{
		return F.IntEqual(F.Local(cachedState), F.Literal(StateMachineState.NotStartedOrRunningState));
	}

	protected override BoundStatement VisitBody(BoundStatement body)
	{
		AddState(StateMachineState.InitialAsyncIteratorState, out GeneratedLabelSymbol resumeLabel);
		BoundStatement boundStatement = (BoundStatement)Visit(body);
		return F.Block(F.Label(resumeLabel), GenerateJumpToCurrentDisposalLabel(), GenerateSetBothStates(StateMachineState.NotStartedOrRunningState), boundStatement);
	}

	public override BoundNode VisitYieldReturnStatement(BoundYieldReturnStatement node)
	{
		AddResumableState(_iteratorStateAllocator, node.Syntax, default(AwaitDebugId), out StateMachineState stateNumber, out GeneratedLabelSymbol resumeLabel);
		BoundExpression right = (BoundExpression)Visit(node.Expression);
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		instance.Add(F.Assignment(F.InstanceField(_asyncIteratorInfo.CurrentField), right));
		instance.Add(GenerateSetBothStates(stateNumber));
		instance.Add(F.Goto(_exprReturnLabelTrue));
		instance.Add(F.Label(resumeLabel));
		instance.Add(F.HiddenSequencePoint());
		instance.Add(GenerateSetBothStates(StateMachineState.NotStartedOrRunningState));
		instance.Add(GenerateJumpToCurrentDisposalLabel());
		instance.Add(F.HiddenSequencePoint());
		return F.Block(instance.ToImmutableAndFree());
	}

	public override BoundNode VisitYieldBreakStatement(BoundYieldBreakStatement node)
	{
		return F.Block(SetDisposeMode(value: true), F.Goto(_currentDisposalLabel));
	}

	protected override BoundStatement MakeAwaitPreamble()
	{
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(F.Diagnostics, F.Compilation.Assembly);
		FieldSymbol currentField = _asyncIteratorInfo.CurrentField;
		bool num = currentField.Type.IsManagedType(ref useSiteInfo);
		F.Diagnostics.Add(currentField.GetFirstLocationOrNone(), useSiteInfo);
		if (num)
		{
			return GenerateClearCurrent();
		}
		return null;
	}

	private BoundExpressionStatement SetDisposeMode(bool value)
	{
		return F.Assignment(F.InstanceField(_asyncIteratorInfo.DisposeModeField), F.Literal(value));
	}

	public override BoundNode VisitTryStatement(BoundTryStatement node)
	{
		LabelSymbol currentDisposalLabel = _currentDisposalLabel;
		LabelSymbol labelSymbol = null;
		if (node.FinallyBlockOpt != null)
		{
			labelSymbol = (_currentDisposalLabel = F.GenerateLabel("afterFinally"));
		}
		else if ((object)node.FinallyLabelOpt != null)
		{
			_currentDisposalLabel = node.FinallyLabelOpt;
		}
		BoundStatement boundStatement = (BoundStatement)base.VisitTryStatement(node);
		if (labelSymbol != null)
		{
			boundStatement = F.Block(boundStatement, F.Label(labelSymbol));
		}
		_currentDisposalLabel = currentDisposalLabel;
		if (node.FinallyBlockOpt != null && (object)_currentDisposalLabel != null)
		{
			boundStatement = AppendJumpToCurrentDisposalLabel(boundStatement);
		}
		return boundStatement;
	}

	protected override BoundBlock VisitFinally(BoundBlock finallyBlock)
	{
		LabelSymbol currentDisposalLabel = _currentDisposalLabel;
		_currentDisposalLabel = null;
		BoundBlock result = base.VisitFinally(finallyBlock);
		_currentDisposalLabel = currentDisposalLabel;
		return result;
	}

	public override BoundNode VisitExtractedFinallyBlock(BoundExtractedFinallyBlock extractedFinally)
	{
		BoundStatement boundStatement = VisitFinally(extractedFinally.FinallyBlock);
		if ((object)_currentDisposalLabel != null)
		{
			boundStatement = AppendJumpToCurrentDisposalLabel(boundStatement);
		}
		return boundStatement;
	}
}
