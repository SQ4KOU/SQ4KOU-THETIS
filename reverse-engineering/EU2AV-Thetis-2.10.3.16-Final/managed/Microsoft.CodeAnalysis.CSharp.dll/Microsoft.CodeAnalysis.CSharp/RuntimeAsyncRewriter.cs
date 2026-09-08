using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class RuntimeAsyncRewriter : BoundTreeRewriterWithStackGuard
{
	private readonly SyntheticBoundNodeFactory _factory;

	private readonly Dictionary<BoundAwaitableValuePlaceholder, BoundExpression> _placeholderMap;

	private readonly IReadOnlySet<Symbol> _variablesToHoist;

	private readonly RefInitializationHoister<LocalSymbol, BoundLocal> _refInitializationHoister;

	private readonly ArrayBuilder<LocalSymbol> _hoistedLocals;

	private readonly Dictionary<Symbol, CapturedSymbolReplacement> _proxies = new Dictionary<Symbol, CapturedSymbolReplacement>();

	public static BoundStatement Rewrite(BoundStatement node, MethodSymbol method, TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		if (!method.IsAsync)
		{
			return node;
		}
		OrderedSet<Symbol> variablesToHoist = IteratorAndAsyncCaptureWalker.Analyze(compilationState.Compilation, method, node, isRuntimeAsync: true, diagnostics.DiagnosticBag);
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(method, node.Syntax, compilationState, diagnostics);
		RuntimeAsyncRewriter runtimeAsyncRewriter = new RuntimeAsyncRewriter(syntheticBoundNodeFactory, variablesToHoist, instance);
		BoundAssignmentOperator boundAssignmentOperator = hoistThisIfNeeded(runtimeAsyncRewriter);
		BoundStatement boundStatement = (BoundStatement)runtimeAsyncRewriter.Visit(node);
		if (boundAssignmentOperator != null)
		{
			boundStatement = syntheticBoundNodeFactory.Block(instance.ToImmutableAndFree(), syntheticBoundNodeFactory.HiddenSequencePoint(), syntheticBoundNodeFactory.ExpressionStatement(boundAssignmentOperator), boundStatement);
		}
		else if (instance.Count > 0)
		{
			boundStatement = syntheticBoundNodeFactory.Block(instance.ToImmutableAndFree(), boundStatement);
		}
		else
		{
			instance.Free();
		}
		return SpillSequenceSpiller.Rewrite(boundStatement, method, compilationState, diagnostics);
		static BoundAssignmentOperator? hoistThisIfNeeded(RuntimeAsyncRewriter rewriter)
		{
			ParameterSymbol thisParameter = rewriter._factory.CurrentFunction.ThisParameter;
			if ((object)thisParameter != null)
			{
				TypeSymbol type = thisParameter.Type;
				if ((object)type != null && type.IsValueType && thisParameter.RefKind != RefKind.None)
				{
					BoundLocal boundLocal = rewriter._factory.StoreToTemp(rewriter._factory.This(), out BoundAssignmentOperator store, RefKind.None, SynthesizedLocalKind.AwaitByRefSpill);
					rewriter._hoistedLocals.Add(boundLocal.LocalSymbol);
					rewriter._proxies.Add(thisParameter, new CapturedToExpressionSymbolReplacement<ParameterSymbol>(boundLocal, ImmutableArray<ParameterSymbol>.Empty, isReusable: true));
					return store;
				}
			}
			return null;
		}
	}

	private RuntimeAsyncRewriter(SyntheticBoundNodeFactory factory, IReadOnlySet<Symbol> variablesToHoist, ArrayBuilder<LocalSymbol> hoistedLocals)
	{
		_factory = factory;
		_placeholderMap = new Dictionary<BoundAwaitableValuePlaceholder, BoundExpression>();
		_variablesToHoist = variablesToHoist;
		_refInitializationHoister = new RefInitializationHoister<LocalSymbol, BoundLocal>(_factory, _factory.CurrentFunction, TypeMap.Empty);
		_hoistedLocals = hoistedLocals;
	}

	[return: NotNullIfNotNull("node")]
	public override BoundNode? Visit(BoundNode? node)
	{
		if (node == null)
		{
			return node;
		}
		SyntaxNode syntax = _factory.Syntax;
		_factory.Syntax = node.Syntax;
		BoundNode? result = base.Visit(node);
		_factory.Syntax = syntax;
		return result;
	}

	[return: NotNullIfNotNull("node")]
	public BoundExpression? VisitExpression(BoundExpression? node)
	{
		return (BoundExpression)Visit(node);
	}

	public override BoundNode? VisitAwaitExpression(BoundAwaitExpression node)
	{
		_ = node.Expression.Type;
		BoundAwaitableInfo awaitableInfo = node.AwaitableInfo;
		if (awaitableInfo.IsDynamic)
		{
			_factory.Diagnostics.Add(ErrorCode.ERR_UnsupportedFeatureInRuntimeAsync, node.Syntax.Location, _factory.CurrentFunction);
			return node.WithHasErrors();
		}
		if (awaitableInfo.RuntimeAsyncAwaitCall.Method.Name == "Await")
		{
			BoundExpression value = VisitExpression(node.Expression);
			_placeholderMap.Add(awaitableInfo.RuntimeAsyncAwaitCallPlaceholder, value);
			BoundNode? result = Visit(awaitableInfo.RuntimeAsyncAwaitCall);
			_placeholderMap.Remove(awaitableInfo.RuntimeAsyncAwaitCallPlaceholder);
			return result;
		}
		return RewriteCustomAwaiterAwait(node);
	}

	private BoundExpression RewriteCustomAwaiterAwait(BoundAwaitExpression node)
	{
		BoundExpression value = VisitExpression(node.Expression);
		BoundAwaitableInfo awaitableInfo = node.AwaitableInfo;
		BoundAwaitableValuePlaceholder awaitableInstancePlaceholder = awaitableInfo.AwaitableInstancePlaceholder;
		if (awaitableInstancePlaceholder != null)
		{
			_placeholderMap.Add(awaitableInstancePlaceholder, value);
		}
		BoundExpression argument = VisitExpression(awaitableInfo.GetAwaiter);
		if (awaitableInstancePlaceholder != null)
		{
			_placeholderMap.Remove(awaitableInstancePlaceholder);
		}
		BoundLocal boundLocal = _factory.StoreToTemp(argument, out BoundAssignmentOperator store, RefKind.None, SynthesizedLocalKind.Awaiter);
		MethodSymbol getMethod = awaitableInfo.IsCompleted.GetMethod;
		BoundCall expression = _factory.Call(boundLocal, getMethod);
		_placeholderMap.Add(awaitableInfo.RuntimeAsyncAwaitCallPlaceholder, boundLocal);
		BoundCall expr = (BoundCall)Visit(awaitableInfo.RuntimeAsyncAwaitCall);
		_placeholderMap.Remove(awaitableInfo.RuntimeAsyncAwaitCallPlaceholder);
		BoundStatement boundStatement = _factory.If(_factory.Not(expression), _factory.ExpressionStatement(expr));
		MethodSymbol getResult = awaitableInfo.GetResult;
		BoundCall result = _factory.Call(boundLocal, getResult);
		return _factory.SpillSequence(ImmutableCollectionsMarshal.AsImmutableArray(new LocalSymbol[1] { boundLocal.LocalSymbol }), ImmutableCollectionsMarshal.AsImmutableArray(new BoundStatement[2]
		{
			_factory.ExpressionStatement(store),
			boundStatement
		}), result);
	}

	public override BoundNode VisitAwaitableValuePlaceholder(BoundAwaitableValuePlaceholder node)
	{
		return _placeholderMap[node];
	}

	public override BoundNode? VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		if (!(node.Left is BoundLocal boundLocal))
		{
			return base.VisitAssignmentOperator(node);
		}
		BoundExpression visitedRight;
		if (_variablesToHoist.Contains(boundLocal.LocalSymbol) && !_proxies.ContainsKey(boundLocal.LocalSymbol))
		{
			visitedRight = VisitExpression(node.Right);
			return _refInitializationHoister.HoistRefInitialization(boundLocal.LocalSymbol, visitedRight, _proxies, createHoistedLocal, createHoistedAccess, this, isRuntimeAsync: true);
		}
		BoundExpression boundExpression = VisitExpression(boundLocal);
		visitedRight = VisitExpression(node.Right);
		if (!(boundExpression is BoundLocal))
		{
			BoundExpression boundExpression2 = _factory.AssignmentExpression(boundLocal, boundExpression, isRef: true);
			return _factory.Sequence(new BoundExpression[1] { boundExpression2 }, node.Update(boundLocal, visitedRight, node.IsRef, node.Type));
		}
		return node.Update(boundExpression, visitedRight, node.IsRef, node.Type);
		static BoundLocal createHoistedAccess(LocalSymbol local, RuntimeAsyncRewriter @this)
		{
			return @this._factory.Local(local);
		}
		static LocalSymbol createHoistedLocal(TypeSymbol type, RuntimeAsyncRewriter @this, LocalSymbol local)
		{
			LocalSymbol localSymbol = @this._factory.SynthesizedLocal(type, local.GetDeclaratorSyntax(), isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.AwaitByRefSpill);
			@this._hoistedLocals.Add(localSymbol);
			return localSymbol;
		}
	}

	private bool TryReplaceWithProxy(Symbol localOrParameter, SyntaxNode syntax, [NotNullWhen(true)] out BoundNode? replacement)
	{
		if (_proxies.TryGetValue(localOrParameter, out CapturedSymbolReplacement value))
		{
			replacement = value.Replacement(syntax, null, this);
			return true;
		}
		replacement = null;
		return false;
	}

	public override BoundNode VisitLocal(BoundLocal node)
	{
		if (TryReplaceWithProxy(node.LocalSymbol, node.Syntax, out BoundNode replacement))
		{
			return replacement;
		}
		return base.VisitLocal(node);
	}

	public override BoundNode? VisitParameter(BoundParameter node)
	{
		if (TryReplaceWithProxy(node.ParameterSymbol, node.Syntax, out BoundNode _))
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/AsyncRewriter/RuntimeAsyncRewriter.cs", 295);
		}
		return base.VisitParameter(node);
	}

	public override BoundNode? VisitThisReference(BoundThisReference node)
	{
		ParameterSymbol thisParameter = _factory.CurrentFunction.ThisParameter;
		if (TryReplaceWithProxy(thisParameter, node.Syntax, out BoundNode replacement))
		{
			return replacement;
		}
		return base.VisitThisReference(node);
	}

	public override BoundNode? VisitExpressionStatement(BoundExpressionStatement node)
	{
		BoundExpression boundExpression = VisitExpression(node.Expression);
		if (boundExpression == null)
		{
			return _factory.StatementList();
		}
		return node.Update(boundExpression);
	}
}
