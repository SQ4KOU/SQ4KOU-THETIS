using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedPrimaryConstructorParameterBackingFieldSymbol : SynthesizedBackingFieldSymbolBase
{
	public readonly ParameterSymbol ParameterSymbol;

	internal override bool HasInitializer => true;

	protected override IAttributeTargetSymbol AttributeOwner => this;

	internal override Location ErrorLocation => ParameterSymbol.TryGetFirstLocation() ?? NoLocation.Singleton;

	public override Symbol? AssociatedSymbol => null;

	public override ImmutableArray<Location> Locations => ParameterSymbol.Locations;

	public override RefKind RefKind => RefKind.None;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	internal override bool HasPointerType => base.HasPointerType;

	public override Symbol ContainingSymbol => ParameterSymbol.ContainingSymbol.ContainingSymbol;

	public override NamedTypeSymbol ContainingType => ParameterSymbol.ContainingSymbol.ContainingType;

	public SynthesizedPrimaryConstructorParameterBackingFieldSymbol(ParameterSymbol parameterSymbol, string name, bool isReadOnly)
		: base(name, isReadOnly, isStatic: false)
	{
		ParameterSymbol = parameterSymbol;
	}

	protected override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return OneOrMany<SyntaxList<AttributeListSyntax>>.Empty;
	}

	internal override TypeWithAnnotations GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		return ParameterSymbol.TypeWithAnnotations;
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		foreach (CSharpAttributeData attribute in ParameterSymbol.GetAttributes())
		{
			NamedTypeSymbol attributeClass = attribute.AttributeClass;
			if ((object)attributeClass != null && attributeClass.HasCompilerLoweringPreserveAttribute && (attributeClass.GetAttributeUsageInfo().ValidTargets & AttributeTargets.Field) != 0)
			{
				Symbol.AddSynthesizedAttribute(ref attributes, attribute);
			}
		}
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
	}
}
