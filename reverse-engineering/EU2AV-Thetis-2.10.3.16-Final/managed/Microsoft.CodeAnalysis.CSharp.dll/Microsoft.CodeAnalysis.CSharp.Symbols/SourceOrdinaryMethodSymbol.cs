using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.RuntimeMembers;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceOrdinaryMethodSymbol : SourceOrdinaryMethodSymbolBase
{
	private sealed class SourceOrdinaryMethodSymbolSimple : SourceOrdinaryMethodSymbol
	{
		internal sealed override SourceOrdinaryMethodSymbol OtherPartOfPartial => null;

		protected sealed override TypeSymbol ExplicitInterfaceType => null;

		public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

		public SourceOrdinaryMethodSymbolSimple(NamedTypeSymbol containingType, string name, Location location, MethodDeclarationSyntax syntax, MethodKind methodKind, bool isNullableAnalysisEnabled, BindingDiagnosticBag diagnostics)
			: base(containingType, name, location, syntax, methodKind, isNullableAnalysisEnabled, diagnostics)
		{
		}

		protected sealed override MethodSymbol FindExplicitlyImplementedMethod(BindingDiagnosticBag diagnostics)
		{
			return null;
		}

		public sealed override ImmutableArray<ImmutableArray<TypeWithAnnotations>> GetTypeParameterConstraintTypes()
		{
			return ImmutableArray<ImmutableArray<TypeWithAnnotations>>.Empty;
		}

		public sealed override ImmutableArray<TypeParameterConstraintKind> GetTypeParameterConstraintKinds()
		{
			return ImmutableArray<TypeParameterConstraintKind>.Empty;
		}

		protected sealed override void CheckConstraintsForExplicitInterfaceType(ConversionsBase conversions, BindingDiagnosticBag diagnostics)
		{
		}
	}

	private sealed class SourceOrdinaryMethodSymbolComplex : SourceOrdinaryMethodSymbol
	{
		private readonly TypeSymbol _explicitInterfaceType;

		private readonly TypeParameterInfo _typeParameterInfo;

		private SourceOrdinaryMethodSymbol _otherPartOfPartial;

		protected sealed override TypeSymbol ExplicitInterfaceType => _explicitInterfaceType;

		internal sealed override SourceOrdinaryMethodSymbol OtherPartOfPartial => _otherPartOfPartial;

		public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => _typeParameterInfo.LazyTypeParameters;

		public SourceOrdinaryMethodSymbolComplex(NamedTypeSymbol containingType, TypeSymbol explicitInterfaceType, string name, Location location, MethodDeclarationSyntax syntax, MethodKind methodKind, bool isNullableAnalysisEnabled, BindingDiagnosticBag diagnostics)
			: base(containingType, name, location, syntax, methodKind, isNullableAnalysisEnabled, diagnostics)
		{
			_explicitInterfaceType = explicitInterfaceType;
			ImmutableArray<TypeParameterSymbol> lazyTypeParameters = MakeTypeParameters(syntax, diagnostics);
			_typeParameterInfo = (lazyTypeParameters.IsEmpty ? TypeParameterInfo.Empty : new TypeParameterInfo
			{
				LazyTypeParameters = lazyTypeParameters
			});
		}

		internal static void InitializePartialMethodParts(SourceOrdinaryMethodSymbolComplex definition, SourceOrdinaryMethodSymbolComplex implementation)
		{
			definition._otherPartOfPartial = implementation;
			implementation._otherPartOfPartial = definition;
		}

		protected sealed override MethodSymbol FindExplicitlyImplementedMethod(BindingDiagnosticBag diagnostics)
		{
			MethodDeclarationSyntax syntax = GetSyntax();
			return this.FindExplicitlyImplementedMethod(isOperator: false, _explicitInterfaceType, syntax.Identifier.ValueText, syntax.ExplicitInterfaceSpecifier, diagnostics);
		}

		public sealed override ImmutableArray<ImmutableArray<TypeWithAnnotations>> GetTypeParameterConstraintTypes()
		{
			if (_typeParameterInfo.LazyTypeParameterConstraintTypes.IsDefault)
			{
				GetTypeParameterConstraintKinds();
				BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
				MethodDeclarationSyntax syntax = GetSyntax();
				Binder binder = DeclaringCompilation.GetBinderFactory(syntax.SyntaxTree).GetBinder(syntax.ReturnType, syntax, this);
				ImmutableArray<ImmutableArray<TypeWithAnnotations>> value = this.MakeTypeParameterConstraintTypes(binder, TypeParameters, syntax.TypeParameterList, syntax.ConstraintClauses, instance);
				if (ImmutableInterlocked.InterlockedInitialize(ref _typeParameterInfo.LazyTypeParameterConstraintTypes, value))
				{
					AddDeclarationDiagnostics(instance);
				}
				instance.Free();
			}
			return _typeParameterInfo.LazyTypeParameterConstraintTypes;
		}

		public sealed override ImmutableArray<TypeParameterConstraintKind> GetTypeParameterConstraintKinds()
		{
			if (_typeParameterInfo.LazyTypeParameterConstraintKinds.IsDefault)
			{
				MethodDeclarationSyntax syntax = GetSyntax();
				Binder binder = DeclaringCompilation.GetBinderFactory(syntax.SyntaxTree).GetBinder(syntax.ReturnType, syntax, this);
				ImmutableArray<TypeParameterConstraintKind> value = this.MakeTypeParameterConstraintKinds(binder, TypeParameters, syntax.TypeParameterList, syntax.ConstraintClauses);
				ImmutableInterlocked.InterlockedInitialize(ref _typeParameterInfo.LazyTypeParameterConstraintKinds, value);
			}
			return _typeParameterInfo.LazyTypeParameterConstraintKinds;
		}

		protected sealed override void CheckConstraintsForExplicitInterfaceType(ConversionsBase conversions, BindingDiagnosticBag diagnostics)
		{
			if ((object)_explicitInterfaceType != null)
			{
				MethodDeclarationSyntax syntax = GetSyntax();
				_explicitInterfaceType.CheckAllConstraints(DeclaringCompilation, conversions, new SourceLocation(syntax.ExplicitInterfaceSpecifier.Name), diagnostics);
			}
		}

		private ImmutableArray<TypeParameterSymbol> MakeTypeParameters(MethodDeclarationSyntax syntax, BindingDiagnosticBag diagnostics)
		{
			if (syntax.Arity == 0)
			{
				return ImmutableArray<TypeParameterSymbol>.Empty;
			}
			MessageID.IDS_FeatureGenerics.CheckFeatureAvailability(diagnostics, syntax.TypeParameterList.LessThanToken);
			OverriddenMethodTypeParameterMapBase overriddenMethodTypeParameterMapBase = null;
			if (IsOverride)
			{
				overriddenMethodTypeParameterMapBase = new OverriddenMethodTypeParameterMap(this);
			}
			else if (IsExplicitInterfaceImplementation)
			{
				overriddenMethodTypeParameterMapBase = new ExplicitInterfaceMethodTypeParameterMap(this);
			}
			SeparatedSyntaxList<TypeParameterSyntax> parameters = syntax.TypeParameterList.Parameters;
			ArrayBuilder<TypeParameterSymbol> instance = ArrayBuilder<TypeParameterSymbol>.GetInstance();
			for (int i = 0; i < parameters.Count; i++)
			{
				TypeParameterSyntax typeParameterSyntax = parameters[i];
				if (typeParameterSyntax.VarianceKeyword.Kind() != SyntaxKind.None)
				{
					diagnostics.Add(ErrorCode.ERR_IllegalVarianceSyntax, typeParameterSyntax.VarianceKeyword.GetLocation());
				}
				SyntaxToken identifier = typeParameterSyntax.Identifier;
				Location location = identifier.GetLocation();
				string valueText = identifier.ValueText;
				TypeParameterSymbol typeParameterSymbol = ContainingType.FindEnclosingTypeParameter(valueText);
				bool flag = true;
				if ((object)typeParameterSymbol != null)
				{
					if (typeParameterSymbol.ContainingSymbol is NamedTypeSymbol { IsExtension: not false })
					{
						diagnostics.Add(ErrorCode.ERR_TypeParameterSameNameAsExtensionTypeParameter, location, valueText);
						flag = false;
					}
					else
					{
						diagnostics.Add(ErrorCode.WRN_TypeParameterSameAsOuterTypeParameter, location, valueText, typeParameterSymbol.ContainingType);
					}
				}
				if (flag)
				{
					for (int j = 0; j < instance.Count; j++)
					{
						if (valueText == instance[j].Name)
						{
							diagnostics.Add(ErrorCode.ERR_DuplicateTypeParameter, location, valueText);
							break;
						}
					}
				}
				SourceMemberContainerTypeSymbol.ReportReservedTypeName(identifier.Text, DeclaringCompilation, diagnostics.DiagnosticBag, location);
				ImmutableArray<SyntaxReference> syntaxRefs = ImmutableArray.Create(typeParameterSyntax.GetReference());
				ImmutableArray<Location> locations = ImmutableArray.Create(location);
				TypeParameterSymbol item = ((overriddenMethodTypeParameterMapBase != null) ? ((SourceMethodTypeParameterSymbol)new SourceOverridingMethodTypeParameterSymbol(overriddenMethodTypeParameterMapBase, valueText, i, locations, syntaxRefs)) : ((SourceMethodTypeParameterSymbol)new SourceNotOverridingMethodTypeParameterSymbol(this, valueText, i, locations, syntaxRefs)));
				instance.Add(item);
			}
			return instance.ToImmutableAndFree();
		}
	}

	private const DeclarationModifiers PartialMethodExtendedModifierMask = DeclarationModifiers.Sealed | DeclarationModifiers.New | DeclarationModifiers.Extern | DeclarationModifiers.Virtual | DeclarationModifiers.Override;

	private bool HasAnyBody => flags.HasAnyBody;

	protected sealed override Location ReturnTypeLocation => GetSyntax().ReturnType.Location;

	internal abstract SourceOrdinaryMethodSymbol OtherPartOfPartial { get; }

	internal bool IsPartialDefinition
	{
		get
		{
			if (base.IsPartial && !HasAnyBody)
			{
				return !base.HasExternModifier;
			}
			return false;
		}
	}

	internal bool IsPartialImplementation
	{
		get
		{
			if (base.IsPartial)
			{
				if (!HasAnyBody)
				{
					return base.HasExternModifier;
				}
				return true;
			}
			return false;
		}
	}

	internal bool IsPartialWithoutImplementation
	{
		get
		{
			if (IsPartialDefinition)
			{
				return (object)OtherPartOfPartial == null;
			}
			return false;
		}
	}

	internal SourceOrdinaryMethodSymbol SourcePartialDefinition
	{
		get
		{
			if (!IsPartialImplementation)
			{
				return null;
			}
			return OtherPartOfPartial;
		}
	}

	internal SourceOrdinaryMethodSymbol SourcePartialImplementation
	{
		get
		{
			if (!IsPartialDefinition)
			{
				return null;
			}
			return OtherPartOfPartial;
		}
	}

	public sealed override MethodSymbol PartialDefinitionPart => SourcePartialDefinition;

	public sealed override MethodSymbol PartialImplementationPart => SourcePartialImplementation;

	public sealed override bool IsExtern
	{
		get
		{
			if (!IsPartialDefinition)
			{
				return base.HasExternModifier;
			}
			return OtherPartOfPartial?.IsExtern ?? false;
		}
	}

	protected sealed override SourceMemberMethodSymbol BoundAttributesSource => SourcePartialDefinition;

	private SyntaxList<AttributeListSyntax> AttributeDeclarationSyntaxList
	{
		get
		{
			if (ContainingType is SourceMemberContainerTypeSymbol { AnyMemberHasAttributes: not false })
			{
				return GetSyntax().AttributeLists;
			}
			return default(SyntaxList<AttributeListSyntax>);
		}
	}

	internal sealed override bool GenerateDebugInfo
	{
		get
		{
			if (IsIterator)
			{
				return false;
			}
			if (IsAsync)
			{
				return DeclaringCompilation.IsRuntimeAsyncEnabledIn(this);
			}
			return true;
		}
	}

	internal bool HasExplicitAccessModifier => flags.HasExplicitAccessModifier;

	private bool HasExtendedPartialModifier => (DeclarationModifiers & (DeclarationModifiers.Sealed | DeclarationModifiers.New | DeclarationModifiers.Extern | DeclarationModifiers.Virtual | DeclarationModifiers.Override)) != 0;

	public static SourceOrdinaryMethodSymbol CreateMethodSymbol(NamedTypeSymbol containingType, Binder bodyBinder, MethodDeclarationSyntax syntax, bool isNullableAnalysisEnabled, BindingDiagnosticBag diagnostics)
	{
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = syntax.ExplicitInterfaceSpecifier;
		SyntaxToken token = syntax.Identifier;
		string memberNameAndInterfaceSymbol = ExplicitInterfaceHelpers.GetMemberNameAndInterfaceSymbol(bodyBinder, syntax.Modifiers, explicitInterfaceSpecifier, token.ValueText, diagnostics, out var explicitInterfaceTypeOpt, out var _);
		SourceLocation location = new SourceLocation(in token);
		MethodKind methodKind = ((explicitInterfaceSpecifier == null) ? MethodKind.Ordinary : MethodKind.ExplicitInterfaceImplementation);
		if ((object)explicitInterfaceTypeOpt != null || syntax.Modifiers.Any(SyntaxKind.PartialKeyword) || syntax.Arity != 0)
		{
			return new SourceOrdinaryMethodSymbolComplex(containingType, explicitInterfaceTypeOpt, memberNameAndInterfaceSymbol, location, syntax, methodKind, isNullableAnalysisEnabled, diagnostics);
		}
		return new SourceOrdinaryMethodSymbolSimple(containingType, memberNameAndInterfaceSymbol, location, syntax, methodKind, isNullableAnalysisEnabled, diagnostics);
	}

	private SourceOrdinaryMethodSymbol(NamedTypeSymbol containingType, string name, Location location, MethodDeclarationSyntax syntax, MethodKind methodKind, bool isNullableAnalysisEnabled, BindingDiagnosticBag diagnostics)
		: base(containingType, name, location, syntax, SyntaxFacts.HasYieldOperations(syntax.Body), MakeModifiersAndFlags(containingType, location, syntax, methodKind, isNullableAnalysisEnabled, diagnostics))
	{
		this.CheckUnsafeModifier(DeclarationModifiers, diagnostics);
		bool flag = syntax.HasAnyBody();
		CheckFeatureAvailabilityAndRuntimeSupport(syntax, location, flag, diagnostics);
		if (flag)
		{
			CheckModifiersForBody(location, diagnostics);
		}
		ModifierUtils.CheckAccessibility(DeclarationModifiers, this, methodKind == MethodKind.ExplicitInterfaceImplementation, diagnostics, location);
		if (syntax.Arity == 0)
		{
			Symbol.ReportErrorIfHasConstraints(syntax.ConstraintClauses, diagnostics.DiagnosticBag);
		}
		Symbol.CheckForBlockAndExpressionBody(syntax.Body, syntax.ExpressionBody, syntax, diagnostics);
	}

	private static (DeclarationModifiers, Flags) MakeModifiersAndFlags(NamedTypeSymbol containingType, Location location, MethodDeclarationSyntax syntax, MethodKind methodKind, bool isNullableAnalysisEnabled, BindingDiagnosticBag diagnostics)
	{
		(DeclarationModifiers mods, bool hasExplicitAccessMod) tuple = MakeModifiers(syntax, containingType, methodKind, syntax.HasAnyBody(), location, diagnostics);
		DeclarationModifiers item = tuple.mods;
		bool item2 = tuple.hasExplicitAccessMod;
		RefKind refKindInLocalOrReturn = syntax.ReturnType.SkipScoped(out var _).GetRefKindInLocalOrReturn(diagnostics);
		bool hasAnyBody = syntax.HasAnyBody();
		bool isExpressionBodied = syntax.IsExpressionBodied();
		ParameterSyntax parameterSyntax = syntax.ParameterList.Parameters.FirstOrDefault();
		Flags item3 = new Flags(methodKind, refKindInLocalOrReturn, item, returnsVoid: false, returnsVoidIsSet: false, hasAnyBody, isExpressionBodied, parameterSyntax != null && !parameterSyntax.IsArgList && parameterSyntax.Modifiers.Any(SyntaxKind.ThisKeyword) && !containingType.IsExtension, isNullableAnalysisEnabled, syntax.IsVarArg(), methodKind == MethodKind.ExplicitInterfaceImplementation, hasThisInitializer: false, item2);
		return (item, item3);
	}

	private (TypeWithAnnotations ReturnType, ImmutableArray<ParameterSymbol> Parameters, ImmutableArray<TypeParameterConstraintClause> DeclaredConstraintsForOverrideOrImplementation) MakeParametersAndBindReturnType(BindingDiagnosticBag diagnostics)
	{
		MethodDeclarationSyntax syntax = GetSyntax();
		Binder binder = DeclaringCompilation.GetBinderFactory(syntax.SyntaxTree).GetBinder(syntax.ReturnType, syntax, this).WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.SuppressConstraintChecks, this);
		ParameterListSyntax parameterList = syntax.ParameterList;
		bool allowThis = !this.IsExtensionBlockMember();
		bool isScoped = IsVirtual || IsAbstract;
		ImmutableArray<ParameterSymbol> item = ParameterHelpers.MakeParameters(binder, this, parameterList, out var _, diagnostics, allowRefOrOut: true, allowThis, isScoped).Cast<SourceParameterSymbol, ParameterSymbol>();
		TypeSyntax returnType = syntax.ReturnType;
		returnType = returnType.SkipScoped(out isScoped).SkipRef();
		TypeWithAnnotations typeWithAnnotations = binder.BindType(returnType, diagnostics);
		if (typeWithAnnotations.IsRestrictedType(ignoreSpanLikeTypes: true) && (typeWithAnnotations.SpecialType != SpecialType.System_TypedReference || (ContainingType.SpecialType != SpecialType.System_TypedReference && ContainingType.SpecialType != SpecialType.System_ArgIterator)))
		{
			diagnostics.Add(ErrorCode.ERR_MethodReturnCantBeRefAny, syntax.ReturnType.Location, typeWithAnnotations.Type);
		}
		ImmutableArray<TypeParameterConstraintClause> immutableArray = default(ImmutableArray<TypeParameterConstraintClause>);
		if (Arity != 0 && (syntax.ExplicitInterfaceSpecifier != null || IsOverride))
		{
			if (syntax.ConstraintClauses.Count > 0)
			{
				Binder.CheckFeatureAvailability(syntax.ConstraintClauses[0].WhereKeyword, MessageID.IDS_OverrideWithConstraints, diagnostics);
				immutableArray = binder.WithAdditionalFlags(BinderFlags.SuppressConstraintChecks | BinderFlags.GenericConstraintsClause).BindTypeParameterConstraintClauses(this, TypeParameters, syntax.TypeParameterList, syntax.ConstraintClauses, diagnostics, performOnlyCycleSafeValidation: false, isForOverride: true);
			}
			foreach (ParameterSymbol item2 in item)
			{
				forceMethodTypeParameters(item2.TypeWithAnnotations, this, immutableArray);
			}
			forceMethodTypeParameters(typeWithAnnotations, this, immutableArray);
		}
		return (ReturnType: typeWithAnnotations, Parameters: item, DeclaredConstraintsForOverrideOrImplementation: immutableArray);
		static void forceMethodTypeParameters(TypeWithAnnotations type, SourceOrdinaryMethodSymbol method, ImmutableArray<TypeParameterConstraintClause> declaredConstraints)
		{
			type.VisitType(null, delegate(TypeWithAnnotations typeWithAnnotations2, (SourceOrdinaryMethodSymbol method, ImmutableArray<TypeParameterConstraintClause> declaredConstraints) args, bool unused2)
			{
				if (typeWithAnnotations2.DefaultType is TypeParameterSymbol typeParameterSymbol && (object)typeParameterSymbol.DeclaringMethod == args.method)
				{
					bool asValueType = args.declaredConstraints.IsDefault || (args.declaredConstraints[typeParameterSymbol.Ordinal].Constraints & (TypeParameterConstraintKind.ReferenceType | TypeParameterConstraintKind.Default)) == 0;
					typeWithAnnotations2.TryForceResolve(asValueType);
				}
				return false;
			}, null, (method, declaredConstraints), canDigThroughNullable: false, useDefaultType: true);
		}
	}

	protected sealed override void ExtensionMethodChecks(BindingDiagnosticBag diagnostics)
	{
		if (IsExtensionMethod)
		{
			MethodDeclarationSyntax syntax = GetSyntax();
			TypeWithAnnotations typeWithAnnotations = Parameters[0].TypeWithAnnotations;
			RefKind refKind = Parameters[0].RefKind;
			if (!typeWithAnnotations.Type.IsValidExtensionParameterType())
			{
				Location location = syntax.ParameterList.Parameters[0].Type.Location;
				diagnostics.Add(ErrorCode.ERR_BadTypeforThis, location, typeWithAnnotations.Type);
				return;
			}
			if (refKind == RefKind.Ref && !typeWithAnnotations.Type.IsValueType)
			{
				diagnostics.Add(ErrorCode.ERR_RefExtensionMustBeValueTypeOrConstrainedToOne, _location, Name);
				return;
			}
			bool flag = refKind - 3 <= RefKind.Ref;
			if (flag && !typeWithAnnotations.Type.IsValidInOrRefReadonlyExtensionParameterType())
			{
				diagnostics.Add(ErrorCode.ERR_InExtensionMustBeValueType, _location, Name);
			}
			else if ((object)ContainingType.ContainingType != null)
			{
				diagnostics.Add(ErrorCode.ERR_ExtensionMethodsDecl, _location, ContainingType.Name);
			}
			else if (!ContainingType.IsScriptClass && (!ContainingType.IsStatic || ContainingType.Arity != 0))
			{
				Location location2 = ((syntax.Parent is TypeDeclarationSyntax typeDeclarationSyntax) ? typeDeclarationSyntax.Identifier : syntax.Identifier).GetLocation();
				diagnostics.Add(ErrorCode.ERR_BadExtensionAgg, location2);
			}
			else if (!IsStatic)
			{
				diagnostics.Add(ErrorCode.ERR_BadExtensionMeth, _location);
			}
			else
			{
				CheckExtensionAttributeAvailability(DeclaringCompilation, syntax.ParameterList.Parameters[0].Modifiers.FirstOrDefault(SyntaxKind.ThisKeyword).GetLocation(), diagnostics);
			}
			return;
		}
		NamedTypeSymbol containingType = ContainingType;
		if ((object)containingType != null && containingType.IsExtension)
		{
			ParameterSymbol extensionParameter = containingType.ExtensionParameter;
			if ((object)extensionParameter != null && extensionParameter.Name == "" && !IsStatic)
			{
				diagnostics.Add(ErrorCode.ERR_InstanceMemberWithUnnamedExtensionsParameter, _location, Name);
			}
		}
	}

	internal static void CheckExtensionAttributeAvailability(CSharpCompilation compilation, Location location, BindingDiagnosticBag diagnostics)
	{
		if ((object)Binder.GetWellKnownTypeMember(compilation, WellKnownMember.System_Runtime_CompilerServices_ExtensionAttribute__ctor, out var useSiteInfo) == null)
		{
			MemberDescriptor descriptor = WellKnownMembers.GetDescriptor(WellKnownMember.System_Runtime_CompilerServices_ExtensionAttribute__ctor);
			diagnostics.Add(ErrorCode.ERR_ExtensionAttrNotFound, location, descriptor.DeclaringTypeMetadataName);
		}
		else
		{
			diagnostics.Add(useSiteInfo, location);
		}
	}

	internal MethodDeclarationSyntax GetSyntax()
	{
		return (MethodDeclarationSyntax)syntaxReferenceOpt.GetSyntax();
	}

	internal sealed override ExecutableCodeBinder TryGetBodyBinder(BinderFactory binderFactoryOpt = null, bool ignoreAccessibility = false)
	{
		return TryGetBodyBinderFromSyntax(binderFactoryOpt, ignoreAccessibility);
	}

	protected sealed override void CompleteAsyncMethodChecksBetweenStartAndFinish()
	{
		if (IsPartialDefinition)
		{
			DeclaringCompilation.SymbolDeclaredEvent(this);
		}
	}

	protected sealed override int GetParameterCountFromSyntax()
	{
		return GetSyntax().ParameterList.ParameterCount;
	}

	internal static void InitializePartialMethodParts(SourceOrdinaryMethodSymbol definition, SourceOrdinaryMethodSymbol implementation)
	{
		SourceOrdinaryMethodSymbolComplex.InitializePartialMethodParts((SourceOrdinaryMethodSymbolComplex)definition, (SourceOrdinaryMethodSymbolComplex)implementation);
	}

	public sealed override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return SourceDocumentationCommentUtils.GetAndCacheDocumentationComment(this, expandIncludes, ref expandIncludes ? ref lazyExpandedDocComment : ref lazyDocComment);
	}

	internal sealed override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		if ((object)SourcePartialImplementation != null)
		{
			return OneOrMany.Create(ImmutableArray.Create(AttributeDeclarationSyntaxList, SourcePartialImplementation.AttributeDeclarationSyntaxList));
		}
		return OneOrMany.Create(AttributeDeclarationSyntaxList);
	}

	private static DeclarationModifiers MakeDeclarationModifiers(MethodDeclarationSyntax syntax, NamedTypeSymbol containingType, Location location, DeclarationModifiers allowedModifiers, BindingDiagnosticBag diagnostics)
	{
		bool modifierErrors;
		bool hasExplicitAccessModifier;
		return ModifierUtils.MakeAndCheckNonTypeMemberModifiers(isOrdinaryMethod: true, containingType.IsInterface, syntax.Modifiers, DeclarationModifiers.None, allowedModifiers, location, diagnostics, out modifierErrors, out hasExplicitAccessModifier);
	}

	internal sealed override void ForceComplete(SourceLocation locationOpt, Predicate<Symbol> filter, CancellationToken cancellationToken)
	{
		SourcePartialImplementation?.ForceComplete(locationOpt, filter, cancellationToken);
		base.ForceComplete(locationOpt, filter, cancellationToken);
	}

	public sealed override bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithinSpan, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!Symbol.IsDefinedInSourceTree(base.SyntaxRef, tree, definedWithinSpan))
		{
			return SourcePartialImplementation?.IsDefinedInSourceTree(tree, definedWithinSpan, cancellationToken) ?? false;
		}
		return true;
	}

	protected abstract override void CheckConstraintsForExplicitInterfaceType(ConversionsBase conversions, BindingDiagnosticBag diagnostics);

	protected sealed override void PartialMethodChecks(BindingDiagnosticBag diagnostics)
	{
		SourceOrdinaryMethodSymbol sourcePartialImplementation = SourcePartialImplementation;
		if ((object)sourcePartialImplementation != null)
		{
			PartialMethodChecks(this, sourcePartialImplementation, diagnostics);
		}
	}

	private static void PartialMethodChecks(SourceOrdinaryMethodSymbol definition, SourceOrdinaryMethodSymbol implementation, BindingDiagnosticBag diagnostics)
	{
		MethodSymbol methodSymbol = definition.ConstructIfGeneric(TypeMap.TypeParametersAsTypeSymbolsWithIgnoredAnnotations(implementation.TypeParameters));
		bool flag = !methodSymbol.ReturnTypeWithAnnotations.Equals(implementation.ReturnTypeWithAnnotations, TypeCompareKind.AllIgnoreOptions);
		if (flag)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMethodReturnTypeDifference, implementation.GetFirstLocation());
		}
		else if (MemberSignatureComparer.ConsideringTupleNamesCreatesDifference(definition, implementation))
		{
			flag = true;
			diagnostics.Add(ErrorCode.ERR_PartialMemberInconsistentTupleNames, implementation.GetFirstLocation(), definition, implementation);
		}
		if (definition.RefKind != implementation.RefKind)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberRefReturnDifference, implementation.GetFirstLocation());
		}
		if (definition.IsStatic != implementation.IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberStaticDifference, implementation.GetFirstLocation());
		}
		if (definition.IsDeclaredReadOnly != implementation.IsDeclaredReadOnly)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberReadOnlyDifference, implementation.GetFirstLocation());
		}
		if (definition.IsExtensionMethod != implementation.IsExtensionMethod)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMethodExtensionDifference, implementation.GetFirstLocation());
		}
		if (definition.IsUnsafe != implementation.IsUnsafe && definition.CompilationAllowsUnsafe())
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberUnsafeDifference, implementation.GetFirstLocation());
		}
		if (definition.IsParams() != implementation.IsParams())
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberParamsDifference, implementation.GetFirstLocation());
		}
		if (definition.HasExplicitAccessModifier != implementation.HasExplicitAccessModifier || definition.DeclaredAccessibility != implementation.DeclaredAccessibility)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberAccessibilityDifference, implementation.GetFirstLocation());
		}
		if (definition.IsVirtual != implementation.IsVirtual || definition.IsOverride != implementation.IsOverride || definition.IsSealed != implementation.IsSealed || definition.IsNew != implementation.IsNew)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberExtendedModDifference, implementation.GetFirstLocation());
		}
		PartialMethodConstraintsChecks(definition, implementation, diagnostics);
		if (SourceMemberContainerTypeSymbol.CheckValidScopedOverride(methodSymbol, implementation, diagnostics, delegate(BindingDiagnosticBag bindingDiagnosticBag, MethodSymbol implementedMethod, MethodSymbol implementingMethod, ParameterSymbol implementingParameter, bool blameAttributes, object arg)
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_ScopedMismatchInParameterOfPartial, implementingMethod.GetFirstLocation(), new FormattedSymbol(implementingParameter, SymbolDisplayFormat.ShortFormat));
		}, null, allowVariance: false, invokedAsExtensionMethod: false))
		{
			flag = true;
		}
		if (SourceMemberContainerTypeSymbol.CheckValidNullableMethodOverride(implementation.DeclaringCompilation, methodSymbol, implementation, diagnostics, delegate(BindingDiagnosticBag bindingDiagnosticBag, MethodSymbol implementedMethod, MethodSymbol implementingMethod, bool topLevel, object arg)
		{
			bindingDiagnosticBag.Add(ErrorCode.WRN_NullabilityMismatchInReturnTypeOnPartial, implementingMethod.GetFirstLocation());
		}, delegate(BindingDiagnosticBag bindingDiagnosticBag, MethodSymbol implementedMethod, MethodSymbol implementingMethod, ParameterSymbol implementingParameter, bool blameAttributes, object arg)
		{
			bindingDiagnosticBag.Add(ErrorCode.WRN_NullabilityMismatchInParameterTypeOnPartial, implementingMethod.GetFirstLocation(), new FormattedSymbol(implementingParameter, SymbolDisplayFormat.ShortFormat));
		}, null))
		{
			flag = true;
		}
		if ((!flag && !MemberSignatureComparer.PartialMethodsStrictComparer.Equals(definition, implementation)) || hasDifferencesInParameterOrTypeParameterName(definition, implementation))
		{
			diagnostics.Add(ErrorCode.WRN_PartialMethodTypeDifference, implementation.GetFirstLocation(), new FormattedSymbol(definition, SymbolDisplayFormat.MinimallyQualifiedFormat), new FormattedSymbol(implementation, SymbolDisplayFormat.MinimallyQualifiedFormat));
		}
		static bool hasDifferencesInParameterOrTypeParameterName(SourceOrdinaryMethodSymbol sourceOrdinaryMethodSymbol, SourceOrdinaryMethodSymbol sourceOrdinaryMethodSymbol2)
		{
			if (sourceOrdinaryMethodSymbol.Parameters.SequenceEqual(sourceOrdinaryMethodSymbol2.Parameters, (ParameterSymbol a, ParameterSymbol b) => a.Name == b.Name))
			{
				return !sourceOrdinaryMethodSymbol.TypeParameters.SequenceEqual(sourceOrdinaryMethodSymbol2.TypeParameters, (TypeParameterSymbol a, TypeParameterSymbol b) => a.Name == b.Name);
			}
			return true;
		}
	}

	private static void PartialMethodConstraintsChecks(SourceOrdinaryMethodSymbol definition, SourceOrdinaryMethodSymbol implementation, BindingDiagnosticBag diagnostics)
	{
		ImmutableArray<TypeParameterSymbol> typeParameters = definition.TypeParameters;
		int length = typeParameters.Length;
		if (length == 0)
		{
			return;
		}
		ImmutableArray<TypeParameterSymbol> typeParameters2 = implementation.TypeParameters;
		ImmutableArray<TypeWithAnnotations> to = IndexedTypeParameterSymbol.Take(length);
		TypeMap typeMap = new TypeMap(typeParameters, to, allowAlpha: true);
		TypeMap typeMap2 = new TypeMap(typeParameters2, to, allowAlpha: true);
		for (int i = 0; i < length; i++)
		{
			TypeParameterSymbol typeParameter = typeParameters[i];
			TypeParameterSymbol typeParameterSymbol = typeParameters2[i];
			if (!MemberSignatureComparer.HaveSameConstraints(typeParameter, typeMap, typeParameterSymbol, typeMap2, TypeCompareKind.IgnoreDynamicAndTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes))
			{
				diagnostics.Add(ErrorCode.ERR_PartialMethodInconsistentConstraints, implementation.GetFirstLocation(), implementation, typeParameterSymbol.Name);
			}
			else if (!MemberSignatureComparer.HaveSameNullabilityInConstraints(typeParameter, typeMap, typeParameterSymbol, typeMap2))
			{
				diagnostics.Add(ErrorCode.WRN_NullabilityMismatchInConstraintsOnPartialImplementation, implementation.GetFirstLocation(), implementation, typeParameterSymbol.Name);
			}
		}
	}

	internal sealed override bool CallsAreOmitted(SyntaxTree syntaxTree)
	{
		if (IsPartialWithoutImplementation)
		{
			return true;
		}
		return base.CallsAreOmitted(syntaxTree);
	}

	protected override void MethodChecks(BindingDiagnosticBag diagnostics)
	{
		(TypeWithAnnotations ReturnType, ImmutableArray<ParameterSymbol> Parameters, ImmutableArray<TypeParameterConstraintClause> DeclaredConstraintsForOverrideOrImplementation) tuple = MakeParametersAndBindReturnType(diagnostics);
		TypeWithAnnotations item = tuple.ReturnType;
		ImmutableArray<ParameterSymbol> item2 = tuple.Parameters;
		ImmutableArray<TypeParameterConstraintClause> item3 = tuple.DeclaredConstraintsForOverrideOrImplementation;
		MethodSymbol methodSymbol = MethodChecks(item, item2, diagnostics);
		if (!item3.IsDefault && (object)methodSymbol != null)
		{
			for (int i = 0; i < item3.Length; i++)
			{
				TypeParameterSymbol typeParameterSymbol = TypeParameters[i];
				TypeParameterConstraintKind typeParameterConstraintKind = item3[i].Constraints & (TypeParameterConstraintKind.ReferenceType | TypeParameterConstraintKind.ValueType | TypeParameterConstraintKind.Default);
				ErrorCode code;
				if (typeParameterConstraintKind != TypeParameterConstraintKind.ReferenceType)
				{
					if (typeParameterConstraintKind != TypeParameterConstraintKind.ValueType)
					{
						if (typeParameterConstraintKind != TypeParameterConstraintKind.Default || (!typeParameterSymbol.IsReferenceType && !typeParameterSymbol.IsValueType))
						{
							continue;
						}
						code = ErrorCode.ERR_OverrideDefaultConstraintNotSatisfied;
					}
					else
					{
						if (typeParameterSymbol.IsNonNullableValueType())
						{
							continue;
						}
						code = ErrorCode.ERR_OverrideValConstraintNotSatisfied;
					}
				}
				else
				{
					if (typeParameterSymbol.IsReferenceType)
					{
						continue;
					}
					code = ErrorCode.ERR_OverrideRefConstraintNotSatisfied;
				}
				diagnostics.Add(code, typeParameterSymbol.GetFirstLocation(), this, typeParameterSymbol, methodSymbol.TypeParameters[i], methodSymbol);
			}
		}
		CheckModifiers(MethodKind == MethodKind.ExplicitInterfaceImplementation, _location, diagnostics);
	}

	internal override void AfterAddingTypeMembersChecks(ConversionsBase conversions, BindingDiagnosticBag diagnostics)
	{
		base.AfterAddingTypeMembersChecks(conversions, diagnostics);
		TypeSymbol returnType = base.ReturnType;
		if ((object)returnType != null && returnType.IsErrorType() && GetSyntax().ReturnType is IdentifierNameSyntax { Identifier: { RawContextualKind: 8406 } })
		{
			MessageID.IDS_FeaturePartialEventsAndConstructors.CheckFeatureAvailability(diagnostics, DeclaringCompilation, ReturnTypeLocation);
		}
	}

	private static (DeclarationModifiers mods, bool hasExplicitAccessMod) MakeModifiers(MethodDeclarationSyntax syntax, NamedTypeSymbol containingType, MethodKind methodKind, bool hasBody, Location location, BindingDiagnosticBag diagnostics)
	{
		bool isInterface = containingType.IsInterface;
		bool isExtension = containingType.IsExtension;
		bool flag = methodKind == MethodKind.ExplicitInterfaceImplementation;
		DeclarationModifiers declarationModifiers = ((!isInterface || flag) ? DeclarationModifiers.Private : DeclarationModifiers.None);
		DeclarationModifiers declarationModifiers2 = DeclarationModifiers.Partial | DeclarationModifiers.Unsafe;
		DeclarationModifiers declarationModifiers3 = DeclarationModifiers.None;
		if (!flag)
		{
			declarationModifiers2 |= DeclarationModifiers.AccessibilityMask | DeclarationModifiers.Static;
			if (!isExtension)
			{
				declarationModifiers2 |= DeclarationModifiers.Abstract | DeclarationModifiers.Sealed | DeclarationModifiers.New | DeclarationModifiers.Virtual;
				if (!isInterface)
				{
					declarationModifiers2 |= DeclarationModifiers.Override;
				}
				else
				{
					declarationModifiers3 |= DeclarationModifiers.AccessibilityMask | DeclarationModifiers.Abstract | DeclarationModifiers.Sealed | DeclarationModifiers.Static | DeclarationModifiers.Extern | DeclarationModifiers.Partial | DeclarationModifiers.Virtual | DeclarationModifiers.Async;
				}
			}
		}
		else
		{
			if (isInterface)
			{
				declarationModifiers2 |= DeclarationModifiers.Abstract;
			}
			declarationModifiers2 |= DeclarationModifiers.Static;
		}
		declarationModifiers2 |= DeclarationModifiers.Extern | DeclarationModifiers.Async;
		if (containingType.IsStructType())
		{
			declarationModifiers2 |= DeclarationModifiers.ReadOnly;
		}
		DeclarationModifiers declarationModifiers4 = MakeDeclarationModifiers(syntax, containingType, location, declarationModifiers2, diagnostics);
		bool item;
		if ((declarationModifiers4 & DeclarationModifiers.AccessibilityMask) == 0)
		{
			item = false;
			declarationModifiers4 |= declarationModifiers;
		}
		else
		{
			item = true;
		}
		ModifierUtils.CheckFeatureAvailabilityForStaticAbstractMembersInInterfacesIfNeeded(declarationModifiers4, flag, location, diagnostics);
		ModifierUtils.ReportDefaultInterfaceImplementationModifiers(hasBody, declarationModifiers4, declarationModifiers3, location, diagnostics);
		declarationModifiers4 = AddImpliedModifiers(declarationModifiers4, isInterface, methodKind, hasBody);
		return (mods: declarationModifiers4, hasExplicitAccessMod: item);
	}

	private static DeclarationModifiers AddImpliedModifiers(DeclarationModifiers mods, bool containingTypeIsInterface, MethodKind methodKind, bool hasBody)
	{
		if (containingTypeIsInterface)
		{
			mods = ModifierUtils.AdjustModifiersForAnInterfaceMember(mods, hasBody, methodKind == MethodKind.ExplicitInterfaceImplementation, forMethod: true);
		}
		else if (methodKind == MethodKind.ExplicitInterfaceImplementation)
		{
			mods = (DeclarationModifiers)(((uint)mods & 0xFFFFFC0Fu) | 0x100);
		}
		return mods;
	}

	private void CheckModifiers(bool isExplicitInterfaceImplementation, Location location, BindingDiagnosticBag diagnostics)
	{
		bool isVararg = IsVararg;
		bool flag = isExplicitInterfaceImplementation && ContainingType.IsInterface;
		if (base.IsPartial && HasExplicitAccessModifier)
		{
			Binder.CheckFeatureAvailability(base.SyntaxNode, MessageID.IDS_FeatureExtendedPartialMethods, diagnostics, location);
		}
		if (base.IsPartial && IsAbstract)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberCannotBeAbstract, location);
		}
		else if (base.IsPartial && !HasExplicitAccessModifier && !ReturnsVoid)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMethodWithNonVoidReturnMustHaveAccessMods, location, this);
		}
		else if (base.IsPartial && !HasExplicitAccessModifier && HasExtendedPartialModifier)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMethodWithExtendedModMustHaveAccessMods, location, this);
		}
		else if (base.IsPartial && !HasExplicitAccessModifier && Parameters.Any((ParameterSymbol p) => p.RefKind == RefKind.Out))
		{
			diagnostics.Add(ErrorCode.ERR_PartialMethodWithOutParamMustHaveAccessMods, location, this);
		}
		else if (DeclaredAccessibility == Accessibility.Private && (IsVirtual || (IsAbstract && !flag) || IsOverride))
		{
			diagnostics.Add(ErrorCode.ERR_VirtualPrivate, location, this);
		}
		else if (IsOverride && (base.IsNew || IsVirtual))
		{
			diagnostics.Add(ErrorCode.ERR_OverrideNotNew, location, this);
		}
		else if (IsSealed && !IsOverride && (!flag || !IsAbstract))
		{
			diagnostics.Add(ErrorCode.ERR_SealedNonOverride, location, this);
		}
		else if (IsSealed && ContainingType.TypeKind == TypeKind.Struct)
		{
			diagnostics.Add(ErrorCode.ERR_BadMemberFlag, location, SyntaxFacts.GetText(SyntaxKind.SealedKeyword));
		}
		else if (base.ReturnType.IsStatic)
		{
			diagnostics.Add(ErrorFacts.GetStaticClassReturnCode(ContainingType.IsInterfaceType()), location, base.ReturnType);
		}
		else if (IsAbstract && IsExtern)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractAndExtern, location, this);
		}
		else if (IsAbstract && IsSealed && !flag)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractAndSealed, location, this);
		}
		else if (IsAbstract && IsVirtual)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractNotVirtual, location, Kind.Localize(), this);
		}
		else if (IsAbstract && ContainingType.TypeKind == TypeKind.Struct)
		{
			diagnostics.Add(ErrorCode.ERR_BadMemberFlag, location, SyntaxFacts.GetText(SyntaxKind.AbstractKeyword));
		}
		else if (IsVirtual && ContainingType.TypeKind == TypeKind.Struct)
		{
			diagnostics.Add(ErrorCode.ERR_BadMemberFlag, location, SyntaxFacts.GetText(SyntaxKind.VirtualKeyword));
		}
		else if (IsStatic && IsDeclaredReadOnly)
		{
			diagnostics.Add(ErrorCode.ERR_StaticMemberCantBeReadOnly, location, this);
		}
		else if (IsAbstract && !ContainingType.IsAbstract && (ContainingType.TypeKind == TypeKind.Class || ContainingType.TypeKind == TypeKind.Submission))
		{
			diagnostics.Add(ErrorCode.ERR_AbstractInConcreteClass, location, this, ContainingType);
		}
		else if (IsVirtual && ContainingType.IsSealed)
		{
			diagnostics.Add(ErrorCode.ERR_NewVirtualInSealed, location, this, ContainingType);
		}
		else if (!HasAnyBody && IsAsync)
		{
			diagnostics.Add(ErrorCode.ERR_BadAsyncLacksBody, location);
		}
		else if (!HasAnyBody && !IsExtern && !IsAbstract && !base.IsPartial && !base.IsExpressionBodied)
		{
			diagnostics.Add(ErrorCode.ERR_ConcreteMissingBody, location, this);
		}
		else if (ContainingType.IsSealed && DeclaredAccessibility.HasProtected() && !IsOverride)
		{
			diagnostics.Add(AccessCheck.GetProtectedMemberInSealedTypeError(ContainingType), location, this);
		}
		else if (ContainingType.IsStatic && !IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_InstanceMemberInStaticClass, location, Name);
		}
		else if (isVararg && (IsGenericMethod || ContainingType.IsGenericType || (Parameters.Length > 0 && Parameters[Parameters.Length - 1].IsParams)))
		{
			diagnostics.Add(ErrorCode.ERR_BadVarargs, location);
		}
		else if (isVararg && IsAsync)
		{
			diagnostics.Add(ErrorCode.ERR_VarargsAsync, location);
		}
	}
}
