using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ExtensionMethodReferenceRewriter : BoundTreeRewriterWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
{
	private ExtensionMethodReferenceRewriter()
	{
	}

	public static BoundStatement Rewrite(BoundStatement statement)
	{
		return (BoundStatement)new ExtensionMethodReferenceRewriter().Visit(statement);
	}

	public override BoundNode VisitCall(BoundCall node)
	{
		return VisitCall(this, node);
	}

	public static BoundNode VisitCall(BoundTreeRewriter rewriter, BoundCall node)
	{
		BoundExpression boundExpression;
		if (LocalRewriter.TryGetReceiver(node, out BoundCall receiver))
		{
			ArrayBuilder<BoundCall> instance = ArrayBuilder<BoundCall>.GetInstance();
			instance.Push(node);
			node = receiver;
			BoundCall receiver2;
			while (LocalRewriter.TryGetReceiver(node, out receiver2))
			{
				instance.Push(node);
				node = receiver2;
			}
			BoundExpression rewrittenReceiver = (BoundExpression)rewriter.Visit(node.ReceiverOpt);
			do
			{
				boundExpression = visitArgumentsAndFinishRewrite(rewriter, node, rewrittenReceiver);
				rewrittenReceiver = boundExpression;
			}
			while (instance.TryPop(out node));
			instance.Free();
		}
		else
		{
			BoundExpression rewrittenReceiver2 = (BoundExpression)rewriter.Visit(node.ReceiverOpt);
			boundExpression = visitArgumentsAndFinishRewrite(rewriter, node, rewrittenReceiver2);
		}
		return boundExpression;
		static BoundExpression updateCall(BoundCall boundCall, MethodSymbol method, ImmutableArray<MethodSymbol> originalMethodsOpt, BoundExpression? receiverOpt, ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> argumentRefKinds, bool invokedAsExtensionMethod, TypeSymbol type)
		{
			if (receiverOpt != null && arguments.Length == method.ParameterCount - 1)
			{
				RefKind refKind = method.Parameters[0].RefKind;
				if (argumentRefKinds.IsDefault)
				{
					if (refKind != RefKind.None)
					{
						ArrayBuilder<RefKind> instance2 = ArrayBuilder<RefKind>.GetInstance(method.ParameterCount, RefKind.None);
						instance2[0] = ReceiverArgumentRefKindFromReceiverRefKind(refKind);
						argumentRefKinds = instance2.ToImmutableAndFree();
					}
				}
				else
				{
					argumentRefKinds = argumentRefKinds.Insert(0, ReceiverArgumentRefKindFromReceiverRefKind(refKind));
				}
				invokedAsExtensionMethod = true;
				arguments = arguments.Insert(0, receiverOpt);
				receiverOpt = null;
			}
			return boundCall.Update(receiverOpt, boundCall.InitialBindingReceiverIsSubjectToCloning, method, arguments, default(ImmutableArray<string>), argumentRefKinds, boundCall.IsDelegateCall, boundCall.Expanded, invokedAsExtensionMethod, default(ImmutableArray<int>), default(BitVector), boundCall.ResultKind, originalMethodsOpt, type);
		}
		static BoundExpression visitArgumentsAndFinishRewrite(BoundTreeRewriter boundTreeRewriter, BoundCall boundCall, BoundExpression? receiverOpt)
		{
			return updateCall(boundCall, VisitMethodSymbolWithExtensionRewrite(boundTreeRewriter, boundCall.Method), boundTreeRewriter.VisitSymbols(boundCall.OriginalMethodsOpt), receiverOpt, boundTreeRewriter.VisitList(boundCall.Arguments), boundCall.ArgumentRefKindsOpt, boundCall.InvokedAsExtensionMethod, boundTreeRewriter.VisitType(boundCall.Type));
		}
	}

	public static RefKind ReceiverArgumentRefKindFromReceiverRefKind(RefKind receiverRefKind)
	{
		return SyntheticBoundNodeFactory.ArgumentRefKindFromParameterRefKind(receiverRefKind, useStrictArgumentRefKinds: false);
	}

	[return: NotNullIfNotNull("method")]
	private static MethodSymbol? VisitMethodSymbolWithExtensionRewrite(BoundTreeRewriter rewriter, MethodSymbol? method)
	{
		if ((object)method != null && method.IsExtensionBlockMember())
		{
			MethodSymbol methodSymbol = method.OriginalDefinition.TryGetCorrespondingExtensionImplementationMethod();
			if ((object)methodSymbol != null)
			{
				method = methodSymbol.AsMember(method.ContainingSymbol.ContainingType).ConstructIfGeneric(method.ContainingType.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics.Concat(method.TypeArgumentsWithAnnotations));
			}
		}
		return rewriter.VisitMethodSymbol(method);
	}

	[return: NotNullIfNotNull("method")]
	public override MethodSymbol? VisitMethodSymbol(MethodSymbol? method)
	{
		return base.VisitMethodSymbol(method);
	}

	public override BoundNode? VisitMethodDefIndex(BoundMethodDefIndex node)
	{
		return VisitMethodDefIndex(this, node);
	}

	public static BoundNode VisitMethodDefIndex(BoundTreeRewriter rewriter, BoundMethodDefIndex node)
	{
		MethodSymbol method = VisitMethodSymbolWithExtensionRewrite(rewriter, node.Method);
		TypeSymbol type = rewriter.VisitType(node.Type);
		return node.Update(method, type);
	}

	public override BoundNode? VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		return VisitDelegateCreationExpression(this, node);
	}

	public static BoundNode VisitDelegateCreationExpression(BoundTreeRewriter rewriter, BoundDelegateCreationExpression node)
	{
		MethodSymbol methodSymbol = VisitMethodSymbolWithExtensionRewrite(rewriter, node.MethodOpt);
		BoundExpression boundExpression = (BoundExpression)rewriter.Visit(node.Argument);
		TypeSymbol type = rewriter.VisitType(node.Type);
		bool flag = node.IsExtensionMethod;
		if (!flag && !(boundExpression is BoundTypeExpression) && (object)methodSymbol != null && methodSymbol.IsStatic)
		{
			flag = true;
		}
		return node.Update(boundExpression, methodSymbol, flag, node.WasTargetTyped, type);
	}

	public override BoundNode VisitFunctionPointerLoad(BoundFunctionPointerLoad node)
	{
		return VisitFunctionPointerLoad(this, node);
	}

	public static BoundNode VisitFunctionPointerLoad(BoundTreeRewriter rewriter, BoundFunctionPointerLoad node)
	{
		MethodSymbol targetMethod = VisitMethodSymbolWithExtensionRewrite(rewriter, node.TargetMethod);
		TypeSymbol constrainedToTypeOpt = rewriter.VisitType(node.ConstrainedToTypeOpt);
		TypeSymbol type = rewriter.VisitType(node.Type);
		return node.Update(targetMethod, constrainedToTypeOpt, type);
	}

	protected override BoundBinaryOperator.UncommonData? VisitBinaryOperatorData(BoundBinaryOperator node)
	{
		return VisitBinaryOperatorData(this, node);
	}

	public static BoundBinaryOperator.UncommonData? VisitBinaryOperatorData(BoundTreeRewriter rewriter, BoundBinaryOperator node)
	{
		MethodSymbol binaryOperatorMethod = node.BinaryOperatorMethod;
		MethodSymbol methodSymbol = VisitMethodSymbolWithExtensionRewrite(rewriter, binaryOperatorMethod);
		TypeSymbol typeSymbol = rewriter.VisitType(node.ConstrainedToType);
		if (Symbol.Equals(methodSymbol, binaryOperatorMethod, TypeCompareKind.AllIgnoreOptions) && TypeSymbol.Equals(typeSymbol, node.ConstrainedToType, TypeCompareKind.AllIgnoreOptions))
		{
			return node.Data;
		}
		return BoundBinaryOperator.UncommonData.CreateIfNeeded(node.ConstantValueOpt, methodSymbol, typeSymbol, node.OriginalUserDefinedOperatorsOpt);
	}

	[return: NotNullIfNotNull("symbol")]
	public override PropertySymbol? VisitPropertySymbol(PropertySymbol? symbol)
	{
		return base.VisitPropertySymbol(symbol);
	}

	public override BoundNode VisitUnaryOperator(BoundUnaryOperator node)
	{
		return VisitUnaryOperator(this, node);
	}

	public static BoundNode VisitUnaryOperator(BoundTreeRewriter rewriter, BoundUnaryOperator node)
	{
		MethodSymbol methodOpt = VisitMethodSymbolWithExtensionRewrite(rewriter, node.MethodOpt);
		ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt = rewriter.VisitSymbols(node.OriginalUserDefinedOperatorsOpt);
		BoundExpression operand = (BoundExpression)rewriter.Visit(node.Operand);
		TypeSymbol constrainedToTypeOpt = rewriter.VisitType(node.ConstrainedToTypeOpt);
		TypeSymbol type = rewriter.VisitType(node.Type);
		return node.Update(node.OperatorKind, operand, node.ConstantValueOpt, methodOpt, constrainedToTypeOpt, node.ResultKind, originalUserDefinedOperatorsOpt, type);
	}
}
