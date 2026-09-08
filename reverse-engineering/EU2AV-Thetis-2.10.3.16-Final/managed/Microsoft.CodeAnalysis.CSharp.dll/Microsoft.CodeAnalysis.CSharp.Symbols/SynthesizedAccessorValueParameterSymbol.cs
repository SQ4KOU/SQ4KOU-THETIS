using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedAccessorValueParameterSymbol : SourceComplexParameterSymbolBase
{
	internal override FlowAnalysisAnnotations FlowAnalysisAnnotations
	{
		get
		{
			FlowAnalysisAnnotations flowAnalysisAnnotations = FlowAnalysisAnnotations.None;
			if (ContainingSymbol is SourcePropertyAccessorSymbol { AssociatedSymbol: SourcePropertySymbolBase associatedSymbol })
			{
				if (associatedSymbol.HasDisallowNull)
				{
					flowAnalysisAnnotations |= FlowAnalysisAnnotations.DisallowNull;
				}
				if (associatedSymbol.HasAllowNull)
				{
					flowAnalysisAnnotations |= FlowAnalysisAnnotations.AllowNull;
				}
			}
			return flowAnalysisAnnotations;
		}
	}

	internal override ImmutableHashSet<string> NotNullIfParameterNotNull => ImmutableHashSet<string>.Empty;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override bool IsImplicitlyDeclared => true;

	protected override IAttributeTargetSymbol AttributeOwner => (SourceMemberMethodSymbol)ContainingSymbol;

	public SynthesizedAccessorValueParameterSymbol(SourceMemberMethodSymbol accessor, int ordinal)
		: base(accessor, ordinal, RefKind.None, "value", accessor.TryGetFirstLocation(), null, hasParamsModifier: false, isParams: false, isExtensionMethodThis: false, ScopedKind.None)
	{
	}

	internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return ((SourceMemberMethodSymbol)ContainingSymbol).GetAttributeDeclarations();
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		AddSynthesizedFlowAnalysisAttributes(ref attributes);
	}

	internal void AddSynthesizedFlowAnalysisAttributes(ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		if (ContainingSymbol is SourcePropertyAccessorSymbol { AssociatedSymbol: SourcePropertySymbolBase associatedSymbol })
		{
			FlowAnalysisAnnotations flowAnalysisAnnotations = FlowAnalysisAnnotations;
			if ((flowAnalysisAnnotations & FlowAnalysisAnnotations.DisallowNull) != FlowAnalysisAnnotations.None)
			{
				Symbol.AddSynthesizedAttribute(ref attributes, SynthesizedAttributeData.Create(associatedSymbol.DisallowNullAttributeIfExists));
			}
			if ((flowAnalysisAnnotations & FlowAnalysisAnnotations.AllowNull) != FlowAnalysisAnnotations.None)
			{
				Symbol.AddSynthesizedAttribute(ref attributes, SynthesizedAttributeData.Create(associatedSymbol.AllowNullAttributeIfExists));
			}
		}
	}
}
