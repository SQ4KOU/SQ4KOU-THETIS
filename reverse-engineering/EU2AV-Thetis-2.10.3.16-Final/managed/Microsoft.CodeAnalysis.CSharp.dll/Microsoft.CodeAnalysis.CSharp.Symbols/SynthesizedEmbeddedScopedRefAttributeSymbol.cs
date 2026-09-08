using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedEmbeddedScopedRefAttributeSymbol : SynthesizedEmbeddedAttributeSymbolBase
{
	private readonly ImmutableArray<MethodSymbol> _constructors;

	public override ImmutableArray<MethodSymbol> Constructors => _constructors;

	public SynthesizedEmbeddedScopedRefAttributeSymbol(string name, NamespaceSymbol containingNamespace, ModuleSymbol containingModule, NamedTypeSymbol systemAttributeType)
		: base(name, containingNamespace, containingModule, systemAttributeType)
	{
		_constructors = ImmutableArray.Create((MethodSymbol)new SynthesizedEmbeddedAttributeConstructorWithBodySymbol(this, (MethodSymbol m) => ImmutableArray<ParameterSymbol>.Empty, delegate
		{
		}));
	}

	internal override AttributeUsageInfo GetAttributeUsageInfo()
	{
		return new AttributeUsageInfo(AttributeTargets.Parameter, allowMultiple: false, inherited: false);
	}
}
