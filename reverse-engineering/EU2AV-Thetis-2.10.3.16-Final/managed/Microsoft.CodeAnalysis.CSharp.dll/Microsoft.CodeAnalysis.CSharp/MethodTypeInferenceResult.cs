using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal readonly struct MethodTypeInferenceResult(bool success, ImmutableArray<TypeWithAnnotations> inferredTypeArguments, bool hasTypeArgumentInferredFromFunctionType)
{
	public readonly ImmutableArray<TypeWithAnnotations> InferredTypeArguments = inferredTypeArguments;

	public readonly bool HasTypeArgumentInferredFromFunctionType = hasTypeArgumentInferredFromFunctionType;

	public readonly bool Success = success;
}
