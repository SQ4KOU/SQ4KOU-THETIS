using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface ISpecializedNestedTypeReference : INestedTypeReference, INamedTypeReference, ITypeReference, IReference, INamedEntity, ITypeMemberReference
{
	[return: NotNull]
	INestedTypeReference GetUnspecializedVersion(EmitContext context);
}
