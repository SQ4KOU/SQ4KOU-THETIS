using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class IteratorMethodToStateMachineRewriter : MethodToStateMachineRewriter
{
	private sealed class IteratorFinallyFrame
	{
		public readonly StateMachineState finalizeState;

		public readonly IteratorFinallyFrame parent;

		public readonly IteratorFinallyMethodSymbol handler;

		public Dictionary<StateMachineState, IteratorFinallyFrame> knownStates;

		public readonly HashSet<LabelSymbol> labels;

		public Dictionary<LabelSymbol, LabelSymbol> proxyLabels;

		public IteratorFinallyFrame(IteratorFinallyFrame parent, StateMachineState finalizeState, IteratorFinallyMethodSymbol handler, HashSet<LabelSymbol> labels)
		{
			this.parent = parent;
			this.finalizeState = finalizeState;
			this.handler = handler;
			this.labels = labels;
		}

		public IteratorFinallyFrame()
		{
			finalizeState = StateMachineState.NotStartedOrRunningState;
		}

		public bool IsRoot()
		{
			return parent == null;
		}

		public void AddState(StateMachineState state)
		{
			if (parent != null)
			{
				parent.AddState(state, this);
			}
		}

		private void AddState(StateMachineState state, IteratorFinallyFrame innerHandler)
		{
			Dictionary<StateMachineState, IteratorFinallyFrame> dictionary = knownStates;
			if (dictionary == null)
			{
				dictionary = (knownStates = new Dictionary<StateMachineState, IteratorFinallyFrame>());
			}
			dictionary.Add(state, innerHandler);
			if (parent != null)
			{
				parent.AddState(state, this);
			}
		}

		public LabelSymbol ProxyLabelIfNeeded(LabelSymbol label)
		{
			if (IsRoot() || (labels != null && labels.Contains(label)))
			{
				return label;
			}
			Dictionary<LabelSymbol, LabelSymbol> dictionary = proxyLabels;
			if (dictionary == null)
			{
				dictionary = (proxyLabels = new Dictionary<LabelSymbol, LabelSymbol>());
			}
			if (!dictionary.TryGetValue(label, out var value))
			{
				value = new GeneratedLabelSymbol("proxy" + label.Name);
				dictionary.Add(label, value);
			}
			return value;
		}
	}

	private sealed class YieldsInTryAnalysis : LabelCollector
	{
		private Dictionary<BoundTryStatement, HashSet<LabelSymbol>> _labelsInYieldingTrys;

		private bool _seenYield;

		public YieldsInTryAnalysis(BoundStatement body)
		{
			_seenYield = false;
			Visit(body);
		}

		public bool ContainsYields(BoundTryStatement statement)
		{
			if (_labelsInYieldingTrys != null)
			{
				return _labelsInYieldingTrys.ContainsKey(statement);
			}
			return false;
		}

		public bool ContainsYieldsInTrys()
		{
			return _labelsInYieldingTrys != null;
		}

		internal HashSet<LabelSymbol> Labels(BoundTryStatement statement)
		{
			return _labelsInYieldingTrys[statement];
		}

		public override BoundNode VisitTryStatement(BoundTryStatement node)
		{
			bool seenYield = _seenYield;
			HashSet<LabelSymbol> hashSet = currentLabels;
			_seenYield = false;
			currentLabels = null;
			base.VisitTryStatement(node);
			if (_seenYield)
			{
				Dictionary<BoundTryStatement, HashSet<LabelSymbol>> dictionary = _labelsInYieldingTrys;
				if (dictionary == null)
				{
					dictionary = (_labelsInYieldingTrys = new Dictionary<BoundTryStatement, HashSet<LabelSymbol>>());
				}
				dictionary.Add(node, currentLabels);
				currentLabels = hashSet;
			}
			else if (currentLabels == null)
			{
				currentLabels = hashSet;
			}
			else if (hashSet != null)
			{
				currentLabels.UnionWith(hashSet);
			}
			_seenYield |= seenYield;
			return null;
		}

		public override BoundNode VisitYieldReturnStatement(BoundYieldReturnStatement node)
		{
			_seenYield = true;
			return base.VisitYieldReturnStatement(node);
		}

		public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
		{
			return null;
		}
	}

	private readonly FieldSymbol _current;

	private YieldsInTryAnalysis? _yieldsInTryAnalysis;

	private int _tryNestingLevel;

	private LabelSymbol? _exitLabel;

	private LocalSymbol? _methodValue;

	private IteratorFinallyFrame _currentFinallyFrame = new IteratorFinallyFrame();

	private StateMachineState _nextFinalizeState;

	protected override bool EnforceAccurateContainerForLocals => false;

	protected sealed override HotReloadExceptionCode EncMissingStateErrorCode => HotReloadExceptionCode.CannotResumeSuspendedIteratorMethod;

	protected override StateMachineState FirstIncreasingResumableState => StateMachineState.FirstResumableIteratorState;

	internal IteratorMethodToStateMachineRewriter(SyntheticBoundNodeFactory F, MethodSymbol originalMethod, FieldSymbol state, FieldSymbol current, FieldSymbol? instanceIdField, IReadOnlySet<Symbol> hoistedVariables, IReadOnlyDictionary<Symbol, CapturedSymbolReplacement> nonReusableLocalProxies, ImmutableArray<FieldSymbol> nonReusableFieldsForCleanup, SynthesizedLocalOrdinalsDispenser synthesizedLocalOrdinals, ArrayBuilder<StateMachineStateDebugInfo> stateMachineStateDebugInfoBuilder, VariableSlotAllocator slotAllocatorOpt, int nextFreeHoistedLocalSlot, BindingDiagnosticBag diagnostics)
		: base(F, originalMethod, state, instanceIdField, hoistedVariables, nonReusableLocalProxies, nonReusableFieldsForCleanup, synthesizedLocalOrdinals, stateMachineStateDebugInfoBuilder, slotAllocatorOpt, nextFreeHoistedLocalSlot, diagnostics)
	{
		_current = current;
		_nextFinalizeState = slotAllocatorOpt?.GetFirstUnusedStateMachineState(increasing: false) ?? StateMachineState.InitialAsyncIteratorState;
	}

	internal void GenerateMoveNextAndDispose(BoundStatement body, SynthesizedImplementationMethod moveNextMethod, SynthesizedImplementationMethod disposeMethod)
	{
		_yieldsInTryAnalysis = new YieldsInTryAnalysis(body);
		if (_yieldsInTryAnalysis.ContainsYieldsInTrys())
		{
			_tryNestingLevel++;
		}
		F.CurrentFunction = moveNextMethod;
		AddState(StateMachineState.FirstUnusedState, out GeneratedLabelSymbol resumeLabel);
		BoundStatement boundStatement = (BoundStatement)Visit(body);
		boundStatement = F.Block(((object)cachedThis == null) ? ImmutableArray.Create(cachedState) : ImmutableArray.Create<LocalSymbol>(cachedState, cachedThis), F.HiddenSequencePoint(), F.Assignment(F.Local(cachedState), F.Field(F.This(), stateField)), CacheThisIfNeeded(), Dispatch(isOutermost: true), GenerateReturn(finished: true), F.Label(resumeLabel), F.Assignment(F.Field(F.This(), stateField), F.Literal(StateMachineState.NotStartedOrRunningState)), boundStatement);
		if (_yieldsInTryAnalysis.ContainsYieldsInTrys())
		{
			BoundBlock faultBlock = F.Block(F.ExpressionStatement(F.Call(F.This(), disposeMethod)));
			boundStatement = F.Fault((BoundBlock)boundStatement, faultBlock);
		}
		boundStatement = F.Instrument(F.SequencePoint(body.Syntax, HandleReturn(boundStatement)), instrumentation);
		F.CloseMethod(boundStatement);
		F.CurrentFunction = disposeMethod;
		IteratorFinallyFrame currentFinallyFrame = _currentFinallyFrame;
		if (currentFinallyFrame.knownStates == null)
		{
			BoundBlock body2 = F.Block(GenerateAllHoistedLocalsCleanup(), F.Assignment(F.Field(F.This(), stateField), F.Literal(StateMachineState.FinishedState)), F.Return());
			F.CloseMethod(body2);
		}
		else
		{
			LocalSymbol localSymbol = F.SynthesizedLocal(stateField.Type);
			BoundLocal state = F.Local(localSymbol);
			BoundBlock body3 = F.Block(ImmutableArray.Create(localSymbol), F.Assignment(F.Local(localSymbol), F.Field(F.This(), stateField)), EmitFinallyFrame(currentFinallyFrame, state), GenerateAllHoistedLocalsCleanup(), F.Assignment(F.Field(F.This(), stateField), F.Literal(StateMachineState.FinishedState)), F.Return());
			F.CloseMethod(body3);
		}
	}

	private BoundBlock HandleReturn(BoundStatement newBody)
	{
		if ((object)_exitLabel == null)
		{
			return F.Block(newBody, F.Return(F.Literal(value: false)));
		}
		return F.Block(ImmutableArray.Create(_methodValue), newBody, F.Assignment(F.Local(_methodValue), F.Literal(value: true)), F.Label(_exitLabel), F.Return(F.Local(_methodValue)));
	}

	private BoundStatement EmitFinallyFrame(IteratorFinallyFrame frame, BoundLocal state)
	{
		BoundStatement boundStatement = null;
		if (frame.knownStates != null)
		{
			GeneratedLabelSymbol breakLabel = F.GenerateLabel("break");
			IEnumerable<SyntheticBoundNodeFactory.SyntheticSwitchSection> items = from g in frame.knownStates.GroupBy(delegate(KeyValuePair<StateMachineState, IteratorFinallyFrame> ft)
				{
					KeyValuePair<StateMachineState, IteratorFinallyFrame> keyValuePair = ft;
					return keyValuePair.Value;
				}, delegate(KeyValuePair<StateMachineState, IteratorFinallyFrame> ft)
				{
					KeyValuePair<StateMachineState, IteratorFinallyFrame> keyValuePair = ft;
					return keyValuePair.Key;
				})
				select F.SwitchSection(g.SelectAsArray((StateMachineState result) => (int)result), EmitFinallyFrame(g.Key, state), F.Goto(breakLabel));
			boundStatement = F.Block(F.Switch(state, items.ToImmutableArray()), F.Label(breakLabel));
		}
		if (!frame.IsRoot())
		{
			BoundBlock tryBlock = ((boundStatement != null) ? F.Block(boundStatement) : F.Block());
			boundStatement = F.Try(tryBlock, ImmutableArray<BoundCatchBlock>.Empty, F.Block(F.ExpressionStatement(F.Call(F.This(), frame.handler))));
		}
		return boundStatement;
	}

	protected override BoundStatement GenerateReturn(bool finished)
	{
		BoundLiteral boundLiteral = F.Literal(!finished);
		if (_tryNestingLevel == 0)
		{
			return F.Return(boundLiteral);
		}
		if ((object)_exitLabel == null)
		{
			_exitLabel = F.GenerateLabel("exitLabel");
			_methodValue = F.SynthesizedLocal(boundLiteral.Type);
		}
		BoundGotoStatement boundGotoStatement = F.Goto(_exitLabel);
		if (finished)
		{
			boundGotoStatement = (BoundGotoStatement)VisitGotoStatement(boundGotoStatement);
		}
		return F.Block(F.Assignment(F.Local(_methodValue), boundLiteral), boundGotoStatement);
	}

	public override BoundNode VisitYieldBreakStatement(BoundYieldBreakStatement node)
	{
		return GenerateReturn(finished: true);
	}

	public override BoundNode VisitYieldReturnStatement(BoundYieldReturnStatement node)
	{
		AddResumableState(node.Syntax, default(AwaitDebugId), out StateMachineState state, out GeneratedLabelSymbol resumeLabel);
		_currentFinallyFrame.AddState(state);
		BoundExpression right = (BoundExpression)Visit(node.Expression);
		return F.Block(F.Assignment(F.Field(F.This(), _current), right), F.Assignment(F.Field(F.This(), stateField), F.Literal(state)), GenerateReturn(finished: false), F.Label(resumeLabel), F.HiddenSequencePoint(), F.Assignment(F.Field(F.This(), stateField), F.Literal(_currentFinallyFrame.finalizeState)));
	}

	public override BoundNode VisitGotoStatement(BoundGotoStatement node)
	{
		BoundExpression caseExpressionOpt = (BoundExpression)Visit(node.CaseExpressionOpt);
		BoundLabel labelExpressionOpt = (BoundLabel)Visit(node.LabelExpressionOpt);
		LabelSymbol label = _currentFinallyFrame.ProxyLabelIfNeeded(node.Label);
		return node.Update(label, caseExpressionOpt, labelExpressionOpt);
	}

	public override BoundNode VisitConditionalGoto(BoundConditionalGoto node)
	{
		return base.VisitConditionalGoto(node);
	}

	public override BoundNode VisitTryStatement(BoundTryStatement node)
	{
		if (!ContainsYields(node))
		{
			_tryNestingLevel++;
			BoundTryStatement result = node.Update((BoundBlock)Visit(node.TryBlock), VisitList(node.CatchBlocks), (BoundBlock)Visit(node.FinallyBlockOpt), node.FinallyLabelOpt, node.PreferFaultHandler);
			_tryNestingLevel--;
			return result;
		}
		IteratorFinallyFrame iteratorFinallyFrame = PushFrame(node);
		_tryNestingLevel++;
		BoundStatement item = (BoundStatement)Visit(node.TryBlock);
		IteratorFinallyMethodSymbol handler = iteratorFinallyFrame.handler;
		MethodSymbol currentFunction = F.CurrentFunction;
		F.CurrentFunction = handler;
		BoundStatement boundStatement = (BoundStatement)Visit(node.FinallyBlockOpt);
		_tryNestingLevel--;
		PopFrame();
		boundStatement = F.Block(((object)cachedThis != null) ? ImmutableArray.Create(cachedThis) : ImmutableArray<LocalSymbol>.Empty, F.Assignment(F.Field(F.This(), stateField), F.Literal(iteratorFinallyFrame.parent.finalizeState)), CacheThisIfNeeded(), boundStatement, F.Return());
		F.CloseMethod(boundStatement);
		F.CurrentFunction = currentFunction;
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		instance.Add(F.Assignment(F.Field(F.This(), stateField), F.Literal(iteratorFinallyFrame.finalizeState)));
		instance.Add(item);
		instance.Add(F.ExpressionStatement(F.Call(F.This(), handler)));
		if (iteratorFinallyFrame.proxyLabels != null)
		{
			GeneratedLabelSymbol label = F.GenerateLabel("dropThrough");
			instance.Add(F.Goto(label));
			IteratorFinallyFrame parent = iteratorFinallyFrame.parent;
			foreach (KeyValuePair<LabelSymbol, LabelSymbol> proxyLabel in iteratorFinallyFrame.proxyLabels)
			{
				LabelSymbol value = proxyLabel.Value;
				LabelSymbol key = proxyLabel.Key;
				instance.Add(F.Label(value));
				instance.Add(F.ExpressionStatement(F.Call(F.This(), handler)));
				LabelSymbol label2 = parent.ProxyLabelIfNeeded(key);
				instance.Add(F.Goto(label2));
			}
			instance.Add(F.Label(label));
		}
		return F.Block(instance.ToImmutableAndFree());
	}

	private IteratorFinallyFrame PushFrame(BoundTryStatement statement)
	{
		SyntaxNode syntax = statement.Syntax;
		VariableSlotAllocator? variableSlotAllocator = slotAllocator;
		if (variableSlotAllocator == null || !variableSlotAllocator.TryGetPreviousStateMachineState(syntax, default(AwaitDebugId), out var state))
		{
			state = _nextFinalizeState--;
		}
		AddStateDebugInfo(syntax, default(AwaitDebugId), state);
		IteratorFinallyMethodSymbol handler = MakeSynthesizedFinally(state);
		IteratorFinallyFrame iteratorFinallyFrame = new IteratorFinallyFrame(_currentFinallyFrame, state, handler, _yieldsInTryAnalysis.Labels(statement));
		iteratorFinallyFrame.AddState(state);
		_currentFinallyFrame = iteratorFinallyFrame;
		return iteratorFinallyFrame;
	}

	private void PopFrame()
	{
		IteratorFinallyFrame currentFinallyFrame = _currentFinallyFrame;
		_currentFinallyFrame = currentFinallyFrame.parent;
	}

	private bool ContainsYields(BoundTryStatement statement)
	{
		return _yieldsInTryAnalysis.ContainsYields(statement);
	}

	private IteratorFinallyMethodSymbol MakeSynthesizedFinally(StateMachineState finalizeState)
	{
		IteratorStateMachine iteratorStateMachine = (IteratorStateMachine)F.CurrentType;
		IteratorFinallyMethodSymbol iteratorFinallyMethodSymbol = new IteratorFinallyMethodSymbol(iteratorStateMachine, GeneratedNames.MakeIteratorFinallyMethodName(finalizeState));
		F.ModuleBuilderOpt.AddSynthesizedDefinition(iteratorStateMachine, iteratorFinallyMethodSymbol.GetCciAdapter());
		return iteratorFinallyMethodSymbol;
	}
}
