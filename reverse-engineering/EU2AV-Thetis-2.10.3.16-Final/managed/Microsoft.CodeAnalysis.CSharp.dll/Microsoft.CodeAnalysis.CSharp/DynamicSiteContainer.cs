using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class DynamicSiteContainer : SynthesizedContainer, ISynthesizedMethodBodyImplementationSymbol, ISymbolInternal
{
	private readonly MethodSymbol _topLevelMethod;

	public override Symbol ContainingSymbol
	{
		get
		{
			Symbol containingSymbol = _topLevelMethod.ContainingSymbol;
			if (containingSymbol is NamedTypeSymbol { IsExtension: not false })
			{
				return containingSymbol.ContainingSymbol;
			}
			return containingSymbol;
		}
	}

	public override TypeKind TypeKind => TypeKind.Class;

	public sealed override bool AreLocalsZeroed
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/DynamicSiteContainer.cs", 47);
		}
	}

	internal override bool IsRecord => false;

	internal override bool IsRecordStruct => false;

	bool ISynthesizedMethodBodyImplementationSymbol.HasMethodBodyDependency => true;

	IMethodSymbolInternal ISynthesizedMethodBodyImplementationSymbol.Method => _topLevelMethod;

	internal DynamicSiteContainer(string name, MethodSymbol topLevelMethod, MethodSymbol containingMethod)
		: base(name, ((topLevelMethod.ContainingSymbol is NamedTypeSymbol { IsExtension: not false } namedTypeSymbol) ? namedTypeSymbol.TypeParameters : ImmutableArray<TypeParameterSymbol>.Empty).Concat(TypeMap.ConcatMethodTypeParameters(containingMethod, null)))
	{
		_topLevelMethod = topLevelMethod;
	}

	internal override bool HasPossibleWellKnownCloneMethod()
	{
		return false;
	}
}
