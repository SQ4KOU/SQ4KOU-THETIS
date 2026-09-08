using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class LocalFunctionSymbol : LocalFunctionOrSourceMemberMethodSymbol
{
	private readonly Binder _binder;

	private readonly Symbol _containingSymbol;

	private readonly DeclarationModifiers _declarationModifiers;

	private readonly ImmutableArray<SourceMethodTypeParameterSymbol> _typeParameters;

	private readonly RefKind _refKind;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	private bool _lazyIsVarArg;

	private ImmutableArray<ImmutableArray<TypeWithAnnotations>> _lazyTypeParameterConstraintTypes;

	private ImmutableArray<TypeParameterConstraintKind> _lazyTypeParameterConstraintKinds;

	private TypeWithAnnotations.Boxed? _lazyReturnType;

	private ImmutableArray<CustomModifier> _lazyRefCustomModifiers;

	private readonly DiagnosticBag _declarationDiagnostics;

	private readonly HashSet<AssemblySymbol> _declarationDependencies;

	internal Binder ScopeBinder { get; }

	internal override Binder OuterBinder => _binder;

	internal override Binder WithTypeParametersBinder
	{
		get
		{
			if (!_typeParameters.IsEmpty)
			{
				return new WithMethodTypeParametersBinder(this, _binder);
			}
			return _binder;
		}
	}

	internal LocalFunctionStatementSyntax Syntax => (LocalFunctionStatementSyntax)syntaxReferenceOpt.GetSyntax();

	public override bool RequiresInstanceReceiver => false;

	public override bool IsVararg
	{
		get
		{
			ComputeParameters();
			return _lazyIsVarArg;
		}
	}

	public override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			ComputeParameters();
			return _lazyParameters;
		}
	}

	public override TypeWithAnnotations ReturnTypeWithAnnotations
	{
		get
		{
			ComputeReturnType();
			return _lazyReturnType.Value;
		}
	}

	public override RefKind RefKind => _refKind;

	public override bool ReturnsVoid => base.ReturnType.IsVoidType();

	public override int Arity => TypeParameters.Length;

	public override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotations => GetTypeParametersAsTypeArguments();

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => _typeParameters.Cast<SourceMethodTypeParameterSymbol, TypeParameterSymbol>();

	public override bool IsExtensionMethod
	{
		get
		{
			ParameterSyntax parameterSyntax = Syntax.ParameterList.Parameters.FirstOrDefault();
			if (parameterSyntax != null && !parameterSyntax.IsArgList)
			{
				return parameterSyntax.Modifiers.Any(SyntaxKind.ThisKeyword);
			}
			return false;
		}
	}

	public override MethodKind MethodKind => MethodKind.LocalFunction;

	public sealed override Symbol ContainingSymbol => _containingSymbol;

	public override string Name => Syntax.Identifier.ValueText ?? "";

	public SyntaxToken NameToken => Syntax.Identifier;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	public override ImmutableArray<Location> Locations => ImmutableArray.Create(Syntax.Identifier.GetLocation());

	internal override bool GenerateDebugInfo => true;

	public override ImmutableArray<CustomModifier> RefCustomModifiers
	{
		get
		{
			ref ImmutableArray<CustomModifier> lazyRefCustomModifiers;
			ImmutableArray<CustomModifier> value;
			if (_lazyRefCustomModifiers.IsDefault)
			{
				lazyRefCustomModifiers = ref _lazyRefCustomModifiers;
				if (_refKind == RefKind.In)
				{
					CSharpCompilation declaringCompilation = DeclaringCompilation;
					if (declaringCompilation != null)
					{
						value = ImmutableCollectionsMarshal.AsImmutableArray(new CustomModifier[1] { CSharpCustomModifier.CreateRequired(declaringCompilation.GetWellKnownType(WellKnownType.System_Runtime_InteropServices_InAttribute)) });
						goto IL_004b;
					}
				}
				value = ImmutableArray<CustomModifier>.Empty;
				goto IL_004b;
			}
			goto IL_0051;
			IL_0051:
			return _lazyRefCustomModifiers;
			IL_004b:
			ImmutableInterlocked.InterlockedInitialize(ref lazyRefCustomModifiers, value);
			goto IL_0051;
		}
	}

	internal override Microsoft.Cci.CallingConvention CallingConvention => Microsoft.Cci.CallingConvention.Default;

	public override Symbol? AssociatedSymbol => null;

	public override Accessibility DeclaredAccessibility => ModifierUtils.EffectiveAccessibility(_declarationModifiers);

	public override bool IsAsync => (_declarationModifiers & DeclarationModifiers.Async) != 0;

	public override bool IsStatic => (_declarationModifiers & DeclarationModifiers.Static) != 0;

	public override bool IsVirtual => (_declarationModifiers & DeclarationModifiers.Virtual) != 0;

	public override bool IsOverride => (_declarationModifiers & DeclarationModifiers.Override) != 0;

	public override bool IsAbstract => (_declarationModifiers & DeclarationModifiers.Abstract) != 0;

	public override bool IsSealed => (_declarationModifiers & DeclarationModifiers.Sealed) != 0;

	public override bool IsExtern => (_declarationModifiers & DeclarationModifiers.Extern) != 0;

	public bool IsUnsafe => (_declarationModifiers & DeclarationModifiers.Unsafe) != 0;

	internal bool IsExpressionBodied
	{
		get
		{
			LocalFunctionStatementSyntax syntax = Syntax;
			if (syntax != null && syntax.Body == null)
			{
				return syntax.ExpressionBody != null;
			}
			return false;
		}
	}

	internal override bool IsDeclaredReadOnly => false;

	internal override bool IsInitOnly => false;

	public LocalFunctionSymbol(Binder binder, Symbol containingSymbol, LocalFunctionStatementSyntax syntax)
		: base(syntax.GetReference(), SyntaxFacts.HasYieldOperations(syntax.Body))
	{
		_containingSymbol = containingSymbol;
		_declarationDiagnostics = new DiagnosticBag();
		_declarationDependencies = new HashSet<AssemblySymbol>();
		_declarationModifiers = DeclarationModifiers.Private | syntax.Modifiers.ToDeclarationModifiers(isForTypeDeclaration: false, _declarationDiagnostics);
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
		this.CheckUnsafeModifier(_declarationModifiers, instance);
		ScopeBinder = binder;
		binder = binder.SetOrClearUnsafeRegionIfNecessary(syntax.Modifiers);
		_binder = binder;
		if (syntax.TypeParameterList != null)
		{
			_typeParameters = MakeTypeParameters(instance);
		}
		else
		{
			_typeParameters = ImmutableArray<SourceMethodTypeParameterSymbol>.Empty;
			Symbol.ReportErrorIfHasConstraints(syntax.ConstraintClauses, _declarationDiagnostics);
		}
		if (IsExtensionMethod)
		{
			_declarationDiagnostics.Add(ErrorCode.ERR_BadExtensionAgg, GetFirstLocation());
		}
		foreach (ParameterSyntax parameter in syntax.ParameterList.Parameters)
		{
			ReportAttributesDisallowed(parameter.AttributeLists, instance);
		}
		syntax.ReturnType.SkipRefInLocalOrReturn(instance, out _refKind);
		_declarationDiagnostics.AddRange(instance.DiagnosticBag);
		_declarationDependencies.AddAll(instance.DependenciesBag);
		instance.Free();
	}

	internal void GetDeclarationDiagnostics(BindingDiagnosticBag addTo)
	{
		foreach (SourceMethodTypeParameterSymbol typeParameter in _typeParameters)
		{
			typeParameter.ForceComplete(null, null, default(CancellationToken));
		}
		ComputeParameters();
		foreach (ParameterSymbol lazyParameter in _lazyParameters)
		{
			lazyParameter.ForceComplete(null, null, default(CancellationToken));
		}
		ComputeReturnType();
		GetAttributes();
		GetReturnTypeAttributes();
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		ParameterHelpers.EnsureRefKindAttributesExist(declaringCompilation, Parameters, addTo, modifyCompilation: false);
		ParameterHelpers.EnsureParamCollectionAttributeExists(declaringCompilation, Parameters, addTo, modifyCompilation: false);
		ParameterHelpers.EnsureNativeIntegerAttributeExists(declaringCompilation, Parameters, addTo, modifyCompilation: false);
		ParameterHelpers.EnsureScopedRefAttributeExists(declaringCompilation, Parameters, addTo, modifyCompilation: false);
		ParameterHelpers.EnsureNullableAttributeExists(declaringCompilation, this, Parameters, addTo, modifyCompilation: false);
		addTo.AddRange(_declarationDiagnostics);
		addTo.AddDependencies((IReadOnlyCollection<AssemblySymbol>?)_declarationDependencies);
		AsyncMethodChecks(addTo);
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: false, addTo.AccumulatesDependencies);
		if (base.IsEntryPointCandidate && !IsGenericMethod && ContainingSymbol is SynthesizedSimpleProgramEntryPointSymbol && declaringCompilation.HasEntryPointSignature(this, instance).IsCandidate)
		{
			addTo.Add(ErrorCode.WRN_MainIgnored, Syntax.Identifier.GetLocation(), this);
		}
		addTo.AddRangeAndFree(instance);
	}

	internal override void AddDeclarationDiagnostics(BindingDiagnosticBag diagnostics)
	{
		DiagnosticBag diagnosticBag = diagnostics.DiagnosticBag;
		if (diagnosticBag != null)
		{
			_declarationDiagnostics.AddRange(diagnosticBag);
		}
		ICollection<AssemblySymbol> dependenciesBag = diagnostics.DependenciesBag;
		if (dependenciesBag != null)
		{
			_declarationDependencies.AddAll(dependenciesBag);
		}
	}

	private void ComputeParameters()
	{
		if (!RoslynImmutableInterlocked.VolatileRead(in _lazyParameters).IsDefault)
		{
			return;
		}
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
		ImmutableArray<ParameterSymbol> value = ParameterHelpers.MakeParameters(WithTypeParametersBinder, this, Syntax.ParameterList, out var arglistToken, instance, allowRefOrOut: true, allowThis: true, addRefReadOnlyModifier: false).Cast<SourceParameterSymbol, ParameterSymbol>();
		foreach (ParameterSyntax parameter in Syntax.ParameterList.Parameters)
		{
			WithTypeParametersBinder.ReportFieldContextualKeywordConflictIfAny(parameter, instance);
		}
		bool flag = arglistToken.Kind() == SyntaxKind.ArgListKeyword;
		if (flag)
		{
			instance.Add(ErrorCode.ERR_IllegalVarArgs, arglistToken.GetLocation());
		}
		lock (_declarationDiagnostics)
		{
			if (!_lazyParameters.IsDefault)
			{
				instance.Free();
				return;
			}
			_declarationDiagnostics.AddRange(instance.DiagnosticBag);
			_declarationDependencies.AddAll(instance.DependenciesBag);
			instance.Free();
			_lazyIsVarArg = flag;
			RoslynImmutableInterlocked.VolatileWrite(ref _lazyParameters, value);
		}
	}

	internal void ComputeReturnType()
	{
		if (_lazyReturnType != null)
		{
			return;
		}
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
		TypeSyntax returnType = Syntax.ReturnType;
		TypeWithAnnotations value = WithTypeParametersBinder.BindType(returnType.SkipScoped(out var _).SkipRef(), instance);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		if (declaringCompilation != null)
		{
			Location location = null;
			if (_refKind == RefKind.In)
			{
				declaringCompilation.EnsureIsReadOnlyAttributeExists(instance, location ?? (location = returnType.Location), modifyCompilation: false);
				Binder.GetWellKnownType(DeclaringCompilation, WellKnownType.System_Runtime_InteropServices_InAttribute, instance, location ?? (location = returnType.Location));
			}
			if (declaringCompilation.ShouldEmitNativeIntegerAttributes(value.Type))
			{
				declaringCompilation.EnsureNativeIntegerAttributeExists(instance, location ?? (location = returnType.Location), modifyCompilation: false);
			}
			if (declaringCompilation.ShouldEmitNullableAttributes(this) && value.NeedsNullableAttribute())
			{
				declaringCompilation.EnsureNullableAttributeExists(instance, location ?? (location = returnType.Location), modifyCompilation: false);
			}
		}
		if (value.IsRestrictedType(ignoreSpanLikeTypes: true))
		{
			instance.Add(ErrorCode.ERR_MethodReturnCantBeRefAny, returnType.Location, value.Type);
		}
		lock (_declarationDiagnostics)
		{
			if (_lazyReturnType != null)
			{
				instance.Free();
				return;
			}
			_declarationDiagnostics.AddRange(instance.DiagnosticBag);
			_declarationDependencies.AddAll(instance.DependenciesBag);
			instance.Free();
			Interlocked.CompareExchange(ref _lazyReturnType, new TypeWithAnnotations.Boxed(value), null);
		}
	}

	public override Location TryGetFirstLocation()
	{
		return Syntax.Identifier.GetLocation();
	}

	internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return OneOrMany.Create(Syntax.AttributeLists);
	}

	protected override void NoteAttributesComplete(bool forReturnType)
	{
	}

	internal override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	internal override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
	{
		return false;
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/LocalFunctionSymbol.cs", 408);
	}

	internal override bool TryGetThisParameter(out ParameterSymbol? thisParameter)
	{
		thisParameter = null;
		return true;
	}

	private void ReportAttributesDisallowed(SyntaxList<AttributeListSyntax> attributes, BindingDiagnosticBag diagnostics)
	{
		CSDiagnosticInfo featureAvailabilityDiagnosticInfo = MessageID.IDS_FeatureLocalFunctionAttributes.GetFeatureAvailabilityDiagnosticInfo((CSharpParseOptions)syntaxReferenceOpt.SyntaxTree.Options);
		if (featureAvailabilityDiagnosticInfo != null)
		{
			foreach (AttributeListSyntax item in attributes)
			{
				diagnostics.Add(featureAvailabilityDiagnosticInfo, item.Location);
			}
		}
	}

	private ImmutableArray<SourceMethodTypeParameterSymbol> MakeTypeParameters(BindingDiagnosticBag diagnostics)
	{
		ArrayBuilder<SourceMethodTypeParameterSymbol> instance = ArrayBuilder<SourceMethodTypeParameterSymbol>.GetInstance();
		SeparatedSyntaxList<TypeParameterSyntax> separatedSyntaxList = Syntax.TypeParameterList?.Parameters ?? default(SeparatedSyntaxList<TypeParameterSyntax>);
		for (int i = 0; i < separatedSyntaxList.Count; i++)
		{
			TypeParameterSyntax typeParameterSyntax = separatedSyntaxList[i];
			if (typeParameterSyntax.VarianceKeyword.Kind() != SyntaxKind.None)
			{
				diagnostics.Add(ErrorCode.ERR_IllegalVarianceSyntax, typeParameterSyntax.VarianceKeyword.GetLocation());
			}
			ReportAttributesDisallowed(typeParameterSyntax.AttributeLists, diagnostics);
			SyntaxToken identifier = typeParameterSyntax.Identifier;
			Location location = identifier.GetLocation();
			string text = identifier.ValueText ?? "";
			foreach (SourceMethodTypeParameterSymbol item in instance)
			{
				if (text == item.Name)
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateTypeParameter, location, text);
					break;
				}
			}
			SourceMemberContainerTypeSymbol.ReportReservedTypeName(identifier.Text, DeclaringCompilation, diagnostics.DiagnosticBag, location);
			TypeParameterSymbol typeParameterSymbol = ContainingSymbol.FindEnclosingTypeParameter(text);
			if ((object)typeParameterSymbol != null)
			{
				ErrorCode code = ((typeParameterSymbol.ContainingSymbol.Kind != SymbolKind.Method) ? ErrorCode.WRN_TypeParameterSameAsOuterTypeParameter : ErrorCode.WRN_TypeParameterSameAsOuterMethodTypeParameter);
				diagnostics.Add(code, location, text, typeParameterSymbol.ContainingSymbol);
			}
			SourceNotOverridingMethodTypeParameterSymbol sourceNotOverridingMethodTypeParameterSymbol = new SourceNotOverridingMethodTypeParameterSymbol(this, text, i, ImmutableArray.Create(location), ImmutableArray.Create(typeParameterSyntax.GetReference()));
			_binder.ReportFieldContextualKeywordConflictIfAny(sourceNotOverridingMethodTypeParameterSymbol, typeParameterSyntax, identifier, diagnostics);
			instance.Add(sourceNotOverridingMethodTypeParameterSymbol);
		}
		return instance.ToImmutableAndFree();
	}

	public override ImmutableArray<ImmutableArray<TypeWithAnnotations>> GetTypeParameterConstraintTypes()
	{
		if (_lazyTypeParameterConstraintTypes.IsDefault)
		{
			GetTypeParameterConstraintKinds();
			LocalFunctionStatementSyntax syntax = Syntax;
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
			ImmutableArray<ImmutableArray<TypeWithAnnotations>> lazyTypeParameterConstraintTypes = this.MakeTypeParameterConstraintTypes(WithTypeParametersBinder, TypeParameters, syntax.TypeParameterList, syntax.ConstraintClauses, instance);
			lock (_declarationDiagnostics)
			{
				if (_lazyTypeParameterConstraintTypes.IsDefault)
				{
					_declarationDiagnostics.AddRange(instance.DiagnosticBag);
					_declarationDependencies.AddAll(instance.DependenciesBag);
					_lazyTypeParameterConstraintTypes = lazyTypeParameterConstraintTypes;
				}
			}
			instance.Free();
		}
		return _lazyTypeParameterConstraintTypes;
	}

	public override ImmutableArray<TypeParameterConstraintKind> GetTypeParameterConstraintKinds()
	{
		if (_lazyTypeParameterConstraintKinds.IsDefault)
		{
			LocalFunctionStatementSyntax syntax = Syntax;
			ImmutableArray<TypeParameterConstraintKind> value = this.MakeTypeParameterConstraintKinds(WithTypeParametersBinder, TypeParameters, syntax.TypeParameterList, syntax.ConstraintClauses);
			ImmutableInterlocked.InterlockedInitialize(ref _lazyTypeParameterConstraintKinds, value);
		}
		return _lazyTypeParameterConstraintKinds;
	}

	internal override bool IsNullableAnalysisEnabled()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/LocalFunctionSymbol.cs", 540);
	}

	public override int GetHashCode()
	{
		return Syntax.GetHashCode();
	}

	public sealed override bool Equals(Symbol symbol, TypeCompareKind compareKind)
	{
		if ((object)this == symbol)
		{
			return true;
		}
		return (symbol as LocalFunctionSymbol)?.Syntax == Syntax;
	}
}
