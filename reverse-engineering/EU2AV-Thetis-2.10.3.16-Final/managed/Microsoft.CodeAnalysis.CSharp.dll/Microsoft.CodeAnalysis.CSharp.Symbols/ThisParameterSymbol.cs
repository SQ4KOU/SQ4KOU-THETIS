using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ThisParameterSymbol : ThisParameterSymbolBase
{
	private readonly MethodSymbol? _containingMethod;

	private readonly TypeSymbol _containingType;

	public override TypeWithAnnotations TypeWithAnnotations => TypeWithAnnotations.Create(_containingType, NullableAnnotation.NotAnnotated);

	public override RefKind RefKind
	{
		get
		{
			NamedTypeSymbol containingType = ContainingType;
			if ((object)containingType == null || containingType.TypeKind != TypeKind.Struct)
			{
				return RefKind.None;
			}
			MethodSymbol? containingMethod = _containingMethod;
			if ((object)containingMethod != null && containingMethod.MethodKind == MethodKind.Constructor)
			{
				return RefKind.Out;
			}
			MethodSymbol? containingMethod2 = _containingMethod;
			if ((object)containingMethod2 != null && containingMethod2.IsEffectivelyReadOnly)
			{
				return RefKind.In;
			}
			return RefKind.Ref;
		}
	}

	public override ImmutableArray<Location> Locations
	{
		get
		{
			if ((object)_containingMethod == null)
			{
				return ImmutableArray<Location>.Empty;
			}
			return _containingMethod.Locations;
		}
	}

	public override Symbol ContainingSymbol => (Symbol)(((object)_containingMethod) ?? ((object)_containingType));

	internal override ScopedKind DeclaredScope
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/ThisParameterSymbol.cs", 186);
		}
	}

	internal override ScopedKind EffectiveScope
	{
		get
		{
			ScopedKind scopedKind = (_containingType.IsStructType() ? ScopedKind.ScopedRef : ScopedKind.None);
			if (scopedKind != ScopedKind.None && HasUnscopedRefAttribute && UseUpdatedEscapeRules)
			{
				return ScopedKind.None;
			}
			return scopedKind;
		}
	}

	internal override bool HasUnscopedRefAttribute => _containingMethod.HasUnscopedRefAttributeOnMethodOrProperty();

	internal sealed override bool UseUpdatedEscapeRules => _containingMethod?.UseUpdatedEscapeRules ?? _containingType.ContainingModule.UseUpdatedEscapeRules;

	internal ThisParameterSymbol(MethodSymbol forMethod)
		: this(forMethod, forMethod.ContainingType)
	{
	}

	internal ThisParameterSymbol(MethodSymbol? forMethod, TypeSymbol containingType)
	{
		_containingMethod = forMethod;
		_containingType = containingType;
	}
}
