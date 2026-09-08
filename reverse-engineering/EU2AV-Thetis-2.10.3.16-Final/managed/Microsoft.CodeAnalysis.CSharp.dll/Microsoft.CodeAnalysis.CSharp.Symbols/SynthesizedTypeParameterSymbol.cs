using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedTypeParameterSymbol : SubstitutedTypeParameterSymbolBase
{
	private readonly bool _propagateAttributes;

	public override bool IsImplicitlyDeclared => true;

	public override TypeParameterKind TypeParameterKind
	{
		get
		{
			if (!(ContainingSymbol is MethodSymbol))
			{
				return TypeParameterKind.Type;
			}
			return TypeParameterKind.Method;
		}
	}

	public override TypeParameterSymbol OriginalDefinition => this;

	private bool PropagateAttributes
	{
		get
		{
			if (!_propagateAttributes)
			{
				if (ContainingSymbol is SynthesizedMethodBaseSymbol synthesizedMethodBaseSymbol)
				{
					return synthesizedMethodBaseSymbol.InheritsBaseMethodAttributes;
				}
				return false;
			}
			return true;
		}
	}

	public SynthesizedTypeParameterSymbol(Symbol owner, TypeMap map, TypeParameterSymbol substitutedFrom, int ordinal, bool propagateAttributes)
		: base(owner, map, substitutedFrom, ordinal)
	{
		_propagateAttributes = propagateAttributes;
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		if (ContainingSymbol.Kind == SymbolKind.NamedType && !PropagateAttributes)
		{
			TypeParameterSymbol originalDefinition = _underlyingTypeParameter.OriginalDefinition;
			if (ContainingSymbol.ContainingModule == originalDefinition.ContainingModule)
			{
				foreach (CSharpAttributeData attribute in originalDefinition.GetAttributes())
				{
					NamedTypeSymbol attributeClass = attribute.AttributeClass;
					if ((object)attributeClass != null && attributeClass.HasCompilerLoweringPreserveAttribute)
					{
						Symbol.AddSynthesizedAttribute(ref attributes, attribute);
					}
				}
			}
		}
		if (HasUnmanagedTypeConstraint)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeIsUnmanagedAttribute(this));
		}
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		if (PropagateAttributes)
		{
			return _underlyingTypeParameter.GetAttributes();
		}
		return ImmutableArray<CSharpAttributeData>.Empty;
	}
}
