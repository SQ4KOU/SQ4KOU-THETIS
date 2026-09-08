using System.Collections.Generic;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface IGenericParameter : IDefinition, IReference, IGenericParameterReference, ITypeReference, INamedEntity, IParameterListEntry
{
	bool MustBeReferenceType { get; }

	bool MustBeValueType { get; }

	bool AllowsRefLikeType { get; }

	bool MustHaveDefaultConstructor { get; }

	TypeParameterVariance Variance { get; }

	IGenericMethodParameter? AsGenericMethodParameter { get; }

	IGenericTypeParameter? AsGenericTypeParameter { get; }

	IEnumerable<TypeReferenceWithAttributes> GetConstraints(EmitContext context);
}
