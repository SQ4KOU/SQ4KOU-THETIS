using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal readonly struct InferredLambdaReturnType
{
	internal readonly int NumExpressions;

	internal readonly bool IsExplicitType;

	internal readonly bool HadExpressionlessReturn;

	internal readonly RefKind RefKind;

	internal readonly ImmutableArray<CustomModifier> RefCustomModifiers;

	internal readonly TypeWithAnnotations TypeWithAnnotations;

	internal readonly bool InferredFromFunctionType;

	internal readonly ImmutableArray<DiagnosticInfo> UseSiteDiagnostics;

	internal readonly ImmutableArray<AssemblySymbol> Dependencies;

	internal InferredLambdaReturnType(int numExpressions, bool isExplicitType, bool hadExpressionlessReturn, RefKind refKind, ImmutableArray<CustomModifier> refCustomModifiers, TypeWithAnnotations typeWithAnnotations, bool inferredFromFunctionType, ImmutableArray<DiagnosticInfo> useSiteDiagnostics, ImmutableArray<AssemblySymbol> dependencies)
	{
		NumExpressions = numExpressions;
		IsExplicitType = isExplicitType;
		HadExpressionlessReturn = hadExpressionlessReturn;
		RefKind = refKind;
		RefCustomModifiers = refCustomModifiers;
		TypeWithAnnotations = typeWithAnnotations;
		InferredFromFunctionType = inferredFromFunctionType;
		UseSiteDiagnostics = useSiteDiagnostics;
		Dependencies = dependencies;
	}
}
