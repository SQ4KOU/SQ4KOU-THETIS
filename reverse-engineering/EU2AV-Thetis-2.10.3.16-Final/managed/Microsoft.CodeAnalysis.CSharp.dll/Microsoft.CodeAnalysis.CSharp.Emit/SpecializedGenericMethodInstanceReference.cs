using System.Collections.Generic;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class SpecializedGenericMethodInstanceReference : SpecializedMethodReference, IGenericMethodInstanceReference, IMethodReference, ISignature, ITypeMemberReference, IReference, INamedEntity
{
	private readonly SpecializedMethodReference _genericMethod;

	public override IGenericMethodInstanceReference AsGenericMethodInstanceReference => this;

	public SpecializedGenericMethodInstanceReference(MethodSymbol underlyingMethod)
		: base(underlyingMethod)
	{
		_genericMethod = new SpecializedMethodReference(underlyingMethod);
	}

	IEnumerable<ITypeReference> IGenericMethodInstanceReference.GetGenericArguments(EmitContext context)
	{
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		foreach (TypeWithAnnotations typeArgumentsWithAnnotation in UnderlyingMethod.TypeArgumentsWithAnnotations)
		{
			yield return moduleBeingBuilt.Translate(typeArgumentsWithAnnotation.Type, (CSharpSyntaxNode)context.SyntaxNode, context.Diagnostics);
		}
	}

	IMethodReference IGenericMethodInstanceReference.GetGenericMethod(EmitContext context)
	{
		return _genericMethod;
	}

	public override void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}
}
