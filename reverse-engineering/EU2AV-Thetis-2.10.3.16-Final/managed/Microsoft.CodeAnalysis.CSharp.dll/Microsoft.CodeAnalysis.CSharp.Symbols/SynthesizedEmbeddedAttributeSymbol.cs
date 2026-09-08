using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedEmbeddedAttributeSymbol : SynthesizedEmbeddedAttributeSymbolBase
{
	private readonly ImmutableArray<MethodSymbol> _constructors;

	public override ImmutableArray<MethodSymbol> Constructors => _constructors;

	public SynthesizedEmbeddedAttributeSymbol(string name, NamespaceSymbol containingNamespace, ModuleSymbol containingModule, NamedTypeSymbol baseType)
		: base(name, containingNamespace, containingModule, baseType)
	{
		_constructors = ImmutableArray.Create((MethodSymbol)new SynthesizedEmbeddedAttributeConstructorSymbol(this, (MethodSymbol m) => ImmutableArray<ParameterSymbol>.Empty));
	}
}
