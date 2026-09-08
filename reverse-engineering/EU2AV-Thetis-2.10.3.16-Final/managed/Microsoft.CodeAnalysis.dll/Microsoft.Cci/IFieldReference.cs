using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface IFieldReference : ITypeMemberReference, IReference, INamedEntity
{
	ImmutableArray<ICustomModifier> RefCustomModifiers { get; }

	bool IsByReference { get; }

	ISpecializedFieldReference? AsSpecializedFieldReference { get; }

	bool IsContextualNamedEntity { get; }

	ITypeReference GetType(EmitContext context);

	IFieldDefinition? GetResolvedField(EmitContext context);
}
