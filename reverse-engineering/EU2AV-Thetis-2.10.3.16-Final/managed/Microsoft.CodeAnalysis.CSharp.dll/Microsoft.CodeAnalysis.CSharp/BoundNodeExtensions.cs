using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class BoundNodeExtensions
{
	private class ContainsAwaitVisitor : BoundTreeWalkerWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
	{
		public bool ContainsAwait;

		public override BoundNode? Visit(BoundNode? node)
		{
			if (!ContainsAwait)
			{
				return base.Visit(node);
			}
			return null;
		}

		public override BoundNode? VisitAwaitExpression(BoundAwaitExpression node)
		{
			ContainsAwait = true;
			return null;
		}
	}

	public static bool HasErrors<T>(this ImmutableArray<T> nodeArray) where T : BoundNode
	{
		if (nodeArray.IsDefault)
		{
			return false;
		}
		int i = 0;
		for (int length = nodeArray.Length; i < length; i++)
		{
			if (nodeArray[i].HasErrors)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasErrors([NotNullWhen(true)] this BoundNode? node)
	{
		return node?.HasErrors ?? false;
	}

	public static bool IsConstructorInitializer(this BoundStatement statement)
	{
		if (statement.Kind == BoundKind.ExpressionStatement)
		{
			BoundExpression boundExpression = ((BoundExpressionStatement)statement).Expression;
			if (boundExpression.Kind == BoundKind.Sequence && ((BoundSequence)boundExpression).SideEffects.IsDefaultOrEmpty)
			{
				boundExpression = ((BoundSequence)boundExpression).Value;
			}
			if (boundExpression.Kind == BoundKind.Call)
			{
				return ((BoundCall)boundExpression).IsConstructorInitializer();
			}
			return false;
		}
		return false;
	}

	public static bool IsConstructorInitializer(this BoundCall call)
	{
		MethodSymbol method = call.Method;
		BoundExpression receiverOpt = call.ReceiverOpt;
		if (method.MethodKind == MethodKind.Constructor && receiverOpt != null)
		{
			if (receiverOpt.Kind != BoundKind.ThisReference)
			{
				return receiverOpt.Kind == BoundKind.BaseReference;
			}
			return true;
		}
		return false;
	}

	public static T MakeCompilerGenerated<T>(this T node) where T : BoundNode
	{
		node.WasCompilerGenerated = true;
		return node;
	}

	public static bool ContainsAwaitExpression(this ImmutableArray<BoundExpression> expressions)
	{
		ContainsAwaitVisitor containsAwaitVisitor = new ContainsAwaitVisitor();
		foreach (BoundExpression item in expressions)
		{
			containsAwaitVisitor.Visit(item);
			if (containsAwaitVisitor.ContainsAwait)
			{
				return true;
			}
		}
		return false;
	}

	public static bool VisitBinaryOperatorInterpolatedString<TInterpolatedStringType, TArg>(this BoundBinaryOperator binary, TArg arg, Func<TInterpolatedStringType, TArg, bool> stringCallback, Action<BoundBinaryOperator, TArg>? binaryOperatorCallback = null) where TInterpolatedStringType : BoundInterpolatedStringBase
	{
		ArrayBuilder<BoundBinaryOperator> instance = ArrayBuilder<BoundBinaryOperator>.GetInstance();
		pushLeftNodes(binary, instance, arg, binaryOperatorCallback);
		BoundBinaryOperator result;
		while (instance.TryPop(out result))
		{
			BoundExpression left = result.Left;
			if (!(left is BoundBinaryOperator))
			{
				if (!(left is TInterpolatedStringType arg2))
				{
					throw ExceptionUtilities.UnexpectedValue(result.Left.Kind);
				}
				if (!stringCallback(arg2, arg))
				{
					return false;
				}
			}
			left = result.Right;
			if (!(left is BoundBinaryOperator binary2))
			{
				if (!(left is TInterpolatedStringType arg3))
				{
					throw ExceptionUtilities.UnexpectedValue(result.Right.Kind);
				}
				if (!stringCallback(arg3, arg))
				{
					return false;
				}
			}
			else
			{
				pushLeftNodes(binary2, instance, arg, binaryOperatorCallback);
			}
		}
		instance.Free();
		return true;
		static void pushLeftNodes(BoundBinaryOperator boundBinaryOperator2, ArrayBuilder<BoundBinaryOperator> stack, TArg arg4, Action<BoundBinaryOperator, TArg>? action)
		{
			for (BoundBinaryOperator boundBinaryOperator = boundBinaryOperator2; boundBinaryOperator != null; boundBinaryOperator = boundBinaryOperator.Left as BoundBinaryOperator)
			{
				action?.Invoke(boundBinaryOperator, arg4);
				stack.Push(boundBinaryOperator);
			}
		}
	}

	public static TResult RewriteInterpolatedStringAddition<TInterpolatedStringType, TArg, TResult>(this BoundBinaryOperator binary, TArg arg, Func<TInterpolatedStringType, int, TArg, TResult> interpolatedStringFactory, Func<BoundBinaryOperator, TResult, TResult, TArg, TResult> binaryOperatorFactory) where TInterpolatedStringType : BoundInterpolatedStringBase
	{
		int i = 0;
		return doRewrite(binary, arg, interpolatedStringFactory, binaryOperatorFactory, ref i);
		static TResult doRewrite(BoundBinaryOperator binary2, TArg val3, Func<TInterpolatedStringType, int, TArg, TResult> func, Func<BoundBinaryOperator, TResult, TResult, TArg, TResult> func2, ref int reference)
		{
			TResult val = default(TResult);
			ArrayBuilder<BoundBinaryOperator> instance = ArrayBuilder<BoundBinaryOperator>.GetInstance();
			pushLeftNodes(binary2, instance);
			BoundBinaryOperator result;
			while (instance.TryPop(out result))
			{
				BoundExpression left = result.Left;
				TResult val2;
				if (!(left is TInterpolatedStringType arg2))
				{
					if (!(left is BoundBinaryOperator))
					{
						throw ExceptionUtilities.UnexpectedValue(result.Left.Kind);
					}
					val2 = val;
				}
				else
				{
					val2 = func(arg2, reference++, val3);
				}
				TResult arg3 = val2;
				left = result.Right;
				if (!(left is TInterpolatedStringType arg4))
				{
					if (!(left is BoundBinaryOperator binary3))
					{
						throw ExceptionUtilities.UnexpectedValue(result.Right.Kind);
					}
					val2 = doRewrite(binary3, val3, func, func2, ref reference);
				}
				else
				{
					val2 = func(arg4, reference++, val3);
				}
				TResult arg5 = val2;
				val = func2(result, arg3, arg5, val3);
			}
			instance.Free();
			return val;
		}
		static void pushLeftNodes(BoundBinaryOperator boundBinaryOperator2, ArrayBuilder<BoundBinaryOperator> stack)
		{
			for (BoundBinaryOperator boundBinaryOperator = boundBinaryOperator2; boundBinaryOperator != null; boundBinaryOperator = boundBinaryOperator.Left as BoundBinaryOperator)
			{
				stack.Push(boundBinaryOperator);
			}
		}
	}

	public static InterpolatedStringHandlerData GetInterpolatedStringHandlerData(this BoundExpression e, bool throwOnMissing = true)
	{
		if (e is BoundBinaryOperator { InterpolatedStringHandlerData: var interpolatedStringHandlerData })
		{
			if (interpolatedStringHandlerData.HasValue)
			{
				return interpolatedStringHandlerData.GetValueOrDefault();
			}
		}
		else
		{
			if (!(e is BoundInterpolatedString { InterpolationData: var interpolationData }))
			{
				throw ExceptionUtilities.UnexpectedValue(e.Kind);
			}
			if (interpolationData.HasValue)
			{
				InterpolatedStringHandlerData valueOrDefault = interpolationData.GetValueOrDefault();
				if ((object)valueOrDefault.BuilderType != null)
				{
					return valueOrDefault;
				}
			}
		}
		if (!throwOnMissing)
		{
			return default(InterpolatedStringHandlerData);
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/BoundTree/BoundNodeExtensions.cs", 255);
	}
}
