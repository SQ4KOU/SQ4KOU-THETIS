using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class AsyncExceptionHandlerRewriter : BoundTreeRewriterWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
{
	private sealed class AwaitInFinallyAnalysis : LabelCollector
	{
		private Dictionary<BoundTryStatement, HashSet<LabelSymbol>> _labelsInInterestingTry;

		private HashSet<BoundCatchBlock> _awaitContainingCatches;

		private bool _seenAwait;

		public AwaitInFinallyAnalysis(BoundStatement body)
		{
			_seenAwait = false;
			Visit(body);
		}

		public bool FinallyContainsAwaits(BoundTryStatement statement)
		{
			if (_labelsInInterestingTry != null)
			{
				return _labelsInInterestingTry.ContainsKey(statement);
			}
			return false;
		}

		internal bool CatchContainsAwait(BoundCatchBlock node)
		{
			if (_awaitContainingCatches != null)
			{
				return _awaitContainingCatches.Contains(node);
			}
			return false;
		}

		public bool ContainsAwaitInHandlers()
		{
			if (_labelsInInterestingTry == null)
			{
				return _awaitContainingCatches != null;
			}
			return true;
		}

		internal HashSet<LabelSymbol> Labels(BoundTryStatement statement)
		{
			return _labelsInInterestingTry[statement];
		}

		public override BoundNode VisitTryStatement(BoundTryStatement node)
		{
			HashSet<LabelSymbol> hashSet = currentLabels;
			currentLabels = null;
			Visit(node.TryBlock);
			VisitList(node.CatchBlocks);
			bool seenAwait = _seenAwait;
			_seenAwait = false;
			Visit(node.FinallyBlockOpt);
			if (_seenAwait)
			{
				Dictionary<BoundTryStatement, HashSet<LabelSymbol>> dictionary = _labelsInInterestingTry;
				if (dictionary == null)
				{
					dictionary = (_labelsInInterestingTry = new Dictionary<BoundTryStatement, HashSet<LabelSymbol>>());
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
			_seenAwait |= seenAwait;
			return null;
		}

		public override BoundNode VisitCatchBlock(BoundCatchBlock node)
		{
			bool seenAwait = _seenAwait;
			_seenAwait = false;
			BoundNode? result = base.VisitCatchBlock(node);
			if (_seenAwait)
			{
				HashSet<BoundCatchBlock> awaitContainingCatches = _awaitContainingCatches;
				if (awaitContainingCatches == null)
				{
					awaitContainingCatches = (_awaitContainingCatches = new HashSet<BoundCatchBlock>());
				}
				_awaitContainingCatches.Add(node);
			}
			_seenAwait |= seenAwait;
			return result;
		}

		public override BoundNode VisitAwaitExpression(BoundAwaitExpression node)
		{
			_seenAwait = true;
			return base.VisitAwaitExpression(node);
		}

		public override BoundNode VisitLambda(BoundLambda node)
		{
			HashSet<LabelSymbol> hashSet = currentLabels;
			bool seenAwait = _seenAwait;
			currentLabels = null;
			_seenAwait = false;
			base.VisitLambda(node);
			currentLabels = hashSet;
			_seenAwait = seenAwait;
			return null;
		}

		public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
		{
			HashSet<LabelSymbol> hashSet = currentLabels;
			bool seenAwait = _seenAwait;
			currentLabels = null;
			_seenAwait = false;
			base.VisitLocalFunctionStatement(node);
			currentLabels = hashSet;
			_seenAwait = seenAwait;
			return null;
		}
	}

	private sealed class AwaitFinallyFrame
	{
		public readonly AwaitFinallyFrame ParentOpt;

		public readonly HashSet<LabelSymbol> LabelsOpt;

		private readonly SyntaxNode _syntaxOpt;

		public Dictionary<LabelSymbol, LabelSymbol> proxyLabels;

		public List<LabelSymbol> proxiedLabels;

		public GeneratedLabelSymbol returnProxyLabel;

		public SynthesizedLocal returnValue;

		public AwaitFinallyFrame()
		{
		}

		public AwaitFinallyFrame(AwaitFinallyFrame parent, HashSet<LabelSymbol> labelsOpt, SyntaxNode syntax)
		{
			ParentOpt = parent;
			LabelsOpt = labelsOpt;
			_syntaxOpt = syntax;
		}

		public bool IsRoot()
		{
			return ParentOpt == null;
		}

		public LabelSymbol ProxyLabelIfNeeded(LabelSymbol label)
		{
			if (IsRoot() || (LabelsOpt != null && LabelsOpt.Contains(label)))
			{
				return label;
			}
			Dictionary<LabelSymbol, LabelSymbol> dictionary = proxyLabels;
			List<LabelSymbol> list = proxiedLabels;
			if (dictionary == null)
			{
				dictionary = (proxyLabels = new Dictionary<LabelSymbol, LabelSymbol>());
				list = (proxiedLabels = new List<LabelSymbol>());
			}
			if (!dictionary.TryGetValue(label, out var value))
			{
				value = new GeneratedLabelSymbol("proxy" + label.Name);
				dictionary.Add(label, value);
				list.Add(label);
			}
			return value;
		}

		public LabelSymbol ProxyReturnIfNeeded(MethodSymbol containingMethod, BoundExpression valueOpt, out SynthesizedLocal returnValue)
		{
			returnValue = null;
			if (IsRoot())
			{
				return null;
			}
			GeneratedLabelSymbol generatedLabelSymbol = returnProxyLabel;
			if (generatedLabelSymbol == null)
			{
				generatedLabelSymbol = (returnProxyLabel = new GeneratedLabelSymbol("returnProxy"));
			}
			if (valueOpt != null)
			{
				returnValue = this.returnValue;
				if (returnValue == null)
				{
					this.returnValue = (returnValue = new SynthesizedLocal(containingMethod, TypeWithAnnotations.Create(valueOpt.Type), SynthesizedLocalKind.AsyncMethodReturnValue, _syntaxOpt));
				}
			}
			return generatedLabelSymbol;
		}
	}

	private sealed class AwaitCatchFrame
	{
		public readonly SynthesizedLocal pendingCaughtException;

		public readonly SynthesizedLocal pendingCatch;

		public readonly List<BoundBlock> handlers;

		private readonly AwaitCatchFrame _parentOpt;

		private readonly Dictionary<LocalSymbol, LocalSymbol> _hoistedLocals;

		private readonly List<LocalSymbol> _orderedHoistedLocals;

		public AwaitCatchFrame(SyntheticBoundNodeFactory F, TryStatementSyntax tryStatementSyntax, AwaitCatchFrame parentOpt)
		{
			pendingCaughtException = new SynthesizedLocal(F.CurrentFunction, TypeWithAnnotations.Create(F.SpecialType(SpecialType.System_Object)), SynthesizedLocalKind.TryAwaitPendingCaughtException, tryStatementSyntax);
			pendingCatch = new SynthesizedLocal(F.CurrentFunction, TypeWithAnnotations.Create(F.SpecialType(SpecialType.System_Int32)), SynthesizedLocalKind.TryAwaitPendingCatch, tryStatementSyntax);
			handlers = new List<BoundBlock>();
			_parentOpt = parentOpt;
			_hoistedLocals = new Dictionary<LocalSymbol, LocalSymbol>();
			_orderedHoistedLocals = new List<LocalSymbol>();
		}

		public void HoistLocal(LocalSymbol local, SyntheticBoundNodeFactory F)
		{
			if (!_hoistedLocals.Keys.Any((LocalSymbol l) => l.Name == local.Name && TypeSymbol.Equals(l.Type, local.Type, TypeCompareKind.ConsiderEverything)))
			{
				_hoistedLocals.Add(local, local);
				_orderedHoistedLocals.Add(local);
			}
			else
			{
				LocalSymbol localSymbol = F.SynthesizedLocal(local.Type, pendingCatch.SyntaxOpt, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.ExceptionFilterAwaitHoistedExceptionLocal);
				_hoistedLocals.Add(local, localSymbol);
				_orderedHoistedLocals.Add(localSymbol);
			}
		}

		public IEnumerable<LocalSymbol> GetHoistedLocals()
		{
			return _orderedHoistedLocals;
		}

		public bool TryGetHoistedLocal(LocalSymbol originalLocal, out LocalSymbol hoistedLocal)
		{
			if (!_hoistedLocals.TryGetValue(originalLocal, out hoistedLocal))
			{
				return _parentOpt?.TryGetHoistedLocal(originalLocal, out hoistedLocal) ?? false;
			}
			return true;
		}
	}

	private readonly SyntheticBoundNodeFactory _F;

	private readonly AwaitInFinallyAnalysis _analysis;

	private AwaitCatchFrame _currentAwaitCatchFrame;

	private AwaitFinallyFrame _currentAwaitFinallyFrame = new AwaitFinallyFrame();

	private bool _inCatchWithoutAwaits;

	private bool _needsFinalThrow;

	private AsyncExceptionHandlerRewriter(MethodSymbol containingMethod, NamedTypeSymbol containingType, SyntheticBoundNodeFactory factory, AwaitInFinallyAnalysis analysis)
	{
		_F = factory;
		_F.CurrentFunction = containingMethod;
		_analysis = analysis;
	}

	public static BoundStatement Rewrite(MethodSymbol containingSymbol, NamedTypeSymbol containingType, BoundStatement statement, TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		AwaitInFinallyAnalysis awaitInFinallyAnalysis = new AwaitInFinallyAnalysis(statement);
		if (!awaitInFinallyAnalysis.ContainsAwaitInHandlers())
		{
			return statement;
		}
		SyntheticBoundNodeFactory factory = new SyntheticBoundNodeFactory(containingSymbol, statement.Syntax, compilationState, diagnostics);
		AsyncExceptionHandlerRewriter asyncExceptionHandlerRewriter = new AsyncExceptionHandlerRewriter(containingSymbol, containingType, factory, awaitInFinallyAnalysis);
		BoundStatement loweredStatement = (BoundStatement)asyncExceptionHandlerRewriter.Visit(statement);
		return asyncExceptionHandlerRewriter.FinalizeMethodBody(loweredStatement);
	}

	private BoundStatement FinalizeMethodBody(BoundStatement loweredStatement)
	{
		if (loweredStatement == null)
		{
			return null;
		}
		BoundStatement result = loweredStatement;
		if (_needsFinalThrow)
		{
			result = _F.Block(loweredStatement, _F.Throw(_F.Null(_F.SpecialType(SpecialType.System_Object))));
		}
		return result;
	}

	public override BoundNode VisitTryStatement(BoundTryStatement node)
	{
		SyntaxNode syntax = node.Syntax;
		SyntaxNode syntax2 = _F.Syntax;
		_F.Syntax = syntax;
		BoundNode result = visitTryStatement(node, syntax);
		_F.Syntax = syntax2;
		return result;
		BoundNode visitTryStatement(BoundTryStatement boundTryStatement, SyntaxNode tryStatementSyntax)
		{
			BoundStatement boundStatement;
			BoundBlock boundBlock;
			if (!_analysis.FinallyContainsAwaits(boundTryStatement))
			{
				boundStatement = RewriteFinalizedRegion(boundTryStatement);
				boundBlock = (BoundBlock)Visit(boundTryStatement.FinallyBlockOpt);
				if (boundBlock == null)
				{
					return boundStatement;
				}
				if (boundStatement is BoundTryStatement boundTryStatement2)
				{
					return boundTryStatement2.Update(boundTryStatement2.TryBlock, boundTryStatement2.CatchBlocks, boundBlock, boundTryStatement2.FinallyLabelOpt, boundTryStatement2.PreferFaultHandler);
				}
				return _F.Try((BoundBlock)boundStatement, ImmutableArray<BoundCatchBlock>.Empty, boundBlock);
			}
			AwaitFinallyFrame awaitFinallyFrame = PushFrame(boundTryStatement);
			boundStatement = RewriteFinalizedRegion(boundTryStatement);
			boundBlock = (BoundBlock)VisitBlock(boundTryStatement.FinallyBlockOpt);
			PopFrame();
			NamedTypeSymbol typeSymbol = _F.SpecialType(SpecialType.System_Object);
			SynthesizedLocal synthesizedLocal = new SynthesizedLocal(_F.CurrentFunction, TypeWithAnnotations.Create(typeSymbol), SynthesizedLocalKind.TryAwaitPendingException, tryStatementSyntax);
			GeneratedLabelSymbol generatedLabelSymbol = _F.GenerateLabel("finallyLabel");
			SynthesizedLocal synthesizedLocal2 = new SynthesizedLocal(_F.CurrentFunction, TypeWithAnnotations.Create(_F.SpecialType(SpecialType.System_Int32)), SynthesizedLocalKind.TryAwaitPendingBranch, tryStatementSyntax);
			BoundCatchBlock item = _F.Catch(_F.Local(synthesizedLocal), _F.Block());
			BoundStatement item2 = _F.Try(_F.Block(boundStatement, _F.HiddenSequencePoint(), _F.Goto(generatedLabelSymbol), PendBranches(awaitFinallyFrame, synthesizedLocal2, generatedLabelSymbol)), ImmutableArray.Create(item), null, generatedLabelSymbol);
			BoundBlock boundBlock2 = _F.Block(_F.HiddenSequencePoint(), _F.Label(generatedLabelSymbol), boundBlock, _F.HiddenSequencePoint(), UnpendException(synthesizedLocal), UnpendBranches(awaitFinallyFrame, synthesizedLocal2));
			BoundStatement item3 = boundBlock2;
			if (_F.CurrentFunction.IsAsync && _F.CurrentFunction.IsIterator)
			{
				item3 = _F.ExtractedFinallyBlock(boundBlock2);
			}
			ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
			ArrayBuilder<BoundStatement> instance2 = ArrayBuilder<BoundStatement>.GetInstance();
			instance2.Add(_F.HiddenSequencePoint());
			instance.Add(synthesizedLocal);
			instance2.Add(_F.Assignment(_F.Local(synthesizedLocal), _F.Default(synthesizedLocal.Type)));
			instance.Add(synthesizedLocal2);
			instance2.Add(_F.Assignment(_F.Local(synthesizedLocal2), _F.Default(synthesizedLocal2.Type)));
			LocalSymbol returnValue = awaitFinallyFrame.returnValue;
			if (returnValue != null)
			{
				instance.Add(returnValue);
			}
			instance2.Add(item2);
			instance2.Add(item3);
			return _F.Block(instance.ToImmutableAndFree(), instance2.ToImmutableAndFree());
		}
	}

	private BoundBlock PendBranches(AwaitFinallyFrame frame, LocalSymbol pendingBranchVar, LabelSymbol finallyLabel)
	{
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		List<LabelSymbol> proxiedLabels = frame.proxiedLabels;
		Dictionary<LabelSymbol, LabelSymbol> proxyLabels = frame.proxyLabels;
		int i = 1;
		if (proxiedLabels != null)
		{
			for (int count = proxiedLabels.Count; i <= count; i++)
			{
				LabelSymbol key = proxiedLabels[i - 1];
				LabelSymbol proxy = proxyLabels[key];
				PendBranch(instance, proxy, i, pendingBranchVar, finallyLabel);
			}
		}
		GeneratedLabelSymbol returnProxyLabel = frame.returnProxyLabel;
		if (returnProxyLabel != null)
		{
			PendBranch(instance, returnProxyLabel, i, pendingBranchVar, finallyLabel);
		}
		return _F.Block(instance.ToImmutableAndFree());
	}

	private void PendBranch(ArrayBuilder<BoundStatement> bodyStatements, LabelSymbol proxy, int i, LocalSymbol pendingBranchVar, LabelSymbol finallyLabel)
	{
		bodyStatements.Add(_F.Label(proxy));
		bodyStatements.Add(_F.Assignment(_F.Local(pendingBranchVar), _F.Literal(i)));
		bodyStatements.Add(_F.Goto(finallyLabel));
	}

	private BoundStatement UnpendBranches(AwaitFinallyFrame frame, SynthesizedLocal pendingBranchVar)
	{
		AwaitFinallyFrame parentOpt = frame.ParentOpt;
		List<LabelSymbol> proxiedLabels = frame.proxiedLabels;
		int i = 1;
		ArrayBuilder<SyntheticBoundNodeFactory.SyntheticSwitchSection> instance = ArrayBuilder<SyntheticBoundNodeFactory.SyntheticSwitchSection>.GetInstance();
		if (proxiedLabels != null)
		{
			for (int count = proxiedLabels.Count; i <= count; i++)
			{
				LabelSymbol label = proxiedLabels[i - 1];
				LabelSymbol label2 = parentOpt.ProxyLabelIfNeeded(label);
				SyntheticBoundNodeFactory.SyntheticSwitchSection item = _F.SwitchSection(i, _F.Goto(label2));
				instance.Add(item);
			}
		}
		if (frame.returnProxyLabel != null)
		{
			BoundLocal boundLocal = null;
			if (frame.returnValue != null)
			{
				boundLocal = _F.Local(frame.returnValue);
			}
			LabelSymbol labelSymbol = parentOpt.ProxyReturnIfNeeded(_F.CurrentFunction, boundLocal, out var returnValue);
			BoundStatement boundStatement = ((labelSymbol == null) ? new BoundReturnStatement(_F.Syntax, RefKind.None, boundLocal, @checked: false) : ((boundLocal != null) ? ((BoundStatement)_F.Block(_F.Assignment(_F.Local(returnValue), boundLocal), _F.Goto(labelSymbol))) : ((BoundStatement)_F.Goto(labelSymbol))));
			SyntheticBoundNodeFactory.SyntheticSwitchSection item2 = _F.SwitchSection(i, boundStatement);
			instance.Add(item2);
		}
		_needsFinalThrow = true;
		return _F.Switch(_F.Local(pendingBranchVar), instance.ToImmutableAndFree());
	}

	public override BoundNode VisitGotoStatement(BoundGotoStatement node)
	{
		BoundExpression caseExpressionOpt = (BoundExpression)Visit(node.CaseExpressionOpt);
		BoundLabel labelExpressionOpt = (BoundLabel)Visit(node.LabelExpressionOpt);
		LabelSymbol label = _currentAwaitFinallyFrame.ProxyLabelIfNeeded(node.Label);
		return node.Update(label, caseExpressionOpt, labelExpressionOpt);
	}

	public override BoundNode VisitConditionalGoto(BoundConditionalGoto node)
	{
		return base.VisitConditionalGoto(node);
	}

	public override BoundNode VisitReturnStatement(BoundReturnStatement node)
	{
		LabelSymbol labelSymbol = _currentAwaitFinallyFrame.ProxyReturnIfNeeded(_F.CurrentFunction, node.ExpressionOpt, out var returnValue);
		if (labelSymbol == null)
		{
			return base.VisitReturnStatement(node);
		}
		BoundExpression boundExpression = (BoundExpression)Visit(node.ExpressionOpt);
		if (boundExpression != null)
		{
			return _F.Block(_F.Assignment(_F.Local(returnValue), boundExpression), _F.Goto(labelSymbol));
		}
		return _F.Goto(labelSymbol);
	}

	private BoundStatement UnpendException(LocalSymbol pendingExceptionLocal)
	{
		if (_F.Compilation.IsRuntimeAsyncEnabledIn(_F.CurrentFunction))
		{
			return checkAndThrow(pendingExceptionLocal);
		}
		LocalSymbol localSymbol = _F.SynthesizedLocal(_F.SpecialType(SpecialType.System_Object));
		BoundExpressionStatement boundExpressionStatement = _F.Assignment(_F.Local(localSymbol), _F.Local(pendingExceptionLocal));
		return _F.Block(ImmutableArray.Create(localSymbol), boundExpressionStatement, checkAndThrow(localSymbol));
		BoundStatement checkAndThrow(LocalSymbol obj)
		{
			BoundStatement thenClause = Rethrow(obj);
			return _F.If(_F.ObjectNotEqual(_F.Local(obj), _F.Null(obj.Type)), thenClause);
		}
	}

	private BoundStatement Rethrow(LocalSymbol obj)
	{
		BoundStatement boundStatement = _F.Throw(_F.Local(obj));
		MethodSymbol methodSymbol = _F.WellKnownMethod(WellKnownMember.System_Runtime_ExceptionServices_ExceptionDispatchInfo__Capture, isOptional: true);
		MethodSymbol methodSymbol2 = _F.WellKnownMethod(WellKnownMember.System_Runtime_ExceptionServices_ExceptionDispatchInfo__Throw, isOptional: true);
		if (methodSymbol != null && methodSymbol2 != null)
		{
			LocalSymbol localSymbol = _F.SynthesizedLocal(_F.WellKnownType(WellKnownType.System_Exception));
			BoundExpressionStatement boundExpressionStatement = _F.Assignment(_F.Local(localSymbol), _F.As(_F.Local(obj), localSymbol.Type));
			boundStatement = _F.Block(ImmutableArray.Create(localSymbol), boundExpressionStatement, _F.If(_F.ObjectEqual(_F.Local(localSymbol), _F.Null(localSymbol.Type)), boundStatement), _F.ExpressionStatement(_F.Call(_F.StaticCall(methodSymbol.ContainingType, methodSymbol, _F.Local(localSymbol)), methodSymbol2)));
		}
		return boundStatement;
	}

	private BoundStatement RewriteFinalizedRegion(BoundTryStatement node)
	{
		BoundBlock boundBlock = (BoundBlock)VisitBlock(node.TryBlock);
		if (node.CatchBlocks.IsDefaultOrEmpty)
		{
			return boundBlock;
		}
		AwaitCatchFrame currentAwaitCatchFrame = _currentAwaitCatchFrame;
		_currentAwaitCatchFrame = null;
		ImmutableArray<BoundCatchBlock> catchBlocks = node.CatchBlocks.SelectAsArray(delegate(BoundCatchBlock catchBlock, (AsyncExceptionHandlerRewriter, AwaitCatchFrame origAwaitCatchFrame) arg)
		{
			var (asyncExceptionHandlerRewriter, parentAwaitCatchFrame) = arg;
			return (BoundCatchBlock)asyncExceptionHandlerRewriter.VisitCatchBlock(catchBlock, parentAwaitCatchFrame);
		}, (this, currentAwaitCatchFrame));
		BoundStatement boundStatement = _F.Try(boundBlock, catchBlocks);
		AwaitCatchFrame currentAwaitCatchFrame2 = _currentAwaitCatchFrame;
		if (currentAwaitCatchFrame2 != null)
		{
			GeneratedLabelSymbol label = _F.GenerateLabel("handled");
			List<BoundBlock> handlers = currentAwaitCatchFrame2.handlers;
			ArrayBuilder<SyntheticBoundNodeFactory.SyntheticSwitchSection> instance = ArrayBuilder<SyntheticBoundNodeFactory.SyntheticSwitchSection>.GetInstance(handlers.Count);
			int num = 0;
			for (int count = handlers.Count; num < count; num++)
			{
				instance.Add(_F.SwitchSection(num + 1, _F.Block(handlers[num], _F.Goto(label))));
			}
			boundStatement = _F.Block(ImmutableArray.Create((LocalSymbol)currentAwaitCatchFrame2.pendingCaughtException, (LocalSymbol)currentAwaitCatchFrame2.pendingCatch).AddRange(currentAwaitCatchFrame2.GetHoistedLocals()), _F.HiddenSequencePoint(), _F.Assignment(_F.Local(currentAwaitCatchFrame2.pendingCatch), _F.Default(currentAwaitCatchFrame2.pendingCatch.Type)), boundStatement, _F.HiddenSequencePoint(), _F.Switch(_F.Local(currentAwaitCatchFrame2.pendingCatch), instance.ToImmutableAndFree()), _F.HiddenSequencePoint(), _F.Label(label));
			_needsFinalThrow = true;
		}
		_currentAwaitCatchFrame = currentAwaitCatchFrame;
		return boundStatement;
	}

	public override BoundNode VisitCatchBlock(BoundCatchBlock node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/AsyncRewriter/AsyncExceptionHandlerRewriter.cs", 583);
	}

	private BoundNode VisitCatchBlock(BoundCatchBlock node, AwaitCatchFrame parentAwaitCatchFrame)
	{
		if (!_analysis.CatchContainsAwait(node))
		{
			AwaitCatchFrame currentAwaitCatchFrame = _currentAwaitCatchFrame;
			_currentAwaitCatchFrame = parentAwaitCatchFrame;
			bool inCatchWithoutAwaits = _inCatchWithoutAwaits;
			_inCatchWithoutAwaits = true;
			BoundNode? result = base.VisitCatchBlock(node);
			_currentAwaitCatchFrame = currentAwaitCatchFrame;
			_inCatchWithoutAwaits = inCatchWithoutAwaits;
			return result;
		}
		AwaitCatchFrame awaitCatchFrame = _currentAwaitCatchFrame;
		if (awaitCatchFrame == null)
		{
			TryStatementSyntax tryStatementSyntax = (TryStatementSyntax)node.Syntax.Parent;
			awaitCatchFrame = (_currentAwaitCatchFrame = new AwaitCatchFrame(_F, tryStatementSyntax, parentAwaitCatchFrame));
		}
		TypeSymbol typeSymbol = node.ExceptionTypeOpt ?? _F.SpecialType(SpecialType.System_Object);
		LocalSymbol localSymbol = _F.SynthesizedLocal(typeSymbol);
		BoundLocal arg = _F.Local(localSymbol);
		TypeSymbol type = awaitCatchFrame.pendingCaughtException.Type;
		Conversion conversion = _F.ClassifyEmitConversion(arg, type);
		BoundExpression expr = _F.AssignmentExpression(_F.Local(awaitCatchFrame.pendingCaughtException), _F.Convert(type, arg, conversion));
		BoundExpressionStatement boundExpressionStatement = _F.Assignment(_F.Local(awaitCatchFrame.pendingCatch), _F.Literal(awaitCatchFrame.handlers.Count + 1));
		BoundStatementList exceptionFilterPrologueOpt = node.ExceptionFilterPrologueOpt;
		BoundExpression exceptionFilterOpt = node.ExceptionFilterOpt;
		BoundCatchBlock result2;
		ImmutableArray<LocalSymbol> locals;
		if (exceptionFilterOpt == null)
		{
			result2 = node.Update(ImmutableArray.Create(localSymbol), _F.Local(localSymbol), typeSymbol, exceptionFilterPrologueOpt, null, _F.Block(_F.HiddenSequencePoint(), _F.ExpressionStatement(expr), boundExpressionStatement), node.IsSynthesizedAsyncCatchAll);
			locals = node.Locals;
		}
		else
		{
			locals = ImmutableArray<LocalSymbol>.Empty;
			foreach (LocalSymbol local in node.Locals)
			{
				awaitCatchFrame.HoistLocal(local, _F);
			}
			BoundStatementList boundStatementList = (BoundStatementList)Visit(exceptionFilterPrologueOpt);
			ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
			BoundExpression exceptionSourceOpt = node.ExceptionSourceOpt;
			instance.Add(_F.ExpressionStatement(expr));
			if (exceptionSourceOpt != null)
			{
				instance.Add(_F.ExpressionStatement(AssignCatchSource((BoundExpression)Visit(exceptionSourceOpt), awaitCatchFrame)));
			}
			if (boundStatementList != null)
			{
				instance.Add(boundStatementList);
			}
			BoundStatementList exceptionFilterPrologueOpt2 = _F.StatementList(instance.ToImmutableAndFree());
			BoundExpression exceptionFilterOpt2 = (BoundExpression)Visit(exceptionFilterOpt);
			result2 = node.Update(ImmutableArray.Create(localSymbol), _F.Local(localSymbol), typeSymbol, exceptionFilterPrologueOpt2, exceptionFilterOpt2, _F.Block(_F.HiddenSequencePoint(), boundExpressionStatement), node.IsSynthesizedAsyncCatchAll);
		}
		ArrayBuilder<BoundStatement> instance2 = ArrayBuilder<BoundStatement>.GetInstance();
		instance2.Add(_F.HiddenSequencePoint());
		if (exceptionFilterOpt == null)
		{
			BoundExpression exceptionSourceOpt2 = node.ExceptionSourceOpt;
			if (exceptionSourceOpt2 != null)
			{
				BoundExpression expr2 = AssignCatchSource((BoundExpression)Visit(exceptionSourceOpt2), awaitCatchFrame);
				instance2.Add(_F.ExpressionStatement(expr2));
			}
		}
		instance2.Add((BoundStatement)Visit(node.Body));
		BoundBlock item = _F.Block(locals, instance2.ToImmutableAndFree());
		awaitCatchFrame.handlers.Add(item);
		return result2;
	}

	private BoundExpression AssignCatchSource(BoundExpression rewrittenSource, AwaitCatchFrame currentAwaitCatchFrame)
	{
		BoundExpression result = null;
		if (rewrittenSource != null)
		{
			BoundLocal arg = _F.Local(currentAwaitCatchFrame.pendingCaughtException);
			TypeSymbol type = rewrittenSource.Type;
			Conversion conversion = _F.ClassifyEmitConversion(arg, type);
			result = _F.AssignmentExpression(rewrittenSource, _F.Convert(type, arg, conversion));
		}
		return result;
	}

	public override BoundNode VisitLocal(BoundLocal node)
	{
		AwaitCatchFrame currentAwaitCatchFrame = _currentAwaitCatchFrame;
		if (currentAwaitCatchFrame == null || !currentAwaitCatchFrame.TryGetHoistedLocal(node.LocalSymbol, out var hoistedLocal))
		{
			return base.VisitLocal(node);
		}
		return node.Update(hoistedLocal, node.ConstantValueOpt, hoistedLocal.Type);
	}

	public override BoundNode VisitThrowStatement(BoundThrowStatement node)
	{
		if (node.ExpressionOpt != null || _currentAwaitCatchFrame == null || _inCatchWithoutAwaits)
		{
			return base.VisitThrowStatement(node);
		}
		return Rethrow(_currentAwaitCatchFrame.pendingCaughtException);
	}

	public override BoundNode VisitLambda(BoundLambda node)
	{
		MethodSymbol currentFunction = _F.CurrentFunction;
		AwaitFinallyFrame currentAwaitFinallyFrame = _currentAwaitFinallyFrame;
		bool needsFinalThrow = _needsFinalThrow;
		_F.CurrentFunction = node.Symbol;
		_currentAwaitFinallyFrame = new AwaitFinallyFrame();
		_needsFinalThrow = false;
		BoundLambda boundLambda = (BoundLambda)base.VisitLambda(node);
		boundLambda = boundLambda.Update(boundLambda.UnboundLambda, boundLambda.Symbol, (BoundBlock)FinalizeMethodBody(boundLambda.Body), node.Diagnostics, node.Binder, node.Type);
		_F.CurrentFunction = currentFunction;
		_currentAwaitFinallyFrame = currentAwaitFinallyFrame;
		_needsFinalThrow = needsFinalThrow;
		return boundLambda;
	}

	public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		MethodSymbol currentFunction = _F.CurrentFunction;
		AwaitFinallyFrame currentAwaitFinallyFrame = _currentAwaitFinallyFrame;
		bool needsFinalThrow = _needsFinalThrow;
		_F.CurrentFunction = node.Symbol;
		_currentAwaitFinallyFrame = new AwaitFinallyFrame();
		_needsFinalThrow = false;
		BoundLocalFunctionStatement boundLocalFunctionStatement = (BoundLocalFunctionStatement)base.VisitLocalFunctionStatement(node);
		boundLocalFunctionStatement = boundLocalFunctionStatement.Update(node.Symbol, (BoundBlock)FinalizeMethodBody(boundLocalFunctionStatement.Body), (BoundBlock)FinalizeMethodBody(boundLocalFunctionStatement.ExpressionBody));
		_F.CurrentFunction = currentFunction;
		_currentAwaitFinallyFrame = currentAwaitFinallyFrame;
		_needsFinalThrow = needsFinalThrow;
		return boundLocalFunctionStatement;
	}

	private AwaitFinallyFrame PushFrame(BoundTryStatement statement)
	{
		return _currentAwaitFinallyFrame = new AwaitFinallyFrame(_currentAwaitFinallyFrame, _analysis.Labels(statement), statement.Syntax);
	}

	private void PopFrame()
	{
		AwaitFinallyFrame currentAwaitFinallyFrame = _currentAwaitFinallyFrame;
		_currentAwaitFinallyFrame = currentAwaitFinallyFrame.ParentOpt;
	}
}
