using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceOrdinaryMethodOrUserDefinedOperatorSymbol : SourceMemberMethodSymbol
{
	private ImmutableArray<MethodSymbol> _lazyExplicitInterfaceImplementations;

	private ImmutableArray<CustomModifier> _lazyRefCustomModifiers;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	private TypeWithAnnotations _lazyReturnType;

	protected abstract Location ReturnTypeLocation { get; }

	public sealed override bool ReturnsVoid
	{
		get
		{
			LazyMethodChecks();
			return base.ReturnsVoid;
		}
	}

	protected abstract TypeSymbol? ExplicitInterfaceType { get; }

	internal sealed override int ParameterCount
	{
		get
		{
			if (!_lazyParameters.IsDefault)
			{
				return _lazyParameters.Length;
			}
			return GetParameterCountFromSyntax();
		}
	}

	public sealed override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			LazyMethodChecks();
			return _lazyParameters;
		}
	}

	public sealed override TypeWithAnnotations ReturnTypeWithAnnotations
	{
		get
		{
			LazyMethodChecks();
			return _lazyReturnType;
		}
	}

	internal sealed override bool IsExplicitInterfaceImplementation => MethodKind == MethodKind.ExplicitInterfaceImplementation;

	public sealed override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations
	{
		get
		{
			LazyMethodChecks();
			return _lazyExplicitInterfaceImplementations;
		}
	}

	public sealed override ImmutableArray<CustomModifier> RefCustomModifiers
	{
		get
		{
			LazyMethodChecks();
			return _lazyRefCustomModifiers;
		}
	}

	protected SourceOrdinaryMethodOrUserDefinedOperatorSymbol(NamedTypeSymbol containingType, SyntaxReference syntaxReferenceOpt, Location location, bool isIterator, (DeclarationModifiers declarationModifiers, Flags flags) modifiersAndFlags)
		: base(containingType, syntaxReferenceOpt, location, isIterator, modifiersAndFlags)
	{
	}

	protected MethodSymbol? MethodChecks(TypeWithAnnotations returnType, ImmutableArray<ParameterSymbol> parameters, BindingDiagnosticBag diagnostics)
	{
		_lazyReturnType = returnType;
		_lazyParameters = parameters;
		SetReturnsVoid(_lazyReturnType.IsVoidType());
		CheckEffectiveAccessibility(_lazyReturnType, _lazyParameters, diagnostics);
		CheckFileTypeUsage(_lazyReturnType, _lazyParameters, diagnostics);
		if (Name == "Finalize" && ParameterCount == 0 && Arity == 0 && ReturnsVoid)
		{
			diagnostics.Add(ErrorCode.WRN_FinalizeMethod, _location);
		}
		ExtensionMethodChecks(diagnostics);
		if (base.IsPartial)
		{
			if (MethodKind == MethodKind.ExplicitInterfaceImplementation)
			{
				diagnostics.Add(ErrorCode.ERR_PartialMemberNotExplicit, _location);
			}
			if (!ContainingType.IsPartial())
			{
				diagnostics.Add(ErrorCode.ERR_PartialMemberOnlyInPartialClass, _location);
			}
		}
		_lazyRefCustomModifiers = ImmutableArray<CustomModifier>.Empty;
		MethodSymbol methodSymbol = null;
		if (MethodKind != MethodKind.ExplicitInterfaceImplementation)
		{
			_lazyExplicitInterfaceImplementations = ImmutableArray<MethodSymbol>.Empty;
			if (IsOverride)
			{
				methodSymbol = base.OverriddenMethod;
				if ((object)methodSymbol != null)
				{
					CustomModifierUtils.CopyMethodCustomModifiers(methodSymbol, this, out _lazyReturnType, out _lazyRefCustomModifiers, out _lazyParameters, alsoCopyParamsModifier: true);
				}
			}
			else if (RefKind == RefKind.In)
			{
				NamedTypeSymbol wellKnownType = Binder.GetWellKnownType(DeclaringCompilation, WellKnownType.System_Runtime_InteropServices_InAttribute, diagnostics, ReturnTypeLocation);
				_lazyRefCustomModifiers = ImmutableArray.Create(CSharpCustomModifier.CreateRequired(wellKnownType));
			}
		}
		else if ((object)ExplicitInterfaceType != null)
		{
			methodSymbol = FindExplicitlyImplementedMethod(diagnostics);
			if ((object)methodSymbol != null)
			{
				_lazyExplicitInterfaceImplementations = ImmutableArray.Create(methodSymbol);
				CustomModifierUtils.CopyMethodCustomModifiers(methodSymbol, this, out _lazyReturnType, out _lazyRefCustomModifiers, out _lazyParameters, alsoCopyParamsModifier: false);
				this.FindExplicitlyImplementedMemberVerification(methodSymbol, diagnostics);
				TypeSymbol.CheckModifierMismatchOnImplementingMember(ContainingType, this, methodSymbol, isExplicit: true, diagnostics);
			}
			else
			{
				_lazyExplicitInterfaceImplementations = ImmutableArray<MethodSymbol>.Empty;
			}
		}
		return methodSymbol;
	}

	protected abstract void ExtensionMethodChecks(BindingDiagnosticBag diagnostics);

	protected abstract MethodSymbol? FindExplicitlyImplementedMethod(BindingDiagnosticBag diagnostics);

	protected abstract int GetParameterCountFromSyntax();

	internal override void AfterAddingTypeMembersChecks(ConversionsBase conversions, BindingDiagnosticBag diagnostics)
	{
		base.AfterAddingTypeMembersChecks(conversions, diagnostics);
		Location returnTypeLocation = null;
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		CheckConstraintsForExplicitInterfaceType(conversions, diagnostics);
		base.ReturnType.CheckAllConstraints(declaringCompilation, conversions, GetFirstLocation(), diagnostics);
		foreach (ParameterSymbol parameter in Parameters)
		{
			parameter.Type.CheckAllConstraints(declaringCompilation, conversions, parameter.GetFirstLocation(), diagnostics);
		}
		PartialMethodChecks(diagnostics);
		if (RefKind == RefKind.In)
		{
			declaringCompilation.EnsureIsReadOnlyAttributeExists(diagnostics, getReturnTypeLocation(), modifyCompilation: true);
		}
		ParameterHelpers.EnsureRefKindAttributesExist(declaringCompilation, Parameters, diagnostics, modifyCompilation: true);
		ParameterHelpers.EnsureParamCollectionAttributeExists(declaringCompilation, Parameters, diagnostics, modifyCompilation: true);
		if (declaringCompilation.ShouldEmitNativeIntegerAttributes(base.ReturnType))
		{
			declaringCompilation.EnsureNativeIntegerAttributeExists(diagnostics, getReturnTypeLocation(), modifyCompilation: true);
		}
		ParameterHelpers.EnsureNativeIntegerAttributeExists(declaringCompilation, Parameters, diagnostics, modifyCompilation: true);
		ParameterHelpers.EnsureScopedRefAttributeExists(declaringCompilation, Parameters, diagnostics, modifyCompilation: true);
		if (declaringCompilation.ShouldEmitNullableAttributes(this) && ReturnTypeWithAnnotations.NeedsNullableAttribute())
		{
			declaringCompilation.EnsureNullableAttributeExists(diagnostics, getReturnTypeLocation(), modifyCompilation: true);
		}
		ParameterHelpers.EnsureNullableAttributeExists(declaringCompilation, this, Parameters, diagnostics, modifyCompilation: true);
		if (this.IsExtensionBlockMember())
		{
			if (MethodKind != MethodKind.Ordinary)
			{
				ParameterHelpers.CheckUnderspecifiedGenericExtension(this, Parameters, diagnostics);
			}
			declaringCompilation.EnsureExtensionMarkerAttributeExists(diagnostics, GetFirstLocation(), modifyCompilation: true);
		}
		Location getReturnTypeLocation()
		{
			if ((object)returnTypeLocation == null)
			{
				returnTypeLocation = ReturnTypeLocation;
			}
			return returnTypeLocation;
		}
	}

	protected abstract void CheckConstraintsForExplicitInterfaceType(ConversionsBase conversions, BindingDiagnosticBag diagnostics);

	protected abstract void PartialMethodChecks(BindingDiagnosticBag diagnostics);
}
