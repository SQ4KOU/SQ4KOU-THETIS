using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceUserDefinedOperatorSymbolBase : SourceOrdinaryMethodOrUserDefinedOperatorSymbol
{
	private const TypeCompareKind ComparisonForUserDefinedOperators = TypeCompareKind.IgnoreTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes;

	private readonly string _name;

	private readonly TypeSymbol? _explicitInterfaceType;

	protected sealed override TypeSymbol? ExplicitInterfaceType => _explicitInterfaceType;

	public sealed override string Name => _name;

	public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	protected SourceUserDefinedOperatorSymbolBase(MethodKind methodKind, TypeSymbol explicitInterfaceType, string name, bool isCompoundAssignmentOrIncrementAssignment, SourceMemberContainerTypeSymbol containingType, Location location, CSharpSyntaxNode syntax, DeclarationModifiers declarationModifiers, bool hasAnyBody, bool isExpressionBodied, bool isIterator, bool isNullableAnalysisEnabled, BindingDiagnosticBag diagnostics)
		: base(containingType, syntax.GetReference(), location, isIterator, (declarationModifiers: declarationModifiers, flags: SourceMemberMethodSymbol.MakeFlags(methodKind, RefKind.None, declarationModifiers, returnsVoid: false, returnsVoidIsSet: false, isExpressionBodied, isExtensionMethod: false, isNullableAnalysisEnabled, isVarArg: false, methodKind == MethodKind.ExplicitInterfaceImplementation, hasThisInitializer: false)))
	{
		_explicitInterfaceType = explicitInterfaceType;
		_name = name;
		this.CheckUnsafeModifier(declarationModifiers, diagnostics);
		if (isCompoundAssignmentOrIncrementAssignment)
		{
			Binder.CheckFeatureAvailability(syntax, MessageID.IDS_FeatureUserDefinedCompoundAssignmentOperators, diagnostics, ((OperatorDeclarationSyntax)syntax).OperatorToken.GetLocation());
		}
		bool flag = ContainingType.IsInterface && !IsAbstract && !IsVirtual && !IsExplicitInterfaceImplementation;
		if (flag)
		{
			SyntaxToken operatorToken = default(SyntaxToken);
			int num;
			if (syntax is OperatorDeclarationSyntax operatorDeclarationSyntax)
			{
				operatorToken = operatorDeclarationSyntax.OperatorToken;
				num = 1;
			}
			else
			{
				num = 0;
			}
			bool flag2 = (byte)num != 0;
			if (flag2)
			{
				SyntaxKind syntaxKind = operatorToken.Kind();
				flag2 = syntaxKind - 8267 > SyntaxKind.List;
			}
			flag = !flag2;
		}
		if (flag)
		{
			diagnostics.Add(ErrorCode.ERR_InterfacesCantContainConversionOrEqualityOperators, GetFirstLocation());
			return;
		}
		if (ContainingType.IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_OperatorInStaticClass, location, this);
			return;
		}
		NamedTypeSymbol containingType2 = ContainingType;
		if ((object)containingType2 != null && containingType2.IsExtension)
		{
			ParameterSymbol extensionParameter = containingType2.ExtensionParameter;
			if ((object)extensionParameter != null)
			{
				TypeSymbol type = extensionParameter.Type;
				if ((object)type != null && type.IsStatic)
				{
					diagnostics.Add(ErrorCode.ERR_OperatorInExtensionOfStaticClass, location);
					return;
				}
			}
		}
		if (IsExplicitInterfaceImplementation)
		{
			if (!IsStatic && !isCompoundAssignmentOrIncrementAssignment)
			{
				diagnostics.Add(ErrorCode.ERR_ExplicitImplementationOfOperatorsMustBeStatic, GetFirstLocation(), this);
			}
		}
		else if (isCompoundAssignmentOrIncrementAssignment)
		{
			if (DeclaredAccessibility != Accessibility.Public)
			{
				diagnostics.Add(ErrorCode.ERR_OperatorsMustBePublic, GetFirstLocation(), this);
			}
		}
		else if (DeclaredAccessibility != Accessibility.Public || !IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_OperatorsMustBeStaticAndPublic, GetFirstLocation(), this);
		}
		if (IsAbstract && IsExtern)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractAndExtern, location, this);
		}
		else if (IsAbstract && IsVirtual)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractNotVirtual, location, Kind.Localize(), this);
		}
		else if (hasAnyBody && (IsExtern || IsAbstract))
		{
			if (IsExtern)
			{
				diagnostics.Add(ErrorCode.ERR_ExternHasBody, location, this);
			}
			else
			{
				diagnostics.Add(ErrorCode.ERR_AbstractHasBody, location, this);
			}
		}
		else if (!hasAnyBody && !IsExtern && !IsAbstract && !base.IsPartial)
		{
			diagnostics.Add(ErrorCode.ERR_ConcreteMissingBody, location, this);
		}
		else if (IsOverride && (base.IsNew || IsVirtual))
		{
			diagnostics.Add(ErrorCode.ERR_OverrideNotNew, location, this);
		}
		else if (IsSealed && !IsOverride && (!IsExplicitInterfaceImplementation || !ContainingType.IsInterface || !IsAbstract))
		{
			diagnostics.Add(ErrorCode.ERR_SealedNonOverride, location, this);
		}
		else if (IsAbstract && IsSealed && !IsExplicitInterfaceImplementation)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractAndSealed, location, this);
		}
		else if (IsAbstract && !ContainingType.IsAbstract && !ContainingType.IsInterface)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractInConcreteClass, location, this, ContainingType);
		}
		else if (IsVirtual && ContainingType.IsSealed)
		{
			diagnostics.Add(ErrorCode.ERR_NewVirtualInSealed, location, this, ContainingType);
		}
		ModifierUtils.CheckAccessibility(DeclarationModifiers, this, isExplicitInterfaceImplementation: false, diagnostics, location);
		if (isCompoundAssignmentOrIncrementAssignment)
		{
			Binder.GetWellKnownTypeMember(DeclaringCompilation, WellKnownMember.System_Runtime_CompilerServices_CompilerFeatureRequiredAttribute__ctor, diagnostics, location);
		}
	}

	protected static DeclarationModifiers MakeDeclarationModifiers(bool isCompoundAssignmentOrIncrementAssignment, MethodKind methodKind, SourceMemberContainerTypeSymbol containingType, BaseMethodDeclarationSyntax syntax, Location location, BindingDiagnosticBag diagnostics)
	{
		bool isInterface = containingType.IsInterface;
		bool isExtension = containingType.IsExtension;
		bool flag = methodKind == MethodKind.ExplicitInterfaceImplementation;
		DeclarationModifiers defaultAccess = ((isInterface && !flag) ? DeclarationModifiers.Public : DeclarationModifiers.Private);
		DeclarationModifiers declarationModifiers = DeclarationModifiers.Extern | DeclarationModifiers.Unsafe;
		if (!isCompoundAssignmentOrIncrementAssignment)
		{
			declarationModifiers |= DeclarationModifiers.Static;
		}
		bool hasExplicitAccessModifier;
		bool modifierErrors;
		if (!flag)
		{
			declarationModifiers |= DeclarationModifiers.AccessibilityMask;
			if (isInterface)
			{
				declarationModifiers |= DeclarationModifiers.Abstract | DeclarationModifiers.Virtual;
				SyntaxToken operatorToken = default(SyntaxToken);
				int num;
				if (syntax is OperatorDeclarationSyntax operatorDeclarationSyntax)
				{
					operatorToken = operatorDeclarationSyntax.OperatorToken;
					num = 1;
				}
				else
				{
					num = 0;
				}
				modifierErrors = (byte)num != 0;
				if (modifierErrors)
				{
					SyntaxKind syntaxKind = operatorToken.Kind();
					hasExplicitAccessModifier = syntaxKind - 8267 <= SyntaxKind.List;
					modifierErrors = !hasExplicitAccessModifier;
				}
				if (modifierErrors)
				{
					declarationModifiers |= DeclarationModifiers.Sealed;
				}
			}
			if (isCompoundAssignmentOrIncrementAssignment && !isExtension)
			{
				if (isInterface)
				{
					declarationModifiers |= DeclarationModifiers.New;
				}
				else
				{
					if (containingType.IsClassType())
					{
						declarationModifiers |= DeclarationModifiers.Abstract | DeclarationModifiers.Sealed | DeclarationModifiers.Virtual;
					}
					declarationModifiers |= DeclarationModifiers.New | DeclarationModifiers.Override;
				}
			}
		}
		else if (isInterface)
		{
			declarationModifiers |= DeclarationModifiers.Abstract;
		}
		if (containingType.IsStructType() & isCompoundAssignmentOrIncrementAssignment)
		{
			declarationModifiers |= DeclarationModifiers.ReadOnly;
		}
		DeclarationModifiers declarationModifiers2 = ModifierUtils.MakeAndCheckNonTypeMemberModifiers(isOrdinaryMethod: false, isInterface, syntax.Modifiers, defaultAccess, declarationModifiers, location, diagnostics, out modifierErrors, out hasExplicitAccessModifier);
		if (isInterface)
		{
			if ((declarationModifiers2 & (DeclarationModifiers.Abstract | DeclarationModifiers.Sealed | DeclarationModifiers.Virtual)) != DeclarationModifiers.None)
			{
				if ((declarationModifiers2 & DeclarationModifiers.Sealed) != DeclarationModifiers.None && (declarationModifiers2 & (DeclarationModifiers.Abstract | DeclarationModifiers.Virtual)) != DeclarationModifiers.None)
				{
					diagnostics.Add(ErrorCode.ERR_BadMemberFlag, location, ModifierUtils.ConvertSingleModifierToSyntaxText(DeclarationModifiers.Sealed));
					declarationModifiers2 = (DeclarationModifiers)((uint)declarationModifiers2 & 0xFFFFFFFDu);
				}
				LanguageVersion languageVersion = ((CSharpParseOptions)location.SourceTree.Options).LanguageVersion;
				LanguageVersion languageVersion2 = MessageID.IDS_FeatureStaticAbstractMembersInInterfaces.RequiredVersion();
				if (languageVersion < languageVersion2)
				{
					CSharpRequiredLanguageVersion requiredVersionArgument = new CSharpRequiredLanguageVersion(languageVersion2);
					string availableVersionArgument = languageVersion.ToDisplayString();
					if ((declarationModifiers2 & DeclarationModifiers.Abstract) != DeclarationModifiers.None)
					{
						reportModifierIfPresent(declarationModifiers2, DeclarationModifiers.Abstract, location, diagnostics, requiredVersionArgument, availableVersionArgument);
					}
					else
					{
						reportModifierIfPresent(declarationModifiers2, DeclarationModifiers.Virtual, location, diagnostics, requiredVersionArgument, availableVersionArgument);
					}
					reportModifierIfPresent(declarationModifiers2, DeclarationModifiers.Sealed, location, diagnostics, requiredVersionArgument, availableVersionArgument);
				}
				declarationModifiers2 = (DeclarationModifiers)((uint)declarationModifiers2 & 0xFFFFFFFDu);
			}
			else if ((declarationModifiers2 & DeclarationModifiers.Static) != DeclarationModifiers.None)
			{
				SyntaxToken operatorToken2 = default(SyntaxToken);
				int num2;
				if (syntax is OperatorDeclarationSyntax operatorDeclarationSyntax2)
				{
					operatorToken2 = operatorDeclarationSyntax2.OperatorToken;
					num2 = 1;
				}
				else
				{
					num2 = 0;
				}
				hasExplicitAccessModifier = (byte)num2 != 0;
				if (hasExplicitAccessModifier)
				{
					SyntaxKind syntaxKind = operatorToken2.Kind();
					modifierErrors = syntaxKind - 8267 <= SyntaxKind.List;
					hasExplicitAccessModifier = !modifierErrors;
				}
				if (hasExplicitAccessModifier)
				{
					Binder.CheckFeatureAvailability(location.SourceTree, MessageID.IDS_DefaultInterfaceImplementation, diagnostics, location);
				}
			}
			else if (!flag & isCompoundAssignmentOrIncrementAssignment)
			{
				declarationModifiers2 = ((!syntax.HasAnyBody()) ? (declarationModifiers2 | DeclarationModifiers.Abstract) : (declarationModifiers2 | DeclarationModifiers.Virtual));
			}
		}
		if (flag && (declarationModifiers2 & DeclarationModifiers.Abstract) != DeclarationModifiers.None)
		{
			declarationModifiers2 |= DeclarationModifiers.Sealed;
		}
		return declarationModifiers2;
		static void reportModifierIfPresent(DeclarationModifiers result, DeclarationModifiers errorModifier, Location location2, BindingDiagnosticBag bindingDiagnosticBag, CSharpRequiredLanguageVersion cSharpRequiredLanguageVersion, string text)
		{
			if ((result & errorModifier) != DeclarationModifiers.None)
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_InvalidModifierForLanguageVersion, location2, ModifierUtils.ConvertSingleModifierToSyntaxText(errorModifier), text, cSharpRequiredLanguageVersion);
			}
		}
	}

	protected (TypeWithAnnotations ReturnType, ImmutableArray<ParameterSymbol> Parameters) MakeParametersAndBindReturnType(BaseMethodDeclarationSyntax declarationSyntax, TypeSyntax returnTypeSyntax, BindingDiagnosticBag diagnostics)
	{
		Binder binder = DeclaringCompilation.GetBinderFactory(declarationSyntax.SyntaxTree).GetBinder(returnTypeSyntax, declarationSyntax, this).WithAdditionalFlags(BinderFlags.SuppressConstraintChecks);
		ParameterListSyntax parameterList = declarationSyntax.ParameterList;
		bool addRefReadOnlyModifier = IsVirtual || IsAbstract;
		ImmutableArray<ParameterSymbol> item = ParameterHelpers.MakeParameters(binder, this, parameterList, out var arglistToken, diagnostics, allowRefOrOut: true, allowThis: false, addRefReadOnlyModifier).Cast<SourceParameterSymbol, ParameterSymbol>();
		if (arglistToken.Kind() == SyntaxKind.ArgListKeyword)
		{
			diagnostics.Add(ErrorCode.ERR_IllegalVarArgs, new SourceLocation(in arglistToken));
		}
		TypeWithAnnotations item2 = binder.BindType(returnTypeSyntax, diagnostics);
		if (item2.IsRestrictedType(ignoreSpanLikeTypes: true))
		{
			diagnostics.Add(ErrorCode.ERR_MethodReturnCantBeRefAny, returnTypeSyntax.Location, item2.Type);
		}
		if (item2.Type.IsStatic)
		{
			diagnostics.Add(ErrorFacts.GetStaticClassReturnCode(useWarning: false), returnTypeSyntax.Location, item2.Type);
		}
		return (ReturnType: item2, Parameters: item);
	}

	protected override void MethodChecks(BindingDiagnosticBag diagnostics)
	{
		var (returnType, parameters) = MakeParametersAndBindReturnType(diagnostics);
		MethodChecks(returnType, parameters, diagnostics);
		if (!ContainingType.IsStatic)
		{
			CheckValueParameters(diagnostics);
			CheckOperatorSignatures(diagnostics);
		}
	}

	protected abstract (TypeWithAnnotations ReturnType, ImmutableArray<ParameterSymbol> Parameters) MakeParametersAndBindReturnType(BindingDiagnosticBag diagnostics);

	protected sealed override void ExtensionMethodChecks(BindingDiagnosticBag diagnostics)
	{
		NamedTypeSymbol containingType = ContainingType;
		if ((object)containingType == null || !containingType.IsExtension)
		{
			return;
		}
		ParameterSymbol extensionParameter = containingType.ExtensionParameter;
		if ((object)extensionParameter == null)
		{
			return;
		}
		TypeSymbol type = extensionParameter.Type;
		if ((object)type == null || type.IsStatic || IsStatic || !OperatorFacts.IsCompoundAssignmentOperatorName(Name))
		{
			return;
		}
		if (extensionParameter.Name == "")
		{
			diagnostics.Add(ErrorCode.ERR_InstanceMemberWithUnnamedExtensionsParameter, _location, new FormattedSymbol(this, SymbolDisplayFormat.ShortFormat));
		}
		switch (extensionParameter.RefKind)
		{
		case RefKind.In:
		case RefKind.RefReadOnlyParameter:
			if (extensionParameter.Type.IsStructType())
			{
				diagnostics.Add(ErrorCode.ERR_InstanceOperatorStructExtensionWrongReceiverRefKind, _location);
			}
			break;
		case RefKind.None:
			type = extensionParameter.Type;
			if ((object)type == null)
			{
				break;
			}
			if (!type.IsValueType)
			{
				if (type.TypeKind == TypeKind.TypeParameter && !type.IsReferenceType)
				{
					diagnostics.Add(ErrorCode.ERR_InstanceOperatorExtensionWrongReceiverType, _location);
				}
			}
			else
			{
				diagnostics.Add(ErrorCode.ERR_InstanceOperatorStructExtensionWrongReceiverRefKind, _location);
			}
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(extensionParameter.RefKind);
		case RefKind.Ref:
		case RefKind.Out:
			break;
		}
	}

	protected sealed override MethodSymbol FindExplicitlyImplementedMethod(BindingDiagnosticBag diagnostics)
	{
		if ((object)_explicitInterfaceType != null)
		{
			SyntaxNode syntax = syntaxReferenceOpt.GetSyntax();
			string interfaceMethodName;
			ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier;
			if (!(syntax is OperatorDeclarationSyntax operatorDeclarationSyntax))
			{
				if (!(syntax is ConversionOperatorDeclarationSyntax conversionOperatorDeclarationSyntax))
				{
					throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/SourceUserDefinedOperatorSymbolBase.cs", 455);
				}
				interfaceMethodName = OperatorFacts.OperatorNameFromDeclaration(conversionOperatorDeclarationSyntax);
				explicitInterfaceSpecifier = conversionOperatorDeclarationSyntax.ExplicitInterfaceSpecifier;
			}
			else
			{
				interfaceMethodName = OperatorFacts.OperatorNameFromDeclaration(operatorDeclarationSyntax);
				explicitInterfaceSpecifier = operatorDeclarationSyntax.ExplicitInterfaceSpecifier;
			}
			return this.FindExplicitlyImplementedMethod(isOperator: true, _explicitInterfaceType, interfaceMethodName, explicitInterfaceSpecifier, diagnostics);
		}
		return null;
	}

	private void CheckValueParameters(BindingDiagnosticBag diagnostics)
	{
		foreach (ParameterSymbol parameter in Parameters)
		{
			if (parameter.RefKind != RefKind.None && parameter.RefKind != RefKind.In)
			{
				diagnostics.Add(ErrorCode.ERR_IllegalRefParam, GetFirstLocation());
				break;
			}
		}
	}

	private void CheckOperatorSignatures(BindingDiagnosticBag diagnostics)
	{
		if (MethodKind == MethodKind.ExplicitInterfaceImplementation || !DoesOperatorHaveCorrectArity(Name, ParameterCount))
		{
			return;
		}
		switch (Name)
		{
		case "op_Implicit":
		case "op_Explicit":
		case "op_CheckedExplicit":
			CheckUserDefinedConversionSignature(diagnostics);
			break;
		case "op_CheckedUnaryNegation":
		case "op_UnaryPlus":
		case "op_LogicalNot":
		case "op_UnaryNegation":
		case "op_OnesComplement":
			CheckUnarySignature(diagnostics);
			break;
		case "op_True":
		case "op_False":
			CheckTrueFalseSignature(diagnostics);
			break;
		case "op_Decrement":
		case "op_Increment":
		case "op_CheckedDecrement":
		case "op_CheckedIncrement":
			CheckIncrementDecrementSignature(diagnostics);
			break;
		case "op_LeftShift":
		case "op_RightShift":
		case "op_UnsignedRightShift":
			CheckShiftSignature(diagnostics);
			break;
		case "op_Equality":
		case "op_Inequality":
			if (IsInInterfaceAndAbstractOrVirtual())
			{
				CheckAbstractEqualitySignature(diagnostics);
			}
			else
			{
				CheckBinarySignature(diagnostics);
			}
			break;
		case "op_BitwiseAndAssignment":
		case "op_RightShiftAssignment":
		case "op_AdditionAssignment":
		case "op_DivisionAssignment":
		case "op_CheckedAdditionAssignment":
		case "op_CheckedDivisionAssignment":
		case "op_CheckedSubtractionAssignment":
		case "op_UnsignedRightShiftAssignment":
		case "op_ExclusiveOrAssignment":
		case "op_SubtractionAssignment":
		case "op_IncrementAssignment":
		case "op_LeftShiftAssignment":
		case "op_BitwiseOrAssignment":
		case "op_DecrementAssignment":
		case "op_CheckedDecrementAssignment":
		case "op_CheckedIncrementAssignment":
		case "op_CheckedMultiplicationAssignment":
		case "op_MultiplicationAssignment":
		case "op_ModulusAssignment":
			if (!ReturnsVoid)
			{
				diagnostics.Add(ErrorCode.ERR_OperatorMustReturnVoid, GetFirstLocation());
			}
			break;
		default:
			CheckBinarySignature(diagnostics);
			break;
		}
	}

	private bool IsInInterfaceAndAbstractOrVirtual()
	{
		if (ContainingType.IsInterface)
		{
			if (!IsAbstract)
			{
				return IsVirtual;
			}
			return true;
		}
		return false;
	}

	private static bool DoesOperatorHaveCorrectArity(string name, int parameterCount)
	{
		switch (name)
		{
		case "op_CheckedDecrement":
		case "op_CheckedIncrement":
		case "op_Decrement":
		case "op_Increment":
			return parameterCount == 1;
		case "op_UnaryPlus":
		case "op_BitwiseAndAssignment":
		case "op_CheckedUnaryNegation":
		case "op_RightShiftAssignment":
		case "op_Explicit":
		case "op_Implicit":
		case "op_CheckedAdditionAssignment":
		case "op_CheckedDivisionAssignment":
		case "op_AdditionAssignment":
		case "op_DivisionAssignment":
		case "op_CheckedSubtractionAssignment":
		case "op_UnsignedRightShiftAssignment":
		case "op_ExclusiveOrAssignment":
		case "op_SubtractionAssignment":
		case "op_LeftShiftAssignment":
		case "op_BitwiseOrAssignment":
		case "op_UnaryNegation":
		case "op_LogicalNot":
		case "op_OnesComplement":
		case "op_True":
		case "op_False":
		case "op_CheckedExplicit":
		case "op_CheckedMultiplicationAssignment":
		case "op_MultiplicationAssignment":
		case "op_ModulusAssignment":
			return parameterCount == 1;
		case "op_IncrementAssignment":
		case "op_DecrementAssignment":
		case "op_CheckedDecrementAssignment":
		case "op_CheckedIncrementAssignment":
			return parameterCount == 0;
		default:
			return parameterCount == 2;
		}
	}

	private void CheckUserDefinedConversionSignature(BindingDiagnosticBag diagnostics)
	{
		CheckReturnIsNotVoid(diagnostics);
		TypeSymbol parameterType = GetParameterType(0);
		TypeSymbol returnType = base.ReturnType;
		TypeSymbol typeSymbol = parameterType.StrippedType();
		TypeSymbol typeSymbol2 = returnType.StrippedType();
		if (typeSymbol.IsInterfaceType() || typeSymbol2.IsInterfaceType())
		{
			diagnostics.Add(ErrorCode.ERR_ConversionWithInterface, GetFirstLocation(), this);
			return;
		}
		if (!MatchesContainingType(typeSymbol, checkStrippedType: false) && !MatchesContainingType(typeSymbol2, checkStrippedType: false) && !MatchesContainingType(parameterType, checkStrippedType: false) && !MatchesContainingType(returnType, checkStrippedType: false))
		{
			diagnostics.Add(IsInInterfaceAndAbstractOrVirtual() ? ErrorCode.ERR_AbstractConversionNotInvolvingContainedType : ErrorCode.ERR_ConversionNotInvolvingContainedType, GetFirstLocation());
			return;
		}
		if ((ContainingType.SpecialType == SpecialType.System_Nullable_T) ? parameterType.Equals(returnType, TypeCompareKind.IgnoreTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes) : typeSymbol.Equals(typeSymbol2, TypeCompareKind.IgnoreTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes))
		{
			diagnostics.Add(ErrorCode.ERR_IdentityConversion, GetFirstLocation());
			return;
		}
		if (parameterType.IsDynamic() || returnType.IsDynamic())
		{
			diagnostics.Add(ErrorCode.ERR_BadDynamicConversion, GetFirstLocation(), this);
			return;
		}
		TypeSymbol typeSymbol3;
		TypeSymbol typeSymbol4;
		if (MatchesContainingType(typeSymbol, checkStrippedType: false))
		{
			typeSymbol3 = parameterType;
			typeSymbol4 = returnType;
		}
		else
		{
			typeSymbol3 = returnType;
			typeSymbol4 = parameterType;
		}
		if (typeSymbol4.IsClassType() && !typeSymbol3.IsTypeParameter())
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, ContainingAssembly);
			if (typeSymbol3.IsDerivedFrom(typeSymbol4, TypeCompareKind.IgnoreTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes, ref useSiteInfo))
			{
				diagnostics.Add(ErrorCode.ERR_ConversionWithBase, GetFirstLocation(), this);
			}
			else if (typeSymbol4.IsDerivedFrom(typeSymbol3, TypeCompareKind.IgnoreTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes, ref useSiteInfo))
			{
				diagnostics.Add(ErrorCode.ERR_ConversionWithDerived, GetFirstLocation(), this);
			}
			diagnostics.Add(GetFirstLocation(), useSiteInfo);
		}
	}

	private void CheckReturnIsNotVoid(BindingDiagnosticBag diagnostics)
	{
		if (ReturnsVoid)
		{
			diagnostics.Add(ErrorCode.ERR_OperatorCantReturnVoid, GetFirstLocation());
		}
	}

	private void CheckUnarySignature(BindingDiagnosticBag diagnostics)
	{
		CheckUnaryParameterType(diagnostics);
		CheckReturnIsNotVoid(diagnostics);
	}

	private void CheckUnaryParameterType(BindingDiagnosticBag diagnostics)
	{
		if (!MatchesContainingType(GetParameterType(0), checkStrippedType: true))
		{
			diagnostics.Add(IsInInterfaceAndAbstractOrVirtual() ? ErrorCode.ERR_BadAbstractUnaryOperatorSignature : (ContainingType.IsExtension ? ErrorCode.ERR_BadExtensionUnaryOperatorSignature : ErrorCode.ERR_BadUnaryOperatorSignature), GetFirstLocation());
		}
	}

	private void CheckTrueFalseSignature(BindingDiagnosticBag diagnostics)
	{
		if (base.ReturnType.SpecialType != SpecialType.System_Boolean)
		{
			diagnostics.Add(ErrorCode.ERR_OpTFRetType, GetFirstLocation());
		}
		CheckUnaryParameterType(diagnostics);
	}

	private void CheckIncrementDecrementSignature(BindingDiagnosticBag diagnostics)
	{
		TypeSymbol parameterType = GetParameterType(0);
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, ContainingAssembly);
		if (!MatchesContainingType(parameterType, checkStrippedType: true))
		{
			diagnostics.Add(IsInInterfaceAndAbstractOrVirtual() ? ErrorCode.ERR_BadAbstractIncDecSignature : (ContainingType.IsExtension ? ErrorCode.ERR_BadExtensionIncDecSignature : ErrorCode.ERR_BadIncDecSignature), GetFirstLocation());
		}
		else
		{
			bool num;
			if (!parameterType.IsTypeParameter())
			{
				if (IsInInterfaceAndAbstractOrVirtual() && IsContainingType(parameterType) && IsSelfConstrainedTypeParameter(base.ReturnType))
				{
					goto IL_00c9;
				}
				num = base.ReturnType.EffectiveTypeNoUseSiteDiagnostics.IsEqualToOrDerivedFrom(parameterType, TypeCompareKind.IgnoreTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes, ref useSiteInfo);
			}
			else
			{
				num = base.ReturnType.Equals(parameterType, TypeCompareKind.IgnoreTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes);
			}
			if (!num)
			{
				diagnostics.Add(IsInInterfaceAndAbstractOrVirtual() ? ErrorCode.ERR_BadAbstractIncDecRetType : ErrorCode.ERR_BadIncDecRetType, GetFirstLocation());
			}
		}
		goto IL_00c9;
		IL_00c9:
		diagnostics.Add(GetFirstLocation(), useSiteInfo);
	}

	private bool MatchesContainingType(TypeSymbol type, bool checkStrippedType)
	{
		NamedTypeSymbol containingType = ContainingType;
		if ((object)containingType != null && containingType.IsExtension)
		{
			ParameterSymbol extensionParameter = containingType.ExtensionParameter;
			if ((object)extensionParameter != null)
			{
				TypeSymbol type2 = extensionParameter.Type;
				if ((object)type2 == null)
				{
					return true;
				}
				return ExtensionOperatorParameterTypeMatchesExtendedType(type, type2);
			}
		}
		if (checkStrippedType)
		{
			type = type.StrippedType();
		}
		if (!IsContainingType(type))
		{
			if (IsInInterfaceAndAbstractOrVirtual())
			{
				return IsSelfConstrainedTypeParameter(type);
			}
			return false;
		}
		return true;
	}

	internal static bool ExtensionOperatorParameterTypeMatchesExtendedType(TypeSymbol type, TypeSymbol extendedType)
	{
		return type.Equals(extendedType, TypeCompareKind.IgnoreTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes);
	}

	private bool IsContainingType(TypeSymbol type)
	{
		return type.Equals(ContainingType, TypeCompareKind.IgnoreTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes);
	}

	public static bool IsSelfConstrainedTypeParameter(TypeSymbol type, NamedTypeSymbol containingType)
	{
		if (type is TypeParameterSymbol typeParameterSymbol && (object)typeParameterSymbol.ContainingSymbol == containingType)
		{
			return typeParameterSymbol.ConstraintTypesNoUseSiteDiagnostics.Any((TypeWithAnnotations typeArgument, NamedTypeSymbol t) => typeArgument.Type.Equals(t, TypeCompareKind.IgnoreTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes), containingType);
		}
		return false;
	}

	private bool IsSelfConstrainedTypeParameter(TypeSymbol type)
	{
		return IsSelfConstrainedTypeParameter(type, ContainingType);
	}

	private void CheckShiftSignature(BindingDiagnosticBag diagnostics)
	{
		if (!MatchesContainingType(GetParameterType(0), checkStrippedType: true))
		{
			diagnostics.Add(IsInInterfaceAndAbstractOrVirtual() ? ErrorCode.ERR_BadAbstractShiftOperatorSignature : (ContainingType.IsExtension ? ErrorCode.ERR_BadExtensionShiftOperatorSignature : ErrorCode.ERR_BadShiftOperatorSignature), GetFirstLocation());
		}
		else if (GetParameterType(1).StrippedType().SpecialType != SpecialType.System_Int32)
		{
			Location firstLocation = GetFirstLocation();
			Binder.CheckFeatureAvailability(firstLocation.SourceTree, MessageID.IDS_FeatureRelaxedShiftOperator, diagnostics, firstLocation);
		}
		CheckReturnIsNotVoid(diagnostics);
	}

	private void CheckBinarySignature(BindingDiagnosticBag diagnostics)
	{
		if (!MatchesContainingType(GetParameterType(0), checkStrippedType: true) && !MatchesContainingType(GetParameterType(1), checkStrippedType: true))
		{
			diagnostics.Add(IsInInterfaceAndAbstractOrVirtual() ? ErrorCode.ERR_BadAbstractBinaryOperatorSignature : (ContainingType.IsExtension ? ErrorCode.ERR_BadExtensionBinaryOperatorSignature : ErrorCode.ERR_BadBinaryOperatorSignature), GetFirstLocation());
		}
		CheckReturnIsNotVoid(diagnostics);
	}

	private void CheckAbstractEqualitySignature(BindingDiagnosticBag diagnostics)
	{
		if (!IsSelfConstrainedTypeParameter(GetParameterType(0).StrippedType()) && !IsSelfConstrainedTypeParameter(GetParameterType(1).StrippedType()))
		{
			diagnostics.Add(ErrorCode.ERR_BadAbstractEqualityOperatorSignature, GetFirstLocation(), ContainingType);
		}
		CheckReturnIsNotVoid(diagnostics);
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
		if ((object)_explicitInterfaceType == null)
		{
			return;
		}
		SyntaxNode syntax = syntaxReferenceOpt.GetSyntax();
		NameSyntax name;
		if (!(syntax is OperatorDeclarationSyntax operatorDeclarationSyntax))
		{
			if (!(syntax is ConversionOperatorDeclarationSyntax conversionOperatorDeclarationSyntax))
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/SourceUserDefinedOperatorSymbolBase.cs", 1028);
			}
			name = conversionOperatorDeclarationSyntax.ExplicitInterfaceSpecifier.Name;
		}
		else
		{
			name = operatorDeclarationSyntax.ExplicitInterfaceSpecifier.Name;
		}
		_explicitInterfaceType.CheckAllConstraints(DeclaringCompilation, conversions, new SourceLocation(name), diagnostics);
	}

	protected sealed override void PartialMethodChecks(BindingDiagnosticBag diagnostics)
	{
	}
}
