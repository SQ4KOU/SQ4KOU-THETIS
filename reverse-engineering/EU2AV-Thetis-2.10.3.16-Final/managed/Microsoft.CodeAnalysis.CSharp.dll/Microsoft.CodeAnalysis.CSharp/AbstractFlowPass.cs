using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class AbstractFlowPass<TLocalState, TLocalFunctionState> : BoundTreeVisitor where TLocalState : AbstractFlowPass<TLocalState, TLocalFunctionState>.ILocalState where TLocalFunctionState : AbstractFlowPass<TLocalState, TLocalFunctionState>.AbstractLocalFunctionState
{
	internal sealed class PendingBranch
	{
		public readonly BoundNode Branch;

		public bool IsConditionalState;

		public TLocalState State;

		public TLocalState StateWhenTrue;

		public TLocalState StateWhenFalse;

		public readonly LabelSymbol? Label;

		public PendingBranch(BoundNode branch, TLocalState state, LabelSymbol label, bool isConditionalState = false, TLocalState stateWhenTrue = default(TLocalState), TLocalState stateWhenFalse = default(TLocalState))
		{
			Branch = branch;
			State = state.Clone();
			IsConditionalState = isConditionalState;
			if (isConditionalState)
			{
				StateWhenTrue = stateWhenTrue.Clone();
				StateWhenFalse = stateWhenFalse.Clone();
			}
			Label = label;
		}
	}

	protected readonly struct SavedPending(PendingBranchesCollection pendingBranches, PooledHashSet<BoundStatement> labelsSeen)
	{
		public readonly PendingBranchesCollection PendingBranches = pendingBranches;

		public readonly PooledHashSet<BoundStatement> LabelsSeen = labelsSeen;
	}

	internal interface ILocalState
	{
		bool Reachable { get; }

		TLocalState Clone();
	}

	internal sealed class PendingBranchesCollection
	{
		private ArrayBuilder<PendingBranch> _unlabeledBranches;

		private PooledDictionary<LabelSymbol, ArrayBuilder<PendingBranch>>? _labeledBranches;

		internal PendingBranchesCollection()
		{
			_unlabeledBranches = ArrayBuilder<PendingBranch>.GetInstance();
		}

		internal void Free()
		{
			_unlabeledBranches.Free();
			_unlabeledBranches = null;
			FreeLabeledBranches();
		}

		internal void Clear()
		{
			_unlabeledBranches.Clear();
			FreeLabeledBranches();
		}

		private void FreeLabeledBranches()
		{
			if (_labeledBranches == null)
			{
				return;
			}
			foreach (ArrayBuilder<PendingBranch> value in _labeledBranches.Values)
			{
				value.Free();
			}
			_labeledBranches.Free();
			_labeledBranches = null;
		}

		internal ImmutableArray<PendingBranch> ToImmutable()
		{
			if (_labeledBranches != null)
			{
				return ImmutableArray.CreateRange(AsEnumerable());
			}
			return _unlabeledBranches.ToImmutable();
		}

		internal ArrayBuilder<PendingBranch>? GetAndRemoveBranches(LabelSymbol? label)
		{
			ArrayBuilder<PendingBranch> value;
			if ((object)label == null)
			{
				if (_unlabeledBranches.Count == 0)
				{
					value = null;
				}
				else
				{
					value = _unlabeledBranches;
					_unlabeledBranches = ArrayBuilder<PendingBranch>.GetInstance();
				}
			}
			else if (_labeledBranches != null && _labeledBranches.TryGetValue(label, out value))
			{
				_labeledBranches.Remove(label);
			}
			else
			{
				value = null;
			}
			return value;
		}

		internal void Add(PendingBranch branch)
		{
			LabelSymbol label = branch.Label;
			if ((object)label == null)
			{
				_unlabeledBranches.Add(branch);
			}
			else
			{
				GetOrAddLabeledBranches(label).Add(branch);
			}
		}

		internal void AddRange(PendingBranchesCollection collection)
		{
			_unlabeledBranches.AddRange(collection._unlabeledBranches);
			if (collection._labeledBranches == null)
			{
				return;
			}
			foreach (KeyValuePair<LabelSymbol, ArrayBuilder<PendingBranch>> labeledBranch in collection._labeledBranches)
			{
				GetOrAddLabeledBranches(labeledBranch.Key).AddRange(labeledBranch.Value);
			}
		}

		private ArrayBuilder<PendingBranch> GetOrAddLabeledBranches(LabelSymbol label)
		{
			if (_labeledBranches == null)
			{
				_labeledBranches = PooledDictionary<LabelSymbol, ArrayBuilder<PendingBranch>>.GetInstance();
			}
			if (!_labeledBranches.TryGetValue(label, out ArrayBuilder<PendingBranch> value))
			{
				value = ArrayBuilder<PendingBranch>.GetInstance();
				_labeledBranches.Add(label, value);
			}
			return value;
		}

		internal IEnumerable<PendingBranch> AsEnumerable()
		{
			if (_labeledBranches != null)
			{
				return asEnumerableCore();
			}
			return _unlabeledBranches;
			IEnumerable<PendingBranch> asEnumerableCore()
			{
				foreach (PendingBranch unlabeledBranch in _unlabeledBranches)
				{
					yield return unlabeledBranch;
				}
				foreach (ArrayBuilder<PendingBranch> value in _labeledBranches.Values)
				{
					foreach (PendingBranch item in value)
					{
						yield return item;
					}
				}
			}
		}
	}

	internal abstract class AbstractLocalFunctionState
	{
		public TLocalState StateFromBottom;

		public TLocalState StateFromTop;

		public bool Visited;

		public AbstractLocalFunctionState(TLocalState stateFromBottom, TLocalState stateFromTop)
		{
			StateFromBottom = stateFromBottom;
			StateFromTop = stateFromTop;
		}
	}

	protected int _recursionDepth;

	protected readonly CSharpCompilation compilation;

	protected Symbol _symbol;

	protected Symbol CurrentSymbol;

	protected readonly BoundNode methodMainNode;

	private readonly PooledDictionary<LabelSymbol, TLocalState> _labels;

	protected bool stateChangedAfterUse;

	private PooledHashSet<BoundStatement> _labelsSeen;

	protected TLocalState State;

	protected TLocalState StateWhenTrue;

	protected TLocalState StateWhenFalse;

	protected bool IsConditionalState;

	private readonly bool _nonMonotonicTransfer;

	protected RegionPlace regionPlace;

	protected readonly BoundNode firstInRegion;

	protected readonly BoundNode lastInRegion;

	protected readonly bool TrackingRegions;

	private readonly Dictionary<BoundLoopStatement, TLocalState> _loopHeadState;

	protected readonly TextSpan RegionSpan;

	protected Optional<TLocalState> NonMonotonicState;

	private SmallDictionary<LocalFunctionSymbol, TLocalFunctionState>? _localFuncVarUsages;

	protected PendingBranchesCollection PendingBranches { get; private set; }

	protected DiagnosticBag Diagnostics { get; }

	protected bool IsInside => regionPlace == RegionPlace.Inside;

	protected ImmutableArray<ParameterSymbol> MethodParameters
	{
		get
		{
			if (_symbol is MethodSymbol methodSymbol)
			{
				return methodSymbol.Parameters;
			}
			return ImmutableArray<ParameterSymbol>.Empty;
		}
	}

	protected ParameterSymbol MethodThisParameter
	{
		get
		{
			ParameterSymbol thisParameter = null;
			(_symbol as MethodSymbol)?.TryGetThisParameter(out thisParameter);
			return thisParameter;
		}
	}

	public abstract bool AwaitUsingAndForeachAddsPendingBranch { get; }

	protected void SetConditionalState((TLocalState whenTrue, TLocalState whenFalse) state)
	{
		SetConditionalState(state.whenTrue, state.whenFalse);
	}

	protected void SetConditionalState(TLocalState whenTrue, TLocalState whenFalse)
	{
		IsConditionalState = true;
		State = default(TLocalState);
		StateWhenTrue = whenTrue;
		StateWhenFalse = whenFalse;
	}

	protected void SetState(TLocalState newState)
	{
		StateWhenTrue = (StateWhenFalse = default(TLocalState));
		IsConditionalState = false;
		State = newState;
	}

	protected void Split()
	{
		if (!IsConditionalState)
		{
			SetConditionalState(State, State.Clone());
		}
	}

	protected void Unsplit()
	{
		if (IsConditionalState)
		{
			Join(ref StateWhenTrue, ref StateWhenFalse);
			SetState(StateWhenTrue);
		}
	}

	protected AbstractFlowPass(CSharpCompilation compilation, Symbol symbol, BoundNode node, BoundNode firstInRegion = null, BoundNode lastInRegion = null, bool trackRegions = false, bool nonMonotonicTransferFunction = false)
	{
		if (firstInRegion != null && lastInRegion != null)
		{
			trackRegions = true;
		}
		if (trackRegions)
		{
			int spanStart = firstInRegion.Syntax.SpanStart;
			int length = lastInRegion.Syntax.Span.End - spanStart;
			RegionSpan = new TextSpan(spanStart, length);
		}
		PendingBranches = new PendingBranchesCollection();
		_labelsSeen = PooledHashSet<BoundStatement>.GetInstance();
		_labels = PooledDictionary<LabelSymbol, TLocalState>.GetInstance();
		Diagnostics = DiagnosticBag.GetInstance();
		this.compilation = compilation;
		_symbol = symbol;
		CurrentSymbol = symbol;
		methodMainNode = node;
		this.firstInRegion = firstInRegion;
		this.lastInRegion = lastInRegion;
		_loopHeadState = new Dictionary<BoundLoopStatement, TLocalState>(ReferenceEqualityComparer.Instance);
		TrackingRegions = trackRegions;
		_nonMonotonicTransfer = nonMonotonicTransferFunction;
	}

	protected abstract string Dump(TLocalState state);

	protected string Dump()
	{
		if (!IsConditionalState)
		{
			return Dump(State);
		}
		return "true: " + Dump(StateWhenTrue) + " false: " + Dump(StateWhenFalse);
	}

	private void EnterRegionIfNeeded(BoundNode node)
	{
		if (TrackingRegions && node == firstInRegion && regionPlace == RegionPlace.Before)
		{
			EnterRegion();
		}
	}

	protected virtual void EnterRegion()
	{
		regionPlace = RegionPlace.Inside;
	}

	private void LeaveRegionIfNeeded(BoundNode node)
	{
		if (TrackingRegions && node == lastInRegion && regionPlace == RegionPlace.Inside)
		{
			LeaveRegion();
		}
	}

	protected virtual void LeaveRegion()
	{
		regionPlace = RegionPlace.After;
	}

	protected bool RegionContains(TextSpan span)
	{
		if (span.Length == 0)
		{
			return RegionSpan.Contains(span.Start);
		}
		return RegionSpan.Contains(span);
	}

	protected virtual void EnterParameters(ImmutableArray<ParameterSymbol> parameters)
	{
		foreach (ParameterSymbol item in parameters)
		{
			EnterParameter(item);
		}
	}

	protected virtual void EnterParameter(ParameterSymbol parameter)
	{
	}

	protected virtual void LeaveParameters(ImmutableArray<ParameterSymbol> parameters, SyntaxNode syntax, Location location)
	{
		foreach (ParameterSymbol item in parameters)
		{
			LeaveParameter(item, syntax, location);
		}
	}

	protected virtual void LeaveParameter(ParameterSymbol parameter, SyntaxNode syntax, Location location)
	{
	}

	public override BoundNode Visit(BoundNode node)
	{
		return VisitAlways(node);
	}

	protected BoundNode VisitAlways(BoundNode node)
	{
		if (node != null)
		{
			EnterRegionIfNeeded(node);
			VisitWithStackGuard(node);
			LeaveRegionIfNeeded(node);
		}
		return null;
	}

	[DebuggerStepThrough]
	private BoundNode VisitWithStackGuard(BoundNode node)
	{
		if ((node is BoundExpression || node is BoundPattern) ? true : false)
		{
			return VisitExpressionOrPatternWithStackGuard(ref _recursionDepth, node);
		}
		return base.Visit(node);
	}

	[DebuggerStepThrough]
	protected override BoundNode VisitExpressionOrPatternWithoutStackGuard(BoundNode node)
	{
		return base.Visit(node);
	}

	protected override bool ConvertInsufficientExecutionStackExceptionToCancelledByStackGuardException()
	{
		return false;
	}

	protected virtual ImmutableArray<PendingBranch> Scan(ref bool badRegion)
	{
		SavedPending oldPending = SavePending();
		Visit(methodMainNode);
		Unsplit();
		RestorePending(oldPending);
		if (TrackingRegions && regionPlace != RegionPlace.After)
		{
			badRegion = true;
		}
		return RemoveReturns();
	}

	protected ImmutableArray<PendingBranch> Analyze(ref bool badRegion, Optional<TLocalState> initialState = default(Optional<TLocalState>))
	{
		ImmutableArray<PendingBranch> result;
		do
		{
			regionPlace = RegionPlace.Before;
			State = (initialState.HasValue ? initialState.Value : TopState());
			PendingBranches.Clear();
			stateChangedAfterUse = false;
			Diagnostics.Clear();
			result = Scan(ref badRegion);
		}
		while (stateChangedAfterUse);
		return result;
	}

	protected virtual void Free()
	{
		Diagnostics.Free();
		PendingBranches.Free();
		_labelsSeen.Free();
		_labels.Free();
	}

	protected bool ShouldAnalyzeOutParameters(out Location location)
	{
		if (!(_symbol is MethodSymbol { Locations: { Length: 1 } } methodSymbol))
		{
			location = null;
			return false;
		}
		location = methodSymbol.GetFirstLocation();
		return true;
	}

	protected virtual TLocalState LabelState(LabelSymbol label)
	{
		if (_labels.TryGetValue(label, out var value))
		{
			return value;
		}
		value = UnreachableState();
		_labels.Add(label, value);
		return value;
	}

	protected virtual ImmutableArray<PendingBranch> RemoveReturns()
	{
		ImmutableArray<PendingBranch> result = PendingBranches.ToImmutable();
		PendingBranches.Clear();
		return result;
	}

	protected void SetUnreachable()
	{
		State = UnreachableState();
	}

	protected void VisitLvalue(BoundExpression node)
	{
		EnterRegionIfNeeded(node);
		switch (node?.Kind)
		{
		case BoundKind.Parameter:
			VisitLvalueParameter((BoundParameter)node);
			break;
		case BoundKind.Local:
			VisitLvalue((BoundLocal)node);
			break;
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)node;
			if (Binder.AccessingAutoPropertyFromConstructor(boundPropertyAccess, _symbol))
			{
				SynthesizedBackingFieldSymbol synthesizedBackingFieldSymbol = (boundPropertyAccess.PropertySymbol as SourcePropertySymbolBase)?.BackingField;
				if (synthesizedBackingFieldSymbol != null)
				{
					VisitFieldAccessInternal(boundPropertyAccess.ReceiverOpt, synthesizedBackingFieldSymbol);
					break;
				}
			}
			goto default;
		}
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)node;
			VisitFieldAccessInternal(boundFieldAccess.ReceiverOpt, boundFieldAccess.FieldSymbol);
			break;
		}
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)node;
			VisitFieldAccessInternal(boundEventAccess.ReceiverOpt, boundEventAccess.EventSymbol.AssociatedField);
			break;
		}
		case BoundKind.TupleLiteral:
		case BoundKind.ConvertedTupleLiteral:
			((BoundTupleExpression)node).VisitAllElements(delegate(BoundExpression x, AbstractFlowPass<TLocalState, TLocalFunctionState> self)
			{
				self.VisitLvalue(x);
			}, this);
			break;
		case BoundKind.InlineArrayAccess:
		{
			BoundInlineArrayAccess access = (BoundInlineArrayAccess)node;
			VisitLvalue(access);
			break;
		}
		default:
			VisitRvalue(node);
			break;
		case BoundKind.ThisReference:
		case BoundKind.BaseReference:
			break;
		}
		LeaveRegionIfNeeded(node);
	}

	protected virtual void VisitLvalue(BoundLocal node)
	{
	}

	protected void VisitCondition(BoundExpression node)
	{
		Visit(node);
		AdjustConditionalState(node);
	}

	protected void AdjustConditionalState(BoundExpression node)
	{
		if (IsConstantTrue(node))
		{
			Unsplit();
			SetConditionalState(State, UnreachableState());
		}
		else if (IsConstantFalse(node))
		{
			Unsplit();
			SetConditionalState(UnreachableState(), State);
		}
		else if ((object)node.Type == null || node.Type.SpecialType != SpecialType.System_Boolean)
		{
			Unsplit();
		}
		Split();
	}

	protected virtual void VisitRvalue(BoundExpression node, bool isKnownToBeAnLvalue = false)
	{
		Visit(node);
		Unsplit();
	}

	[DebuggerHidden]
	protected virtual void VisitStatement(BoundStatement statement)
	{
		Visit(statement);
	}

	protected static bool IsConstantTrue(BoundExpression node)
	{
		return node.ConstantValueOpt == ConstantValue.True;
	}

	protected static bool IsConstantFalse(BoundExpression node)
	{
		return node.ConstantValueOpt == ConstantValue.False;
	}

	protected static bool IsConstantNull(BoundExpression node)
	{
		return node.ConstantValueOpt == ConstantValue.Null;
	}

	private void LoopHead(BoundLoopStatement node)
	{
		if (_loopHeadState.TryGetValue(node, out var value))
		{
			Join(ref State, ref value);
		}
		_loopHeadState[node] = State.Clone();
	}

	private void LoopTail(BoundLoopStatement node)
	{
		TLocalState self = _loopHeadState[node];
		if (Join(ref self, ref State))
		{
			_loopHeadState[node] = self;
			stateChangedAfterUse = true;
		}
	}

	private void ResolveBreaks(TLocalState breakState, LabelSymbol label)
	{
		JoinPendingBranches(ref breakState, label);
		SetState(breakState);
	}

	private void ResolveContinues(LabelSymbol continueLabel)
	{
		JoinPendingBranches(ref State, continueLabel);
	}

	private void JoinPendingBranches(ref TLocalState state, LabelSymbol label)
	{
		ArrayBuilder<PendingBranch> andRemoveBranches = PendingBranches.GetAndRemoveBranches(label);
		if (andRemoveBranches != null)
		{
			foreach (PendingBranch item in andRemoveBranches)
			{
				Join(ref state, ref item.State);
			}
			andRemoveBranches.Free();
		}
	}

	protected virtual void NoteBranch(PendingBranch pending, BoundNode gotoStmt, BoundStatement target)
	{
	}

	private bool ResolveBranches(LabelSymbol label, BoundStatement? target)
	{
		bool labelStateChanged = false;
		ArrayBuilder<PendingBranch> andRemoveBranches = PendingBranches.GetAndRemoveBranches(label);
		if (andRemoveBranches != null)
		{
			foreach (PendingBranch item in andRemoveBranches)
			{
				ResolveBranch(item, label, target, ref labelStateChanged);
			}
			andRemoveBranches.Free();
		}
		return labelStateChanged;
	}

	protected virtual void ResolveBranch(PendingBranch pending, LabelSymbol label, BoundStatement? target, ref bool labelStateChanged)
	{
		TLocalState self = LabelState(label);
		if (target != null)
		{
			NoteBranch(pending, pending.Branch, target);
		}
		if (Join(ref self, ref pending.State))
		{
			labelStateChanged = true;
			_labels[label] = self;
		}
	}

	protected SavedPending SavePending()
	{
		SavedPending result = new SavedPending(PendingBranches, _labelsSeen);
		PendingBranches = new PendingBranchesCollection();
		_labelsSeen = PooledHashSet<BoundStatement>.GetInstance();
		return result;
	}

	protected void RestorePending(SavedPending oldPending)
	{
		foreach (BoundStatement item in _labelsSeen)
		{
			switch (item.Kind)
			{
			case BoundKind.LabeledStatement:
			{
				BoundLabeledStatement boundLabeledStatement = (BoundLabeledStatement)item;
				stateChangedAfterUse |= ResolveBranches(boundLabeledStatement.Label, boundLabeledStatement);
				break;
			}
			case BoundKind.LabelStatement:
			{
				BoundLabelStatement boundLabelStatement = (BoundLabelStatement)item;
				stateChangedAfterUse |= ResolveBranches(boundLabelStatement.Label, boundLabelStatement);
				break;
			}
			case BoundKind.SwitchSection:
			{
				BoundSwitchSection boundSwitchSection = (BoundSwitchSection)item;
				foreach (BoundSwitchLabel switchLabel in boundSwitchSection.SwitchLabels)
				{
					stateChangedAfterUse |= ResolveBranches(switchLabel.Label, boundSwitchSection);
				}
				break;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(item.Kind);
			}
		}
		oldPending.PendingBranches.AddRange(PendingBranches);
		PendingBranches.Free();
		PendingBranches = oldPending.PendingBranches;
		_labelsSeen.Free();
		_labelsSeen = oldPending.LabelsSeen;
	}

	public override BoundNode DefaultVisit(BoundNode node)
	{
		Diagnostics.Add(ErrorCode.ERR_InternalError, node.Syntax.Location);
		return null;
	}

	public override BoundNode VisitAttribute(BoundAttribute node)
	{
		return null;
	}

	public override BoundNode VisitThrowExpression(BoundThrowExpression node)
	{
		VisitRvalue(node.Expression);
		SetUnreachable();
		return node;
	}

	public override BoundNode VisitPassByCopy(BoundPassByCopy node)
	{
		VisitRvalue(node.Expression);
		return node;
	}

	public override BoundNode VisitIsPatternExpression(BoundIsPatternExpression node)
	{
		bool num = node.Pattern.IsNegated(out BoundPattern innerPattern);
		if (VisitPossibleConditionalAccess(node.Expression, out TLocalState stateWhenNotNull))
		{
			SetConditionalState(patternMatchesNull(innerPattern) ? (whenTrue: State, whenFalse: stateWhenNotNull) : (whenTrue: stateWhenNotNull, whenFalse: State));
		}
		else if (IsConditionalState)
		{
			bool? flag = isBoolTest(innerPattern);
			if (flag.HasValue)
			{
				if (flag != true)
				{
					SetConditionalState(StateWhenFalse, StateWhenTrue);
				}
			}
			else
			{
				Unsplit();
			}
		}
		VisitPattern(innerPattern);
		ImmutableHashSet<LabelSymbol> reachableLabels = node.ReachabilityDecisionDag.ReachableLabels;
		if (!reachableLabels.Contains(node.WhenTrueLabel))
		{
			SetState(StateWhenFalse);
			SetConditionalState(UnreachableState(), State);
		}
		else if (!reachableLabels.Contains(node.WhenFalseLabel))
		{
			SetState(StateWhenTrue);
			SetConditionalState(State, UnreachableState());
		}
		if (num)
		{
			SetConditionalState(StateWhenFalse, StateWhenTrue);
		}
		return node;
		static ArrayBuilder<BoundBinaryPattern> getBinaryPatterns(BoundBinaryPattern binaryPattern)
		{
			ArrayBuilder<BoundBinaryPattern> instance = ArrayBuilder<BoundBinaryPattern>.GetInstance();
			while (true)
			{
				instance.Push(binaryPattern);
				if (!(binaryPattern.Left is BoundBinaryPattern boundBinaryPattern))
				{
					break;
				}
				binaryPattern = boundBinaryPattern;
			}
			return instance;
		}
		static bool? isBoolTest(BoundPattern pattern)
		{
			if (pattern is BoundConstantPattern boundConstantPattern)
			{
				ConstantValue constantValue = boundConstantPattern.ConstantValue;
				if ((object)constantValue != null)
				{
					if (constantValue.IsBoolean)
					{
						return constantValue.BooleanValue;
					}
					goto IL_0193;
				}
			}
			else
			{
				if (pattern is BoundNegatedPattern boundNegatedPattern)
				{
					return !isBoolTest(boundNegatedPattern.Negated);
				}
				if (pattern is BoundBinaryPattern binaryPattern)
				{
					ArrayBuilder<BoundBinaryPattern> arrayBuilder = getBinaryPatterns(binaryPattern);
					bool? flag2 = isBoolTest(arrayBuilder.Peek().Left);
					BoundBinaryPattern result;
					while (arrayBuilder.TryPop(out result))
					{
						if (result.Disjunction)
						{
							bool? flag3 = flag2;
							flag2 = ((!flag3.HasValue) ? ((bool?)null) : ((flag3 != isBoolTest(result.Right)) ? ((bool?)null) : flag3));
						}
						else
						{
							bool? flag4 = flag2;
							if (!flag4.HasValue)
							{
								flag2 = isBoolTest(result.Right);
							}
						}
					}
					arrayBuilder.Free();
					return flag2;
				}
				if (pattern is BoundDiscardPattern || pattern is BoundTypePattern || pattern is BoundRecursivePattern || pattern is BoundITuplePattern || pattern is BoundRelationalPattern || pattern is BoundDeclarationPattern || pattern is BoundListPattern || pattern is BoundSlicePattern)
				{
					goto IL_0193;
				}
			}
			throw ExceptionUtilities.UnexpectedValue(pattern.Kind);
			IL_0193:
			return null;
		}
		static bool patternMatchesNull(BoundPattern pattern)
		{
			if (!(pattern is BoundTypePattern) && !(pattern is BoundRecursivePattern) && !(pattern is BoundITuplePattern) && !(pattern is BoundRelationalPattern))
			{
				if (!(pattern is BoundDeclarationPattern boundDeclarationPattern))
				{
					if (pattern is BoundConstantPattern boundConstantPattern)
					{
						ConstantValue constantValue = boundConstantPattern.ConstantValue;
						if ((object)constantValue != null)
						{
							if (constantValue.IsNull)
							{
								return true;
							}
							goto IL_0092;
						}
					}
					else
					{
						if (pattern is BoundListPattern || pattern is BoundSlicePattern)
						{
							goto IL_0092;
						}
						if (pattern is BoundNegatedPattern boundNegatedPattern)
						{
							return !patternMatchesNull(boundNegatedPattern.Negated);
						}
						if (pattern is BoundBinaryPattern binaryPattern)
						{
							ArrayBuilder<BoundBinaryPattern> arrayBuilder = getBinaryPatterns(binaryPattern);
							bool flag2 = patternMatchesNull(arrayBuilder.Peek().Left);
							BoundBinaryPattern result;
							while (arrayBuilder.TryPop(out result))
							{
								flag2 = ((!result.Disjunction) ? (flag2 && patternMatchesNull(result.Right)) : (flag2 || patternMatchesNull(result.Right)));
							}
							arrayBuilder.Free();
							return flag2;
						}
						if (pattern is BoundDiscardPattern)
						{
							goto IL_0102;
						}
					}
					throw ExceptionUtilities.UnexpectedValue(pattern.Kind);
				}
				if (boundDeclarationPattern.IsVar)
				{
					goto IL_0102;
				}
			}
			goto IL_0092;
			IL_0092:
			return false;
			IL_0102:
			return true;
		}
	}

	public virtual void VisitPattern(BoundPattern pattern)
	{
		Split();
	}

	public override BoundNode VisitConstantPattern(BoundConstantPattern node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/AbstractFlowPass.cs", 1110);
	}

	public override BoundNode VisitBinaryPattern(BoundBinaryPattern node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/AbstractFlowPass.cs", 1116);
	}

	public override BoundNode VisitTupleLiteral(BoundTupleLiteral node)
	{
		return VisitTupleExpression(node);
	}

	public override BoundNode VisitConvertedTupleLiteral(BoundConvertedTupleLiteral node)
	{
		return VisitTupleExpression(node);
	}

	private BoundNode VisitTupleExpression(BoundTupleExpression node)
	{
		VisitArguments(node.Arguments, default(ImmutableArray<RefKind>), null, default(ImmutableArray<int>), expanded: false);
		return null;
	}

	public override BoundNode VisitTupleBinaryOperator(BoundTupleBinaryOperator node)
	{
		VisitRvalue(node.Left);
		VisitRvalue(node.Right);
		return null;
	}

	public override BoundNode VisitDynamicObjectCreationExpression(BoundDynamicObjectCreationExpression node)
	{
		VisitArguments(node.Arguments, node.ArgumentRefKindsOpt, null, node.ArgsToParamsOpt, node.Expanded);
		VisitRvalue(node.InitializerExpressionOpt);
		return null;
	}

	public override BoundNode VisitDynamicIndexerAccess(BoundDynamicIndexerAccess node)
	{
		VisitRvalue(node.Receiver);
		VisitArguments(node.Arguments, node.ArgumentRefKindsOpt, null, default(ImmutableArray<int>), expanded: false);
		return null;
	}

	public override BoundNode VisitDynamicMemberAccess(BoundDynamicMemberAccess node)
	{
		VisitRvalue(node.Receiver);
		return null;
	}

	public override BoundNode VisitDynamicInvocation(BoundDynamicInvocation node)
	{
		VisitRvalue(node.Expression);
		VisitArguments(node.Arguments, node.ArgumentRefKindsOpt, null, default(ImmutableArray<int>), expanded: false);
		return null;
	}

	protected BoundNode? VisitInterpolatedStringBase(BoundInterpolatedStringBase node, InterpolatedStringHandlerData? data)
	{
		(BoundExpression, bool, bool) tuple;
		if (data.HasValue)
		{
			InterpolatedStringHandlerData valueOrDefault = data.GetValueOrDefault();
			if ((object)valueOrDefault.BuilderType != null)
			{
				tuple = (valueOrDefault.Construction, valueOrDefault.UsesBoolReturns, valueOrDefault.HasTrailingHandlerValidityParameter);
				goto IL_0043;
			}
		}
		tuple = (null, false, false);
		goto IL_0043;
		IL_0043:
		var (constructor, flag, flag2) = tuple;
		VisitInterpolatedStringHandlerConstructor(constructor);
		bool num = flag | flag2;
		TLocalState shortCircuitState = (num ? State.Clone() : default(TLocalState));
		VisitInterpolatedStringHandlerParts(node, flag, flag2, ref shortCircuitState);
		if (num)
		{
			Join(ref State, ref shortCircuitState);
		}
		return null;
	}

	protected virtual void VisitInterpolatedStringHandlerConstructor(BoundExpression? constructor)
	{
		VisitRvalue(constructor);
	}

	public override BoundNode VisitInterpolatedString(BoundInterpolatedString node)
	{
		return VisitInterpolatedStringBase(node, node.InterpolationData);
	}

	public override BoundNode VisitUnconvertedInterpolatedString(BoundUnconvertedInterpolatedString node)
	{
		return VisitInterpolatedStringBase(node, null);
	}

	public override BoundNode VisitStringInsert(BoundStringInsert node)
	{
		VisitRvalue(node.Value);
		if (node.Alignment != null)
		{
			VisitRvalue(node.Alignment);
		}
		if (node.Format != null)
		{
			VisitRvalue(node.Format);
		}
		return null;
	}

	public override BoundNode VisitInterpolatedStringHandlerPlaceholder(BoundInterpolatedStringHandlerPlaceholder node)
	{
		return null;
	}

	public override BoundNode VisitInterpolatedStringArgumentPlaceholder(BoundInterpolatedStringArgumentPlaceholder node)
	{
		return null;
	}

	public override BoundNode VisitArgList(BoundArgList node)
	{
		return null;
	}

	public override BoundNode VisitArgListOperator(BoundArgListOperator node)
	{
		VisitArguments(node.Arguments, node.ArgumentRefKindsOpt, null, default(ImmutableArray<int>), expanded: false);
		return null;
	}

	public override BoundNode VisitRefTypeOperator(BoundRefTypeOperator node)
	{
		VisitRvalue(node.Operand);
		return null;
	}

	public override BoundNode VisitMakeRefOperator(BoundMakeRefOperator node)
	{
		VisitRvalue(node.Operand, isKnownToBeAnLvalue: true);
		return null;
	}

	public override BoundNode VisitRefValueOperator(BoundRefValueOperator node)
	{
		VisitRvalue(node.Operand);
		return null;
	}

	public override BoundNode VisitGlobalStatementInitializer(BoundGlobalStatementInitializer node)
	{
		VisitStatement(node.Statement);
		return null;
	}

	public override BoundNode VisitLambda(BoundLambda node)
	{
		return null;
	}

	public override BoundNode VisitLocal(BoundLocal node)
	{
		SplitIfBooleanConstant(node);
		return null;
	}

	public override BoundNode VisitLocalDeclaration(BoundLocalDeclaration node)
	{
		if (node.InitializerOpt != null)
		{
			VisitRvalue(node.InitializerOpt, node.LocalSymbol.RefKind != RefKind.None);
			if (node.LocalSymbol.RefKind != RefKind.None)
			{
				WriteArgument(node.InitializerOpt, node.LocalSymbol.RefKind, null);
			}
		}
		return null;
	}

	public override BoundNode VisitBlock(BoundBlock node)
	{
		VisitStatements(node.Statements);
		return null;
	}

	private void VisitStatements(ImmutableArray<BoundStatement> statements)
	{
		foreach (BoundStatement item in statements)
		{
			VisitStatement(item);
		}
	}

	public override BoundNode VisitScope(BoundScope node)
	{
		VisitStatements(node.Statements);
		return null;
	}

	public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
	{
		VisitRvalue(node.Expression);
		return null;
	}

	public override BoundNode VisitCall(BoundCall node)
	{
		bool flag = node.Method.CallsAreOmitted(node.SyntaxTree);
		TLocalState state = default(TLocalState);
		if (flag)
		{
			state = State.Clone();
			SetUnreachable();
		}
		if (node.ReceiverOpt is BoundCall boundCall)
		{
			ArrayBuilder<BoundCall> instance = ArrayBuilder<BoundCall>.GetInstance();
			instance.Push(node);
			node = boundCall;
			while (node.ReceiverOpt is BoundCall boundCall2)
			{
				instance.Push(node);
				node = boundCall2;
			}
			VisitReceiverBeforeCall(node.ReceiverOpt, node.Method);
			do
			{
				visitArgumentsAndCompleteAnalysis(node);
			}
			while (instance.TryPop(out node));
			instance.Free();
		}
		else
		{
			VisitReceiverBeforeCall(node.ReceiverOpt, node.Method);
			visitArgumentsAndCompleteAnalysis(node);
		}
		if (flag)
		{
			State = state;
		}
		return null;
		void visitArgumentsAndCompleteAnalysis(BoundCall boundCall3)
		{
			VisitArgumentsBeforeCall(boundCall3.Arguments, boundCall3.ArgumentRefKindsOpt);
			if (boundCall3.Method?.OriginalDefinition is LocalFunctionSymbol symbol)
			{
				VisitLocalFunctionUse(symbol, boundCall3.Syntax, isCall: true);
			}
			VisitArgumentsAfterCall(boundCall3.Arguments, boundCall3.ArgumentRefKindsOpt, boundCall3.Method, boundCall3.ArgsToParamsOpt, boundCall3.Expanded);
			VisitReceiverAfterCall(boundCall3.ReceiverOpt, boundCall3.Method);
		}
	}

	protected void VisitLocalFunctionUse(LocalFunctionSymbol symbol, SyntaxNode syntax, bool isCall)
	{
		TLocalFunctionState orCreateLocalFuncUsages = GetOrCreateLocalFuncUsages(symbol);
		VisitLocalFunctionUse(symbol, orCreateLocalFuncUsages, syntax, isCall);
	}

	protected virtual void VisitLocalFunctionUse(LocalFunctionSymbol symbol, TLocalFunctionState localFunctionState, SyntaxNode syntax, bool isCall)
	{
		if (isCall && !symbol.IsExtern)
		{
			Join(ref State, ref localFunctionState.StateFromBottom);
			if (!symbol.IsAsync)
			{
				Meet(ref State, ref localFunctionState.StateFromTop);
			}
		}
		localFunctionState.Visited = true;
	}

	private void VisitReceiverBeforeCall(BoundExpression receiverOpt, MethodSymbol method)
	{
		if ((object)method == null || method.MethodKind != MethodKind.Constructor)
		{
			VisitRvalue(receiverOpt);
		}
	}

	private void VisitReceiverAfterCall(BoundExpression receiverOpt, MethodSymbol method)
	{
		if (receiverOpt == null)
		{
			return;
		}
		ParameterSymbol thisParameter;
		if ((object)method == null)
		{
			WriteArgument(receiverOpt, RefKind.Ref, null);
		}
		else if (method.TryGetThisParameter(out thisParameter) && (object)thisParameter != null && !TypeIsImmutable(thisParameter.Type))
		{
			RefKind refKind = thisParameter.RefKind;
			if (refKind.IsWritableReference())
			{
				WriteArgument(receiverOpt, refKind, method);
			}
		}
	}

	private static bool TypeIsImmutable(TypeSymbol t)
	{
		SpecialType specialType = t.SpecialType;
		if ((uint)(specialType - 7) <= 12u || specialType == SpecialType.System_DateTime)
		{
			return true;
		}
		return t.IsNullableType();
	}

	public override BoundNode VisitIndexerAccess(BoundIndexerAccess node)
	{
		MethodSymbol readMethod = GetReadMethod(node.Indexer);
		VisitReceiverBeforeCall(node.ReceiverOpt, readMethod);
		VisitArguments(node.Arguments, node.ArgumentRefKindsOpt, readMethod, node.ArgsToParamsOpt, node.Expanded);
		if ((object)readMethod != null)
		{
			VisitReceiverAfterCall(node.ReceiverOpt, readMethod);
		}
		return null;
	}

	public override BoundNode VisitImplicitIndexerAccess(BoundImplicitIndexerAccess node)
	{
		VisitRvalue(node.Receiver);
		VisitRvalue(node.Argument);
		return null;
	}

	public override BoundNode VisitEventAssignmentOperator(BoundEventAssignmentOperator node)
	{
		VisitRvalue(node.ReceiverOpt);
		VisitRvalue(node.Argument);
		return null;
	}

	protected virtual void VisitArguments(ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> refKindsOpt, MethodSymbol method, ImmutableArray<int> argsToParamsOpt, bool expanded)
	{
		VisitArgumentsBeforeCall(arguments, refKindsOpt);
		VisitArgumentsAfterCall(arguments, refKindsOpt, method, argsToParamsOpt, expanded);
	}

	private void VisitArgumentsBeforeCall(ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> refKindsOpt)
	{
		for (int i = 0; i < arguments.Length; i++)
		{
			RefKind refKind = GetRefKind(refKindsOpt, i);
			if (refKind != RefKind.Out)
			{
				VisitRvalue(arguments[i], refKind != RefKind.None);
			}
			else
			{
				VisitLvalue(arguments[i]);
			}
		}
	}

	private void VisitArgumentsAfterCall(ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> refKindsOpt, MethodSymbol? method, ImmutableArray<int> argsToParamsOpt, bool expanded)
	{
		for (int i = 0; i < arguments.Length; i++)
		{
			RefKind refKind = GetRefKind(refKindsOpt, i);
			switch (refKind)
			{
			case RefKind.Ref:
				if ((object)method != null)
				{
					ParameterSymbol? correspondingParameter = Binder.GetCorrespondingParameter(i, method.Parameters, argsToParamsOpt, expanded);
					if ((object)correspondingParameter != null && !correspondingParameter.RefKind.IsWritableReference())
					{
						break;
					}
				}
				goto case RefKind.Out;
			case RefKind.Out:
				WriteArgument(arguments[i], refKind, method);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(refKind);
			case RefKind.None:
			case RefKind.In:
			case RefKind.RefReadOnlyParameter:
			case (RefKind)5:
				break;
			}
		}
	}

	protected static RefKind GetRefKind(ImmutableArray<RefKind> refKindsOpt, int index)
	{
		if (!refKindsOpt.IsDefault && refKindsOpt.Length > index)
		{
			return refKindsOpt[index];
		}
		return RefKind.None;
	}

	protected virtual void WriteArgument(BoundExpression arg, RefKind refKind, MethodSymbol method)
	{
	}

	public override BoundNode VisitBadExpression(BoundBadExpression node)
	{
		foreach (BoundExpression childBoundNode in node.ChildBoundNodes)
		{
			VisitRvalue(childBoundNode);
		}
		return null;
	}

	public override BoundNode VisitBadStatement(BoundBadStatement node)
	{
		foreach (BoundNode childBoundNode in node.ChildBoundNodes)
		{
			if (childBoundNode is BoundStatement)
			{
				VisitStatement(childBoundNode as BoundStatement);
			}
			else
			{
				VisitRvalue(childBoundNode as BoundExpression);
			}
		}
		return null;
	}

	public override BoundNode VisitArrayInitialization(BoundArrayInitialization node)
	{
		foreach (BoundExpression initializer in node.Initializers)
		{
			VisitRvalue(initializer);
		}
		return null;
	}

	public override BoundNode VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		if (node.Argument is BoundMethodGroup boundMethodGroup)
		{
			if (node.MethodOpt?.OriginalDefinition is LocalFunctionSymbol symbol)
			{
				VisitLocalFunctionUse(symbol, node.Syntax, isCall: false);
			}
			else
			{
				MethodSymbol methodOpt = node.MethodOpt;
				if ((object)methodOpt != null)
				{
					BoundExpression receiverOpt = boundMethodGroup.ReceiverOpt;
					if (receiverOpt != null && !ignoreReceiver(methodOpt))
					{
						EnterRegionIfNeeded(boundMethodGroup);
						VisitRvalue(receiverOpt);
						LeaveRegionIfNeeded(boundMethodGroup);
					}
				}
			}
		}
		else
		{
			VisitRvalue(node.Argument);
		}
		return null;
		static bool ignoreReceiver(MethodSymbol method)
		{
			if (method.IsStatic)
			{
				return !method.IsExtensionMethod;
			}
			return false;
		}
	}

	public override BoundNode VisitTypeExpression(BoundTypeExpression node)
	{
		return null;
	}

	public override BoundNode VisitTypeOrValueExpression(BoundTypeOrValueExpression node)
	{
		return null;
	}

	public override BoundNode VisitLiteral(BoundLiteral node)
	{
		SplitIfBooleanConstant(node);
		return null;
	}

	public override BoundNode VisitUtf8String(BoundUtf8String node)
	{
		return null;
	}

	protected void SplitIfBooleanConstant(BoundExpression node)
	{
		ConstantValue constantValueOpt = node.ConstantValueOpt;
		if ((object)constantValueOpt == null || !constantValueOpt.IsBoolean)
		{
			return;
		}
		bool booleanValue = constantValueOpt.BooleanValue;
		if (node.Type.SpecialType == SpecialType.System_Boolean)
		{
			TLocalState val = UnreachableState();
			Split();
			if (booleanValue)
			{
				StateWhenFalse = val;
			}
			else
			{
				StateWhenTrue = val;
			}
		}
	}

	public override BoundNode VisitLocalId(BoundLocalId node)
	{
		return null;
	}

	public override BoundNode VisitParameterId(BoundParameterId node)
	{
		return null;
	}

	public override BoundNode VisitMethodDefIndex(BoundMethodDefIndex node)
	{
		return null;
	}

	public override BoundNode VisitStateMachineInstanceId(BoundStateMachineInstanceId node)
	{
		return null;
	}

	public override BoundNode VisitMaximumMethodDefIndex(BoundMaximumMethodDefIndex node)
	{
		return null;
	}

	public override BoundNode VisitModuleVersionId(BoundModuleVersionId node)
	{
		return null;
	}

	public override BoundNode VisitModuleVersionIdString(BoundModuleVersionIdString node)
	{
		return null;
	}

	public override BoundNode VisitInstrumentationPayloadRoot(BoundInstrumentationPayloadRoot node)
	{
		return null;
	}

	public override BoundNode VisitThrowIfModuleCancellationRequested(BoundThrowIfModuleCancellationRequested node)
	{
		return null;
	}

	public override BoundNode VisitSourceDocumentIndex(BoundSourceDocumentIndex node)
	{
		return null;
	}

	public override BoundNode VisitConversion(BoundConversion node)
	{
		if (node.ConversionKind == ConversionKind.MethodGroup)
		{
			if (node.IsExtensionMethod || ((object)node.SymbolOpt != null && node.SymbolOpt.RequiresInstanceReceiver))
			{
				BoundExpression receiverOpt = ((BoundMethodGroup)node.Operand).ReceiverOpt;
				EnterRegionIfNeeded(node.Operand);
				VisitRvalue(receiverOpt);
				LeaveRegionIfNeeded(node.Operand);
			}
			else if (node.SymbolOpt?.OriginalDefinition is LocalFunctionSymbol symbol)
			{
				VisitLocalFunctionUse(symbol, node.Syntax, isCall: false);
			}
		}
		else
		{
			Visit(node.Operand);
		}
		AfterVisitConversion(node);
		return null;
	}

	protected virtual void AfterVisitConversion(BoundConversion node)
	{
	}

	public sealed override BoundNode VisitIfStatement(BoundIfStatement node)
	{
		ArrayBuilder<(TLocalState, BoundIfStatement)> instance = ArrayBuilder<(TLocalState, BoundIfStatement)>.GetInstance();
		TLocalState stateWhenTrue;
		while (true)
		{
			VisitCondition(node.Condition);
			stateWhenTrue = StateWhenTrue;
			TLocalState stateWhenFalse = StateWhenFalse;
			SetState(stateWhenTrue);
			VisitStatement(node.Consequence);
			stateWhenTrue = State;
			SetState(stateWhenFalse);
			BoundStatement alternativeOpt = node.AlternativeOpt;
			if (alternativeOpt == null)
			{
				break;
			}
			if (alternativeOpt is BoundIfStatement boundIfStatement)
			{
				node = boundIfStatement;
				instance.Push((stateWhenTrue, node));
				EnterRegionIfNeeded(node);
				continue;
			}
			VisitStatement(alternativeOpt);
			break;
		}
		while (true)
		{
			Join(ref State, ref stateWhenTrue);
			if (!instance.Any())
			{
				break;
			}
			(stateWhenTrue, node) = instance.Pop();
			LeaveRegionIfNeeded(node);
		}
		instance.Free();
		return null;
	}

	public override BoundNode VisitTryStatement(BoundTryStatement node)
	{
		SavedPending oldPending = SavePending();
		TLocalState tryState = State.Clone();
		SavedPending oldPending2 = SavePending();
		VisitTryBlockWithAnyTransferFunction(node.TryBlock, node, ref tryState);
		TLocalState finallyState = tryState.Clone();
		TLocalState self = State;
		foreach (BoundCatchBlock catchBlock in node.CatchBlocks)
		{
			SetState(tryState.Clone());
			VisitCatchBlockWithAnyTransferFunction(catchBlock, ref finallyState);
			Join(ref self, ref State);
		}
		RestorePending(oldPending2);
		if (node.FinallyBlockOpt != null)
		{
			SetState(finallyState);
			SavedPending oldPending3 = SavePending();
			TLocalState stateMovedUp = ReachableBottomState();
			VisitFinallyBlockWithAnyTransferFunction(node.FinallyBlockOpt, ref stateMovedUp);
			foreach (PendingBranch item in oldPending3.PendingBranches.AsEnumerable())
			{
				if (item.Branch != null && item.Branch.Kind != BoundKind.YieldReturnStatement)
				{
					updatePendingBranchState(ref item.State, ref stateMovedUp);
					if (item.IsConditionalState)
					{
						updatePendingBranchState(ref item.StateWhenTrue, ref stateMovedUp);
						updatePendingBranchState(ref item.StateWhenFalse, ref stateMovedUp);
					}
				}
			}
			RestorePending(oldPending3);
			Meet(ref self, ref State);
			if (_nonMonotonicTransfer)
			{
				Join(ref self, ref stateMovedUp);
			}
		}
		SetState(self);
		RestorePending(oldPending);
		return null;
		void updatePendingBranchState(ref TLocalState stateToUpdate, ref TLocalState stateMovedUpInFinally)
		{
			Meet(ref stateToUpdate, ref State);
			if (_nonMonotonicTransfer)
			{
				Join(ref stateToUpdate, ref stateMovedUpInFinally);
			}
		}
	}

	protected virtual void JoinTryBlockState(ref TLocalState self, ref TLocalState other)
	{
		Join(ref self, ref other);
	}

	private void VisitTryBlockWithAnyTransferFunction(BoundStatement tryBlock, BoundTryStatement node, ref TLocalState tryState)
	{
		if (_nonMonotonicTransfer)
		{
			Optional<TLocalState> nonMonotonicState = NonMonotonicState;
			NonMonotonicState = ReachableBottomState();
			VisitTryBlock(tryBlock, node);
			TLocalState other = NonMonotonicState.Value;
			Join(ref tryState, ref other);
			if (nonMonotonicState.HasValue)
			{
				TLocalState self = nonMonotonicState.Value;
				JoinTryBlockState(ref self, ref other);
				nonMonotonicState = self;
			}
			NonMonotonicState = nonMonotonicState;
		}
		else
		{
			VisitTryBlock(tryBlock, node);
		}
	}

	protected virtual void VisitTryBlock(BoundStatement tryBlock, BoundTryStatement node)
	{
		VisitStatement(tryBlock);
	}

	private void VisitCatchBlockWithAnyTransferFunction(BoundCatchBlock catchBlock, ref TLocalState finallyState)
	{
		if (_nonMonotonicTransfer)
		{
			Optional<TLocalState> nonMonotonicState = NonMonotonicState;
			NonMonotonicState = ReachableBottomState();
			VisitCatchBlock(catchBlock);
			TLocalState other = NonMonotonicState.Value;
			Join(ref finallyState, ref other);
			if (nonMonotonicState.HasValue)
			{
				TLocalState self = nonMonotonicState.Value;
				JoinTryBlockState(ref self, ref other);
				nonMonotonicState = self;
			}
			NonMonotonicState = nonMonotonicState;
		}
		else
		{
			VisitCatchBlock(catchBlock);
		}
	}

	public override BoundNode VisitCatchBlock(BoundCatchBlock catchBlock)
	{
		if (catchBlock.ExceptionSourceOpt != null)
		{
			VisitLvalue(catchBlock.ExceptionSourceOpt);
		}
		if (catchBlock.ExceptionFilterPrologueOpt != null)
		{
			VisitStatementList(catchBlock.ExceptionFilterPrologueOpt);
		}
		if (catchBlock.ExceptionFilterOpt != null)
		{
			VisitCondition(catchBlock.ExceptionFilterOpt);
			SetState(StateWhenTrue);
		}
		VisitStatement(catchBlock.Body);
		return null;
	}

	private void VisitFinallyBlockWithAnyTransferFunction(BoundStatement finallyBlock, ref TLocalState stateMovedUp)
	{
		if (_nonMonotonicTransfer)
		{
			Optional<TLocalState> nonMonotonicState = NonMonotonicState;
			NonMonotonicState = ReachableBottomState();
			VisitFinallyBlock(finallyBlock);
			TLocalState other = NonMonotonicState.Value;
			Join(ref stateMovedUp, ref other);
			if (nonMonotonicState.HasValue)
			{
				TLocalState self = nonMonotonicState.Value;
				JoinTryBlockState(ref self, ref other);
				nonMonotonicState = self;
			}
			NonMonotonicState = nonMonotonicState;
		}
		else
		{
			VisitFinallyBlock(finallyBlock);
		}
	}

	protected virtual void VisitFinallyBlock(BoundStatement finallyBlock)
	{
		VisitStatement(finallyBlock);
	}

	public override BoundNode VisitExtractedFinallyBlock(BoundExtractedFinallyBlock node)
	{
		return VisitBlock(node.FinallyBlock);
	}

	public override BoundNode VisitReturnStatement(BoundReturnStatement node)
	{
		BoundNode result = VisitReturnStatementNoAdjust(node);
		PendingBranches.Add(new PendingBranch(node, State, null));
		SetUnreachable();
		return result;
	}

	protected virtual BoundNode VisitReturnStatementNoAdjust(BoundReturnStatement node)
	{
		VisitRvalue(node.ExpressionOpt, node.RefKind != RefKind.None);
		if (node.RefKind != RefKind.None)
		{
			WriteArgument(node.ExpressionOpt, node.RefKind, null);
		}
		return null;
	}

	public override BoundNode VisitThisReference(BoundThisReference node)
	{
		return null;
	}

	public override BoundNode VisitPreviousSubmissionReference(BoundPreviousSubmissionReference node)
	{
		return null;
	}

	public override BoundNode VisitHostObjectMemberReference(BoundHostObjectMemberReference node)
	{
		return null;
	}

	public override BoundNode VisitParameter(BoundParameter node)
	{
		return null;
	}

	protected virtual void VisitLvalueParameter(BoundParameter node)
	{
	}

	public override BoundNode VisitObjectCreationExpression(BoundObjectCreationExpression node)
	{
		VisitArguments(node.Arguments, node.ArgumentRefKindsOpt, node.Constructor, node.ArgsToParamsOpt, node.Expanded);
		VisitRvalue(node.InitializerExpressionOpt);
		return null;
	}

	public override BoundNode VisitCollectionExpression(BoundCollectionExpression node)
	{
		VisitCollectionExpression(node.Elements);
		return null;
	}

	public override BoundNode VisitUnconvertedCollectionExpression(BoundUnconvertedCollectionExpression node)
	{
		VisitCollectionExpression(node.Elements);
		return null;
	}

	private void VisitCollectionExpression(ImmutableArray<BoundNode> elements)
	{
		foreach (BoundNode item in elements)
		{
			if (item is BoundExpression node)
			{
				VisitRvalue(node);
			}
			else
			{
				Visit(item);
			}
		}
	}

	public override BoundNode VisitCollectionExpressionSpreadElement(BoundCollectionExpressionSpreadElement node)
	{
		VisitRvalue(node.Expression);
		return null;
	}

	public override BoundNode VisitNewT(BoundNewT node)
	{
		VisitRvalue(node.InitializerExpressionOpt);
		return null;
	}

	public override BoundNode VisitNoPiaObjectCreationExpression(BoundNoPiaObjectCreationExpression node)
	{
		VisitRvalue(node.InitializerExpressionOpt);
		return null;
	}

	protected virtual void PropertySetter(BoundExpression node, BoundExpression receiver, MethodSymbol setter, BoundExpression value = null)
	{
		VisitReceiverAfterCall(receiver, setter);
	}

	private bool RegularPropertyAccess(BoundExpression expr)
	{
		if (expr.Kind != BoundKind.PropertyAccess)
		{
			return false;
		}
		return !Binder.AccessingAutoPropertyFromConstructor((BoundPropertyAccess)expr, _symbol);
	}

	public override BoundNode VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		if (RegularPropertyAccess(node.Left))
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)node.Left;
			PropertySymbol propertySymbol = boundPropertyAccess.PropertySymbol;
			if (propertySymbol.RefKind == RefKind.None)
			{
				MethodSymbol writeMethod = GetWriteMethod(propertySymbol);
				VisitReceiverBeforeCall(boundPropertyAccess.ReceiverOpt, writeMethod);
				VisitRvalue(node.Right);
				PropertySetter(node, boundPropertyAccess.ReceiverOpt, writeMethod, node.Right);
				return null;
			}
		}
		VisitLvalue(node.Left);
		VisitRvalue(node.Right, node.IsRef);
		if (node.IsRef)
		{
			RefKind refKind = ((node.Left.Kind == BoundKind.BadExpression) ? RefKind.Ref : node.Left.GetRefKind());
			WriteArgument(node.Right, refKind, null);
		}
		return null;
	}

	public override BoundNode VisitDeconstructionAssignmentOperator(BoundDeconstructionAssignmentOperator node)
	{
		VisitLvalue(node.Left);
		VisitRvalue(node.Right);
		return null;
	}

	public sealed override BoundNode VisitOutDeconstructVarPendingInference(OutDeconstructVarPendingInference node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/AbstractFlowPass.cs", 2182);
	}

	public override BoundNode VisitCompoundAssignmentOperator(BoundCompoundAssignmentOperator node)
	{
		VisitCompoundAssignmentTarget(node);
		VisitRvalue(node.Right);
		AfterRightHasBeenVisited(node);
		return null;
	}

	protected void VisitCompoundAssignmentTarget(BoundCompoundAssignmentOperator node)
	{
		if (RegularPropertyAccess(node.Left))
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)node.Left;
			PropertySymbol propertySymbol = boundPropertyAccess.PropertySymbol;
			if (propertySymbol.RefKind == RefKind.None)
			{
				MethodSymbol readMethod = GetReadMethod(propertySymbol);
				VisitReceiverBeforeCall(boundPropertyAccess.ReceiverOpt, readMethod);
				VisitReceiverAfterCall(boundPropertyAccess.ReceiverOpt, readMethod);
				return;
			}
		}
		VisitRvalue(node.Left, isKnownToBeAnLvalue: true);
	}

	protected void AfterRightHasBeenVisited(BoundCompoundAssignmentOperator node)
	{
		if (RegularPropertyAccess(node.Left))
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)node.Left;
			PropertySymbol propertySymbol = boundPropertyAccess.PropertySymbol;
			if (propertySymbol.RefKind == RefKind.None)
			{
				MethodSymbol writeMethod = GetWriteMethod(propertySymbol);
				PropertySetter(node, boundPropertyAccess.ReceiverOpt, writeMethod);
				VisitReceiverAfterCall(boundPropertyAccess.ReceiverOpt, writeMethod);
			}
		}
	}

	public override BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		VisitFieldAccessInternal(node.ReceiverOpt, node.FieldSymbol);
		SplitIfBooleanConstant(node);
		return null;
	}

	private void VisitFieldAccessInternal(BoundExpression receiverOpt, FieldSymbol fieldSymbol)
	{
		if ((object)fieldSymbol != null && (fieldSymbol.IsFixedSizeBuffer || (!fieldSymbol.IsStatic && fieldSymbol.ContainingType.TypeKind == TypeKind.Struct && receiverOpt != null && receiverOpt.Kind != BoundKind.TypeExpression && (object)receiverOpt.Type != null && !receiverOpt.Type.IsPrimitiveRecursiveStruct())))
		{
			VisitLvalue(receiverOpt);
		}
		else
		{
			VisitRvalue(receiverOpt);
		}
	}

	public override BoundNode VisitFieldInfo(BoundFieldInfo node)
	{
		return null;
	}

	public override BoundNode VisitMethodInfo(BoundMethodInfo node)
	{
		return null;
	}

	public override BoundNode VisitPropertyAccess(BoundPropertyAccess node)
	{
		PropertySymbol propertySymbol = node.PropertySymbol;
		if (Binder.AccessingAutoPropertyFromConstructor(node, _symbol))
		{
			SynthesizedBackingFieldSymbol synthesizedBackingFieldSymbol = (propertySymbol as SourcePropertySymbolBase)?.BackingField;
			if (synthesizedBackingFieldSymbol != null)
			{
				VisitFieldAccessInternal(node.ReceiverOpt, synthesizedBackingFieldSymbol);
				return null;
			}
		}
		MethodSymbol readMethod = GetReadMethod(propertySymbol);
		VisitReceiverBeforeCall(node.ReceiverOpt, readMethod);
		VisitReceiverAfterCall(node.ReceiverOpt, readMethod);
		return null;
	}

	public override BoundNode VisitEventAccess(BoundEventAccess node)
	{
		VisitFieldAccessInternal(node.ReceiverOpt, node.EventSymbol.AssociatedField);
		return null;
	}

	public override BoundNode VisitRangeVariable(BoundRangeVariable node)
	{
		return null;
	}

	public override BoundNode VisitQueryClause(BoundQueryClause node)
	{
		VisitRvalue(node.UnoptimizedForm ?? node.Value);
		return null;
	}

	private BoundNode VisitMultipleLocalDeclarationsBase(BoundMultipleLocalDeclarationsBase node)
	{
		foreach (BoundLocalDeclaration localDeclaration in node.LocalDeclarations)
		{
			Visit(localDeclaration);
		}
		return null;
	}

	public override BoundNode VisitMultipleLocalDeclarations(BoundMultipleLocalDeclarations node)
	{
		return VisitMultipleLocalDeclarationsBase(node);
	}

	public override BoundNode VisitUsingLocalDeclarations(BoundUsingLocalDeclarations node)
	{
		if (AwaitUsingAndForeachAddsPendingBranch && node.AwaitOpt != null)
		{
			PendingBranches.Add(new PendingBranch(node, State, null));
		}
		return VisitMultipleLocalDeclarationsBase(node);
	}

	public override BoundNode VisitWhileStatement(BoundWhileStatement node)
	{
		LoopHead(node);
		VisitCondition(node.Condition);
		TLocalState stateWhenTrue = StateWhenTrue;
		TLocalState stateWhenFalse = StateWhenFalse;
		SetState(stateWhenTrue);
		VisitStatement(node.Body);
		ResolveContinues(node.ContinueLabel);
		LoopTail(node);
		ResolveBreaks(stateWhenFalse, node.BreakLabel);
		return null;
	}

	public override BoundNode VisitWithExpression(BoundWithExpression node)
	{
		VisitRvalue(node.Receiver);
		VisitObjectOrCollectionInitializerExpression(node.InitializerExpression.Initializers);
		return null;
	}

	public override BoundNode VisitArrayAccess(BoundArrayAccess node)
	{
		VisitRvalue(node.Expression);
		foreach (BoundExpression index in node.Indices)
		{
			VisitRvalue(index);
		}
		return null;
	}

	public override BoundNode VisitInlineArrayAccess(BoundInlineArrayAccess node)
	{
		VisitRvalue(node.Expression);
		VisitRvalue(node.Argument);
		AfterVisitInlineArrayAccess(node);
		return null;
	}

	protected virtual void AfterVisitInlineArrayAccess(BoundInlineArrayAccess node)
	{
	}

	protected virtual void VisitLvalue(BoundInlineArrayAccess access)
	{
		VisitLvalue(access.Expression);
		VisitRvalue(access.Argument);
	}

	public override BoundNode VisitBinaryOperator(BoundBinaryOperator node)
	{
		if (node.OperatorKind.IsLogical())
		{
			VisitBinaryLogicalOperatorChildren(node);
		}
		else
		{
			InterpolatedStringHandlerData? interpolatedStringHandlerData = node.InterpolatedStringHandlerData;
			if (interpolatedStringHandlerData.HasValue)
			{
				interpolatedStringHandlerData.GetValueOrDefault();
				VisitBinaryInterpolatedStringAddition(node);
			}
			else
			{
				VisitBinaryOperatorChildren(node);
			}
		}
		return null;
	}

	public override BoundNode VisitUserDefinedConditionalLogicalOperator(BoundUserDefinedConditionalLogicalOperator node)
	{
		VisitBinaryLogicalOperatorChildren(node);
		return null;
	}

	private void VisitBinaryLogicalOperatorChildren(BoundExpression node)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		BoundExpression boundExpression = node;
		while (true)
		{
			BoundExpression boundExpression2;
			switch (boundExpression.Kind)
			{
			case BoundKind.BinaryOperator:
			{
				BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)boundExpression;
				if (boundBinaryOperator.OperatorKind.IsLogical())
				{
					boundExpression2 = boundExpression;
					boundExpression = boundBinaryOperator.Left;
					goto IL_0049;
				}
				break;
			}
			case BoundKind.UserDefinedConditionalLogicalOperator:
				boundExpression2 = boundExpression;
				boundExpression = ((BoundUserDefinedConditionalLogicalOperator)boundExpression2).Left;
				goto IL_0049;
			}
			break;
			IL_0049:
			instance.Push(boundExpression2);
		}
		VisitBinaryLogicalOperatorChildren(instance);
		instance.Free();
	}

	protected virtual void VisitBinaryLogicalOperatorChildren(ArrayBuilder<BoundExpression> stack)
	{
		BoundExpression boundExpression = stack.Pop();
		VisitCondition(boundExpression.Kind switch
		{
			BoundKind.BinaryOperator => ((BoundBinaryOperator)boundExpression).Left, 
			BoundKind.UserDefinedConditionalLogicalOperator => ((BoundUserDefinedConditionalLogicalOperator)boundExpression).Left, 
			_ => throw ExceptionUtilities.UnexpectedValue(boundExpression.Kind), 
		});
		while (true)
		{
			BinaryOperatorKind operatorKind;
			BoundExpression right;
			switch (boundExpression.Kind)
			{
			case BoundKind.BinaryOperator:
			{
				BoundBinaryOperator obj2 = (BoundBinaryOperator)boundExpression;
				operatorKind = obj2.OperatorKind;
				right = obj2.Right;
				break;
			}
			case BoundKind.UserDefinedConditionalLogicalOperator:
			{
				BoundUserDefinedConditionalLogicalOperator obj = (BoundUserDefinedConditionalLogicalOperator)boundExpression;
				operatorKind = obj.OperatorKind;
				right = obj.Right;
				break;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(boundExpression.Kind);
			}
			bool flag = operatorKind.Operator() == BinaryOperatorKind.And;
			bool isBool = operatorKind.OperandTypes() == BinaryOperatorKind.Bool;
			TLocalState leftTrue = StateWhenTrue;
			TLocalState leftFalse = StateWhenFalse;
			SetState(flag ? leftTrue : leftFalse);
			AfterLeftChildOfBinaryLogicalOperatorHasBeenVisited(boundExpression, right, flag, isBool, ref leftTrue, ref leftFalse);
			if (stack.Count != 0)
			{
				AdjustConditionalState(boundExpression);
				boundExpression = stack.Pop();
				continue;
			}
			break;
		}
	}

	protected virtual void AfterLeftChildOfBinaryLogicalOperatorHasBeenVisited(BoundExpression binary, BoundExpression right, bool isAnd, bool isBool, ref TLocalState leftTrue, ref TLocalState leftFalse)
	{
		Visit(right);
		AfterRightChildOfBinaryLogicalOperatorHasBeenVisited(right, isAnd, isBool, ref leftTrue, ref leftFalse);
	}

	protected void AfterRightChildOfBinaryLogicalOperatorHasBeenVisited(BoundExpression right, bool isAnd, bool isBool, ref TLocalState leftTrue, ref TLocalState leftFalse)
	{
		AdjustConditionalState(right);
		if (!isBool)
		{
			Unsplit();
			Split();
		}
		TLocalState self = StateWhenTrue;
		TLocalState self2 = StateWhenFalse;
		if (isAnd)
		{
			Join(ref self2, ref leftFalse);
		}
		else
		{
			Join(ref self, ref leftTrue);
		}
		SetConditionalState(self, self2);
		if (!isBool)
		{
			Unsplit();
		}
	}

	private void VisitBinaryOperatorChildren(BoundBinaryOperator node)
	{
		ArrayBuilder<BoundBinaryOperator> instance = ArrayBuilder<BoundBinaryOperator>.GetInstance();
		BoundBinaryOperator boundBinaryOperator = node;
		do
		{
			instance.Push(boundBinaryOperator);
			EnterRegionIfNeeded(boundBinaryOperator);
			boundBinaryOperator = boundBinaryOperator.Left as BoundBinaryOperator;
		}
		while (boundBinaryOperator != null && !boundBinaryOperator.OperatorKind.IsLogical() && !boundBinaryOperator.InterpolatedStringHandlerData.HasValue);
		VisitBinaryOperatorChildren(instance);
		instance.Free();
	}

	protected virtual void VisitBinaryOperatorChildren(ArrayBuilder<BoundBinaryOperator> stack)
	{
		BoundBinaryOperator boundBinaryOperator = stack.Pop();
		if (VisitPossibleConditionalAccess(boundBinaryOperator.Left, out TLocalState stateWhenNotNull) && canLearnFromOperator(boundBinaryOperator) && isKnownNullOrNotNull(boundBinaryOperator.Right))
		{
			if (_nonMonotonicTransfer)
			{
				Optional<TLocalState> nonMonotonicState = NonMonotonicState;
				NonMonotonicState = ReachableBottomState();
				VisitRvalue(boundBinaryOperator.Right);
				TLocalState other = NonMonotonicState.Value;
				Join(ref stateWhenNotNull, ref other);
				if (nonMonotonicState.HasValue)
				{
					TLocalState self = nonMonotonicState.Value;
					Join(ref self, ref other);
					nonMonotonicState = self;
				}
				NonMonotonicState = nonMonotonicState;
			}
			else
			{
				VisitRvalue(boundBinaryOperator.Right);
				Meet(ref stateWhenNotNull, ref State);
			}
			bool flag = boundBinaryOperator.Right.ConstantValueOpt?.IsNull ?? false;
			SetConditionalState((flag == isEquals(boundBinaryOperator)) ? (whenTrue: State, whenFalse: stateWhenNotNull) : (whenTrue: stateWhenNotNull, whenFalse: State));
			LeaveRegionIfNeeded(boundBinaryOperator);
			if (stack.Count == 0)
			{
				return;
			}
			boundBinaryOperator = stack.Pop();
		}
		while (true)
		{
			if (!canLearnFromOperator(boundBinaryOperator) || !learnFromOperator(boundBinaryOperator))
			{
				Unsplit();
				VisitRvalue(boundBinaryOperator.Right);
			}
			LeaveRegionIfNeeded(boundBinaryOperator);
			if (stack.Count != 0)
			{
				boundBinaryOperator = stack.Pop();
				continue;
			}
			break;
		}
		static bool canLearnFromOperator(BoundBinaryOperator binary)
		{
			BinaryOperatorKind operatorKind = binary.OperatorKind;
			BinaryOperatorKind binaryOperatorKind = operatorKind.Operator();
			if ((binaryOperatorKind == BinaryOperatorKind.Equal || binaryOperatorKind == BinaryOperatorKind.NotEqual) ? true : false)
			{
				if (operatorKind.IsUserDefined())
				{
					return operatorKind.IsLifted();
				}
				return true;
			}
			return false;
		}
		static bool isEquals(BoundBinaryOperator binary)
		{
			return binary.OperatorKind.Operator() == BinaryOperatorKind.Equal;
		}
		static bool isKnownNullOrNotNull(BoundExpression expr)
		{
			bool flag2 = (object)expr.ConstantValueOpt != null;
			BoundConversion boundConversion;
			bool flag3;
			if (!flag2)
			{
				boundConversion = expr as BoundConversion;
				if (boundConversion != null)
				{
					ConversionKind conversionKind = boundConversion.ConversionKind;
					if (conversionKind == ConversionKind.ImplicitNullable || conversionKind == ConversionKind.ExplicitNullable)
					{
						flag3 = true;
						goto IL_002e;
					}
				}
				flag3 = false;
				goto IL_002e;
			}
			goto IL_0045;
			IL_0045:
			return flag2;
			IL_002e:
			flag2 = flag3 && boundConversion.Operand.Type.IsNonNullableValueType();
			goto IL_0045;
		}
		bool learnFromOperator(BoundBinaryOperator binary)
		{
			if (isKnownNullOrNotNull(binary.Left) && TryVisitConditionalAccess(binary.Right, out TLocalState stateWhenNotNull2))
			{
				bool flag2 = binary.Left.ConstantValueOpt?.IsNull ?? false;
				SetConditionalState((flag2 == isEquals(binary)) ? (whenTrue: State, whenFalse: stateWhenNotNull2) : (whenTrue: stateWhenNotNull2, whenFalse: State));
				return true;
			}
			if (IsConditionalState)
			{
				ConstantValue constantValueOpt = binary.Right.ConstantValueOpt;
				if ((object)constantValueOpt != null && constantValueOpt.IsBoolean)
				{
					TLocalState val = StateWhenTrue.Clone();
					TLocalState val2 = StateWhenFalse.Clone();
					TLocalState val3 = val;
					Unsplit();
					Visit(binary.Right);
					SetConditionalState((isEquals(binary) == constantValueOpt.BooleanValue) ? (whenTrue: val3, whenFalse: val2) : (whenTrue: val2, whenFalse: val3));
					return true;
				}
			}
			ConstantValue constantValueOpt2 = binary.Left.ConstantValueOpt;
			if ((object)constantValueOpt2 != null && constantValueOpt2.IsBoolean)
			{
				Unsplit();
				Visit(binary.Right);
				if (IsConditionalState && isEquals(binary) != constantValueOpt2.BooleanValue)
				{
					SetConditionalState(StateWhenFalse, StateWhenTrue);
				}
				return true;
			}
			return false;
		}
	}

	protected void VisitBinaryInterpolatedStringAddition(BoundBinaryOperator node)
	{
		ArrayBuilder<BoundInterpolatedString> instance = ArrayBuilder<BoundInterpolatedString>.GetInstance();
		InterpolatedStringHandlerData valueOrDefault = node.InterpolatedStringHandlerData.GetValueOrDefault();
		node.VisitBinaryOperatorInterpolatedString<BoundInterpolatedString, (ArrayBuilder<BoundInterpolatedString>, AbstractFlowPass<TLocalState, TLocalFunctionState>)>((instance, this), delegate(BoundInterpolatedString interpolatedString, (ArrayBuilder<BoundInterpolatedString> parts, AbstractFlowPass<TLocalState, TLocalFunctionState> @this) arg)
		{
			arg.parts.Add(interpolatedString);
			return true;
		}, delegate(BoundBinaryOperator op, (ArrayBuilder<BoundInterpolatedString> parts, AbstractFlowPass<TLocalState, TLocalFunctionState> @this) arg)
		{
			arg.@this.VisitInterpolatedStringBinaryOperatorNode(op);
		});
		VisitInterpolatedStringHandlerConstructor(valueOrDefault.Construction);
		bool flag = false;
		bool hasTrailingHandlerValidityParameter = valueOrDefault.HasTrailingHandlerValidityParameter;
		bool flag2 = valueOrDefault.UsesBoolReturns | hasTrailingHandlerValidityParameter;
		TLocalState shortCircuitState = (flag2 ? State.Clone() : default(TLocalState));
		foreach (BoundInterpolatedString item in instance)
		{
			flag |= VisitInterpolatedStringHandlerParts(item, valueOrDefault.UsesBoolReturns, flag | hasTrailingHandlerValidityParameter, ref shortCircuitState);
		}
		if (flag2)
		{
			Join(ref State, ref shortCircuitState);
		}
		instance.Free();
	}

	protected virtual void VisitInterpolatedStringBinaryOperatorNode(BoundBinaryOperator node)
	{
	}

	protected virtual bool VisitInterpolatedStringHandlerParts(BoundInterpolatedStringBase node, bool usesBoolReturns, bool firstPartIsConditional, ref TLocalState? shortCircuitState)
	{
		ImmutableArray<BoundExpression> parts = node.Parts;
		if (parts.IsEmpty)
		{
			return false;
		}
		ReadOnlySpan<BoundExpression> readOnlySpan;
		ReadOnlySpan<BoundExpression> readOnlySpan2;
		if (firstPartIsConditional)
		{
			parts = node.Parts;
			readOnlySpan = parts.AsSpan();
		}
		else
		{
			parts = node.Parts;
			VisitRvalue(parts[0]);
			shortCircuitState = State.Clone();
			parts = node.Parts;
			readOnlySpan2 = parts.AsSpan();
			readOnlySpan = readOnlySpan2.Slice(1, readOnlySpan2.Length - 1);
		}
		readOnlySpan2 = readOnlySpan;
		for (int i = 0; i < readOnlySpan2.Length; i++)
		{
			BoundExpression node2 = readOnlySpan2[i];
			VisitRvalue(node2);
			if (usesBoolReturns)
			{
				Join(ref shortCircuitState, ref State);
			}
		}
		return true;
	}

	public override BoundNode VisitUnaryOperator(BoundUnaryOperator node)
	{
		if (node.OperatorKind == UnaryOperatorKind.BoolLogicalNegation)
		{
			VisitCondition(node.Operand);
			SetConditionalState(StateWhenFalse, StateWhenTrue);
		}
		else
		{
			VisitRvalue(node.Operand);
		}
		return null;
	}

	public override BoundNode VisitRangeExpression(BoundRangeExpression node)
	{
		if (node.LeftOperandOpt != null)
		{
			VisitRvalue(node.LeftOperandOpt);
		}
		if (node.RightOperandOpt != null)
		{
			VisitRvalue(node.RightOperandOpt);
		}
		return null;
	}

	public override BoundNode VisitFromEndIndexExpression(BoundFromEndIndexExpression node)
	{
		VisitRvalue(node.Operand);
		return null;
	}

	public override BoundNode VisitAwaitExpression(BoundAwaitExpression node)
	{
		VisitRvalue(node.Expression);
		PendingBranches.Add(new PendingBranch(node, State, null));
		return null;
	}

	public override BoundNode VisitIncrementOperator(BoundIncrementOperator node)
	{
		if (RegularPropertyAccess(node.Operand))
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)node.Operand;
			PropertySymbol propertySymbol = boundPropertyAccess.PropertySymbol;
			if (propertySymbol.RefKind == RefKind.None)
			{
				MethodSymbol readMethod = GetReadMethod(propertySymbol);
				MethodSymbol writeMethod = GetWriteMethod(propertySymbol);
				VisitReceiverBeforeCall(boundPropertyAccess.ReceiverOpt, readMethod);
				VisitReceiverAfterCall(boundPropertyAccess.ReceiverOpt, readMethod);
				PropertySetter(node, boundPropertyAccess.ReceiverOpt, writeMethod);
				return null;
			}
		}
		VisitRvalue(node.Operand);
		return null;
	}

	public override BoundNode VisitArrayCreation(BoundArrayCreation node)
	{
		foreach (BoundExpression bound in node.Bounds)
		{
			VisitRvalue(bound);
		}
		VisitRvalue(node.InitializerOpt);
		return null;
	}

	public override BoundNode VisitForStatement(BoundForStatement node)
	{
		if (node.Initializer != null)
		{
			VisitStatement(node.Initializer);
		}
		LoopHead(node);
		TLocalState state;
		TLocalState breakState;
		if (node.Condition != null)
		{
			VisitCondition(node.Condition);
			state = StateWhenTrue;
			breakState = StateWhenFalse;
		}
		else
		{
			state = State;
			breakState = UnreachableState();
		}
		SetState(state);
		VisitStatement(node.Body);
		ResolveContinues(node.ContinueLabel);
		if (node.Increment != null)
		{
			VisitStatement(node.Increment);
		}
		LoopTail(node);
		ResolveBreaks(breakState, node.BreakLabel);
		return null;
	}

	public override BoundNode VisitForEachStatement(BoundForEachStatement node)
	{
		VisitForEachExpression(node);
		LoopHead(node);
		TLocalState breakState = State.Clone();
		VisitForEachIterationVariables(node);
		VisitStatement(node.Body);
		ResolveContinues(node.ContinueLabel);
		LoopTail(node);
		ResolveBreaks(breakState, node.BreakLabel);
		if (AwaitUsingAndForeachAddsPendingBranch && ((CommonForEachStatementSyntax)node.Syntax).AwaitKeyword != default(SyntaxToken))
		{
			PendingBranches.Add(new PendingBranch(node, State, null));
		}
		return null;
	}

	protected virtual void VisitForEachExpression(BoundForEachStatement node)
	{
		VisitRvalue(node.Expression);
	}

	public virtual void VisitForEachIterationVariables(BoundForEachStatement node)
	{
	}

	public override BoundNode VisitAsOperator(BoundAsOperator node)
	{
		VisitRvalue(node.Operand);
		return null;
	}

	public override BoundNode VisitIsOperator(BoundIsOperator node)
	{
		if (VisitPossibleConditionalAccess(node.Operand, out TLocalState stateWhenNotNull))
		{
			SetConditionalState(stateWhenNotNull, State);
		}
		else
		{
			Unsplit();
		}
		return null;
	}

	public override BoundNode VisitMethodGroup(BoundMethodGroup node)
	{
		if (node.ReceiverOpt != null)
		{
			VisitRvalue(node.ReceiverOpt);
		}
		return null;
	}

	public override BoundNode? VisitNullCoalescingOperator(BoundNullCoalescingOperator node)
	{
		if (IsConstantNull(node.LeftOperand))
		{
			VisitRvalue(node.LeftOperand);
			Visit(node.RightOperand);
		}
		else
		{
			TLocalState other;
			if (VisitPossibleConditionalAccess(node.LeftOperand, out TLocalState stateWhenNotNull))
			{
				other = stateWhenNotNull;
			}
			else
			{
				Unsplit();
				other = State.Clone();
			}
			if (node.LeftOperand.ConstantValueOpt != null)
			{
				SetUnreachable();
			}
			Visit(node.RightOperand);
			if (IsConditionalState)
			{
				Join(ref StateWhenTrue, ref other);
				Join(ref StateWhenFalse, ref other);
			}
			else
			{
				Join(ref State, ref other);
			}
		}
		return null;
	}

	private bool TryVisitConditionalAccess(BoundExpression node, [NotNullWhen(true)] out TLocalState? stateWhenNotNull)
	{
		BoundConditionalAccess boundConditionalAccess = ((node is BoundConditionalAccess boundConditionalAccess2) ? boundConditionalAccess2 : ((!(node is BoundConversion { Conversion: var conversion, Operand: BoundConditionalAccess operand }) || !CanPropagateStateWhenNotNull(conversion)) ? null : operand));
		BoundConditionalAccess boundConditionalAccess3 = boundConditionalAccess;
		if (boundConditionalAccess3 != null)
		{
			EnterRegionIfNeeded(boundConditionalAccess3);
			Unsplit();
			VisitConditionalAccess(boundConditionalAccess3, out stateWhenNotNull);
			LeaveRegionIfNeeded(boundConditionalAccess3);
			return true;
		}
		stateWhenNotNull = default(TLocalState);
		return false;
	}

	protected static bool CanPropagateStateWhenNotNull(Conversion conversion)
	{
		if (!conversion.IsValid)
		{
			return false;
		}
		if (!conversion.IsUserDefined)
		{
			return true;
		}
		return conversion.Method.Parameters[0].Type.IsNonNullableValueType();
	}

	private bool VisitPossibleConditionalAccess(BoundExpression node, [NotNullWhen(true)] out TLocalState? stateWhenNotNull)
	{
		if (TryVisitConditionalAccess(node, out stateWhenNotNull))
		{
			return true;
		}
		Visit(node);
		return false;
	}

	private void VisitConditionalAccess(BoundConditionalAccess node, out TLocalState stateWhenNotNull)
	{
		if (VisitPossibleConditionalAccess(node.Receiver, out TLocalState stateWhenNotNull2))
		{
			stateWhenNotNull = stateWhenNotNull2;
		}
		else
		{
			Unsplit();
			stateWhenNotNull = State.Clone();
		}
		if (node.Receiver.ConstantValueOpt != null && !IsConstantNull(node.Receiver))
		{
			if (VisitPossibleConditionalAccess(node.AccessExpression, out TLocalState stateWhenNotNull3))
			{
				stateWhenNotNull = stateWhenNotNull3;
				return;
			}
			Unsplit();
			stateWhenNotNull = State.Clone();
			return;
		}
		TLocalState self = State.Clone();
		if (IsConstantNull(node.Receiver))
		{
			SetUnreachable();
		}
		else
		{
			SetState(stateWhenNotNull);
		}
		BoundExpression accessExpression = node.AccessExpression;
		while (accessExpression is BoundConditionalAccess boundConditionalAccess)
		{
			VisitRvalue(boundConditionalAccess.Receiver);
			accessExpression = boundConditionalAccess.AccessExpression;
			Join(ref self, ref State);
		}
		VisitRvalue(accessExpression);
		stateWhenNotNull = State;
		State = self;
		Join(ref State, ref stateWhenNotNull);
	}

	public override BoundNode? VisitConditionalAccess(BoundConditionalAccess node)
	{
		VisitConditionalAccess(node, out var _);
		return null;
	}

	public override BoundNode VisitLoweredConditionalAccess(BoundLoweredConditionalAccess node)
	{
		VisitRvalue(node.Receiver);
		TLocalState other = State.Clone();
		VisitRvalue(node.WhenNotNull);
		Join(ref State, ref other);
		if (node.WhenNullOpt != null)
		{
			other = State.Clone();
			VisitRvalue(node.WhenNullOpt);
			Join(ref State, ref other);
		}
		return null;
	}

	public override BoundNode VisitConditionalReceiver(BoundConditionalReceiver node)
	{
		return null;
	}

	public override BoundNode VisitComplexConditionalReceiver(BoundComplexConditionalReceiver node)
	{
		TLocalState other = State.Clone();
		VisitRvalue(node.ValueTypeReceiver);
		Join(ref State, ref other);
		other = State.Clone();
		VisitRvalue(node.ReferenceTypeReceiver);
		Join(ref State, ref other);
		return null;
	}

	public override BoundNode VisitSequence(BoundSequence node)
	{
		ImmutableArray<BoundExpression> sideEffects = node.SideEffects;
		if (!sideEffects.IsEmpty)
		{
			foreach (BoundExpression item in sideEffects)
			{
				VisitRvalue(item);
			}
		}
		Visit(node.Value);
		return null;
	}

	public override BoundNode VisitSequencePoint(BoundSequencePoint node)
	{
		if (node.StatementOpt != null)
		{
			VisitStatement(node.StatementOpt);
		}
		return null;
	}

	public override BoundNode VisitSequencePointExpression(BoundSequencePointExpression node)
	{
		Visit(node.Expression);
		return null;
	}

	public override BoundNode VisitSequencePointWithSpan(BoundSequencePointWithSpan node)
	{
		if (node.StatementOpt != null)
		{
			VisitStatement(node.StatementOpt);
		}
		return null;
	}

	public override BoundNode VisitModuleCancellationTokenExpression(ModuleCancellationTokenExpression node)
	{
		return null;
	}

	public override BoundNode VisitStatementList(BoundStatementList node)
	{
		return VisitStatementListWorker(node);
	}

	private BoundNode VisitStatementListWorker(BoundStatementList node)
	{
		foreach (BoundStatement statement in node.Statements)
		{
			VisitStatement(statement);
		}
		return null;
	}

	public override BoundNode VisitTypeOrInstanceInitializers(BoundTypeOrInstanceInitializers node)
	{
		return VisitStatementListWorker(node);
	}

	public override BoundNode VisitUnboundLambda(UnboundLambda node)
	{
		return VisitLambda(node.BindForErrorRecovery());
	}

	public override BoundNode VisitBreakStatement(BoundBreakStatement node)
	{
		PendingBranches.Add(new PendingBranch(node, State, node.Label));
		SetUnreachable();
		return null;
	}

	public override BoundNode VisitContinueStatement(BoundContinueStatement node)
	{
		PendingBranches.Add(new PendingBranch(node, State, node.Label));
		SetUnreachable();
		return null;
	}

	public override BoundNode VisitUnconvertedConditionalOperator(BoundUnconvertedConditionalOperator node)
	{
		return VisitConditionalOperatorCore(node, isByRef: false, node.Condition, node.Consequence, node.Alternative);
	}

	public override BoundNode VisitConditionalOperator(BoundConditionalOperator node)
	{
		return VisitConditionalOperatorCore(node, node.IsRef, node.Condition, node.Consequence, node.Alternative);
	}

	protected virtual BoundNode? VisitConditionalOperatorCore(BoundExpression node, bool isByRef, BoundExpression condition, BoundExpression consequence, BoundExpression alternative)
	{
		VisitCondition(condition);
		TLocalState stateWhenTrue = StateWhenTrue;
		TLocalState stateWhenFalse = StateWhenFalse;
		if (IsConstantTrue(condition))
		{
			VisitConditionalOperand(stateWhenFalse, alternative, isByRef);
			VisitConditionalOperand(stateWhenTrue, consequence, isByRef);
		}
		else if (IsConstantFalse(condition))
		{
			VisitConditionalOperand(stateWhenTrue, consequence, isByRef);
			VisitConditionalOperand(stateWhenFalse, alternative, isByRef);
		}
		else
		{
			VisitConditionalOperand(stateWhenTrue, consequence, isByRef);
			bool isConditionalState = IsConditionalState;
			TLocalState other;
			TLocalState other2;
			if (!isConditionalState)
			{
				TLocalState state = State;
				TLocalState state2 = State;
				other = state2;
				other2 = state;
			}
			else
			{
				TLocalState stateWhenTrue2 = StateWhenTrue;
				TLocalState state2 = StateWhenFalse;
				other = state2;
				other2 = stateWhenTrue2;
			}
			VisitConditionalOperand(stateWhenFalse, alternative, isByRef);
			if (!isConditionalState && !IsConditionalState)
			{
				Join(ref State, ref other2);
			}
			else
			{
				Split();
				Join(ref StateWhenTrue, ref other2);
				Join(ref StateWhenFalse, ref other);
			}
		}
		return null;
	}

	private void VisitConditionalOperand(TLocalState state, BoundExpression operand, bool isByRef)
	{
		SetState(state);
		if (isByRef)
		{
			VisitLvalue(operand);
			WriteArgument(operand, RefKind.Ref, null);
		}
		else
		{
			Visit(operand);
		}
	}

	public override BoundNode VisitBaseReference(BoundBaseReference node)
	{
		return null;
	}

	public override BoundNode VisitDoStatement(BoundDoStatement node)
	{
		LoopHead(node);
		VisitStatement(node.Body);
		ResolveContinues(node.ContinueLabel);
		VisitCondition(node.Condition);
		TLocalState stateWhenFalse = StateWhenFalse;
		SetState(StateWhenTrue);
		LoopTail(node);
		ResolveBreaks(stateWhenFalse, node.BreakLabel);
		return null;
	}

	public override BoundNode VisitGotoStatement(BoundGotoStatement node)
	{
		PendingBranches.Add(new PendingBranch(node, State, node.Label));
		SetUnreachable();
		return null;
	}

	protected void VisitLabel(LabelSymbol label, BoundStatement node)
	{
		ResolveBranches(label, node);
		TLocalState other = LabelState(label);
		Join(ref State, ref other);
		_labels[label] = State.Clone();
		_labelsSeen.Add(node);
	}

	protected virtual void VisitLabel(BoundLabeledStatement node)
	{
		VisitLabel(node.Label, node);
	}

	public override BoundNode VisitLabelStatement(BoundLabelStatement node)
	{
		VisitLabel(node.Label, node);
		return null;
	}

	public override BoundNode VisitLabeledStatement(BoundLabeledStatement node)
	{
		VisitLabel(node);
		VisitStatement(node.Body);
		return null;
	}

	public override BoundNode VisitLockStatement(BoundLockStatement node)
	{
		VisitRvalue(node.Argument);
		VisitStatement(node.Body);
		return null;
	}

	public override BoundNode VisitNoOpStatement(BoundNoOpStatement node)
	{
		return null;
	}

	public override BoundNode VisitNamespaceExpression(BoundNamespaceExpression node)
	{
		return null;
	}

	public override BoundNode VisitUsingStatement(BoundUsingStatement node)
	{
		if (node.ExpressionOpt != null)
		{
			VisitRvalue(node.ExpressionOpt);
		}
		if (node.DeclarationsOpt != null)
		{
			VisitStatement(node.DeclarationsOpt);
		}
		VisitStatement(node.Body);
		if (AwaitUsingAndForeachAddsPendingBranch && node.AwaitOpt != null)
		{
			PendingBranches.Add(new PendingBranch(node, State, null));
		}
		return null;
	}

	public override BoundNode VisitFixedStatement(BoundFixedStatement node)
	{
		VisitStatement(node.Declarations);
		VisitStatement(node.Body);
		return null;
	}

	public override BoundNode VisitFixedLocalCollectionInitializer(BoundFixedLocalCollectionInitializer node)
	{
		VisitRvalue(node.Expression);
		return null;
	}

	public override BoundNode VisitThrowStatement(BoundThrowStatement node)
	{
		BoundExpression expressionOpt = node.ExpressionOpt;
		VisitRvalue(expressionOpt);
		SetUnreachable();
		return null;
	}

	public override BoundNode VisitYieldBreakStatement(BoundYieldBreakStatement node)
	{
		PendingBranches.Add(new PendingBranch(node, State, null));
		SetUnreachable();
		return null;
	}

	public override BoundNode VisitYieldReturnStatement(BoundYieldReturnStatement node)
	{
		VisitRvalue(node.Expression);
		PendingBranches.Add(new PendingBranch(node, State, null));
		return null;
	}

	public override BoundNode VisitDefaultLiteral(BoundDefaultLiteral node)
	{
		return null;
	}

	public override BoundNode VisitDefaultExpression(BoundDefaultExpression node)
	{
		return null;
	}

	public override BoundNode VisitUnconvertedObjectCreationExpression(BoundUnconvertedObjectCreationExpression node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/AbstractFlowPass.cs", 3489);
	}

	public override BoundNode VisitTypeOfOperator(BoundTypeOfOperator node)
	{
		VisitTypeExpression(node.SourceType);
		return null;
	}

	public override BoundNode VisitNameOfOperator(BoundNameOfOperator node)
	{
		TLocalState state = State;
		SetState(UnreachableState());
		Visit(node.Argument);
		SetState(state);
		return null;
	}

	public override BoundNode VisitAddressOfOperator(BoundAddressOfOperator node)
	{
		VisitAddressOfOperand(node.Operand, shouldReadOperand: false);
		return null;
	}

	protected void VisitAddressOfOperand(BoundExpression operand, bool shouldReadOperand)
	{
		if (shouldReadOperand)
		{
			VisitRvalue(operand);
		}
		else
		{
			VisitLvalue(operand);
		}
		WriteArgument(operand, RefKind.Out, null);
	}

	public override BoundNode VisitPointerIndirectionOperator(BoundPointerIndirectionOperator node)
	{
		VisitRvalue(node.Operand);
		return null;
	}

	public override BoundNode VisitPointerElementAccess(BoundPointerElementAccess node)
	{
		VisitRvalue(node.Expression);
		VisitRvalue(node.Index);
		return null;
	}

	public override BoundNode VisitSizeOfOperator(BoundSizeOfOperator node)
	{
		return null;
	}

	private BoundNode VisitStackAllocArrayCreationBase(BoundStackAllocArrayCreationBase node)
	{
		VisitRvalue(node.Count);
		VisitRvalue(node.InitializerOpt);
		return null;
	}

	public override BoundNode VisitStackAllocArrayCreation(BoundStackAllocArrayCreation node)
	{
		return VisitStackAllocArrayCreationBase(node);
	}

	public override BoundNode VisitConvertedStackAllocExpression(BoundConvertedStackAllocExpression node)
	{
		return VisitStackAllocArrayCreationBase(node);
	}

	public override BoundNode VisitAnonymousObjectCreationExpression(BoundAnonymousObjectCreationExpression node)
	{
		VisitArguments(node.Arguments, default(ImmutableArray<RefKind>), node.Constructor, default(ImmutableArray<int>), expanded: false);
		return null;
	}

	public override BoundNode VisitArrayLength(BoundArrayLength node)
	{
		VisitRvalue(node.Expression);
		return null;
	}

	public override BoundNode VisitConditionalGoto(BoundConditionalGoto node)
	{
		VisitCondition(node.Condition);
		if (node.JumpIfTrue)
		{
			PendingBranches.Add(new PendingBranch(node, StateWhenTrue, node.Label));
			SetState(StateWhenFalse);
		}
		else
		{
			PendingBranches.Add(new PendingBranch(node, StateWhenFalse, node.Label));
			SetState(StateWhenTrue);
		}
		return null;
	}

	public override BoundNode VisitObjectInitializerExpression(BoundObjectInitializerExpression node)
	{
		return VisitObjectOrCollectionInitializerExpression(node.Initializers);
	}

	public override BoundNode VisitCollectionInitializerExpression(BoundCollectionInitializerExpression node)
	{
		return VisitObjectOrCollectionInitializerExpression(node.Initializers);
	}

	private BoundNode VisitObjectOrCollectionInitializerExpression(ImmutableArray<BoundExpression> initializers)
	{
		foreach (BoundExpression item in initializers)
		{
			VisitRvalue(item);
		}
		return null;
	}

	public override BoundNode VisitObjectInitializerMember(BoundObjectInitializerMember node)
	{
		if (!node.Arguments.IsDefaultOrEmpty)
		{
			MethodSymbol method = null;
			Symbol? memberSymbol = node.MemberSymbol;
			if ((object)memberSymbol != null && memberSymbol.Kind == SymbolKind.Property)
			{
				method = GetReadMethod((PropertySymbol)node.MemberSymbol);
			}
			VisitArguments(node.Arguments, node.ArgumentRefKindsOpt, method, node.ArgsToParamsOpt, node.Expanded);
		}
		return null;
	}

	public override BoundNode VisitDynamicObjectInitializerMember(BoundDynamicObjectInitializerMember node)
	{
		return null;
	}

	public override BoundNode VisitCollectionElementInitializer(BoundCollectionElementInitializer node)
	{
		if (node.AddMethod.CallsAreOmitted(node.SyntaxTree))
		{
			TLocalState state = (state = State.Clone());
			SetUnreachable();
			VisitArguments(node.Arguments, default(ImmutableArray<RefKind>), node.AddMethod, node.ArgsToParamsOpt, node.Expanded);
			State = state;
		}
		else
		{
			VisitArguments(node.Arguments, default(ImmutableArray<RefKind>), node.AddMethod, node.ArgsToParamsOpt, node.Expanded);
		}
		return null;
	}

	public override BoundNode VisitDynamicCollectionElementInitializer(BoundDynamicCollectionElementInitializer node)
	{
		VisitArguments(node.Arguments, default(ImmutableArray<RefKind>), null, default(ImmutableArray<int>), expanded: false);
		return null;
	}

	public override BoundNode VisitImplicitReceiver(BoundImplicitReceiver node)
	{
		return null;
	}

	public override BoundNode VisitFieldEqualsValue(BoundFieldEqualsValue node)
	{
		VisitRvalue(node.Value);
		return null;
	}

	public override BoundNode VisitPropertyEqualsValue(BoundPropertyEqualsValue node)
	{
		VisitRvalue(node.Value);
		return null;
	}

	public override BoundNode VisitParameterEqualsValue(BoundParameterEqualsValue node)
	{
		VisitRvalue(node.Value);
		return null;
	}

	public override BoundNode VisitDeconstructValuePlaceholder(BoundDeconstructValuePlaceholder node)
	{
		return null;
	}

	public override BoundNode VisitObjectOrCollectionValuePlaceholder(BoundObjectOrCollectionValuePlaceholder node)
	{
		return null;
	}

	public override BoundNode VisitAwaitableValuePlaceholder(BoundAwaitableValuePlaceholder node)
	{
		return null;
	}

	public sealed override BoundNode VisitOutVariablePendingInference(OutVariablePendingInference node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/AbstractFlowPass.cs", 3707);
	}

	public sealed override BoundNode VisitDeconstructionVariablePendingInference(DeconstructionVariablePendingInference node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/AbstractFlowPass.cs", 3712);
	}

	public override BoundNode VisitDiscardExpression(BoundDiscardExpression node)
	{
		return null;
	}

	private static MethodSymbol GetReadMethod(PropertySymbol property)
	{
		return property.GetOwnOrInheritedGetMethod() ?? property.SetMethod;
	}

	private static MethodSymbol GetWriteMethod(PropertySymbol property)
	{
		return property.GetOwnOrInheritedSetMethod() ?? property.GetMethod;
	}

	public override BoundNode VisitConstructorMethodBody(BoundConstructorMethodBody node)
	{
		Visit(node.Initializer);
		VisitMethodBodies(node.BlockBody, node.ExpressionBody);
		return null;
	}

	public override BoundNode VisitNonConstructorMethodBody(BoundNonConstructorMethodBody node)
	{
		VisitMethodBodies(node.BlockBody, node.ExpressionBody);
		return null;
	}

	public override BoundNode VisitNullCoalescingAssignmentOperator(BoundNullCoalescingAssignmentOperator node)
	{
		TLocalState other;
		if (RegularPropertyAccess(node.LeftOperand))
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)node.LeftOperand;
			PropertySymbol propertySymbol = boundPropertyAccess.PropertySymbol;
			if (propertySymbol.RefKind == RefKind.None)
			{
				MethodSymbol ownOrInheritedGetMethod = propertySymbol.GetOwnOrInheritedGetMethod();
				VisitReceiverBeforeCall(boundPropertyAccess.ReceiverOpt, ownOrInheritedGetMethod);
				VisitReceiverAfterCall(boundPropertyAccess.ReceiverOpt, ownOrInheritedGetMethod);
				TLocalState state = State.Clone();
				AdjustStateForNullCoalescingAssignmentNonNullCase(node);
				other = State.Clone();
				SetState(state);
				VisitAssignmentOfNullCoalescingAssignment(node, boundPropertyAccess);
				goto IL_00d1;
			}
		}
		VisitRvalue(node.LeftOperand, isKnownToBeAnLvalue: true);
		TLocalState state2 = State.Clone();
		AdjustStateForNullCoalescingAssignmentNonNullCase(node);
		other = State.Clone();
		SetState(state2);
		VisitAssignmentOfNullCoalescingAssignment(node, null);
		goto IL_00d1;
		IL_00d1:
		Join(ref State, ref other);
		return null;
	}

	public override BoundNode VisitReadOnlySpanFromArray(BoundReadOnlySpanFromArray node)
	{
		VisitRvalue(node.Operand);
		return null;
	}

	public override BoundNode VisitFunctionPointerInvocation(BoundFunctionPointerInvocation node)
	{
		VisitRvalue(node.InvokedExpression);
		VisitArguments(node.Arguments, node.ArgumentRefKindsOpt, node.FunctionPointer.Signature, default(ImmutableArray<int>), expanded: false);
		return null;
	}

	public override BoundNode VisitUnconvertedAddressOfOperator(BoundUnconvertedAddressOfOperator node)
	{
		Visit(node.Operand);
		return null;
	}

	protected virtual void VisitAssignmentOfNullCoalescingAssignment(BoundNullCoalescingAssignmentOperator node, BoundPropertyAccess propertyAccessOpt)
	{
		VisitRvalue(node.RightOperand);
		if (propertyAccessOpt != null)
		{
			MethodSymbol ownOrInheritedSetMethod = propertyAccessOpt.PropertySymbol.GetOwnOrInheritedSetMethod();
			PropertySetter(node, propertyAccessOpt.ReceiverOpt, ownOrInheritedSetMethod);
		}
	}

	public override BoundNode VisitSavePreviousSequencePoint(BoundSavePreviousSequencePoint node)
	{
		return null;
	}

	public override BoundNode VisitRestorePreviousSequencePoint(BoundRestorePreviousSequencePoint node)
	{
		return null;
	}

	public override BoundNode VisitStepThroughSequencePoint(BoundStepThroughSequencePoint node)
	{
		return null;
	}

	protected virtual void AdjustStateForNullCoalescingAssignmentNonNullCase(BoundNullCoalescingAssignmentOperator node)
	{
	}

	private void VisitMethodBodies(BoundBlock blockBody, BoundBlock expressionBody)
	{
		if (blockBody == null)
		{
			Visit(expressionBody);
			return;
		}
		if (expressionBody == null)
		{
			Visit(blockBody);
			return;
		}
		TLocalState state = State.Clone();
		Visit(blockBody);
		TLocalState other = State;
		SetState(state);
		Visit(expressionBody);
		Join(ref State, ref other);
	}

	protected abstract TLocalState TopState();

	protected abstract TLocalState UnreachableState();

	protected virtual TLocalState ReachableBottomState()
	{
		return default(TLocalState);
	}

	protected abstract bool Join(ref TLocalState self, ref TLocalState other);

	protected abstract bool Meet(ref TLocalState self, ref TLocalState other);

	protected abstract TLocalFunctionState CreateLocalFunctionState(LocalFunctionSymbol symbol);

	protected TLocalFunctionState GetOrCreateLocalFuncUsages(LocalFunctionSymbol localFunc)
	{
		if (_localFuncVarUsages == null)
		{
			_localFuncVarUsages = new SmallDictionary<LocalFunctionSymbol, TLocalFunctionState>();
		}
		if (!_localFuncVarUsages.TryGetValue(localFunc, out var value))
		{
			value = CreateLocalFunctionState(localFunc);
			_localFuncVarUsages[localFunc] = value;
		}
		return value;
	}

	protected bool HasLocalFuncUsagesCreated(LocalFunctionSymbol localFunc)
	{
		return _localFuncVarUsages?.ContainsKey(localFunc) ?? false;
	}

	public override BoundNode? VisitLocalFunctionStatement(BoundLocalFunctionStatement localFunc)
	{
		if (localFunc.Symbol.IsExtern)
		{
			return null;
		}
		Symbol currentSymbol = CurrentSymbol;
		MethodSymbol methodSymbol = (MethodSymbol)(CurrentSymbol = localFunc.Symbol);
		SavedPending oldPending = SavePending();
		TLocalState state = State;
		State = TopState();
		Optional<TLocalState> nonMonotonicState = NonMonotonicState;
		if (_nonMonotonicTransfer)
		{
			NonMonotonicState = ReachableBottomState();
		}
		if (!localFunc.WasCompilerGenerated)
		{
			EnterParameters(methodSymbol.Parameters);
		}
		TLocalFunctionState orCreateLocalFuncUsages = GetOrCreateLocalFuncUsages((LocalFunctionSymbol)methodSymbol);
		TLocalFunctionState savedState = LocalFunctionStart(orCreateLocalFuncUsages);
		SavedPending oldPending2 = SavePending();
		if (methodSymbol.IsIterator)
		{
			PendingBranches.Add(new PendingBranch(null, State, null));
		}
		VisitAlways(localFunc.Body);
		RestorePending(oldPending2);
		ImmutableArray<PendingBranch> immutableArray = RemoveReturns();
		RestorePending(oldPending);
		Location location = methodSymbol.TryGetFirstLocation();
		LeaveParameters(methodSymbol.Parameters, localFunc.Syntax, location);
		TLocalState self = State;
		foreach (PendingBranch item in immutableArray)
		{
			State = item.State;
			BoundNode branch = item.Branch;
			LeaveParameters(methodSymbol.Parameters, branch?.Syntax, (branch != null && !branch.WasCompilerGenerated) ? null : location);
			Join(ref self, ref State);
		}
		if (RecordStateChange(savedState, orCreateLocalFuncUsages, ref self) && orCreateLocalFuncUsages.Visited)
		{
			stateChangedAfterUse = true;
			orCreateLocalFuncUsages.Visited = false;
		}
		State = state;
		NonMonotonicState = nonMonotonicState;
		CurrentSymbol = currentSymbol;
		return null;
	}

	private bool RecordStateChange(TLocalFunctionState savedState, TLocalFunctionState currentState, ref TLocalState stateAtReturn)
	{
		bool flag = LocalFunctionEnd(savedState, currentState, ref stateAtReturn);
		flag |= Join(ref currentState.StateFromTop, ref stateAtReturn);
		if (NonMonotonicState.HasValue)
		{
			TLocalState self = NonMonotonicState.Value;
			Meet(ref self, ref stateAtReturn);
			flag |= Join(ref currentState.StateFromBottom, ref self);
		}
		return flag;
	}

	protected virtual TLocalFunctionState LocalFunctionStart(TLocalFunctionState state)
	{
		return state;
	}

	protected virtual bool LocalFunctionEnd(TLocalFunctionState savedState, TLocalFunctionState currentState, ref TLocalState stateAtReturn)
	{
		return false;
	}

	public override BoundNode VisitSwitchStatement(BoundSwitchStatement node)
	{
		TLocalState self = VisitSwitchStatementDispatch(node);
		ImmutableArray<BoundSwitchSection> switchSections = node.SwitchSections;
		int num = switchSections.Length - 1;
		for (int i = 0; i <= num; i++)
		{
			VisitSwitchSection(switchSections[i], i == num);
			Join(ref self, ref State);
		}
		ResolveBreaks(self, node.BreakLabel);
		return null;
	}

	protected virtual TLocalState VisitSwitchStatementDispatch(BoundSwitchStatement node)
	{
		VisitRvalue(node.Expression);
		TLocalState other = State.Clone();
		ImmutableHashSet<LabelSymbol> reachableLabels = node.ReachabilityDecisionDag.ReachableLabels;
		foreach (BoundSwitchSection switchSection in node.SwitchSections)
		{
			foreach (BoundSwitchLabel switchLabel in switchSection.SwitchLabels)
			{
				if (reachableLabels.Contains(switchLabel.Label) || switchLabel.HasErrors || (switchLabel == node.DefaultLabel && node.Expression.ConstantValueOpt == null && IsTraditionalSwitch(node)))
				{
					SetState(other.Clone());
				}
				else
				{
					SetUnreachable();
				}
				VisitPattern(switchLabel.Pattern);
				SetState(StateWhenTrue);
				if (switchLabel.WhenClause != null)
				{
					VisitCondition(switchLabel.WhenClause);
					SetState(StateWhenTrue);
				}
				PendingBranches.Add(new PendingBranch(switchLabel, State, switchLabel.Label));
			}
		}
		TLocalState self = UnreachableState();
		if (node.ReachabilityDecisionDag.ReachableLabels.Contains(node.BreakLabel) || (node.DefaultLabel == null && node.Expression.ConstantValueOpt == null && IsTraditionalSwitch(node)))
		{
			Join(ref self, ref other);
		}
		return self;
	}

	private bool IsTraditionalSwitch(BoundSwitchStatement node)
	{
		if (compilation.LanguageVersion >= MessageID.IDS_FeatureRecursivePatterns.RequiredVersion())
		{
			return false;
		}
		if (!node.Expression.Type.IsValidV6SwitchGoverningType())
		{
			return false;
		}
		foreach (SwitchSectionSyntax section in ((SwitchStatementSyntax)node.Syntax).Sections)
		{
			foreach (SwitchLabelSyntax label in section.Labels)
			{
				if (label.Kind() == SyntaxKind.CasePatternSwitchLabel)
				{
					return false;
				}
			}
		}
		return true;
	}

	protected virtual void VisitSwitchSection(BoundSwitchSection node, bool isLastSection)
	{
		SetState(UnreachableState());
		foreach (BoundSwitchLabel switchLabel in node.SwitchLabels)
		{
			VisitLabel(switchLabel.Label, node);
		}
		VisitStatementList(node);
	}

	public override BoundNode VisitSwitchDispatch(BoundSwitchDispatch node)
	{
		VisitRvalue(node.Expression);
		TLocalState state = State.Clone();
		PendingBranches.Add(new PendingBranch(node, state, node.DefaultLabel));
		foreach (var @case in node.Cases)
		{
			LabelSymbol item = @case.label;
			PendingBranches.Add(new PendingBranch(node, state, item));
		}
		SetUnreachable();
		return null;
	}

	public override BoundNode VisitConvertedSwitchExpression(BoundConvertedSwitchExpression node)
	{
		return VisitSwitchExpression(node);
	}

	public override BoundNode VisitUnconvertedSwitchExpression(BoundUnconvertedSwitchExpression node)
	{
		return VisitSwitchExpression(node);
	}

	private BoundNode VisitSwitchExpression(BoundSwitchExpression node)
	{
		VisitRvalue(node.Expression);
		TLocalState state = State;
		TLocalState self = UnreachableState();
		ImmutableHashSet<LabelSymbol> reachableLabels = node.ReachabilityDecisionDag.ReachableLabels;
		foreach (BoundSwitchExpressionArm switchArm in node.SwitchArms)
		{
			SetState(state.Clone());
			VisitPattern(switchArm.Pattern);
			SetState(StateWhenTrue);
			if (!reachableLabels.Contains(switchArm.Label) || switchArm.Pattern.HasErrors)
			{
				SetUnreachable();
			}
			if (switchArm.WhenClause != null)
			{
				VisitCondition(switchArm.WhenClause);
				SetState(StateWhenTrue);
			}
			VisitRvalue(switchArm.Value);
			Join(ref self, ref State);
		}
		SetState(self);
		return node;
	}
}
