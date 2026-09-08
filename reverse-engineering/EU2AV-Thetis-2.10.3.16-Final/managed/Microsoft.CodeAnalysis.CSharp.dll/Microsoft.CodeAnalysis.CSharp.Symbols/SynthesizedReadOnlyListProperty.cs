using System.Collections.Immutable;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedReadOnlyListProperty : PropertySymbol
{
	private readonly NamedTypeSymbol _containingType;

	private readonly PropertySymbol _interfaceProperty;

	public override string Name { get; }

	public override RefKind RefKind => RefKind.None;

	public override TypeWithAnnotations TypeWithAnnotations => _interfaceProperty.TypeWithAnnotations;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<ParameterSymbol> Parameters { get; }

	public override bool IsIndexer => Parameters.Length > 0;

	public override MethodSymbol? GetMethod { get; }

	public override MethodSymbol? SetMethod { get; }

	public override ImmutableArray<PropertySymbol> ExplicitInterfaceImplementations => ImmutableArray.Create(_interfaceProperty);

	public override Symbol ContainingSymbol => _containingType;

	public override ImmutableArray<Location> Locations => _containingType.Locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => _containingType.DeclaringSyntaxReferences;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override bool IsStatic => false;

	public override bool IsVirtual => false;

	public override bool IsOverride => false;

	public override bool IsAbstract => false;

	public override bool IsSealed => false;

	public override bool IsExtern => false;

	internal override bool IsRequired => false;

	internal override bool HasSpecialName => false;

	internal override CallingConvention CallingConvention => _interfaceProperty.CallingConvention;

	internal override bool MustCallMethodsDirectly => false;

	internal override bool HasUnscopedRefAttribute => false;

	internal override ObsoleteAttributeData? ObsoleteAttributeData => null;

	internal SynthesizedReadOnlyListProperty(NamedTypeSymbol containingType, PropertySymbol interfaceProperty, GenerateMethodBodyDelegate getAccessorBody, GenerateMethodBodyDelegate? setAccessorBody = null)
	{
		_containingType = containingType;
		_interfaceProperty = interfaceProperty;
		Name = ExplicitInterfaceHelpers.GetMemberName(interfaceProperty.Name, interfaceProperty.ContainingType, null);
		Parameters = interfaceProperty.Parameters.SelectAsArray((ParameterSymbol p, SynthesizedReadOnlyListProperty t) => SynthesizedParameterSymbol.DeriveParameter(t, p), this);
		GetMethod = new SynthesizedReadOnlyListMethod(containingType, interfaceProperty.GetMethod, getAccessorBody);
		SetMethod = (((object)interfaceProperty.SetMethod == null) ? null : new SynthesizedReadOnlyListMethod(containingType, interfaceProperty.SetMethod, setAccessorBody));
	}

	internal override int TryGetOverloadResolutionPriority()
	{
		return 0;
	}
}
