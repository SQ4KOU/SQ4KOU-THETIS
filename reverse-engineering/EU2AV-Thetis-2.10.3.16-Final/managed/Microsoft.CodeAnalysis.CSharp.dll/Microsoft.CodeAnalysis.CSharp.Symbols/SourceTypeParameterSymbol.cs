using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceTypeParameterSymbol : TypeParameterSymbol, IAttributeTargetSymbol
{
	private readonly ImmutableArray<SyntaxReference> _syntaxRefs;

	private readonly ImmutableArray<Location> _locations;

	private readonly string _name;

	private readonly short _ordinal;

	private SymbolCompletionState _state;

	private CustomAttributesBag<CSharpAttributeData> _lazyCustomAttributesBag;

	private TypeParameterBounds _lazyBounds = TypeParameterBounds.Unset;

	public override ImmutableArray<Location> Locations => _locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => _syntaxRefs;

	internal ImmutableArray<SyntaxReference> SyntaxReferences => _syntaxRefs;

	public override int Ordinal => _ordinal;

	public override VarianceKind Variance => VarianceKind.None;

	public override string Name => _name;

	internal ImmutableArray<SyntaxList<AttributeListSyntax>> MergedAttributeDeclarationSyntaxLists
	{
		get
		{
			ArrayBuilder<SyntaxList<AttributeListSyntax>> instance = ArrayBuilder<SyntaxList<AttributeListSyntax>>.GetInstance();
			foreach (SyntaxReference syntaxRef in _syntaxRefs)
			{
				TypeParameterSyntax typeParameterSyntax = (TypeParameterSyntax)syntaxRef.GetSyntax();
				instance.Add(typeParameterSyntax.AttributeLists);
			}
			if (ContainingSymbol is SourceOrdinaryMethodSymbol { IsPartial: not false, SourcePartialImplementation: { TypeParameters: var typeParameters } })
			{
				SourceTypeParameterSymbol sourceTypeParameterSymbol = (SourceTypeParameterSymbol)typeParameters[_ordinal];
				instance.AddRange(sourceTypeParameterSymbol.MergedAttributeDeclarationSyntaxLists);
			}
			return instance.ToImmutableAndFree();
		}
	}

	IAttributeTargetSymbol IAttributeTargetSymbol.AttributesOwner => this;

	AttributeLocation IAttributeTargetSymbol.DefaultAttributeLocation => AttributeLocation.TypeParameter;

	AttributeLocation IAttributeTargetSymbol.AllowedAttributeLocations => AttributeLocation.TypeParameter;

	protected abstract ImmutableArray<TypeParameterSymbol> ContainerTypeParameters { get; }

	protected SourceTypeParameterSymbol(string name, int ordinal, ImmutableArray<Location> locations, ImmutableArray<SyntaxReference> syntaxRefs)
	{
		_name = name;
		_ordinal = (short)ordinal;
		_locations = locations;
		_syntaxRefs = syntaxRefs;
	}

	internal override ImmutableArray<TypeWithAnnotations> GetConstraintTypes(ConsList<TypeParameterSymbol> inProgress)
	{
		return GetBounds(inProgress)?.ConstraintTypes ?? ImmutableArray<TypeWithAnnotations>.Empty;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfaces(ConsList<TypeParameterSymbol> inProgress)
	{
		return GetBounds(inProgress)?.Interfaces ?? ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override NamedTypeSymbol GetEffectiveBaseClass(ConsList<TypeParameterSymbol> inProgress)
	{
		TypeParameterBounds bounds = GetBounds(inProgress);
		if (bounds == null)
		{
			return GetDefaultBaseType();
		}
		return bounds.EffectiveBaseClass;
	}

	internal override TypeSymbol GetDeducedBaseType(ConsList<TypeParameterSymbol> inProgress)
	{
		TypeParameterBounds bounds = GetBounds(inProgress);
		if (bounds == null)
		{
			return GetDefaultBaseType();
		}
		return bounds.DeducedBaseType;
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return GetAttributesBag().Attributes;
	}

	internal virtual CustomAttributesBag<CSharpAttributeData> GetAttributesBag()
	{
		if (_lazyCustomAttributesBag == null || !_lazyCustomAttributesBag.IsSealed)
		{
			bool flag = false;
			if (!(ContainingSymbol is SourceOrdinaryMethodSymbol { SourcePartialDefinition: not null } sourceOrdinaryMethodSymbol))
			{
				flag = LoadAndValidateAttributes(OneOrMany.Create(MergedAttributeDeclarationSyntaxLists), ref _lazyCustomAttributesBag, AttributeLocation.None, earlyDecodingOnly: false, (ContainingSymbol as LocalFunctionSymbol)?.WithTypeParametersBinder);
			}
			else
			{
				CustomAttributesBag<CSharpAttributeData> attributesBag = ((SourceTypeParameterSymbol)sourceOrdinaryMethodSymbol.SourcePartialDefinition.TypeParameters[_ordinal]).GetAttributesBag();
				flag = Interlocked.CompareExchange(ref _lazyCustomAttributesBag, attributesBag, null) == null;
			}
			if (flag)
			{
				_state.NotePartComplete(CompletionPart.Attributes);
			}
		}
		return _lazyCustomAttributesBag;
	}

	internal override void EnsureAllConstraintsAreResolved()
	{
		if (!_lazyBounds.IsSet())
		{
			TypeParameterSymbol.EnsureAllConstraintsAreResolved(ContainerTypeParameters);
		}
	}

	private TypeParameterBounds GetBounds(ConsList<TypeParameterSymbol> inProgress)
	{
		if (!_lazyBounds.IsSet())
		{
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
			TypeParameterBounds value = ResolveBounds(inProgress, instance);
			if (Interlocked.CompareExchange(ref _lazyBounds, value, TypeParameterBounds.Unset) == TypeParameterBounds.Unset)
			{
				CheckConstraintTypeConstraints(instance);
				CheckUnmanagedConstraint(instance);
				EnsureAttributesFromConstraints(instance);
				AddDeclarationDiagnostics(instance);
				_state.NotePartComplete(CompletionPart.Members);
			}
			instance.Free();
		}
		return _lazyBounds;
	}

	protected abstract TypeParameterBounds ResolveBounds(ConsList<TypeParameterSymbol> inProgress, BindingDiagnosticBag diagnostics);

	private void CheckConstraintTypeConstraints(BindingDiagnosticBag diagnostics)
	{
		ImmutableArray<TypeWithAnnotations> constraintTypesNoUseSiteDiagnostics = base.ConstraintTypesNoUseSiteDiagnostics;
		if (constraintTypesNoUseSiteDiagnostics.Length == 0)
		{
			return;
		}
		ConstraintsHelper.CheckConstraintsArgsBoxed checkConstraintsArgsBoxed = ConstraintsHelper.CheckConstraintsArgsBoxed.Allocate(DeclaringCompilation, ContainingAssembly.CorLibrary.TypeConversions, _locations[0], diagnostics);
		foreach (TypeWithAnnotations item in constraintTypesNoUseSiteDiagnostics)
		{
			if (!diagnostics.ReportUseSite(item.Type, checkConstraintsArgsBoxed.Args.Location))
			{
				item.Type.CheckAllConstraints(checkConstraintsArgsBoxed);
			}
		}
		checkConstraintsArgsBoxed.Free();
	}

	private void CheckUnmanagedConstraint(BindingDiagnosticBag diagnostics)
	{
		if (HasUnmanagedTypeConstraint)
		{
			DeclaringCompilation.EnsureIsUnmanagedAttributeExists(diagnostics, this.GetNonNullSyntaxNode().Location, ModifyCompilationForAttributeEmbedding());
		}
	}

	private bool ModifyCompilationForAttributeEmbedding()
	{
		Symbol containingSymbol = ContainingSymbol;
		if (!(containingSymbol is SourceOrdinaryMethodSymbol) && !(containingSymbol is SourceMemberContainerTypeSymbol))
		{
			if (containingSymbol is LocalFunctionSymbol)
			{
				return false;
			}
			throw ExceptionUtilities.UnexpectedValue(ContainingSymbol);
		}
		return true;
	}

	private void EnsureAttributesFromConstraints(BindingDiagnosticBag diagnostics)
	{
		if (DeclaringCompilation.ShouldEmitNativeIntegerAttributes() && base.ConstraintTypesNoUseSiteDiagnostics.Any((TypeWithAnnotations t) => t.ContainsNativeIntegerWrapperType()))
		{
			DeclaringCompilation.EnsureNativeIntegerAttributeExists(diagnostics, getLocation(), ModifyCompilationForAttributeEmbedding());
		}
		if (ConstraintsNeedNullableAttribute())
		{
			DeclaringCompilation.EnsureNullableAttributeExists(diagnostics, getLocation(), ModifyCompilationForAttributeEmbedding());
		}
		Location getLocation()
		{
			return this.GetNonNullSyntaxNode().Location;
		}
	}

	internal bool ConstraintsNeedNullableAttribute()
	{
		if (!DeclaringCompilation.ShouldEmitNullableAttributes(this))
		{
			return false;
		}
		if (HasReferenceTypeConstraint && ReferenceTypeConstraintIsNullable.HasValue)
		{
			return true;
		}
		if (base.ConstraintTypesNoUseSiteDiagnostics.Any((TypeWithAnnotations c) => c.NeedsNullableAttribute()))
		{
			return true;
		}
		if (HasNotNullConstraint)
		{
			return true;
		}
		if (!HasReferenceTypeConstraint && !HasValueTypeConstraint && base.ConstraintTypesNoUseSiteDiagnostics.IsEmpty)
		{
			return IsNotNullable == false;
		}
		return false;
	}

	private NamedTypeSymbol GetDefaultBaseType()
	{
		return ContainingAssembly.GetSpecialType(SpecialType.System_Object);
	}

	internal override void ForceComplete(SourceLocation locationOpt, Predicate<Symbol> filter, CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = _state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.Attributes:
				GetAttributes();
				break;
			case CompletionPart.Members:
				_ = base.ConstraintTypesNoUseSiteDiagnostics;
				break;
			case CompletionPart.None:
				return;
			default:
				_state.NotePartComplete(CompletionPart.ImportsAll | CompletionPart.ReturnTypeAttributes | CompletionPart.Parameters | CompletionPart.Type | CompletionPart.StartInterfaces | CompletionPart.FinishInterfaces | CompletionPart.EnumUnderlyingType | CompletionPart.TypeArguments | CompletionPart.TypeParameters | CompletionPart.TypeMembers | CompletionPart.SynthesizedExplicitImplementations | CompletionPart.StartMemberChecks | CompletionPart.FinishMemberChecks | CompletionPart.MembersCompletedChecksStarted | CompletionPart.MembersCompleted);
				break;
			}
			_state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		if (HasUnmanagedTypeConstraint)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeIsUnmanagedAttribute(this));
		}
		if (DeclaringCompilation.ShouldEmitNullableAttributes(this))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeNullableAttributeIfNecessary(GetNullableContextValue(), GetSynthesizedNullableAttributeValue()));
		}
	}

	internal byte GetSynthesizedNullableAttributeValue()
	{
		if (HasReferenceTypeConstraint)
		{
			bool? referenceTypeConstraintIsNullable = ReferenceTypeConstraintIsNullable;
			if (referenceTypeConstraintIsNullable.HasValue)
			{
				if (referenceTypeConstraintIsNullable == true)
				{
					return 2;
				}
				return 1;
			}
		}
		else
		{
			if (HasNotNullConstraint)
			{
				return 1;
			}
			if (!HasValueTypeConstraint && base.ConstraintTypesNoUseSiteDiagnostics.IsEmpty && IsNotNullable == false)
			{
				return 2;
			}
		}
		return 0;
	}

	protected sealed override void DecodeWellKnownAttributeImpl(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		_ = arguments.Attribute;
		ReportExplicitUseOfReservedAttributes(in arguments, ReservedAttributes.NullableAttribute | ReservedAttributes.ExtensionMarkerAttribute);
		base.DecodeWellKnownAttributeImpl(ref arguments);
	}

	protected bool? CalculateReferenceTypeConstraintIsNullable(TypeParameterConstraintKind constraints)
	{
		if ((constraints & TypeParameterConstraintKind.ReferenceType) == 0)
		{
			return false;
		}
		return (constraints & TypeParameterConstraintKind.AllReferenceTypeKinds) switch
		{
			TypeParameterConstraintKind.NullableReferenceType => true, 
			TypeParameterConstraintKind.NotNullableReferenceType => false, 
			_ => null, 
		};
	}
}
