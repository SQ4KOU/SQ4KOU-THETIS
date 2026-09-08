using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class MethodBodySynthesizer
{
	public const int HASH_FACTOR = -1521134295;

	public static BoundExpression GenerateHashCombine(BoundExpression currentHashValue, MethodSymbol system_Collections_Generic_EqualityComparer_T__GetHashCode, MethodSymbol system_Collections_Generic_EqualityComparer_T__get_Default, ref BoundLiteral? boundHashFactor, BoundExpression valueToHash, SyntheticBoundNodeFactory F)
	{
		TypeSymbol type = currentHashValue.Type;
		if (boundHashFactor == null)
		{
			boundHashFactor = F.Literal(-1521134295);
		}
		currentHashValue = F.Binary(BinaryOperatorKind.IntMultiplication, type, currentHashValue, boundHashFactor);
		currentHashValue = F.Binary(BinaryOperatorKind.IntAddition, type, currentHashValue, GenerateGetHashCode(system_Collections_Generic_EqualityComparer_T__GetHashCode, system_Collections_Generic_EqualityComparer_T__get_Default, valueToHash, F));
		return currentHashValue;
	}

	public static BoundCall GenerateGetHashCode(MethodSymbol system_Collections_Generic_EqualityComparer_T__GetHashCode, MethodSymbol system_Collections_Generic_EqualityComparer_T__get_Default, BoundExpression valueToHash, SyntheticBoundNodeFactory F)
	{
		NamedTypeSymbol namedTypeSymbol = system_Collections_Generic_EqualityComparer_T__GetHashCode.ContainingType.Construct(valueToHash.Type);
		return F.Call(F.StaticCall(namedTypeSymbol, system_Collections_Generic_EqualityComparer_T__get_Default.AsMember(namedTypeSymbol)), system_Collections_Generic_EqualityComparer_T__GetHashCode.AsMember(namedTypeSymbol), valueToHash);
	}

	public static BoundExpression GenerateFieldEquals(BoundExpression? initialExpression, BoundExpression otherReceiver, ArrayBuilder<FieldSymbol> fields, SyntheticBoundNodeFactory F)
	{
		MethodSymbol methodSymbol = F.WellKnownMethod(WellKnownMember.System_Collections_Generic_EqualityComparer_T__get_Default);
		MethodSymbol methodSymbol2 = F.WellKnownMethod(WellKnownMember.System_Collections_Generic_EqualityComparer_T__Equals);
		NamedTypeSymbol containingType = methodSymbol2.ContainingType;
		BoundExpression boundExpression = initialExpression;
		foreach (FieldSymbol field in fields)
		{
			NamedTypeSymbol namedTypeSymbol = containingType.Construct(field.Type);
			BoundExpression boundExpression2 = F.Call(F.StaticCall(namedTypeSymbol, methodSymbol.AsMember(namedTypeSymbol)), methodSymbol2.AsMember(namedTypeSymbol), F.Field(F.This(), field), F.Field(otherReceiver, field));
			boundExpression = ((boundExpression == null) ? boundExpression2 : F.LogicalAnd(boundExpression, boundExpression2));
		}
		return boundExpression;
	}

	internal static BoundBlock ConstructSingleInvocationMethodBody(SyntheticBoundNodeFactory F, MethodSymbol methodToInvoke, bool useBaseReference)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		foreach (ParameterSymbol parameter in F.CurrentFunction.Parameters)
		{
			instance.Add(F.Parameter(parameter));
		}
		BoundExpression boundExpression = F.Call(methodToInvoke.IsStatic ? null : (useBaseReference ? ((BoundExpression)F.Base(methodToInvoke.ContainingType)) : ((BoundExpression)F.This())), methodToInvoke, instance.ToImmutableAndFree());
		if (!F.CurrentFunction.ReturnsVoid)
		{
			return F.Block(F.Return(boundExpression));
		}
		return F.Block(F.ExpressionStatement(boundExpression), F.Return());
	}
}
