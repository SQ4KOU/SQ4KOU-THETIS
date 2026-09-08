using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class FunctionPointerMethodSymbol : MethodSymbol
{
	private readonly ImmutableArray<FunctionPointerParameterSymbol> _parameters;

	private ImmutableHashSet<CustomModifier>? _lazyCallingConventionModifiers;

	internal override ImmutableArray<NamedTypeSymbol> UnmanagedCallingConventionTypes
	{
		get
		{
			if (!CallingConvention.IsCallingConvention(CallingConvention.Unmanaged))
			{
				return ImmutableArray<NamedTypeSymbol>.Empty;
			}
			ImmutableArray<CustomModifier> immutableArray = ((RefKind != RefKind.None) ? RefCustomModifiers : ReturnTypeWithAnnotations.CustomModifiers);
			if (immutableArray.IsEmpty)
			{
				return ImmutableArray<NamedTypeSymbol>.Empty;
			}
			ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance(immutableArray.Length);
			foreach (CSharpCustomModifier item in immutableArray)
			{
				if (FunctionPointerTypeSymbol.IsCallingConventionModifier(item.ModifierSymbol))
				{
					instance.Add(item.ModifierSymbol);
				}
			}
			return instance.ToImmutableAndFree();
		}
	}

	internal override CallingConvention CallingConvention { get; }

	internal override bool UseUpdatedEscapeRules { get; }

	public override bool ReturnsVoid => ReturnTypeWithAnnotations.IsVoidType();

	public override RefKind RefKind { get; }

	public override TypeWithAnnotations ReturnTypeWithAnnotations { get; }

	public override ImmutableArray<ParameterSymbol> Parameters => _parameters.Cast<FunctionPointerParameterSymbol, ParameterSymbol>();

	public override ImmutableArray<CustomModifier> RefCustomModifiers { get; }

	public override MethodKind MethodKind => MethodKind.FunctionPointerSignature;

	public override bool IsVararg => CallingConvention.IsCallingConvention(CallingConvention.ExtraArguments);

	public override Symbol? ContainingSymbol => null;

	public override int Arity => 0;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override bool IsExtensionMethod => false;

	public override bool HidesBaseMethodsByName => false;

	public override bool IsAsync => false;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	public override Symbol? AssociatedSymbol => null;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override bool IsStatic => false;

	public override bool IsVirtual => false;

	public override bool IsOverride => false;

	public override bool IsAbstract => false;

	public override bool IsSealed => false;

	public override bool IsExtern => false;

	public override bool IsImplicitlyDeclared => true;

	public override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotations => ImmutableArray<TypeWithAnnotations>.Empty;

	internal override bool HasSpecialName => false;

	internal override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.IL;

	internal override bool HasDeclarativeSecurity => false;

	internal override MarshalPseudoCustomAttributeData? ReturnValueMarshallingInformation => null;

	internal override bool RequiresSecurityObject => false;

	internal override bool IsDeclaredReadOnly => false;

	internal override bool IsInitOnly => false;

	public override FlowAnalysisAnnotations ReturnTypeFlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

	public override ImmutableHashSet<string> ReturnNotNullIfParameterNotNull => ImmutableHashSet<string>.Empty;

	public override FlowAnalysisAnnotations FlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

	internal sealed override bool HasSpecialNameAttribute
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionPointers/FunctionPointerMethodSymbol.cs", 851);
		}
	}

	internal override bool GenerateDebugInfo
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionPointers/FunctionPointerMethodSymbol.cs", 853);
		}
	}

	internal override ObsoleteAttributeData? ObsoleteAttributeData
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionPointers/FunctionPointerMethodSymbol.cs", 854);
		}
	}

	public override bool AreLocalsZeroed
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionPointers/FunctionPointerMethodSymbol.cs", 856);
		}
	}

	protected sealed override bool HasSetsRequiredMembersImpl
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionPointers/FunctionPointerMethodSymbol.cs", 861);
		}
	}

	internal sealed override bool HasUnscopedRefAttribute => false;

	public static FunctionPointerMethodSymbol CreateFromSource(FunctionPointerTypeSyntax syntax, Binder typeBinder, BindingDiagnosticBag diagnostics, ConsList<TypeSymbol> basesBeingResolved, bool suppressUseSiteDiagnostics)
	{
		ArrayBuilder<CustomModifier> instance = ArrayBuilder<CustomModifier>.GetInstance();
		CallingConvention callingConvention = getCallingConvention(typeBinder.Compilation, syntax.CallingConvention, instance, diagnostics);
		RefKind refKind = RefKind.None;
		TypeWithAnnotations typeWithAnnotations;
		if (syntax.ParameterList.Parameters.Count == 0)
		{
			typeWithAnnotations = TypeWithAnnotations.Create(typeBinder.CreateErrorType());
		}
		else
		{
			SeparatedSyntaxList<FunctionPointerParameterSyntax> parameters = syntax.ParameterList.Parameters;
			FunctionPointerParameterSyntax functionPointerParameterSyntax = parameters[parameters.Count - 1];
			SyntaxTokenList modifiers = functionPointerParameterSyntax.Modifiers;
			for (int i = 0; i < modifiers.Count; i++)
			{
				SyntaxToken token = modifiers[i];
				if (token.Kind() == SyntaxKind.RefKeyword)
				{
					if (refKind == RefKind.None)
					{
						if (modifiers.Count > i + 1 && modifiers[i + 1].Kind() == SyntaxKind.ReadOnlyKeyword)
						{
							i++;
							refKind = RefKind.In;
							instance.AddRange(ParameterHelpers.CreateInModifiers(typeBinder, diagnostics, functionPointerParameterSyntax));
						}
						else
						{
							refKind = RefKind.Ref;
						}
					}
					else
					{
						diagnostics.Add(ErrorCode.ERR_DupReturnTypeMod, token.GetLocation(), token.Text);
					}
				}
				else
				{
					diagnostics.Add(ErrorCode.ERR_InvalidFuncPointerReturnTypeModifier, token.GetLocation(), token.Text);
				}
			}
			typeWithAnnotations = typeBinder.BindType(functionPointerParameterSyntax.Type, diagnostics, basesBeingResolved, suppressUseSiteDiagnostics);
			if (typeWithAnnotations.IsVoidType() && refKind != RefKind.None)
			{
				diagnostics.Add(ErrorCode.ERR_NoVoidHere, functionPointerParameterSyntax.Location);
			}
			else if (typeWithAnnotations.IsStatic)
			{
				diagnostics.Add(ErrorFacts.GetStaticClassReturnCode(useWarning: false), functionPointerParameterSyntax.Location, typeWithAnnotations);
			}
			else if (typeWithAnnotations.IsRestrictedType(ignoreSpanLikeTypes: true))
			{
				diagnostics.Add(ErrorCode.ERR_MethodReturnCantBeRefAny, functionPointerParameterSyntax.Location, typeWithAnnotations);
			}
		}
		ImmutableArray<CustomModifier> refCustomModifiers = ImmutableArray<CustomModifier>.Empty;
		if (refKind != RefKind.None)
		{
			refCustomModifiers = instance.ToImmutableAndFree();
		}
		else
		{
			typeWithAnnotations = typeWithAnnotations.WithModifiers(instance.ToImmutableAndFree());
		}
		return new FunctionPointerMethodSymbol(callingConvention, refKind, typeWithAnnotations, refCustomModifiers, syntax, typeBinder, diagnostics, suppressUseSiteDiagnostics, typeBinder.UseUpdatedEscapeRules);
		static void checkUnmanagedSupport(CSharpCompilation compilation, Location errorLocation, BindingDiagnosticBag bindingDiagnosticBag)
		{
			if (!compilation.Assembly.RuntimeSupportsUnmanagedSignatureCallingConvention)
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_RuntimeDoesNotSupportUnmanagedDefaultCallConv, errorLocation);
			}
		}
		static CallingConvention getCallingConvention(CSharpCompilation compilation, FunctionPointerCallingConventionSyntax? callingConventionSyntax, ArrayBuilder<CustomModifier> customModifiers, BindingDiagnosticBag bindingDiagnosticBag)
		{
			SyntaxKind? syntaxKind = callingConventionSyntax?.ManagedOrUnmanagedKeyword.Kind();
			switch (syntaxKind)
			{
			case null:
				return CallingConvention.Default;
			case SyntaxKind.ManagedKeyword:
				if (callingConventionSyntax.UnmanagedCallingConventionList != null && !callingConventionSyntax.ContainsDiagnostics)
				{
					bindingDiagnosticBag.Add(ErrorCode.ERR_CannotSpecifyManagedWithUnmanagedSpecifiers, callingConventionSyntax.UnmanagedCallingConventionList.GetLocation());
				}
				return CallingConvention.Default;
			case SyntaxKind.UnmanagedKeyword:
			{
				FunctionPointerUnmanagedCallingConventionListSyntax unmanagedCallingConventionList = callingConventionSyntax.UnmanagedCallingConventionList;
				if (unmanagedCallingConventionList != null)
				{
					SeparatedSyntaxList<FunctionPointerUnmanagedCallingConventionSyntax> callingConventions = unmanagedCallingConventionList.CallingConventions;
					switch (callingConventions.Count)
					{
					case 1:
						return callingConventions[0].Name.ValueText switch
						{
							"Cdecl" => CallingConvention.CDecl, 
							"Stdcall" => CallingConvention.Standard, 
							"Thiscall" => CallingConvention.ThisCall, 
							"Fastcall" => CallingConvention.FastCall, 
							_ => handleSingleConvention(callingConventions[0], compilation, customModifiers, bindingDiagnosticBag), 
						};
					case 0:
						if (!unmanagedCallingConventionList.ContainsDiagnostics)
						{
							bindingDiagnosticBag.Add(ErrorCode.ERR_InvalidFunctionPointerCallingConvention, unmanagedCallingConventionList.OpenBracketToken.GetLocation(), "");
						}
						return CallingConvention.Default;
					default:
					{
						SeparatedSyntaxList<FunctionPointerUnmanagedCallingConventionSyntax> separatedSyntaxList = callingConventions;
						checkUnmanagedSupport(compilation, callingConventionSyntax.ManagedOrUnmanagedKeyword.GetLocation(), bindingDiagnosticBag);
						foreach (FunctionPointerUnmanagedCallingConventionSyntax item in separatedSyntaxList)
						{
							CustomModifier customModifier = handleIndividualUnrecognizedSpecifier(item, compilation, bindingDiagnosticBag);
							if (customModifier != null)
							{
								customModifiers.Add(customModifier);
							}
						}
						return CallingConvention.Unmanaged;
					}
					}
				}
				checkUnmanagedSupport(compilation, callingConventionSyntax.ManagedOrUnmanagedKeyword.GetLocation(), bindingDiagnosticBag);
				return CallingConvention.Unmanaged;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(syntaxKind);
			}
		}
		static CustomModifier? handleIndividualUnrecognizedSpecifier(FunctionPointerUnmanagedCallingConventionSyntax specifier, CSharpCompilation compilation, BindingDiagnosticBag bindingDiagnosticBag)
		{
			string valueText = specifier.Name.ValueText;
			if (string.IsNullOrEmpty(valueText))
			{
				return null;
			}
			string text = "CallConv" + valueText;
			MetadataTypeName emittedName = MetadataTypeName.FromNamespaceAndTypeName("System.Runtime.CompilerServices", text, useCLSCompliantNameArityEncoding: true, 0);
			NamedTypeSymbol namedTypeSymbol = compilation.Assembly.CorLibrary.LookupDeclaredTopLevelMetadataType(ref emittedName);
			if ((object)namedTypeSymbol == null)
			{
				namedTypeSymbol = new MissingMetadataTypeSymbol.TopLevel(compilation.Assembly.CorLibrary.Modules[0], ref emittedName, new CSDiagnosticInfo(ErrorCode.ERR_TypeNotFound, text));
			}
			else if (namedTypeSymbol.DeclaredAccessibility != Accessibility.Public)
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_TypeMustBePublic, specifier.GetLocation(), namedTypeSymbol);
			}
			bindingDiagnosticBag.Add(namedTypeSymbol.GetUseSiteInfo(), specifier);
			return CSharpCustomModifier.CreateOptional(namedTypeSymbol);
		}
		static CallingConvention handleSingleConvention(FunctionPointerUnmanagedCallingConventionSyntax specifier, CSharpCompilation compilation, ArrayBuilder<CustomModifier> customModifiers, BindingDiagnosticBag diagnostics2)
		{
			checkUnmanagedSupport(compilation, specifier.GetLocation(), diagnostics2);
			CustomModifier customModifier = handleIndividualUnrecognizedSpecifier(specifier, compilation, diagnostics2);
			if (customModifier != null)
			{
				customModifiers.Add(customModifier);
			}
			return CallingConvention.Unmanaged;
		}
	}

	internal static FunctionPointerMethodSymbol CreateFromPartsForTest(CallingConvention callingConvention, TypeWithAnnotations returnType, ImmutableArray<CustomModifier> refCustomModifiers, RefKind returnRefKind, ImmutableArray<TypeWithAnnotations> parameterTypes, ImmutableArray<ImmutableArray<CustomModifier>> parameterRefCustomModifiers, ImmutableArray<RefKind> parameterRefKinds, CSharpCompilation compilation)
	{
		return new FunctionPointerMethodSymbol(callingConvention, returnRefKind, returnType, refCustomModifiers, parameterTypes, parameterRefCustomModifiers, parameterRefKinds, compilation);
	}

	internal static FunctionPointerMethodSymbol CreateFromParts(CallingConvention callingConvention, ImmutableArray<CustomModifier> callingConventionModifiers, TypeWithAnnotations returnTypeWithAnnotations, RefKind returnRefKind, ImmutableArray<TypeWithAnnotations> parameterTypes, ImmutableArray<RefKind> parameterRefKinds, CSharpCompilation compilation)
	{
		ArrayBuilder<CustomModifier> instance = ArrayBuilder<CustomModifier>.GetInstance();
		if (!callingConventionModifiers.IsDefaultOrEmpty)
		{
			instance.AddRange(callingConventionModifiers);
		}
		ImmutableArray<CustomModifier> refCustomModifiers;
		if (returnRefKind == RefKind.None)
		{
			refCustomModifiers = ImmutableArray<CustomModifier>.Empty;
			returnTypeWithAnnotations = returnTypeWithAnnotations.WithModifiers(instance.ToImmutableAndFree());
		}
		else
		{
			CustomModifier customModifierForRefKind = GetCustomModifierForRefKind(returnRefKind, compilation);
			if (customModifierForRefKind != null)
			{
				instance.Add(customModifierForRefKind);
			}
			refCustomModifiers = instance.ToImmutableAndFree();
		}
		return new FunctionPointerMethodSymbol(callingConvention, returnRefKind, returnTypeWithAnnotations, refCustomModifiers, parameterTypes, default(ImmutableArray<ImmutableArray<CustomModifier>>), parameterRefKinds, compilation);
	}

	private static CustomModifier? GetCustomModifierForRefKind(RefKind refKind, CSharpCompilation compilation)
	{
		NamedTypeSymbol namedTypeSymbol = refKind switch
		{
			RefKind.In => compilation.GetWellKnownType(WellKnownType.System_Runtime_InteropServices_InAttribute), 
			RefKind.Out => compilation.GetWellKnownType(WellKnownType.System_Runtime_InteropServices_OutAttribute), 
			_ => null, 
		};
		if ((object)namedTypeSymbol == null)
		{
			return null;
		}
		return CSharpCustomModifier.CreateRequired(namedTypeSymbol);
	}

	public static FunctionPointerMethodSymbol CreateFromMetadata(ModuleSymbol containingModule, CallingConvention callingConvention, ImmutableArray<ParamInfo<TypeSymbol>> retAndParamTypes)
	{
		return new FunctionPointerMethodSymbol(callingConvention, retAndParamTypes, containingModule.UseUpdatedEscapeRules);
	}

	public FunctionPointerMethodSymbol SubstituteParameterSymbols(TypeWithAnnotations substitutedReturnType, ImmutableArray<TypeWithAnnotations> substitutedParameterTypes, ImmutableArray<CustomModifier> refCustomModifiers = default(ImmutableArray<CustomModifier>), ImmutableArray<ImmutableArray<CustomModifier>> paramRefCustomModifiers = default(ImmutableArray<ImmutableArray<CustomModifier>>))
	{
		return new FunctionPointerMethodSymbol(CallingConvention, RefKind, substitutedReturnType, refCustomModifiers.IsDefault ? RefCustomModifiers : refCustomModifiers, Parameters, substitutedParameterTypes, paramRefCustomModifiers, UseUpdatedEscapeRules);
	}

	internal FunctionPointerMethodSymbol MergeEquivalentTypes(FunctionPointerMethodSymbol signature, VarianceKind variance)
	{
		VarianceKind variance2 = ((RefKind == RefKind.None) ? variance : VarianceKind.None);
		TypeWithAnnotations substitutedReturnType = ReturnTypeWithAnnotations.MergeEquivalentTypes(signature.ReturnTypeWithAnnotations, variance2);
		ImmutableArray<TypeWithAnnotations> substitutedParameterTypes = ImmutableArray<TypeWithAnnotations>.Empty;
		bool flag = false;
		if (_parameters.Length > 0)
		{
			ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance(_parameters.Length);
			for (int num = 0; num < _parameters.Length; num++)
			{
				FunctionPointerParameterSymbol functionPointerParameterSymbol = _parameters[num];
				FunctionPointerParameterSymbol functionPointerParameterSymbol2 = signature._parameters[num];
				RefKind refKind = functionPointerParameterSymbol.RefKind;
				VarianceKind varianceKind;
				if (variance != VarianceKind.Out)
				{
					if (variance != VarianceKind.In || refKind != RefKind.None)
					{
						goto IL_009e;
					}
					varianceKind = VarianceKind.Out;
				}
				else
				{
					if (refKind != RefKind.None)
					{
						goto IL_009e;
					}
					varianceKind = VarianceKind.In;
				}
				goto IL_00a1;
				IL_009e:
				varianceKind = VarianceKind.None;
				goto IL_00a1;
				IL_00a1:
				VarianceKind variance3 = varianceKind;
				TypeWithAnnotations item = functionPointerParameterSymbol.TypeWithAnnotations.MergeEquivalentTypes(functionPointerParameterSymbol2.TypeWithAnnotations, variance3);
				instance.Add(item);
				if (!item.IsSameAs(functionPointerParameterSymbol.TypeWithAnnotations))
				{
					flag = true;
				}
			}
			if (flag)
			{
				substitutedParameterTypes = instance.ToImmutableAndFree();
			}
			else
			{
				instance.Free();
				substitutedParameterTypes = base.ParameterTypesWithAnnotations;
			}
		}
		if (flag || !substitutedReturnType.IsSameAs(ReturnTypeWithAnnotations))
		{
			return SubstituteParameterSymbols(substitutedReturnType, substitutedParameterTypes);
		}
		return this;
	}

	public FunctionPointerMethodSymbol SetNullabilityForReferenceTypes(Func<TypeWithAnnotations, TypeWithAnnotations> transform)
	{
		TypeWithAnnotations substitutedReturnType = transform(ReturnTypeWithAnnotations);
		ImmutableArray<TypeWithAnnotations> substitutedParameterTypes = ImmutableArray<TypeWithAnnotations>.Empty;
		bool flag = false;
		if (_parameters.Length > 0)
		{
			ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance(_parameters.Length);
			foreach (FunctionPointerParameterSymbol parameter in _parameters)
			{
				TypeWithAnnotations item = transform(parameter.TypeWithAnnotations);
				instance.Add(item);
				if (!item.IsSameAs(parameter.TypeWithAnnotations))
				{
					flag = true;
				}
			}
			if (flag)
			{
				substitutedParameterTypes = instance.ToImmutableAndFree();
			}
			else
			{
				instance.Free();
				substitutedParameterTypes = base.ParameterTypesWithAnnotations;
			}
		}
		if (flag || !substitutedReturnType.IsSameAs(ReturnTypeWithAnnotations))
		{
			return SubstituteParameterSymbols(substitutedReturnType, substitutedParameterTypes);
		}
		return this;
	}

	private FunctionPointerMethodSymbol(CallingConvention callingConvention, RefKind refKind, TypeWithAnnotations returnType, ImmutableArray<CustomModifier> refCustomModifiers, ImmutableArray<ParameterSymbol> originalParameters, ImmutableArray<TypeWithAnnotations> substitutedParameterTypes, ImmutableArray<ImmutableArray<CustomModifier>> substitutedRefCustomModifiers, bool useUpdatedEscapeRules)
	{
		RefCustomModifiers = refCustomModifiers;
		CallingConvention = callingConvention;
		RefKind = refKind;
		ReturnTypeWithAnnotations = returnType;
		UseUpdatedEscapeRules = useUpdatedEscapeRules;
		if (originalParameters.Length > 0)
		{
			ArrayBuilder<FunctionPointerParameterSymbol> instance = ArrayBuilder<FunctionPointerParameterSymbol>.GetInstance(originalParameters.Length);
			for (int i = 0; i < originalParameters.Length; i++)
			{
				ParameterSymbol parameterSymbol = originalParameters[i];
				TypeWithAnnotations typeWithAnnotations = substitutedParameterTypes[i];
				ImmutableArray<CustomModifier> refCustomModifiers2 = (substitutedRefCustomModifiers.IsDefault ? parameterSymbol.RefCustomModifiers : substitutedRefCustomModifiers[i]);
				instance.Add(new FunctionPointerParameterSymbol(typeWithAnnotations, parameterSymbol.RefKind, parameterSymbol.Ordinal, this, refCustomModifiers2));
			}
			_parameters = instance.ToImmutableAndFree();
		}
		else
		{
			_parameters = ImmutableArray<FunctionPointerParameterSymbol>.Empty;
		}
	}

	private FunctionPointerMethodSymbol(CallingConvention callingConvention, RefKind refKind, TypeWithAnnotations returnTypeWithAnnotations, ImmutableArray<CustomModifier> refCustomModifiers, ImmutableArray<TypeWithAnnotations> parameterTypes, ImmutableArray<ImmutableArray<CustomModifier>> parameterRefCustomModifiers, ImmutableArray<RefKind> parameterRefKinds, CSharpCompilation compilation)
	{
		RefCustomModifiers = (refCustomModifiers.IsDefault ? getCustomModifierArrayForRefKind(refKind, compilation) : refCustomModifiers);
		RefKind = refKind;
		CallingConvention = callingConvention;
		ReturnTypeWithAnnotations = returnTypeWithAnnotations;
		UseUpdatedEscapeRules = compilation.SourceModule.UseUpdatedEscapeRules;
		_parameters = parameterTypes.ZipAsArray(parameterRefKinds, (this, compilation, parameterRefCustomModifiers), delegate(TypeWithAnnotations type, RefKind refKind2, int i, (FunctionPointerMethodSymbol Method, CSharpCompilation Comp, ImmutableArray<ImmutableArray<CustomModifier>> ParamRefCustomModifiers) arg)
		{
			ImmutableArray<CustomModifier> refCustomModifiers2 = (arg.ParamRefCustomModifiers.IsDefault ? getCustomModifierArrayForRefKind(refKind2, arg.Comp) : arg.ParamRefCustomModifiers[i]);
			return new FunctionPointerParameterSymbol(type, refKind2, i, arg.Method, refCustomModifiers2);
		});
		static ImmutableArray<CustomModifier> getCustomModifierArrayForRefKind(RefKind refKind2, CSharpCompilation compilation2)
		{
			CustomModifier customModifierForRefKind = GetCustomModifierForRefKind(refKind2, compilation2);
			if (customModifierForRefKind == null)
			{
				return ImmutableArray<CustomModifier>.Empty;
			}
			return ImmutableArray.Create(customModifierForRefKind);
		}
	}

	private FunctionPointerMethodSymbol(CallingConvention callingConvention, RefKind refKind, TypeWithAnnotations returnType, ImmutableArray<CustomModifier> refCustomModifiers, FunctionPointerTypeSyntax syntax, Binder typeBinder, BindingDiagnosticBag diagnostics, bool suppressUseSiteDiagnostics, bool useUpdatedEscapeRules)
	{
		RefCustomModifiers = refCustomModifiers;
		CallingConvention = callingConvention;
		RefKind = refKind;
		ReturnTypeWithAnnotations = returnType;
		UseUpdatedEscapeRules = useUpdatedEscapeRules;
		_parameters = ((syntax.ParameterList.Parameters.Count > 1) ? ParameterHelpers.MakeFunctionPointerParameters(typeBinder, this, syntax.ParameterList.Parameters, diagnostics, suppressUseSiteDiagnostics) : ImmutableArray<FunctionPointerParameterSymbol>.Empty);
	}

	private FunctionPointerMethodSymbol(CallingConvention callingConvention, ImmutableArray<ParamInfo<TypeSymbol>> retAndParamTypes, bool useUpdatedEscapeRules)
	{
		ParamInfo<TypeSymbol> param = retAndParamTypes[0];
		TypeWithAnnotations typeWithAnnotations = TypeWithAnnotations.Create(param.Type, NullableAnnotation.Oblivious, CSharpCustomModifier.Convert(param.CustomModifiers));
		RefCustomModifiers = CSharpCustomModifier.Convert(param.RefCustomModifiers);
		CallingConvention = callingConvention;
		ReturnTypeWithAnnotations = typeWithAnnotations;
		RefKind = getRefKind(param, RefCustomModifiers, RefKind.In, RefKind.Ref, requiresLocationAllowed: false);
		UseUpdatedEscapeRules = useUpdatedEscapeRules;
		ReadOnlySpan<ParamInfo<TypeSymbol>> readOnlySpan = retAndParamTypes.AsSpan();
		_parameters = makeParametersFromMetadata(readOnlySpan.Slice(1, readOnlySpan.Length - 1), this);
		static RefKind getRefKind(ParamInfo<TypeSymbol> paramInfo, ImmutableArray<CustomModifier> paramRefCustomMods, RefKind hasInRefKind, RefKind hasOutRefKind, bool requiresLocationAllowed)
		{
			if (!paramInfo.IsByRef)
			{
				return RefKind.None;
			}
			if (paramRefCustomMods.HasInAttributeModifier())
			{
				return hasInRefKind;
			}
			if (paramRefCustomMods.HasOutAttributeModifier())
			{
				return hasOutRefKind;
			}
			if (requiresLocationAllowed && paramRefCustomMods.HasRequiresLocationAttributeModifier())
			{
				return RefKind.RefReadOnlyParameter;
			}
			return RefKind.Ref;
		}
		static ImmutableArray<FunctionPointerParameterSymbol> makeParametersFromMetadata(ReadOnlySpan<ParamInfo<TypeSymbol>> parameterTypes, FunctionPointerMethodSymbol parent)
		{
			if (parameterTypes.Length > 0)
			{
				ArrayBuilder<FunctionPointerParameterSymbol> instance = ArrayBuilder<FunctionPointerParameterSymbol>.GetInstance(parameterTypes.Length);
				for (int i = 0; i < parameterTypes.Length; i++)
				{
					ParamInfo<TypeSymbol> param2 = parameterTypes[i];
					ImmutableArray<CustomModifier> immutableArray = CSharpCustomModifier.Convert(param2.RefCustomModifiers);
					TypeWithAnnotations typeWithAnnotations2 = TypeWithAnnotations.Create(param2.Type, NullableAnnotation.Oblivious, CSharpCustomModifier.Convert(param2.CustomModifiers));
					RefKind refKind = getRefKind(param2, immutableArray, RefKind.In, RefKind.Out, requiresLocationAllowed: true);
					instance.Add(new FunctionPointerParameterSymbol(typeWithAnnotations2, refKind, i, parent, immutableArray));
				}
				return instance.ToImmutableAndFree();
			}
			return ImmutableArray<FunctionPointerParameterSymbol>.Empty;
		}
	}

	internal void AddNullableTransforms(ArrayBuilder<byte> transforms)
	{
		ReturnTypeWithAnnotations.AddNullableTransforms(transforms);
		foreach (ParameterSymbol parameter in Parameters)
		{
			parameter.TypeWithAnnotations.AddNullableTransforms(transforms);
		}
	}

	internal FunctionPointerMethodSymbol ApplyNullableTransforms(byte defaultTransformFlag, ImmutableArray<byte> transforms, ref int position)
	{
		bool flag = ReturnTypeWithAnnotations.ApplyNullableTransforms(defaultTransformFlag, transforms, ref position, out var result);
		ImmutableArray<TypeWithAnnotations> substitutedParameterTypes = ImmutableArray<TypeWithAnnotations>.Empty;
		if (!Parameters.IsEmpty)
		{
			ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance(Parameters.Length);
			bool flag2 = false;
			foreach (ParameterSymbol parameter in Parameters)
			{
				flag2 |= parameter.TypeWithAnnotations.ApplyNullableTransforms(defaultTransformFlag, transforms, ref position, out var result2);
				instance.Add(result2);
			}
			if (flag2)
			{
				substitutedParameterTypes = instance.ToImmutableAndFree();
				flag = true;
			}
			else
			{
				instance.Free();
				substitutedParameterTypes = base.ParameterTypesWithAnnotations;
			}
		}
		if (flag)
		{
			return SubstituteParameterSymbols(result, substitutedParameterTypes);
		}
		return this;
	}

	public ImmutableHashSet<CustomModifier> GetCallingConventionModifiers()
	{
		if (_lazyCallingConventionModifiers == null)
		{
			ImmutableArray<CustomModifier> immutableArray = ((RefKind != RefKind.None) ? RefCustomModifiers : ReturnTypeWithAnnotations.CustomModifiers);
			if (immutableArray.IsEmpty || CallingConvention != CallingConvention.Unmanaged)
			{
				_lazyCallingConventionModifiers = ImmutableHashSet<CustomModifier>.Empty;
			}
			else
			{
				PooledHashSet<CustomModifier> instance = PooledHashSet<CustomModifier>.GetInstance();
				foreach (CustomModifier item in immutableArray)
				{
					if (FunctionPointerTypeSymbol.IsCallingConventionModifier(((CSharpCustomModifier)item).ModifierSymbol))
					{
						instance.Add(item);
					}
				}
				if (instance.Count == 0)
				{
					_lazyCallingConventionModifiers = ImmutableHashSet<CustomModifier>.Empty;
				}
				else
				{
					_lazyCallingConventionModifiers = instance.ToImmutableHashSet();
				}
				instance.Free();
			}
		}
		return _lazyCallingConventionModifiers;
	}

	public override bool Equals(Symbol other, TypeCompareKind compareKind)
	{
		if (!(other is FunctionPointerMethodSymbol other2))
		{
			return false;
		}
		return Equals(other2, compareKind);
	}

	internal bool Equals(FunctionPointerMethodSymbol other, TypeCompareKind compareKind)
	{
		if ((object)this != other)
		{
			if (EqualsNoParameters(other, compareKind))
			{
				return _parameters.SequenceEqual(other._parameters, compareKind, (FunctionPointerParameterSymbol param1, FunctionPointerParameterSymbol param2, TypeCompareKind compareKind2) => param1.MethodEqualityChecks(param2, compareKind2));
			}
			return false;
		}
		return true;
	}

	private bool EqualsNoParameters(FunctionPointerMethodSymbol other, TypeCompareKind compareKind)
	{
		if (CallingConvention != other.CallingConvention || !FunctionPointerTypeSymbol.RefKindEquals(compareKind, RefKind, other.RefKind) || !ReturnTypeWithAnnotations.Equals(other.ReturnTypeWithAnnotations, compareKind))
		{
			return false;
		}
		if ((compareKind & TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds) != TypeCompareKind.ConsiderEverything)
		{
			if (CallingConvention.IsCallingConvention(CallingConvention.Unmanaged) && !GetCallingConventionModifiers().SetEqualsWithoutIntermediateHashSet(other.GetCallingConventionModifiers()))
			{
				return false;
			}
		}
		else if (!RefCustomModifiers.SequenceEqual(other.RefCustomModifiers))
		{
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		int num = GetHashCodeNoParameters();
		foreach (FunctionPointerParameterSymbol parameter in _parameters)
		{
			num = Hash.Combine(parameter.MethodHashCode(), num);
		}
		return num;
	}

	internal int GetHashCodeNoParameters()
	{
		return Hash.Combine(base.ReturnType, Hash.Combine(((int)CallingConvention).GetHashCode(), ((int)FunctionPointerTypeSymbol.GetRefKindForHashCode(RefKind)).GetHashCode()));
	}

	internal override UseSiteInfo<AssemblySymbol> GetUseSiteInfo()
	{
		UseSiteInfo<AssemblySymbol> result = default(UseSiteInfo<AssemblySymbol>);
		CalculateUseSiteDiagnostic(ref result);
		if (CallingConvention.IsCallingConvention(CallingConvention.ExtraArguments))
		{
			MergeUseSiteInfo(ref result, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_UnsupportedCallingConvention, this)));
		}
		DiagnosticInfo? diagnosticInfo = result.DiagnosticInfo;
		if (diagnosticInfo == null || diagnosticInfo.Severity != DiagnosticSeverity.Error)
		{
			foreach (ParameterSymbol parameter in Parameters)
			{
				if (parameter.RefCustomModifiers.HasRequiresLocationAttributeModifier() && parameter.RefCustomModifiers.Any((CustomModifier m) => !m.IsOptional))
				{
					MergeUseSiteInfo(ref result, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, this)));
				}
			}
		}
		return result;
	}

	internal bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo? result, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		if (!base.ReturnType.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes) && !Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, RefCustomModifiers, owner, ref checkedTypes))
		{
			return Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, Parameters, owner, ref checkedTypes);
		}
		return true;
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}

	internal override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	internal override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
	{
		return false;
	}

	internal sealed override UnmanagedCallersOnlyAttributeData? GetUnmanagedCallersOnlyAttributeData(bool forceComplete)
	{
		return null;
	}

	public override DllImportData GetDllImportData()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionPointers/FunctionPointerMethodSymbol.cs", 857);
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionPointers/FunctionPointerMethodSymbol.cs", 858);
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionPointers/FunctionPointerMethodSymbol.cs", 859);
	}

	internal sealed override bool IsNullableAnalysisEnabled()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionPointers/FunctionPointerMethodSymbol.cs", 860);
	}

	internal sealed override bool HasAsyncMethodBuilderAttribute(out TypeSymbol? builderArgument)
	{
		builderArgument = null;
		return false;
	}

	internal sealed override int TryGetOverloadResolutionPriority()
	{
		return 0;
	}
}
