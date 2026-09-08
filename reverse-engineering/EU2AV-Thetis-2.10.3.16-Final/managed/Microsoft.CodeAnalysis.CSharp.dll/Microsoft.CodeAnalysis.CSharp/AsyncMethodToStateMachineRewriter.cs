using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal class AsyncMethodToStateMachineRewriter : MethodToStateMachineRewriter
{
	protected readonly MethodSymbol _method;

	protected readonly FieldSymbol _asyncMethodBuilderField;

	protected readonly AsyncMethodBuilderMemberCollection _asyncMethodBuilderMemberCollection;

	protected readonly LabelSymbol _exprReturnLabel;

	private readonly LabelSymbol _exitLabel;

	private readonly LocalSymbol? _exprRetValue;

	private readonly LoweredDynamicOperationFactory _dynamicFactory;

	private readonly Dictionary<TypeSymbol, FieldSymbol> _awaiterFields;

	private int _nextAwaiterId;

	private readonly Dictionary<BoundValuePlaceholderBase, BoundExpression> _placeholderMap;

	protected override bool EnforceAccurateContainerForLocals => false;

	protected sealed override HotReloadExceptionCode EncMissingStateErrorCode => HotReloadExceptionCode.CannotResumeSuspendedAsyncMethod;

	protected sealed override StateMachineState FirstIncreasingResumableState => StateMachineState.FirstUnusedState;

	internal AsyncMethodToStateMachineRewriter(MethodSymbol method, int methodOrdinal, AsyncMethodBuilderMemberCollection asyncMethodBuilderMemberCollection, SyntheticBoundNodeFactory F, FieldSymbol state, FieldSymbol builder, FieldSymbol? instanceIdField, IReadOnlySet<Symbol> hoistedVariables, IReadOnlyDictionary<Symbol, CapturedSymbolReplacement> nonReusableLocalProxies, ImmutableArray<FieldSymbol> nonReusableFieldsForCleanup, SynthesizedLocalOrdinalsDispenser synthesizedLocalOrdinals, ArrayBuilder<StateMachineStateDebugInfo> stateMachineStateDebugInfoBuilder, VariableSlotAllocator? slotAllocatorOpt, int nextFreeHoistedLocalSlot, BindingDiagnosticBag diagnostics)
		: base(F, method, state, instanceIdField, hoistedVariables, nonReusableLocalProxies, nonReusableFieldsForCleanup, synthesizedLocalOrdinals, stateMachineStateDebugInfoBuilder, slotAllocatorOpt, nextFreeHoistedLocalSlot, diagnostics)
	{
		_method = method;
		_asyncMethodBuilderMemberCollection = asyncMethodBuilderMemberCollection;
		_asyncMethodBuilderField = builder;
		_exprReturnLabel = F.GenerateLabel("exprReturn");
		_exitLabel = F.GenerateLabel("exitLabel");
		_exprRetValue = (method.IsAsyncEffectivelyReturningGenericTask(F.Compilation) ? F.SynthesizedLocal(asyncMethodBuilderMemberCollection.ResultType, F.Syntax, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.AsyncMethodReturnValue) : null);
		_dynamicFactory = new LoweredDynamicOperationFactory(F, methodOrdinal);
		_awaiterFields = new Dictionary<TypeSymbol, FieldSymbol>(Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.IgnoringDynamicTupleNamesAndNullability);
		_nextAwaiterId = slotAllocatorOpt?.PreviousAwaiterSlotCount ?? 0;
		_placeholderMap = new Dictionary<BoundValuePlaceholderBase, BoundExpression>();
	}

	private FieldSymbol GetAwaiterField(TypeSymbol awaiterType)
	{
		if (!_awaiterFields.TryGetValue(awaiterType, out FieldSymbol value))
		{
			if (slotAllocator == null || !slotAllocator.TryGetPreviousAwaiterSlotIndex(F.ModuleBuilderOpt.Translate(awaiterType, F.Syntax, F.Diagnostics.DiagnosticBag), F.Diagnostics.DiagnosticBag, out var slotIndex))
			{
				slotIndex = _nextAwaiterId++;
			}
			string name = GeneratedNames.AsyncAwaiterFieldName(slotIndex);
			value = F.StateMachineField(awaiterType, name, SynthesizedLocalKind.AwaiterField, slotIndex);
			_awaiterFields.Add(awaiterType, value);
		}
		return value;
	}

	internal void GenerateMoveNext(BoundStatement body, MethodSymbol moveNextMethod)
	{
		F.CurrentFunction = moveNextMethod;
		BoundStatement statement = VisitBody(body);
		MethodToStateMachineRewriter.TryUnwrapBoundStateMachineScope(ref statement, out var hoistedLocals);
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		instance.Add(F.HiddenSequencePoint());
		instance.Add(F.Assignment(F.Local(cachedState), F.Field(F.This(), stateField)));
		instance.Add(CacheThisIfNeeded());
		LocalSymbol exceptionLocal = F.SynthesizedLocal(F.WellKnownType(WellKnownType.System_Exception));
		instance.Add(GenerateTopLevelTry(F.Block(ImmutableArray<LocalSymbol>.Empty, F.HiddenSequencePoint(), Dispatch(isOutermost: true), statement), F.CatchBlocks(generateExceptionHandling(exceptionLocal, hoistedLocals))));
		instance.Add(F.Label(_exprReturnLabel));
		BoundExpressionStatement boundExpressionStatement = F.Assignment(F.Field(F.This(), stateField), F.Literal(StateMachineState.FinishedState));
		if (!(body.Syntax is BlockSyntax blockSyntax))
		{
			instance.Add(boundExpressionStatement);
		}
		else
		{
			instance.Add(F.SequencePointWithSpan(blockSyntax, blockSyntax.CloseBraceToken.Span, boundExpressionStatement));
			instance.Add(F.HiddenSequencePoint());
		}
		instance.Add(GenerateCleanupForExit(hoistedLocals));
		instance.Add(GenerateSetResultCall());
		instance.Add(F.Label(_exitLabel));
		instance.Add(F.Return());
		ImmutableArray<BoundStatement> statements = instance.ToImmutableAndFree();
		ArrayBuilder<LocalSymbol> instance2 = ArrayBuilder<LocalSymbol>.GetInstance();
		instance2.Add(cachedState);
		if ((object)cachedThis != null)
		{
			instance2.Add(cachedThis);
		}
		if ((object)_exprRetValue != null)
		{
			instance2.Add(_exprRetValue);
		}
		BoundStatement statement2 = F.SequencePoint(body.Syntax, F.Block(instance2.ToImmutableAndFree(), statements));
		if (hoistedLocals.Length > 0)
		{
			statement2 = MakeStateMachineScope(hoistedLocals, statement2);
		}
		statement2 = F.Instrument(statement2, instrumentation);
		F.CloseMethod(statement2);
		BoundCatchBlock generateExceptionHandling(LocalSymbol localSymbol, ImmutableArray<StateMachineFieldSymbol> rootHoistedLocals)
		{
			BoundStatement boundStatement = F.ExpressionStatement(F.AssignmentExpression(F.Field(F.This(), stateField), F.Literal(StateMachineState.FinishedState)));
			BoundStatement boundStatement2 = GenerateSetExceptionCall(localSymbol);
			return new BoundCatchBlock(F.Syntax, ImmutableArray.Create(localSymbol), F.Local(localSymbol), localSymbol.Type, null, null, F.Block(boundStatement, GenerateCleanupForExit(rootHoistedLocals), boundStatement2, GenerateReturn(finished: false)), isSynthesizedAsyncCatchAll: true);
		}
	}

	protected virtual BoundStatement GenerateTopLevelTry(BoundBlock tryBlock, ImmutableArray<BoundCatchBlock> catchBlocks)
	{
		return F.Try(tryBlock, catchBlocks);
	}

	protected virtual BoundStatement GenerateSetResultCall()
	{
		return F.ExpressionStatement(F.Call(F.Field(F.This(), _asyncMethodBuilderField), _asyncMethodBuilderMemberCollection.SetResult, _method.IsAsyncEffectivelyReturningGenericTask(F.Compilation) ? ImmutableArray.Create((BoundExpression)F.Local(_exprRetValue)) : ImmutableArray<BoundExpression>.Empty));
	}

	protected virtual BoundStatement GenerateCleanupForExit(ImmutableArray<StateMachineFieldSymbol> rootHoistedLocals)
	{
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		foreach (StateMachineFieldSymbol item in rootHoistedLocals)
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(F.Diagnostics, F.Compilation.Assembly);
			bool num = item.Type.IsManagedType(ref useSiteInfo);
			F.Diagnostics.Add(item.GetFirstLocationOrNone(), useSiteInfo);
			if (num)
			{
				instance.Add(F.Assignment(F.Field(F.This(), item), F.NullOrDefault(item.Type)));
			}
		}
		return F.Block(instance.ToImmutableAndFree());
	}

	protected virtual BoundStatement GenerateSetExceptionCall(LocalSymbol exceptionLocal)
	{
		return F.ExpressionStatement(F.Call(F.Field(F.This(), _asyncMethodBuilderField), _asyncMethodBuilderMemberCollection.SetException, F.Local(exceptionLocal)));
	}

	protected sealed override BoundStatement GenerateReturn(bool finished)
	{
		return F.Goto(_exitLabel);
	}

	protected virtual BoundStatement VisitBody(BoundStatement body)
	{
		return (BoundStatement)Visit(body);
	}

	public sealed override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
	{
		if (node.Expression.Kind == BoundKind.AwaitExpression)
		{
			return VisitAwaitExpression((BoundAwaitExpression)node.Expression, null);
		}
		if (node.Expression.Kind == BoundKind.AssignmentOperator)
		{
			BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)node.Expression;
			if (boundAssignmentOperator.Right.Kind == BoundKind.AwaitExpression)
			{
				return VisitAwaitExpression((BoundAwaitExpression)boundAssignmentOperator.Right, boundAssignmentOperator.Left);
			}
		}
		BoundExpression boundExpression = (BoundExpression)Visit(node.Expression);
		if (boundExpression == null)
		{
			return F.StatementList();
		}
		return node.Update(boundExpression);
	}

	public sealed override BoundNode VisitAwaitExpression(BoundAwaitExpression node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/AsyncRewriter/AsyncMethodToStateMachineRewriter.cs", 335);
	}

	public sealed override BoundNode VisitBadExpression(BoundBadExpression node)
	{
		return node;
	}

	protected virtual BoundStatement? MakeAwaitPreamble()
	{
		return null;
	}

	private BoundBlock VisitAwaitExpression(BoundAwaitExpression node, BoundExpression resultPlace)
	{
		BoundStatement boundStatement = MakeAwaitPreamble();
		BoundExpression boundExpression = (BoundExpression)Visit(node.Expression);
		BoundAwaitableValuePlaceholder awaitableInstancePlaceholder = node.AwaitableInfo.AwaitableInstancePlaceholder;
		if (awaitableInstancePlaceholder != null)
		{
			_placeholderMap.Add(awaitableInstancePlaceholder, boundExpression);
		}
		BoundExpression boundExpression2 = (node.AwaitableInfo.IsDynamic ? MakeCallMaybeDynamic(boundExpression, null, "GetAwaiter") : ((BoundExpression)Visit(node.AwaitableInfo.GetAwaiter)));
		resultPlace = (BoundExpression)Visit(resultPlace);
		MethodSymbol methodSymbol = VisitMethodSymbol(node.AwaitableInfo.GetResult);
		MethodSymbol getIsCompletedMethod = (((object)node.AwaitableInfo.IsCompleted != null) ? VisitMethodSymbol(node.AwaitableInfo.IsCompleted.GetMethod) : null);
		TypeSymbol type = VisitType(node.Type);
		if (awaitableInstancePlaceholder != null)
		{
			_placeholderMap.Remove(awaitableInstancePlaceholder);
		}
		LocalSymbol localSymbol = F.SynthesizedLocal(boundExpression2.Type, node.Syntax, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.Awaiter);
		BoundBlock item = F.Block(F.Assignment(F.Local(localSymbol), boundExpression2), F.HiddenSequencePoint(), F.If(F.Not(GenerateGetIsCompleted(localSymbol, getIsCompletedMethod)), GenerateAwaitForIncompleteTask(localSymbol, node.DebugInfo)));
		BoundExpression boundExpression3 = MakeCallMaybeDynamic(F.Local(localSymbol), methodSymbol, "GetResult", resultPlace == null);
		BoundStatement item2 = ((resultPlace != null && !type.IsVoidType()) ? F.Assignment(resultPlace, boundExpression3) : F.ExpressionStatement(boundExpression3));
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance((boundStatement == null) ? 2 : 3);
		if (boundStatement != null)
		{
			instance.Add(boundStatement);
		}
		instance.Add(item);
		instance.Add(item2);
		return F.Block(ImmutableCollectionsMarshal.AsImmutableArray(new LocalSymbol[1] { localSymbol }), instance.ToImmutableAndFree());
	}

	public override BoundNode VisitAwaitableValuePlaceholder(BoundAwaitableValuePlaceholder node)
	{
		return _placeholderMap[node];
	}

	private BoundExpression MakeCallMaybeDynamic(BoundExpression receiver, MethodSymbol methodSymbol = null, string methodName = null, bool resultsDiscarded = false)
	{
		if ((object)methodSymbol != null)
		{
			if (!methodSymbol.IsStatic)
			{
				return F.Call(receiver, methodSymbol);
			}
			return F.StaticCall(methodSymbol.ContainingType, methodSymbol, receiver);
		}
		return _dynamicFactory.MakeDynamicMemberInvocation(methodName, receiver, ImmutableArray<TypeWithAnnotations>.Empty, ImmutableArray<BoundExpression>.Empty, ImmutableArray<string>.Empty, ImmutableArray<RefKind>.Empty, hasImplicitReceiver: false, resultsDiscarded).ToExpression();
	}

	private BoundExpression GenerateGetIsCompleted(LocalSymbol awaiterTemp, MethodSymbol getIsCompletedMethod)
	{
		if (awaiterTemp.Type.IsDynamic())
		{
			return _dynamicFactory.MakeDynamicConversion(_dynamicFactory.MakeDynamicGetMember(F.Local(awaiterTemp), "IsCompleted", resultIndexed: false).ToExpression(), isExplicit: true, isArrayIndex: false, isChecked: false, F.SpecialType(SpecialType.System_Boolean)).ToExpression();
		}
		return F.Call(F.Local(awaiterTemp), getIsCompletedMethod);
	}

	private BoundBlock GenerateAwaitForIncompleteTask(LocalSymbol awaiterTemp, BoundAwaitExpressionDebugInfo debugInfo)
	{
		SyntaxNode declaratorSyntax = awaiterTemp.GetDeclaratorSyntax();
		AddResumableState(declaratorSyntax, debugInfo.AwaitId, out StateMachineState state, out GeneratedLabelSymbol resumeLabel);
		TypeSymbol typeSymbol = (awaiterTemp.Type.IsVerifierReference() ? F.SpecialType(SpecialType.System_Object) : awaiterTemp.Type);
		FieldSymbol awaiterField = GetAwaiterField(typeSymbol);
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		instance.Add(GenerateSetBothStates(state));
		instance.Add(F.NoOp(NoOpStatementFlavor.AwaitYieldPoint));
		BoundExpression boundExpression = F.Local(awaiterTemp);
		if (!TypeSymbol.Equals(typeSymbol, awaiterTemp.Type, TypeCompareKind.ConsiderEverything))
		{
			Conversion conversion = F.ClassifyEmitConversion(boundExpression, typeSymbol);
			boundExpression = F.Convert(typeSymbol, boundExpression, conversion);
		}
		instance.Add(F.Assignment(F.Field(F.This(), awaiterField), boundExpression));
		instance.Add(awaiterTemp.Type.IsDynamic() ? GenerateAwaitOnCompletedDynamic(awaiterTemp) : GenerateAwaitOnCompleted(awaiterTemp.Type, awaiterTemp));
		instance.Add(GenerateReturn(finished: false));
		if (F.Compilation.Options.EnableEditAndContinue)
		{
			for (int i = 0; i < debugInfo.ReservedStateMachineCount; i++)
			{
				AddResumableState(declaratorSyntax, new AwaitDebugId((byte)(debugInfo.AwaitId.RelativeStateOrdinal + 1 + i)), out StateMachineState _, out GeneratedLabelSymbol resumeLabel2);
				instance.Add(F.Label(resumeLabel2));
			}
		}
		instance.Add(F.Label(resumeLabel));
		instance.Add(F.NoOp(NoOpStatementFlavor.AwaitResumePoint));
		BoundExpression boundExpression2 = F.Field(F.This(), awaiterField);
		if (!TypeSymbol.Equals(awaiterTemp.Type, awaiterField.Type, TypeCompareKind.ConsiderEverything))
		{
			Conversion conversion2 = F.ClassifyEmitConversion(boundExpression2, awaiterTemp.Type);
			boundExpression2 = F.Convert(awaiterTemp.Type, boundExpression2, conversion2);
		}
		instance.Add(F.Assignment(F.Local(awaiterTemp), boundExpression2));
		instance.Add(F.Assignment(F.Field(F.This(), awaiterField), F.NullOrDefault(awaiterField.Type)));
		instance.Add(GenerateSetBothStates(StateMachineState.NotStartedOrRunningState));
		return F.Block(instance.ToImmutableAndFree());
	}

	private BoundStatement GenerateAwaitOnCompletedDynamic(LocalSymbol awaiterTemp)
	{
		LocalSymbol localSymbol = F.SynthesizedLocal(F.WellKnownType(WellKnownType.System_Runtime_CompilerServices_ICriticalNotifyCompletion));
		LocalSymbol localSymbol2 = F.SynthesizedLocal(F.WellKnownType(WellKnownType.System_Runtime_CompilerServices_INotifyCompletion));
		LocalSymbol localSymbol3 = ((F.CurrentType.TypeKind == TypeKind.Class) ? F.SynthesizedLocal(F.CurrentType) : null);
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		instance.Add(F.Assignment(F.Local(localSymbol), F.As(F.Local(awaiterTemp), localSymbol.Type)));
		if (localSymbol3 != null)
		{
			instance.Add(F.Assignment(F.Local(localSymbol3), F.This()));
		}
		instance.Add(F.If(F.ObjectEqual(F.Local(localSymbol), F.Null(localSymbol.Type)), F.Block(ImmutableArray.Create(localSymbol2), F.Assignment(F.Local(localSymbol2), F.Convert(localSymbol2.Type, F.Local(awaiterTemp), Conversion.ExplicitReference)), F.ExpressionStatement(F.Call(F.Field(F.This(), _asyncMethodBuilderField), _asyncMethodBuilderMemberCollection.AwaitOnCompleted.Construct(localSymbol2.Type, F.This().Type), F.Local(localSymbol2), F.This(localSymbol3))), F.Assignment(F.Local(localSymbol2), F.NullOrDefault(localSymbol2.Type))), F.Block(F.ExpressionStatement(F.Call(F.Field(F.This(), _asyncMethodBuilderField), _asyncMethodBuilderMemberCollection.AwaitUnsafeOnCompleted.Construct(localSymbol.Type, F.This().Type), F.Local(localSymbol), F.This(localSymbol3))))));
		instance.Add(F.Assignment(F.Local(localSymbol), F.NullOrDefault(localSymbol.Type)));
		return F.Block(SingletonOrPair(localSymbol, localSymbol3), instance.ToImmutableAndFree());
	}

	private BoundStatement GenerateAwaitOnCompleted(TypeSymbol loweredAwaiterType, LocalSymbol awaiterTemp)
	{
		LocalSymbol localSymbol = ((F.CurrentType.TypeKind == TypeKind.Class) ? F.SynthesizedLocal(F.CurrentType) : null);
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		MethodSymbol method = (F.Compilation.Conversions.ClassifyImplicitConversionFromType(loweredAwaiterType, F.Compilation.GetWellKnownType(WellKnownType.System_Runtime_CompilerServices_ICriticalNotifyCompletion), ref useSiteInfo).IsImplicit ? _asyncMethodBuilderMemberCollection.AwaitUnsafeOnCompleted : _asyncMethodBuilderMemberCollection.AwaitOnCompleted).Construct(loweredAwaiterType, F.This().Type);
		if (_asyncMethodBuilderMemberCollection.CheckGenericMethodConstraints)
		{
			method.CheckConstraints(new ConstraintsHelper.CheckConstraintsArgs(F.Compilation, F.Compilation.Conversions, includeNullability: false, F.Syntax.Location, Diagnostics));
		}
		BoundExpression boundExpression = F.Call(F.Field(F.This(), _asyncMethodBuilderField), method, F.Local(awaiterTemp), F.This(localSymbol));
		if (localSymbol != null)
		{
			boundExpression = F.Sequence(ImmutableArray.Create(localSymbol), ImmutableArray.Create(F.AssignmentExpression(F.Local(localSymbol), F.This())), boundExpression);
		}
		return F.ExpressionStatement(boundExpression);
	}

	private static ImmutableArray<LocalSymbol> SingletonOrPair(LocalSymbol first, LocalSymbol secondOpt)
	{
		if (!(secondOpt == null))
		{
			return ImmutableArray.Create(first, secondOpt);
		}
		return ImmutableArray.Create(first);
	}

	public sealed override BoundNode VisitReturnStatement(BoundReturnStatement node)
	{
		if (node.ExpressionOpt != null)
		{
			return F.Block(F.Assignment(F.Local(_exprRetValue), (BoundExpression)Visit(node.ExpressionOpt)), F.Goto(_exprReturnLabel));
		}
		return F.Goto(_exprReturnLabel);
	}
}
