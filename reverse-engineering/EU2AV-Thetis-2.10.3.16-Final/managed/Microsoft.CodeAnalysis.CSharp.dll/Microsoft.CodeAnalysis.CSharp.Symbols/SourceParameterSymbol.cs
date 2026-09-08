using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceParameterSymbol : SourceParameterSymbolBase
{
	protected SymbolCompletionState state;

	private readonly string _name;

	private readonly Location? _location;

	private readonly RefKind _refKind;

	private readonly ScopedKind _scope;

	internal sealed override bool RequiresCompletion => true;

	internal abstract bool HasOptionalAttribute { get; }

	internal abstract bool HasDefaultArgumentSyntax { get; }

	internal abstract SyntaxList<AttributeListSyntax> AttributeDeclarationList { get; }

	internal abstract SyntaxReference SyntaxReference { get; }

	internal abstract bool IsExtensionMethodThis { get; }

	public sealed override RefKind RefKind => _refKind;

	internal sealed override ScopedKind DeclaredScope => _scope;

	protected abstract bool HasParamsModifier { get; }

	internal abstract override ScopedKind EffectiveScope { get; }

	internal sealed override bool UseUpdatedEscapeRules => ContainingModule.UseUpdatedEscapeRules;

	public sealed override string Name => _name;

	public sealed override ImmutableArray<Location> Locations
	{
		get
		{
			if ((object)_location != null)
			{
				return ImmutableArray.Create(_location);
			}
			return ImmutableArray<Location>.Empty;
		}
	}

	public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			if (!IsImplicitlyDeclared)
			{
				return Symbol.GetDeclaringSyntaxReferenceHelper<ParameterSyntax>(Locations);
			}
			return ImmutableArray<SyntaxReference>.Empty;
		}
	}

	public override bool IsImplicitlyDeclared
	{
		get
		{
			if (ContainingSymbol is MethodSymbol methodSymbol)
			{
				return methodSymbol.IsAccessor();
			}
			return false;
		}
	}

	internal override bool IsMetadataIn
	{
		get
		{
			RefKind refKind = RefKind;
			if (refKind - 3 <= RefKind.Ref)
			{
				return true;
			}
			return false;
		}
	}

	internal override bool IsMetadataOut => RefKind == RefKind.Out;

	public static SourceParameterSymbol Create(Binder context, Symbol owner, TypeWithAnnotations parameterType, ParameterSyntax syntax, RefKind refKind, SyntaxToken identifier, int ordinal, bool hasParamsModifier, bool isExtensionMethodThis, bool addRefReadOnlyModifier, ScopedKind scope, BindingDiagnosticBag declarationDiagnostics)
	{
		string valueText = identifier.ValueText;
		SourceLocation location = new SourceLocation((valueText == "") ? ((SyntaxNodeOrToken)syntax.Type) : ((SyntaxNodeOrToken)identifier));
		if (hasParamsModifier && parameterType.IsSZArray())
		{
			Binder.ReportUseSiteDiagnosticForSynthesizedAttribute(context.Compilation, WellKnownMember.System_ParamArrayAttribute__ctor, declarationDiagnostics, identifier.Parent.GetLocation());
		}
		ImmutableArray<CustomModifier> refCustomModifiers = ParameterHelpers.ConditionallyCreateInModifiers(refKind, addRefReadOnlyModifier, context, declarationDiagnostics, syntax);
		if (!refCustomModifiers.IsDefaultOrEmpty)
		{
			return new SourceComplexParameterSymbolWithCustomModifiersPrecedingRef(owner, ordinal, parameterType, refKind, refCustomModifiers, valueText, location, syntax.GetReference(), hasParamsModifier, hasParamsModifier, isExtensionMethodThis, scope);
		}
		if (!hasParamsModifier && !isExtensionMethodThis && syntax.Default == null && syntax.AttributeLists.Count == 0 && !owner.IsPartialMember() && scope == ScopedKind.None)
		{
			return new SourceSimpleParameterSymbol(owner, parameterType, ordinal, refKind, valueText, location);
		}
		return new SourceComplexParameterSymbol(owner, ordinal, parameterType, refKind, valueText, location, syntax.GetReference(), hasParamsModifier, hasParamsModifier, isExtensionMethodThis, scope);
	}

	protected SourceParameterSymbol(Symbol owner, int ordinal, RefKind refKind, ScopedKind scope, string name, Location location)
		: base(owner, ordinal)
	{
		_refKind = refKind;
		_scope = scope;
		_name = name;
		_location = location;
	}

	internal override ParameterSymbol WithCustomModifiersAndParams(TypeSymbol newType, ImmutableArray<CustomModifier> newCustomModifiers, ImmutableArray<CustomModifier> newRefCustomModifiers, bool newIsParams)
	{
		return WithCustomModifiersAndParamsCore(newType, newCustomModifiers, newRefCustomModifiers, newIsParams);
	}

	internal SourceParameterSymbol WithCustomModifiersAndParamsCore(TypeSymbol newType, ImmutableArray<CustomModifier> newCustomModifiers, ImmutableArray<CustomModifier> newRefCustomModifiers, bool newIsParams)
	{
		newType = CustomModifierUtils.CopyTypeCustomModifiers(newType, base.Type, ContainingAssembly);
		TypeWithAnnotations parameterType = TypeWithAnnotations.WithTypeAndModifiers(newType, newCustomModifiers);
		if (newRefCustomModifiers.IsEmpty)
		{
			return new SourceComplexParameterSymbol(ContainingSymbol, Ordinal, parameterType, _refKind, _name, _location, SyntaxReference, HasParamsModifier, newIsParams, IsExtensionMethodThis, DeclaredScope);
		}
		return new SourceComplexParameterSymbolWithCustomModifiersPrecedingRef(ContainingSymbol, Ordinal, parameterType, _refKind, newRefCustomModifiers, _name, _location, SyntaxReference, HasParamsModifier, newIsParams, IsExtensionMethodThis, DeclaredScope);
	}

	internal sealed override bool HasComplete(CompletionPart part)
	{
		return state.HasComplete(part);
	}

	internal override void ForceComplete(SourceLocation locationOpt, Predicate<Symbol> filter, CancellationToken cancellationToken)
	{
		state.DefaultForceComplete(this, cancellationToken);
	}

	internal abstract CustomAttributesBag<CSharpAttributeData> GetAttributesBag();

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return GetAttributesBag().Attributes;
	}

	internal override void AddDeclarationDiagnostics(BindingDiagnosticBag diagnostics)
	{
		ContainingSymbol.AddDeclarationDiagnostics(diagnostics);
	}

	protected ScopedKind CalculateEffectiveScopeIgnoringAttributes()
	{
		ScopedKind declaredScope = DeclaredScope;
		if (declaredScope == ScopedKind.None)
		{
			if (ParameterHelpers.IsRefScopedByDefault(this))
			{
				return ScopedKind.ScopedRef;
			}
			if (HasParamsModifier && base.Type.IsRefLikeOrAllowsRefLikeType())
			{
				return ScopedKind.ScopedValue;
			}
		}
		return declaredScope;
	}

	public override Location? TryGetFirstLocation()
	{
		return _location;
	}
}
