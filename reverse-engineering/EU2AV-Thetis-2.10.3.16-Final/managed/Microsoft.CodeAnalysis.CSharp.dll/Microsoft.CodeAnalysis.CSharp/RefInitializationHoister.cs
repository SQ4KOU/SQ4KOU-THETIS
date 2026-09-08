using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal class RefInitializationHoister<THoistedSymbol, THoistedAccess>(SyntheticBoundNodeFactory f, MethodSymbol originalMethod, TypeMap typeMap) where THoistedSymbol : Symbol where THoistedAccess : BoundExpression
{
	private readonly SyntheticBoundNodeFactory _factory = f;

	private readonly MethodSymbol _originalMethod = originalMethod;

	private readonly TypeMap _typeMap = typeMap;

	private bool _reportedError;

	internal BoundExpression? HoistRefInitialization<TArg>(LocalSymbol local, BoundExpression visitedRight, Dictionary<Symbol, CapturedSymbolReplacement> proxies, Func<TypeSymbol, TArg, LocalSymbol, THoistedSymbol> createHoistedSymbol, Func<THoistedSymbol, TArg, THoistedAccess> createHoistedAccess, TArg arg, bool isRuntimeAsync)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		bool needsSacrificialEvaluation = false;
		ArrayBuilder<THoistedSymbol> instance2 = ArrayBuilder<THoistedSymbol>.GetInstance();
		BoundExpression boundExpression = HoistExpression(visitedRight, local, local.RefKind, instance, instance2, ref needsSacrificialEvaluation, createHoistedSymbol, createHoistedAccess, arg, isRuntimeAsync, isFieldAccessOfStruct: false);
		proxies.Add(local, new CapturedToExpressionSymbolReplacement<THoistedSymbol>(boundExpression, instance2.ToImmutableAndFree(), isReusable: true));
		if (needsSacrificialEvaluation)
		{
			TypeSymbol type = _typeMap.SubstituteType(local.Type).Type;
			LocalSymbol localSymbol = _factory.SynthesizedLocal(type, null, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.Ref);
			return _factory.Sequence(ImmutableArray.Create(localSymbol), instance.ToImmutableAndFree(), _factory.AssignmentExpression(_factory.Local(localSymbol), boundExpression, isRef: true));
		}
		if (instance.Count == 0)
		{
			instance.Free();
			return null;
		}
		BoundExpression result = instance.Last();
		instance.RemoveLast();
		return _factory.Sequence(ImmutableArray<LocalSymbol>.Empty, instance.ToImmutableAndFree(), result);
	}

	private BoundExpression HoistExpression<TArg>(BoundExpression expr, LocalSymbol assignedLocal, RefKind refKind, ArrayBuilder<BoundExpression> sideEffects, ArrayBuilder<THoistedSymbol> hoistedSymbols, ref bool needsSacrificialEvaluation, Func<TypeSymbol, TArg, LocalSymbol, THoistedSymbol> createHoistedSymbol, Func<THoistedSymbol, TArg, THoistedAccess> createHoistedAccess, TArg arg, bool isRuntimeAsync, bool isFieldAccessOfStruct)
	{
		switch (expr.Kind)
		{
		case BoundKind.ArrayAccess:
		{
			BoundArrayAccess boundArrayAccess = (BoundArrayAccess)expr;
			BoundExpression expression = HoistExpression(boundArrayAccess.Expression, assignedLocal, RefKind.None, sideEffects, hoistedSymbols, ref needsSacrificialEvaluation, createHoistedSymbol, createHoistedAccess, arg, isRuntimeAsync, isFieldAccessOfStruct: false);
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
			foreach (BoundExpression index in boundArrayAccess.Indices)
			{
				instance.Add(HoistExpression(index, assignedLocal, RefKind.None, sideEffects, hoistedSymbols, ref needsSacrificialEvaluation, createHoistedSymbol, createHoistedAccess, arg, isRuntimeAsync, isFieldAccessOfStruct: false));
			}
			needsSacrificialEvaluation = true;
			return boundArrayAccess.Update(expression, instance.ToImmutableAndFree(), boundArrayAccess.Type);
		}
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)expr;
			if (boundFieldAccess.FieldSymbol.IsStatic)
			{
				if (refKind != RefKind.None || boundFieldAccess.FieldSymbol.IsReadOnly)
				{
					return expr;
				}
			}
			else if (refKind != RefKind.None)
			{
				bool flag = !boundFieldAccess.FieldSymbol.ContainingType.IsReferenceType;
				BoundExpression boundExpression = HoistExpression(boundFieldAccess.ReceiverOpt, assignedLocal, flag ? refKind : RefKind.None, sideEffects, hoistedSymbols, ref needsSacrificialEvaluation, createHoistedSymbol, createHoistedAccess, arg, isRuntimeAsync, flag);
				if (boundExpression.Kind != BoundKind.ThisReference && !flag)
				{
					needsSacrificialEvaluation = true;
				}
				return _factory.Field(boundExpression, boundFieldAccess.FieldSymbol);
			}
			break;
		}
		case BoundKind.DefaultExpression:
		case BoundKind.ThisReference:
		case BoundKind.BaseReference:
			return expr;
		case BoundKind.Call:
		{
			BoundCall boundCall = (BoundCall)expr;
			if (refKind != RefKind.None && refKind != RefKind.In && boundCall.Method.RefKind != RefKind.None)
			{
				_factory.Diagnostics.Add(ErrorCode.ERR_RefReturningCallAndAwait, _factory.Syntax.Location, boundCall.Method);
				_reportedError = true;
			}
			refKind = RefKind.None;
			break;
		}
		case BoundKind.ConditionalOperator:
			_ = (BoundConditionalOperator)expr;
			if (refKind != RefKind.None && refKind != RefKind.In)
			{
				_factory.Diagnostics.Add(ErrorCode.ERR_RefConditionalAndAwait, _factory.Syntax.Location);
				_reportedError = true;
			}
			refKind = RefKind.None;
			break;
		}
		if (expr.ConstantValueOpt != null)
		{
			return expr;
		}
		bool flag2;
		if (refKind != RefKind.None)
		{
			if (isRuntimeAsync)
			{
				if (expr is BoundLocal boundLocal)
				{
					LocalSymbol localSymbol = boundLocal.LocalSymbol;
					if ((object)localSymbol != null && localSymbol.RefKind == RefKind.None)
					{
						goto IL_027f;
					}
				}
				else if (expr is BoundParameter boundParameter)
				{
					ParameterSymbol parameterSymbol = boundParameter.ParameterSymbol;
					if ((object)parameterSymbol != null && parameterSymbol.RefKind == RefKind.None)
					{
						goto IL_027f;
					}
				}
				flag2 = false;
				goto IL_0287;
			}
			throw ExceptionUtilities.UnexpectedValue(expr.Kind);
		}
		goto IL_029e;
		IL_027f:
		flag2 = true;
		goto IL_0287;
		IL_0287:
		if (flag2)
		{
			return expr;
		}
		goto IL_029e;
		IL_029e:
		THoistedSymbol val = createHoistedSymbol(expr.Type, arg, assignedLocal);
		hoistedSymbols.Add(val);
		THoistedAccess val2 = createHoistedAccess(val, arg);
		sideEffects.Add(_factory.AssignmentExpression(val2, expr));
		return val2;
	}
}
