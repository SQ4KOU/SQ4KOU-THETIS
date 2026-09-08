using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class MethodToStateMachineRewriter : MethodToClassRewriter
{
	internal readonly MethodSymbol OriginalMethod;

	protected readonly SyntheticBoundNodeFactory F;

	protected readonly FieldSymbol stateField;

	protected readonly LocalSymbol cachedState;

	protected readonly LocalSymbol? cachedThis;

	protected readonly FieldSymbol? instanceIdField;

	private readonly ResumableStateMachineStateAllocator _resumableStateAllocator;

	private Dictionary<LabelSymbol, List<StateMachineState>> _dispatches = new Dictionary<LabelSymbol, List<StateMachineState>>();

	private Dictionary<TypeSymbol, ArrayBuilder<StateMachineFieldSymbol>>? _lazyAvailableReusableHoistedFields;

	private readonly ArrayBuilder<FieldSymbol> _fieldsForCleanup;

	private int _nextHoistedFieldId = 1;

	private readonly IReadOnlySet<Symbol> _hoistedVariables;

	private readonly SynthesizedLocalOrdinalsDispenser _synthesizedLocalOrdinals;

	private int _nextFreeHoistedLocalSlot;

	private readonly ArrayBuilder<StateMachineStateDebugInfo> _stateDebugInfoBuilder;

	protected BoundBlockInstrumentation? instrumentation;

	private readonly RefInitializationHoister<StateMachineFieldSymbol, BoundFieldAccess> _refInitializationHoister;

	protected abstract StateMachineState FirstIncreasingResumableState { get; }

	protected abstract HotReloadExceptionCode EncMissingStateErrorCode { get; }

	protected override TypeMap TypeMap => ((SynthesizedContainer)F.CurrentType).TypeMap;

	protected override MethodSymbol CurrentMethod => F.CurrentFunction;

	protected override NamedTypeSymbol ContainingType => OriginalMethod.ContainingType;

	internal IReadOnlySet<Symbol> HoistedVariables => _hoistedVariables;

	public MethodToStateMachineRewriter(SyntheticBoundNodeFactory F, MethodSymbol originalMethod, FieldSymbol state, FieldSymbol? instanceIdField, IReadOnlySet<Symbol> hoistedVariables, IReadOnlyDictionary<Symbol, CapturedSymbolReplacement> nonReusableLocalProxies, ImmutableArray<FieldSymbol> nonReusableFieldsForCleanup, SynthesizedLocalOrdinalsDispenser synthesizedLocalOrdinals, ArrayBuilder<StateMachineStateDebugInfo> stateMachineStateDebugInfoBuilder, VariableSlotAllocator? slotAllocatorOpt, int nextFreeHoistedLocalSlot, BindingDiagnosticBag diagnostics)
		: base(slotAllocatorOpt, F.CompilationState, diagnostics)
	{
		this.F = F;
		stateField = state;
		this.instanceIdField = instanceIdField;
		cachedState = F.SynthesizedLocal(F.SpecialType(SpecialType.System_Int32), F.Syntax, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.StateMachineCachedState);
		OriginalMethod = originalMethod;
		_hoistedVariables = hoistedVariables;
		_synthesizedLocalOrdinals = synthesizedLocalOrdinals;
		_nextFreeHoistedLocalSlot = nextFreeHoistedLocalSlot;
		foreach (KeyValuePair<Symbol, CapturedSymbolReplacement> nonReusableLocalProxy in nonReusableLocalProxies)
		{
			proxies.Add(nonReusableLocalProxy.Key, nonReusableLocalProxy.Value);
		}
		_fieldsForCleanup = new ArrayBuilder<FieldSymbol>(nonReusableFieldsForCleanup.Length);
		_fieldsForCleanup.AddRange(nonReusableFieldsForCleanup);
		ParameterSymbol thisParameter = originalMethod.ThisParameter;
		if ((object)thisParameter != null && thisParameter.Type.IsReferenceType && proxies.TryGetValue(thisParameter, out CapturedSymbolReplacement value) && F.Compilation.Options.OptimizationLevel == OptimizationLevel.Release)
		{
			BoundExpression boundExpression = value.Replacement(F.Syntax, (NamedTypeSymbol frameType, SyntheticBoundNodeFactory syntheticBoundNodeFactory) => syntheticBoundNodeFactory.This(), F);
			cachedThis = F.SynthesizedLocal(boundExpression.Type, F.Syntax, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.FrameCache);
		}
		_stateDebugInfoBuilder = stateMachineStateDebugInfoBuilder;
		_resumableStateAllocator = new ResumableStateMachineStateAllocator(slotAllocatorOpt, FirstIncreasingResumableState, increasing: true);
		_refInitializationHoister = new RefInitializationHoister<StateMachineFieldSymbol, BoundFieldAccess>(F, OriginalMethod, TypeMap);
	}

	protected abstract BoundStatement GenerateReturn(bool finished);

	protected override bool NeedsProxy(Symbol localOrParameter)
	{
		return _hoistedVariables.Contains(localOrParameter);
	}

	protected override BoundExpression FramePointer(SyntaxNode syntax, NamedTypeSymbol frameClass)
	{
		SyntaxNode syntax2 = F.Syntax;
		F.Syntax = syntax;
		BoundThisReference result = F.This();
		F.Syntax = syntax2;
		return result;
	}

	protected void AddResumableState(SyntaxNode awaitOrYieldReturnSyntax, AwaitDebugId awaitId, out StateMachineState state, out GeneratedLabelSymbol resumeLabel)
	{
		AddResumableState(_resumableStateAllocator, awaitOrYieldReturnSyntax, awaitId, out state, out resumeLabel);
	}

	protected void AddResumableState(ResumableStateMachineStateAllocator allocator, SyntaxNode awaitOrYieldReturnSyntax, AwaitDebugId awaitId, out StateMachineState stateNumber, out GeneratedLabelSymbol resumeLabel)
	{
		stateNumber = allocator.AllocateState(awaitOrYieldReturnSyntax, awaitId);
		AddStateDebugInfo(awaitOrYieldReturnSyntax, awaitId, stateNumber);
		AddState(stateNumber, out resumeLabel);
	}

	protected void AddStateDebugInfo(SyntaxNode node, AwaitDebugId awaitId, StateMachineState state)
	{
		int syntaxOffset = CurrentMethod.CalculateLocalSyntaxOffset(node.SpanStart, node.SyntaxTree);
		_stateDebugInfoBuilder.Add(new StateMachineStateDebugInfo(syntaxOffset, awaitId, state));
	}

	protected void AddState(StateMachineState stateNumber, out GeneratedLabelSymbol resumeLabel)
	{
		if (_dispatches == null)
		{
			_dispatches = new Dictionary<LabelSymbol, List<StateMachineState>>();
		}
		resumeLabel = F.GenerateLabel("stateMachine");
		_dispatches.Add(resumeLabel, new List<StateMachineState> { stateNumber });
	}

	protected BoundStatement Dispatch(bool isOutermost)
	{
		IEnumerable<SyntheticBoundNodeFactory.SyntheticSwitchSection> items = _dispatches.OrderBy<KeyValuePair<LabelSymbol, List<StateMachineState>>, StateMachineState>(delegate(KeyValuePair<LabelSymbol, List<StateMachineState>> kv)
		{
			KeyValuePair<LabelSymbol, List<StateMachineState>> keyValuePair = kv;
			return keyValuePair.Value[0];
		}).Select(delegate(KeyValuePair<LabelSymbol, List<StateMachineState>> kv)
		{
			SyntheticBoundNodeFactory f = F;
			KeyValuePair<LabelSymbol, List<StateMachineState>> keyValuePair = kv;
			ImmutableArray<int> values = keyValuePair.Value.SelectAsArray((StateMachineState state) => (int)state);
			BoundStatement[] array = new BoundStatement[1];
			SyntheticBoundNodeFactory f2 = F;
			keyValuePair = kv;
			array[0] = f2.Goto(keyValuePair.Key);
			return f.SwitchSection(values, array);
		});
		BoundStatement boundStatement = F.Switch(F.Local(cachedState), items.ToImmutableArray());
		if (isOutermost)
		{
			BoundStatement boundStatement2 = GenerateMissingStateDispatch();
			if (boundStatement2 != null)
			{
				boundStatement = F.Block(boundStatement, boundStatement2);
			}
		}
		return boundStatement;
	}

	protected virtual BoundStatement? GenerateMissingStateDispatch()
	{
		return _resumableStateAllocator.GenerateThrowMissingStateDispatch(F, F.Local(cachedState), EncMissingStateErrorCode);
	}

	private BoundStatement PossibleIteratorScope(ImmutableArray<LocalSymbol> locals, Func<BoundStatement> wrapped)
	{
		if (locals.IsDefaultOrEmpty)
		{
			return wrapped();
		}
		ArrayBuilder<StateMachineFieldSymbol> instance = ArrayBuilder<StateMachineFieldSymbol>.GetInstance();
		foreach (LocalSymbol item in locals)
		{
			if (!NeedsProxy(item) || item.RefKind != RefKind.None)
			{
				continue;
			}
			bool reused = false;
			if (!proxies.TryGetValue(item, out CapturedSymbolReplacement value))
			{
				value = new CapturedToStateMachineFieldReplacement(GetOrAllocateReusableHoistedField(TypeMap.SubstituteType(item.Type).Type, out reused, item), isReusable: true);
				proxies.Add(item, value);
			}
			if (item.SynthesizedKind == SynthesizedLocalKind.UserDefined)
			{
				SyntaxNode scopeDesignatorOpt = item.ScopeDesignatorOpt;
				if (scopeDesignatorOpt == null || scopeDesignatorOpt.Kind() != SyntaxKind.SwitchSection)
				{
					goto IL_00c8;
				}
			}
			if (item.SynthesizedKind != SynthesizedLocalKind.LambdaDisplayClass)
			{
				continue;
			}
			goto IL_00c8;
			IL_00c8:
			if (!reused)
			{
				instance.Add(((CapturedToStateMachineFieldReplacement)value).HoistedField);
			}
		}
		BoundStatement boundStatement = wrapped();
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance();
		foreach (LocalSymbol item2 in locals)
		{
			if (!proxies.TryGetValue(item2, out CapturedSymbolReplacement value2))
			{
				continue;
			}
			if (value2 is CapturedToStateMachineFieldReplacement capturedToStateMachineFieldReplacement)
			{
				AddVariableCleanup(instance2, capturedToStateMachineFieldReplacement.HoistedField);
				if (value2.IsReusable)
				{
					FreeReusableHoistedField(capturedToStateMachineFieldReplacement.HoistedField);
				}
				continue;
			}
			foreach (StateMachineFieldSymbol hoistedSymbol in ((CapturedToExpressionSymbolReplacement<StateMachineFieldSymbol>)value2).HoistedSymbols)
			{
				AddVariableCleanup(instance2, hoistedSymbol);
				if (value2.IsReusable)
				{
					FreeReusableHoistedField(hoistedSymbol);
				}
			}
		}
		if (instance2.Count != 0)
		{
			boundStatement = F.Block(boundStatement, F.Block(instance2.SelectAsArray((Func<BoundExpression, SyntheticBoundNodeFactory, BoundStatement>)((BoundExpression e, SyntheticBoundNodeFactory f) => f.ExpressionStatement(e)), F)));
		}
		instance2.Free();
		if (instance.Count != 0)
		{
			boundStatement = MakeStateMachineScope(instance.ToImmutable(), boundStatement);
		}
		instance.Free();
		return boundStatement;
	}

	internal BoundBlock MakeStateMachineScope(ImmutableArray<StateMachineFieldSymbol> hoistedLocals, BoundStatement statement)
	{
		return F.Block(new BoundStateMachineScope(F.Syntax, hoistedLocals, statement));
	}

	internal static bool TryUnwrapBoundStateMachineScope(ref BoundStatement statement, out ImmutableArray<StateMachineFieldSymbol> hoistedLocals)
	{
		if (statement.Kind == BoundKind.Block)
		{
			ImmutableArray<BoundStatement> statements = ((BoundBlock)statement).Statements;
			if (statements.Length == 1 && statements[0].Kind == BoundKind.StateMachineScope)
			{
				BoundStateMachineScope boundStateMachineScope = (BoundStateMachineScope)statements[0];
				statement = boundStateMachineScope.Statement;
				hoistedLocals = boundStateMachineScope.Fields;
				return true;
			}
		}
		hoistedLocals = ImmutableArray<StateMachineFieldSymbol>.Empty;
		return false;
	}

	private void AddVariableCleanup(ArrayBuilder<BoundExpression> cleanup, FieldSymbol field)
	{
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(F.Diagnostics, F.Compilation.Assembly);
		bool num = field.Type.IsManagedType(ref useSiteInfo);
		F.Diagnostics.Add(field.GetFirstLocationOrNone(), useSiteInfo);
		if (num)
		{
			cleanup.Add(F.AssignmentExpression(F.Field(F.This(), field), F.NullOrDefault(field.Type)));
		}
	}

	protected BoundBlock GenerateAllHoistedLocalsCleanup()
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		foreach (FieldSymbol item in _fieldsForCleanup)
		{
			AddVariableCleanup(instance, item);
		}
		BoundBlock result = F.Block(instance.SelectAsArray((Func<BoundExpression, SyntheticBoundNodeFactory, BoundStatement>)((BoundExpression e, SyntheticBoundNodeFactory f) => f.ExpressionStatement(e)), F));
		instance.Free();
		return result;
	}

	private StateMachineFieldSymbol GetOrAllocateReusableHoistedField(TypeSymbol type, out bool reused, LocalSymbol? local = null)
	{
		if (_lazyAvailableReusableHoistedFields != null && _lazyAvailableReusableHoistedFields.TryGetValue(type, out ArrayBuilder<StateMachineFieldSymbol> value) && value.Count > 0)
		{
			StateMachineFieldSymbol result = value.Last();
			value.RemoveLast();
			reused = true;
			return result;
		}
		reused = false;
		int num = _nextHoistedFieldId++;
		StateMachineFieldSymbol stateMachineFieldSymbol;
		if ((object)local != null && local.SynthesizedKind == SynthesizedLocalKind.UserDefined)
		{
			string name = GeneratedNames.MakeHoistedLocalFieldName(SynthesizedLocalKind.UserDefined, num, local.Name);
			stateMachineFieldSymbol = F.StateMachineField(type, name, SynthesizedLocalKind.UserDefined, num);
		}
		else
		{
			stateMachineFieldSymbol = F.StateMachineField(type, GeneratedNames.ReusableHoistedLocalFieldName(num));
		}
		_fieldsForCleanup.Add(stateMachineFieldSymbol);
		return stateMachineFieldSymbol;
	}

	private void FreeReusableHoistedField(StateMachineFieldSymbol field)
	{
		if (_lazyAvailableReusableHoistedFields == null || !_lazyAvailableReusableHoistedFields.TryGetValue(field.Type, out ArrayBuilder<StateMachineFieldSymbol> value))
		{
			if (_lazyAvailableReusableHoistedFields == null)
			{
				_lazyAvailableReusableHoistedFields = new Dictionary<TypeSymbol, ArrayBuilder<StateMachineFieldSymbol>>(Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.IgnoringDynamicTupleNamesAndNullability);
			}
			_lazyAvailableReusableHoistedFields.Add(field.Type, value = new ArrayBuilder<StateMachineFieldSymbol>());
		}
		value.Add(field);
	}

	public override BoundNode Visit(BoundNode node)
	{
		if (node == null)
		{
			return node;
		}
		SyntaxNode syntax = F.Syntax;
		F.Syntax = node.Syntax;
		BoundNode? result = base.Visit(node);
		F.Syntax = syntax;
		return result;
	}

	public override BoundNode VisitBlock(BoundBlock node)
	{
		if (node.Instrumentation != null)
		{
			instrumentation = (BoundBlockInstrumentation)Visit(node.Instrumentation);
		}
		return PossibleIteratorScope(node.Locals, () => VisitBlock(node, removeInstrumentation: true));
	}

	public override BoundNode VisitStateMachineInstanceId(BoundStateMachineInstanceId node)
	{
		return F.Field(F.This(), instanceIdField);
	}

	public override BoundNode VisitScope(BoundScope node)
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		ArrayBuilder<StateMachineFieldSymbol> instance2 = ArrayBuilder<StateMachineFieldSymbol>.GetInstance();
		bool flag = false;
		foreach (LocalSymbol local in node.Locals)
		{
			if (TryRewriteLocal(local, out LocalSymbol newLocal))
			{
				instance.Add(newLocal);
				flag |= (object)local != newLocal;
			}
			else
			{
				instance2.Add(((CapturedToStateMachineFieldReplacement)proxies[local]).HoistedField);
			}
		}
		ImmutableArray<BoundStatement> statements = VisitList(node.Statements);
		if (instance2.Count != 0)
		{
			BoundStatement statement;
			if (instance.Count == 0)
			{
				instance.Free();
				statement = new BoundStatementList(node.Syntax, statements);
			}
			else
			{
				statement = node.Update(instance.ToImmutableAndFree(), statements);
			}
			return MakeStateMachineScope(instance2.ToImmutable(), statement);
		}
		instance2.Free();
		ImmutableArray<LocalSymbol> locals;
		if (flag)
		{
			locals = instance.ToImmutableAndFree();
		}
		else
		{
			instance.Free();
			locals = node.Locals;
		}
		return node.Update(locals, statements);
	}

	public override BoundNode VisitForStatement(BoundForStatement node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/StateMachineRewriter/MethodToStateMachineRewriter.cs", 602);
	}

	public override BoundNode VisitUsingStatement(BoundUsingStatement node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/StateMachineRewriter/MethodToStateMachineRewriter.cs", 607);
	}

	public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
	{
		BoundExpression boundExpression = (BoundExpression)Visit(node.Expression);
		if (boundExpression != null)
		{
			return node.Update(boundExpression);
		}
		return null;
	}

	public override BoundNode VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		if (node.Left.Kind != BoundKind.Local)
		{
			return base.VisitAssignmentOperator(node);
		}
		LocalSymbol localSymbol = ((BoundLocal)node.Left).LocalSymbol;
		if (!NeedsProxy(localSymbol))
		{
			return base.VisitAssignmentOperator(node);
		}
		if (proxies.ContainsKey(localSymbol))
		{
			return base.VisitAssignmentOperator(node);
		}
		BoundExpression visitedRight = (BoundExpression)Visit(node.Right);
		return _refInitializationHoister.HoistRefInitialization(localSymbol, visitedRight, proxies, createHoistedSymbol, createHoistedAccess, this, isRuntimeAsync: false);
		static BoundFieldAccess createHoistedAccess(StateMachineFieldSymbol fieldSymbol, MethodToStateMachineRewriter @this)
		{
			return @this.F.Field(@this.F.This(), fieldSymbol);
		}
		static StateMachineFieldSymbol createHoistedSymbol(TypeSymbol type, MethodToStateMachineRewriter @this, LocalSymbol assignedLocal)
		{
			StateMachineFieldSymbol stateMachineFieldSymbol;
			if (@this.F.Compilation.Options.OptimizationLevel == OptimizationLevel.Debug)
			{
				SyntaxNode declaratorSyntax = assignedLocal.GetDeclaratorSyntax();
				int syntaxOffset = @this.OriginalMethod.CalculateLocalSyntaxOffset(LambdaUtilities.GetDeclaratorPosition(declaratorSyntax), declaratorSyntax.SyntaxTree);
				int ordinal = @this._synthesizedLocalOrdinals.AssignLocalOrdinal(SynthesizedLocalKind.AwaitByRefSpill, syntaxOffset);
				LocalDebugId localDebugId = new LocalDebugId(syntaxOffset, ordinal);
				if (@this.slotAllocator == null || !@this.slotAllocator.TryGetPreviousHoistedLocalSlotIndex(declaratorSyntax, @this.F.ModuleBuilderOpt.Translate(type, declaratorSyntax, @this.Diagnostics.DiagnosticBag), SynthesizedLocalKind.AwaitByRefSpill, localDebugId, @this.Diagnostics.DiagnosticBag, out var slotIndex))
				{
					slotIndex = @this._nextFreeHoistedLocalSlot++;
				}
				string name = GeneratedNames.MakeHoistedLocalFieldName(SynthesizedLocalKind.AwaitByRefSpill, slotIndex);
				stateMachineFieldSymbol = @this.F.StateMachineField(type, name, new LocalSlotDebugInfo(SynthesizedLocalKind.AwaitByRefSpill, localDebugId), slotIndex);
				@this._fieldsForCleanup.Add(stateMachineFieldSymbol);
			}
			else
			{
				stateMachineFieldSymbol = @this.GetOrAllocateReusableHoistedField(type, out var _);
			}
			return stateMachineFieldSymbol;
		}
	}

	public override BoundNode VisitTryStatement(BoundTryStatement node)
	{
		Dictionary<LabelSymbol, List<StateMachineState>> dictionary = _dispatches;
		_dispatches = null;
		BoundBlock boundBlock = F.Block((BoundStatement)Visit(node.TryBlock));
		GeneratedLabelSymbol generatedLabelSymbol = null;
		if (_dispatches != null)
		{
			generatedLabelSymbol = F.GenerateLabel("tryDispatch");
			boundBlock = F.Block(F.HiddenSequencePoint(), Dispatch(isOutermost: false), boundBlock);
			if (dictionary == null)
			{
				dictionary = new Dictionary<LabelSymbol, List<StateMachineState>>();
			}
			dictionary.Add(generatedLabelSymbol, new List<StateMachineState>(from kv in _dispatches.Values
				from n in kv
				orderby n
				select n));
		}
		_dispatches = dictionary;
		ImmutableArray<BoundCatchBlock> catchBlocks = VisitList(node.CatchBlocks);
		BoundBlock finallyBlockOpt = ((node.FinallyBlockOpt == null) ? null : F.Block(F.HiddenSequencePoint(), F.If(ShouldEnterFinallyBlock(), VisitFinally(node.FinallyBlockOpt)), F.HiddenSequencePoint()));
		BoundStatement boundStatement = node.Update(boundBlock, catchBlocks, finallyBlockOpt, node.FinallyLabelOpt, node.PreferFaultHandler);
		if ((object)generatedLabelSymbol != null)
		{
			boundStatement = F.Block(F.HiddenSequencePoint(), F.Label(generatedLabelSymbol), boundStatement);
		}
		return boundStatement;
	}

	protected virtual BoundBlock VisitFinally(BoundBlock finallyBlock)
	{
		return (BoundBlock)Visit(finallyBlock);
	}

	protected virtual BoundBinaryOperator ShouldEnterFinallyBlock()
	{
		return F.IntLessThan(F.Local(cachedState), F.Literal(StateMachineState.FirstUnusedState));
	}

	protected BoundExpressionStatement GenerateSetBothStates(StateMachineState stateNumber)
	{
		return F.Assignment(F.Field(F.This(), stateField), F.AssignmentExpression(F.Local(cachedState), F.Literal(stateNumber)));
	}

	protected BoundStatement CacheThisIfNeeded()
	{
		if ((object)cachedThis != null)
		{
			BoundExpression right = proxies[OriginalMethod.ThisParameter].Replacement(F.Syntax, (NamedTypeSymbol frameType, SyntheticBoundNodeFactory F) => F.This(), F);
			return F.Assignment(F.Local(cachedThis), right);
		}
		return F.StatementList();
	}

	public sealed override BoundNode VisitThisReference(BoundThisReference node)
	{
		if ((object)cachedThis != null)
		{
			return F.Local(cachedThis);
		}
		ParameterSymbol thisParameter = OriginalMethod.ThisParameter;
		if ((object)thisParameter == null || !proxies.TryGetValue(thisParameter, out CapturedSymbolReplacement value))
		{
			return node.Update(VisitType(node.Type));
		}
		return value.Replacement(F.Syntax, (NamedTypeSymbol frameType, SyntheticBoundNodeFactory F) => F.This(), F);
	}

	public override BoundNode VisitBaseReference(BoundBaseReference node)
	{
		if ((object)cachedThis != null)
		{
			return F.Local(cachedThis);
		}
		return proxies[OriginalMethod.ThisParameter].Replacement(F.Syntax, (NamedTypeSymbol frameType, SyntheticBoundNodeFactory F) => F.This(), F);
	}
}
