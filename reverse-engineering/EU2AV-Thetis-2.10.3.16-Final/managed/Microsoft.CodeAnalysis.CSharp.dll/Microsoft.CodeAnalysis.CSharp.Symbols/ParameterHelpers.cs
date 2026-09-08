using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class ParameterHelpers
{
	internal enum ParameterContext
	{
		Default,
		FunctionPointer,
		Lambda,
		AnonymousMethod,
		ExtensionReceiverParameter
	}

	public static ImmutableArray<SourceParameterSymbol> MakeParameters(Binder withTypeParametersBinder, Symbol owner, BaseParameterListSyntax syntax, out SyntaxToken arglistToken, BindingDiagnosticBag diagnostics, bool allowRefOrOut, bool allowThis, bool addRefReadOnlyModifier)
	{
		return MakeParameters(withTypeParametersBinder, owner, syntax.Parameters, out arglistToken, diagnostics, allowRefOrOut, allowThis, addRefReadOnlyModifier, suppressUseSiteDiagnostics: false, syntax.Parameters.Count - 1, (Binder context, Symbol owner2, TypeWithAnnotations parameterType, ParameterSyntax parameterSyntax, RefKind refKind, int ordinal, SyntaxToken paramsKeyword, SyntaxToken thisKeyword, bool addRefReadOnlyModifier2, ScopedKind scope, BindingDiagnosticBag declarationDiagnostics) => SourceParameterSymbol.Create(context, owner2, parameterType, parameterSyntax, refKind, parameterSyntax.Identifier, ordinal, paramsKeyword.Kind() != SyntaxKind.None, ordinal == 0 && thisKeyword.Kind() != SyntaxKind.None, addRefReadOnlyModifier2, scope, declarationDiagnostics));
	}

	public static SourceParameterSymbol? MakeExtensionReceiverParameter(Binder withTypeParametersBinder, Symbol owner, ParameterListSyntax syntax, BindingDiagnosticBag diagnostics)
	{
		Func<Binder, Symbol, TypeWithAnnotations, ParameterSyntax, RefKind, int, SyntaxToken, SyntaxToken, bool, ScopedKind, BindingDiagnosticBag, SourceParameterSymbol> parameterCreationFunc = (Binder context, Symbol owner2, TypeWithAnnotations parameterType, ParameterSyntax parameterSyntax, RefKind refKind, int ordinal, SyntaxToken paramsKeyword, SyntaxToken thisKeyword, bool addRefReadOnlyModifier, ScopedKind scope, BindingDiagnosticBag declarationDiagnostics) => SourceParameterSymbol.Create(context, owner2, parameterType, parameterSyntax, refKind, parameterSyntax.Identifier, ordinal, hasParamsModifier: false, isExtensionMethodThis: false, addRefReadOnlyModifier, scope, declarationDiagnostics);
		SyntaxToken arglistToken = default(SyntaxToken);
		int firstDefault = -1;
		return MakeParameter(withTypeParametersBinder, owner, syntax.Parameters[0], ref arglistToken, diagnostics, allowRefOrOut: true, allowThis: false, addRefReadOnlyModifier: false, suppressUseSiteDiagnostics: false, syntax.Parameters.Count - 1, parameterCreationFunc, 0, ref firstDefault, ParameterContext.ExtensionReceiverParameter);
	}

	public static ImmutableArray<FunctionPointerParameterSymbol> MakeFunctionPointerParameters(Binder binder, FunctionPointerMethodSymbol owner, SeparatedSyntaxList<FunctionPointerParameterSyntax> parametersList, BindingDiagnosticBag diagnostics, bool suppressUseSiteDiagnostics)
	{
		SyntaxToken arglistToken;
		return MakeParameters(binder, owner, parametersList, out arglistToken, diagnostics, allowRefOrOut: true, allowThis: false, addRefReadOnlyModifier: true, suppressUseSiteDiagnostics, parametersList.Count - 2, delegate(Binder binder2, FunctionPointerMethodSymbol containingSymbol, TypeWithAnnotations parameterType, FunctionPointerParameterSyntax syntax, RefKind refKind, int ordinal, SyntaxToken paramsKeyword, SyntaxToken thisKeyword, bool addRefReadOnlyModifier, ScopedKind scope, BindingDiagnosticBag bindingDiagnosticBag)
		{
			ImmutableArray<CustomModifier> refCustomModifiers = refKind switch
			{
				RefKind.In => CreateInModifiers(binder2, bindingDiagnosticBag, syntax), 
				RefKind.RefReadOnlyParameter => CreateRefReadonlyParameterModifiers(binder2, bindingDiagnosticBag, syntax), 
				RefKind.Out => CreateOutModifiers(binder2, bindingDiagnosticBag, syntax), 
				_ => ImmutableArray<CustomModifier>.Empty, 
			};
			if (parameterType.IsVoidType())
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_NoVoidParameter, syntax.Type.Location);
			}
			return new FunctionPointerParameterSymbol(parameterType, refKind, ordinal, containingSymbol, refCustomModifiers);
		}, parsingFunctionPointer: true);
	}

	private static ImmutableArray<TParameterSymbol> MakeParameters<TParameterSyntax, TParameterSymbol, TOwningSymbol>(Binder withTypeParametersBinder, TOwningSymbol owner, SeparatedSyntaxList<TParameterSyntax> parametersList, out SyntaxToken arglistToken, BindingDiagnosticBag diagnostics, bool allowRefOrOut, bool allowThis, bool addRefReadOnlyModifier, bool suppressUseSiteDiagnostics, int lastIndex, Func<Binder, TOwningSymbol, TypeWithAnnotations, TParameterSyntax, RefKind, int, SyntaxToken, SyntaxToken, bool, ScopedKind, BindingDiagnosticBag, TParameterSymbol> parameterCreationFunc, bool parsingFunctionPointer = false) where TParameterSyntax : BaseParameterSyntax where TParameterSymbol : ParameterSymbol where TOwningSymbol : Symbol
	{
		arglistToken = default(SyntaxToken);
		int num = 0;
		int firstDefault = -1;
		ArrayBuilder<TParameterSymbol> instance = ArrayBuilder<TParameterSymbol>.GetInstance();
		ParameterContext parameterContext = (parsingFunctionPointer ? ParameterContext.FunctionPointer : ParameterContext.Default);
		foreach (TParameterSyntax item in parametersList)
		{
			if (num > lastIndex)
			{
				break;
			}
			TParameterSymbol val = MakeParameter(withTypeParametersBinder, owner, item, ref arglistToken, diagnostics, allowRefOrOut, allowThis, addRefReadOnlyModifier, suppressUseSiteDiagnostics, lastIndex, parameterCreationFunc, num, ref firstDefault, parameterContext);
			if ((object)val != null)
			{
				instance.Add(val);
				num++;
			}
		}
		ImmutableArray<TParameterSymbol> immutableArray = instance.ToImmutableAndFree();
		if (!parsingFunctionPointer)
		{
			MethodSymbol methodSymbol = owner as MethodSymbol;
			ImmutableArray<TypeParameterSymbol> immutableArray2 = methodSymbol?.TypeParameters ?? ImmutableArray<TypeParameterSymbol>.Empty;
			ImmutableArray<ParameterSymbol> parameters = immutableArray.Cast<TParameterSymbol, ParameterSymbol>();
			if (owner.IsExtensionBlockMember())
			{
				immutableArray2 = owner.ContainingType.TypeParameters.Concat(immutableArray2);
				ParameterSymbol extensionParameter = owner.ContainingType.ExtensionParameter;
				if ((object)extensionParameter != null && !(extensionParameter.Name == ""))
				{
					parameters = parameters.Insert(0, extensionParameter);
				}
			}
			bool allowShadowingNames = withTypeParametersBinder.Compilation.IsFeatureEnabled(MessageID.IDS_FeatureNameShadowingInNestedFunctions) && (object)methodSymbol != null && methodSymbol.MethodKind == MethodKind.LocalFunction;
			withTypeParametersBinder.ValidateParameterNameConflicts(immutableArray2, parameters, allowShadowingNames, diagnostics);
		}
		return immutableArray;
	}

	private static TParameterSymbol? MakeParameter<TParameterSyntax, TParameterSymbol, TOwningSymbol>(Binder withTypeParametersBinder, TOwningSymbol owner, TParameterSyntax parameterSyntax, ref SyntaxToken arglistToken, BindingDiagnosticBag diagnostics, bool allowRefOrOut, bool allowThis, bool addRefReadOnlyModifier, bool suppressUseSiteDiagnostics, int lastIndex, Func<Binder, TOwningSymbol, TypeWithAnnotations, TParameterSyntax, RefKind, int, SyntaxToken, SyntaxToken, bool, ScopedKind, BindingDiagnosticBag, TParameterSymbol> parameterCreationFunc, int parameterIndex, ref int firstDefault, ParameterContext parameterContext) where TParameterSyntax : BaseParameterSyntax where TParameterSymbol : ParameterSymbol where TOwningSymbol : Symbol
	{
		CheckParameterModifiers(parameterSyntax, diagnostics, parameterContext);
		bool flag = parameterContext == ParameterContext.ExtensionReceiverParameter;
		RefKind modifiers = GetModifiers(parameterSyntax.Modifiers, flag, out var refnessKeyword, out var paramsKeyword, out var thisKeyword, out var scope);
		if (thisKeyword.Kind() != SyntaxKind.None && !allowThis)
		{
			diagnostics.Add(ErrorCode.ERR_ThisInBadContext, thisKeyword.GetLocation());
		}
		if (parameterSyntax is ParameterSyntax parameterSyntax2)
		{
			if (parameterSyntax2.IsArgList)
			{
				arglistToken = parameterSyntax2.Identifier;
				if ((paramsKeyword.Kind() != SyntaxKind.None || refnessKeyword.Kind() != SyntaxKind.None || thisKeyword.Kind() != SyntaxKind.None) | flag)
				{
					diagnostics.Add(ErrorCode.ERR_IllegalVarArgs, arglistToken.GetLocation());
				}
				if (parameterIndex != lastIndex && !flag)
				{
					diagnostics.Add(ErrorCode.ERR_VarargsLast, parameterSyntax2.GetLocation());
				}
				return null;
			}
			if (parameterSyntax2.Default != null && firstDefault == -1)
			{
				firstDefault = parameterIndex;
			}
		}
		TypeWithAnnotations arg = withTypeParametersBinder.BindType(parameterSyntax.Type, diagnostics, null, suppressUseSiteDiagnostics);
		if (!allowRefOrOut && (modifiers == RefKind.Ref || modifiers == RefKind.Out))
		{
			diagnostics.Add(ErrorCode.ERR_IllegalRefParam, refnessKeyword.GetLocation());
		}
		TParameterSymbol val = parameterCreationFunc(withTypeParametersBinder, owner, arg, parameterSyntax, modifiers, parameterIndex, paramsKeyword, thisKeyword, addRefReadOnlyModifier, scope, diagnostics);
		ReportParameterErrors(owner, parameterSyntax, val.Ordinal, lastIndex, val.IsParams, val.TypeWithAnnotations, val.RefKind, val.ContainingSymbol, thisKeyword, paramsKeyword, firstDefault, diagnostics);
		return val;
	}

	internal static void EnsureRefKindAttributesExist(PEModuleBuilder moduleBuilder, ImmutableArray<ParameterSymbol> parameters)
	{
		EnsureRefKindAttributesExist(moduleBuilder.Compilation, parameters, null, modifyCompilation: false, moduleBuilder);
	}

	internal static void EnsureRefKindAttributesExist(CSharpCompilation? compilation, ImmutableArray<ParameterSymbol> parameters, BindingDiagnosticBag diagnostics, bool modifyCompilation)
	{
		if (compilation != null)
		{
			EnsureRefKindAttributesExist(compilation, parameters, diagnostics, modifyCompilation, null);
		}
	}

	private static void EnsureRefKindAttributesExist(CSharpCompilation compilation, ImmutableArray<ParameterSymbol> parameters, BindingDiagnosticBag? diagnostics, bool modifyCompilation, PEModuleBuilder? moduleBuilder)
	{
		foreach (ParameterSymbol item in parameters)
		{
			if (item.RefKind == RefKind.In)
			{
				if (moduleBuilder != null)
				{
					moduleBuilder.EnsureIsReadOnlyAttributeExists();
				}
				else
				{
					compilation.EnsureIsReadOnlyAttributeExists(diagnostics, GetParameterLocation(item), modifyCompilation);
				}
			}
			else if (item.RefKind == RefKind.RefReadOnlyParameter)
			{
				if (moduleBuilder != null)
				{
					moduleBuilder.EnsureRequiresLocationAttributeExists();
				}
				else
				{
					compilation.EnsureRequiresLocationAttributeExists(diagnostics, GetParameterLocation(item), modifyCompilation);
				}
			}
		}
	}

	internal static void EnsureParamCollectionAttributeExists(PEModuleBuilder moduleBuilder, ImmutableArray<ParameterSymbol> parameters)
	{
		if ((object)parameters.LastOrDefault((ParameterSymbol p) => p.IsParamsCollection) != null)
		{
			moduleBuilder.EnsureParamCollectionAttributeExists(null, null);
		}
	}

	internal static void EnsureParamCollectionAttributeExists(CSharpCompilation compilation, ImmutableArray<ParameterSymbol> parameters, BindingDiagnosticBag diagnostics, bool modifyCompilation)
	{
		ParameterSymbol parameterSymbol = parameters.LastOrDefault((ParameterSymbol p) => p.IsParamsCollection);
		if ((object)parameterSymbol != null)
		{
			compilation.EnsureParamCollectionAttributeExists(diagnostics, GetParameterLocation(parameterSymbol), modifyCompilation);
		}
	}

	internal static void EnsureNativeIntegerAttributeExists(PEModuleBuilder moduleBuilder, ImmutableArray<ParameterSymbol> parameters)
	{
		EnsureNativeIntegerAttributeExists(moduleBuilder.Compilation, parameters, null, modifyCompilation: false, moduleBuilder);
	}

	internal static void EnsureNativeIntegerAttributeExists(CSharpCompilation? compilation, ImmutableArray<ParameterSymbol> parameters, BindingDiagnosticBag diagnostics, bool modifyCompilation)
	{
		if (compilation != null && compilation.ShouldEmitNativeIntegerAttributes())
		{
			EnsureNativeIntegerAttributeExists(compilation, parameters, diagnostics, modifyCompilation, null);
		}
	}

	private static void EnsureNativeIntegerAttributeExists(CSharpCompilation compilation, ImmutableArray<ParameterSymbol> parameters, BindingDiagnosticBag? diagnostics, bool modifyCompilation, PEModuleBuilder? moduleBuilder)
	{
		foreach (ParameterSymbol item in parameters)
		{
			if (item.TypeWithAnnotations.ContainsNativeIntegerWrapperType())
			{
				if (moduleBuilder != null)
				{
					moduleBuilder.EnsureNativeIntegerAttributeExists();
				}
				else
				{
					compilation.EnsureNativeIntegerAttributeExists(diagnostics, GetParameterLocation(item), modifyCompilation);
				}
			}
		}
	}

	internal static bool RequiresScopedRefAttribute(ParameterSymbol parameter)
	{
		ScopedKind effectiveScope = parameter.EffectiveScope;
		if (effectiveScope == ScopedKind.None)
		{
			return false;
		}
		if (IsRefScopedByDefault(parameter))
		{
			return effectiveScope == ScopedKind.ScopedValue;
		}
		return true;
	}

	internal static bool IsRefScopedByDefault(ParameterSymbol parameter)
	{
		return IsRefScopedByDefault(parameter.UseUpdatedEscapeRules, parameter.RefKind);
	}

	internal static bool IsRefScopedByDefault(bool useUpdatedEscapeRules, RefKind refKind)
	{
		if (useUpdatedEscapeRules)
		{
			return refKind == RefKind.Out;
		}
		return false;
	}

	internal static void EnsureScopedRefAttributeExists(PEModuleBuilder moduleBuilder, ImmutableArray<ParameterSymbol> parameters)
	{
		EnsureScopedRefAttributeExists(moduleBuilder.Compilation, parameters, null, modifyCompilation: false, moduleBuilder);
	}

	internal static void EnsureScopedRefAttributeExists(CSharpCompilation? compilation, ImmutableArray<ParameterSymbol> parameters, BindingDiagnosticBag diagnostics, bool modifyCompilation)
	{
		if (compilation != null)
		{
			EnsureScopedRefAttributeExists(compilation, parameters, diagnostics, modifyCompilation, null);
		}
	}

	private static void EnsureScopedRefAttributeExists(CSharpCompilation compilation, ImmutableArray<ParameterSymbol> parameters, BindingDiagnosticBag? diagnostics, bool modifyCompilation, PEModuleBuilder? moduleBuilder)
	{
		foreach (ParameterSymbol item in parameters)
		{
			if (RequiresScopedRefAttribute(item))
			{
				if (moduleBuilder != null)
				{
					moduleBuilder.EnsureScopedRefAttributeExists();
				}
				else
				{
					compilation.EnsureScopedRefAttributeExists(diagnostics, GetParameterLocation(item), modifyCompilation);
				}
			}
		}
	}

	internal static void EnsureNullableAttributeExists(PEModuleBuilder moduleBuilder, Symbol container, ImmutableArray<ParameterSymbol> parameters)
	{
		EnsureNullableAttributeExists(moduleBuilder.Compilation, container, parameters, null, modifyCompilation: false, moduleBuilder);
	}

	internal static void EnsureNullableAttributeExists(CSharpCompilation? compilation, Symbol container, ImmutableArray<ParameterSymbol> parameters, BindingDiagnosticBag? diagnostics, bool modifyCompilation)
	{
		if (compilation != null)
		{
			EnsureNullableAttributeExists(compilation, container, parameters, diagnostics, modifyCompilation, null);
		}
	}

	private static void EnsureNullableAttributeExists(CSharpCompilation compilation, Symbol container, ImmutableArray<ParameterSymbol> parameters, BindingDiagnosticBag? diagnostics, bool modifyCompilation, PEModuleBuilder? moduleBuilder)
	{
		if (parameters.Length <= 0 || !compilation.ShouldEmitNullableAttributes(container))
		{
			return;
		}
		foreach (ParameterSymbol item in parameters)
		{
			if (item.TypeWithAnnotations.NeedsNullableAttribute())
			{
				if (moduleBuilder != null)
				{
					moduleBuilder.EnsureNullableAttributeExists();
				}
				else
				{
					compilation.EnsureNullableAttributeExists(diagnostics, GetParameterLocation(item), modifyCompilation);
				}
			}
		}
	}

	internal static void CheckUnderspecifiedGenericExtension(Symbol extensionMember, ImmutableArray<ParameterSymbol> parameters, BindingDiagnosticBag diagnostics)
	{
		NamedTypeSymbol containingType = extensionMember.ContainingType;
		ParameterSymbol extensionParameter = containingType.ExtensionParameter;
		if ((object)extensionParameter != null)
		{
			NamedTypeSymbol containingType2 = containingType.ContainingType;
			if ((object)containingType2 != null && containingType2.Arity == 0 && containingType.Arity != 0)
			{
				PooledHashSet<TypeParameterSymbol> instance = PooledHashSet<TypeParameterSymbol>.GetInstance();
				reportUnusedExtensionTypeParameters(extensionMember, parameters, diagnostics, containingType, extensionParameter, instance);
				instance.Free();
			}
		}
		static bool collectTypeParameters(TypeSymbol type, PooledHashSet<TypeParameterSymbol> typeParameters, bool ignored)
		{
			if (type is TypeParameterSymbol item)
			{
				typeParameters.Add(item);
			}
			return false;
		}
		static void reportUnusedExtensionTypeParameters(Symbol symbol, ImmutableArray<ParameterSymbol> immutableArray, BindingDiagnosticBag bindingDiagnosticBag, NamedTypeSymbol extension, ParameterSymbol parameterSymbol, PooledHashSet<TypeParameterSymbol> usedTypeParameters)
		{
			int arity = extension.Arity;
			int memberArity = symbol.GetMemberArity();
			parameterSymbol.Type.VisitType(collectTypeParameters, usedTypeParameters);
			if (usedTypeParameters.Count != arity || memberArity != 0)
			{
				foreach (ParameterSymbol item2 in immutableArray)
				{
					item2.Type.VisitType(collectTypeParameters, usedTypeParameters);
					if (usedTypeParameters.Count == arity && memberArity == 0)
					{
						return;
					}
				}
				foreach (TypeParameterSymbol typeParameter in extension.TypeParameters)
				{
					if (!usedTypeParameters.Contains(typeParameter))
					{
						bindingDiagnosticBag.Add(ErrorCode.ERR_UnderspecifiedExtension, symbol.GetFirstLocation(), typeParameter);
					}
				}
			}
		}
	}

	internal static Location GetParameterLocation(ParameterSymbol parameter)
	{
		return parameter.GetNonNullSyntaxNode().Location;
	}

	internal static void CheckParameterModifiers(BaseParameterSyntax parameter, BindingDiagnosticBag diagnostics, ParameterContext parameterContext)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = false;
		SyntaxToken? syntaxToken = null;
		int i = 0;
		SyntaxToken syntaxToken2;
		for (int count = parameter.Modifiers.Count; i < count; syntaxToken = syntaxToken2, i++)
		{
			syntaxToken2 = parameter.Modifiers[i];
			switch (syntaxToken2.Kind())
			{
			case SyntaxKind.ThisKeyword:
				Binder.CheckFeatureAvailability(syntaxToken2, MessageID.IDS_FeatureExtensionMethod, diagnostics);
				if (flag2 | flag5)
				{
					Binder.CheckFeatureAvailability(syntaxToken2, MessageID.IDS_FeatureRefExtensionMethods, diagnostics);
				}
				if ((uint)(parameterContext - 2) <= 1u)
				{
					diagnostics.Add(ErrorCode.ERR_ThisInBadContext, syntaxToken2.GetLocation());
				}
				else if (flag)
				{
					addERR_DupParamMod(diagnostics, syntaxToken2);
				}
				else if (flag3)
				{
					addERR_BadParameterModifiers(diagnostics, syntaxToken2, SyntaxKind.OutKeyword);
				}
				else if (flag4)
				{
					diagnostics.Add(ErrorCode.ERR_BadParamModThis, syntaxToken2.GetLocation());
				}
				else
				{
					flag = true;
				}
				continue;
			case SyntaxKind.RefKeyword:
				if (flag)
				{
					Binder.CheckFeatureAvailability(syntaxToken2, MessageID.IDS_FeatureRefExtensionMethods, diagnostics);
				}
				if (flag2)
				{
					addERR_DupParamMod(diagnostics, syntaxToken2);
				}
				else if (flag4)
				{
					addERR_ParamsCantBeWithModifier(diagnostics, syntaxToken2, SyntaxKind.RefKeyword);
				}
				else if (flag3)
				{
					addERR_BadParameterModifiers(diagnostics, syntaxToken2, SyntaxKind.OutKeyword);
				}
				else if (flag5)
				{
					addERR_BadParameterModifiers(diagnostics, syntaxToken2, SyntaxKind.InKeyword);
				}
				else
				{
					flag2 = true;
				}
				continue;
			case SyntaxKind.OutKeyword:
				if (flag3)
				{
					addERR_DupParamMod(diagnostics, syntaxToken2);
				}
				else if (flag)
				{
					addERR_BadParameterModifiers(diagnostics, syntaxToken2, SyntaxKind.ThisKeyword);
				}
				else if (parameterContext == ParameterContext.ExtensionReceiverParameter)
				{
					addERR_BadParameterModifiers(diagnostics, syntaxToken2, SyntaxKind.ExtensionKeyword);
				}
				else if (flag4)
				{
					addERR_ParamsCantBeWithModifier(diagnostics, syntaxToken2, SyntaxKind.OutKeyword);
				}
				else if (flag2)
				{
					addERR_BadParameterModifiers(diagnostics, syntaxToken2, SyntaxKind.RefKeyword);
				}
				else if (flag5)
				{
					addERR_BadParameterModifiers(diagnostics, syntaxToken2, SyntaxKind.InKeyword);
				}
				else
				{
					flag3 = true;
				}
				continue;
			case SyntaxKind.ParamsKeyword:
				if (parameterContext != ParameterContext.FunctionPointer)
				{
					if ((uint)(parameterContext - 3) <= 1u)
					{
						diagnostics.Add(ErrorCode.ERR_IllegalParams, syntaxToken2.GetLocation());
					}
					else if (flag4)
					{
						addERR_DupParamMod(diagnostics, syntaxToken2);
					}
					else if (flag)
					{
						diagnostics.Add(ErrorCode.ERR_BadParamModThis, syntaxToken2.GetLocation());
					}
					else if (flag2)
					{
						addERR_BadParameterModifiers(diagnostics, syntaxToken2, SyntaxKind.RefKeyword);
					}
					else if (flag5)
					{
						addERR_BadParameterModifiers(diagnostics, syntaxToken2, SyntaxKind.InKeyword);
					}
					else if (flag3)
					{
						addERR_BadParameterModifiers(diagnostics, syntaxToken2, SyntaxKind.OutKeyword);
					}
					else
					{
						flag4 = true;
					}
					if (parameterContext == ParameterContext.Lambda)
					{
						MessageID.IDS_FeatureLambdaParamsArray.CheckFeatureAvailability(diagnostics, syntaxToken2);
						if (parameter is ParameterSyntax { Type: null, Identifier: var identifier })
						{
							string text = identifier.Text;
							diagnostics.Add(ErrorCode.ERR_ImplicitlyTypedParamsParameter, syntaxToken2, text);
						}
					}
					continue;
				}
				if (parameterContext != ParameterContext.FunctionPointer)
				{
					break;
				}
				goto IL_04a7;
			case SyntaxKind.InKeyword:
				Binder.CheckFeatureAvailability(syntaxToken2, MessageID.IDS_FeatureReadOnlyReferences, diagnostics);
				if (flag)
				{
					Binder.CheckFeatureAvailability(syntaxToken2, MessageID.IDS_FeatureRefExtensionMethods, diagnostics);
				}
				if (flag5)
				{
					addERR_DupParamMod(diagnostics, syntaxToken2);
				}
				else if (flag3)
				{
					addERR_BadParameterModifiers(diagnostics, syntaxToken2, SyntaxKind.OutKeyword);
				}
				else if (flag2)
				{
					addERR_BadParameterModifiers(diagnostics, syntaxToken2, SyntaxKind.RefKeyword);
				}
				else if (flag4)
				{
					addERR_ParamsCantBeWithModifier(diagnostics, syntaxToken2, SyntaxKind.InKeyword);
				}
				else
				{
					flag5 = true;
				}
				continue;
			case SyntaxKind.ScopedKeyword:
				if (parameterContext != ParameterContext.FunctionPointer)
				{
					ModifierUtils.CheckScopedModifierAvailability(parameter, syntaxToken2, diagnostics);
					if (flag6)
					{
						addERR_DupParamMod(diagnostics, syntaxToken2);
					}
					else if (flag5 | flag3 | flag2 | flag7)
					{
						diagnostics.Add(ErrorCode.ERR_ScopedAfterInOutRefReadonly, syntaxToken2.GetLocation());
					}
					else if (i < count - 1)
					{
						SyntaxToken token = parameter.Modifiers[i + 1];
						SyntaxKind syntaxKind = token.Kind();
						if ((syntaxKind - 8360 > (SyntaxKind)2 && syntaxKind != SyntaxKind.ScopedKeyword) || 1 == 0)
						{
							diagnostics.Add(ErrorCode.ERR_InvalidModifierAfterScoped, token.GetLocation(), token.Text);
						}
					}
					flag6 = true;
					continue;
				}
				if (parameterContext != ParameterContext.FunctionPointer)
				{
					break;
				}
				goto IL_04a7;
			case SyntaxKind.ReadOnlyKeyword:
				{
					if (flag7)
					{
						addERR_DupParamMod(diagnostics, syntaxToken2);
					}
					else if (!syntaxToken.HasValue || syntaxToken.GetValueOrDefault().Kind() != SyntaxKind.RefKeyword)
					{
						diagnostics.Add(ErrorCode.ERR_RefReadOnlyWrongOrdering, syntaxToken2);
					}
					else if (flag2)
					{
						Binder.CheckFeatureAvailability(syntaxToken2, MessageID.IDS_FeatureRefReadonlyParameters, diagnostics);
						flag7 = true;
					}
					continue;
				}
				IL_04a7:
				diagnostics.Add(ErrorCode.ERR_BadFuncPointerParamModifier, syntaxToken2.GetLocation(), SyntaxFacts.GetText(syntaxToken2.Kind()));
				continue;
			}
			throw ExceptionUtilities.UnexpectedValue(syntaxToken2.Kind());
		}
		static void addERR_BadParameterModifiers(BindingDiagnosticBag bindingDiagnosticBag, SyntaxToken modifier, SyntaxKind otherModifierKind)
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_BadParameterModifiers, modifier.GetLocation(), SyntaxFacts.GetText(modifier.Kind()), SyntaxFacts.GetText(otherModifierKind));
		}
		static void addERR_DupParamMod(BindingDiagnosticBag bindingDiagnosticBag, SyntaxToken modifier)
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_DupParamMod, modifier.GetLocation(), SyntaxFacts.GetText(modifier.Kind()));
		}
		static void addERR_ParamsCantBeWithModifier(BindingDiagnosticBag bindingDiagnosticBag, SyntaxToken modifier, SyntaxKind otherModifierKind)
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_ParamsCantBeWithModifier, modifier.GetLocation(), SyntaxFacts.GetText(otherModifierKind));
		}
	}

	public static void ReportParameterErrors(Symbol? owner, BaseParameterSyntax syntax, int ordinal, int lastParameterIndex, bool isParams, TypeWithAnnotations typeWithAnnotations, RefKind refKind, Symbol? containingSymbol, SyntaxToken thisKeyword, SyntaxToken paramsKeyword, int firstDefault, BindingDiagnosticBag diagnostics)
	{
		bool flag = syntax is ParameterSyntax parameterSyntax && parameterSyntax.Default != null;
		if (thisKeyword.Kind() == SyntaxKind.ThisKeyword && ordinal != 0 && ((object)owner == null || !owner.IsExtensionBlockMember()))
		{
			diagnostics.Add(ErrorCode.ERR_BadThisParam, thisKeyword.GetLocation(), owner?.Name ?? "");
		}
		else if (isParams && (object)owner != null && owner.IsOperator())
		{
			diagnostics.Add(ErrorCode.ERR_IllegalParams, paramsKeyword.GetLocation());
		}
		else if (!typeWithAnnotations.IsDefault && typeWithAnnotations.IsStatic)
		{
			bool num = owner is SynthesizedExtensionMarker;
			bool flag2 = syntax is ParameterSyntax parameterSyntax2 && parameterSyntax2.Identifier.Kind() != SyntaxKind.None;
			if (!num | flag2)
			{
				diagnostics.Add(ErrorFacts.GetStaticClassParameterCode(containingSymbol?.ContainingType?.IsInterfaceType() == true), syntax.Type?.Location ?? syntax.GetLocation(), typeWithAnnotations.Type);
			}
		}
		else if (firstDefault != -1 && ordinal > firstDefault && !flag && !isParams)
		{
			Location location = ((ParameterSyntax)syntax).Identifier.GetNextToken(includeZeroWidth: true).GetLocation();
			diagnostics.Add(ErrorCode.ERR_DefaultValueBeforeRequiredValue, location);
		}
		else if (refKind != RefKind.None && !typeWithAnnotations.IsDefault && typeWithAnnotations.IsRestrictedType(ignoreSpanLikeTypes: true))
		{
			diagnostics.Add(ErrorCode.ERR_MethodArgCantBeRefAny, syntax.Location, typeWithAnnotations.Type);
		}
		if (isParams && ordinal != lastParameterIndex)
		{
			diagnostics.Add(ErrorCode.ERR_ParamsLast, syntax.GetLocation());
		}
	}

	internal static bool ReportDefaultParameterErrors(Binder binder, Symbol owner, ParameterSyntax parameterSyntax, SourceParameterSymbol parameter, BoundExpression defaultExpression, BoundExpression convertedExpression, BindingDiagnosticBag diagnostics)
	{
		bool result = false;
		bool flag = parameter.ContainingSymbol is SynthesizedExtensionMarker;
		if (flag)
		{
			diagnostics.Add(ErrorCode.ERR_ExtensionParameterDisallowsDefaultValue, parameterSyntax.GetLocation());
			return true;
		}
		TypeSymbol type = parameter.Type;
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = binder.GetNewCompoundUseSiteInfo(diagnostics);
		Conversion conversion = binder.Conversions.ClassifyImplicitConversionFromExpression(defaultExpression, type, ref useSiteInfo);
		diagnostics.Add(defaultExpression.Syntax, useSiteInfo);
		RefKind modifiers = GetModifiers(parameterSyntax.Modifiers, flag, out var refnessKeyword, out var paramsKeyword, out var thisKeyword, out var _);
		if (modifiers == RefKind.Ref || modifiers == RefKind.Out)
		{
			diagnostics.Add(ErrorCode.ERR_RefOutDefaultValue, refnessKeyword.GetLocation());
			result = true;
		}
		else if (paramsKeyword.Kind() == SyntaxKind.ParamsKeyword)
		{
			diagnostics.Add(ErrorCode.ERR_DefaultValueForParamsParameter, paramsKeyword.GetLocation());
			result = true;
		}
		else if (thisKeyword.Kind() == SyntaxKind.ThisKeyword)
		{
			if (parameter.Ordinal == 0 && !parameter.ContainingSymbol.IsExtensionBlockMember())
			{
				diagnostics.Add(ErrorCode.ERR_DefaultValueForExtensionParameter, thisKeyword.GetLocation());
				result = true;
			}
		}
		else if (!defaultExpression.HasAnyErrors && !IsValidDefaultValue(defaultExpression.IsImplicitObjectCreation() ? convertedExpression : defaultExpression))
		{
			diagnostics.Add(ErrorCode.ERR_DefaultValueMustBeConstant, parameterSyntax.Default.Value.Location, parameterSyntax.Identifier.ValueText);
			result = true;
		}
		else if (!conversion.Exists || conversion.IsUserDefined || (conversion.IsIdentity && type.SpecialType == SpecialType.System_Object && defaultExpression.Type.IsDynamic()))
		{
			diagnostics.Add(ErrorCode.ERR_NoConversionForDefaultParam, parameterSyntax.Identifier.GetLocation(), defaultExpression.Display, type);
			result = true;
		}
		else if ((conversion.IsReference && (object)defaultExpression.Type != null && defaultExpression.Type.SpecialType == SpecialType.System_String) || conversion.IsBoxing)
		{
			diagnostics.Add(ErrorCode.ERR_NotNullRefDefaultParameter, parameterSyntax.Identifier.GetLocation(), parameterSyntax.Identifier.ValueText, type);
			result = true;
		}
		else if (((conversion.IsNullable && !defaultExpression.Type.IsNullableType()) || (conversion.IsObjectCreation && convertedExpression.Type.IsNullableType())) && !type.GetNullableUnderlyingType().IsEnumType() && !type.GetNullableUnderlyingType().IsIntrinsicType())
		{
			diagnostics.Add(ErrorCode.ERR_NoConversionForNubDefaultParam, parameterSyntax.Identifier.GetLocation(), defaultExpression.IsImplicitObjectCreation() ? convertedExpression.Type.StrippedType() : defaultExpression.Type, parameterSyntax.Identifier.ValueText);
			result = true;
		}
		ConstantValueUtils.CheckLangVersionForConstantValue(convertedExpression, diagnostics);
		if (owner.IsExplicitInterfaceImplementation() || owner.IsPartialImplementation() || owner.IsOperator())
		{
			diagnostics.Add(ErrorCode.WRN_DefaultValueForUnconsumedLocation, parameterSyntax.Identifier.GetLocation(), parameterSyntax.Identifier.ValueText);
		}
		if (modifiers == RefKind.RefReadOnlyParameter)
		{
			diagnostics.Add(ErrorCode.WRN_RefReadonlyParameterDefaultValue, parameterSyntax.Default.Value, parameterSyntax.Identifier.ValueText);
		}
		return result;
	}

	private static bool IsValidDefaultValue(BoundExpression expression)
	{
		if (expression.ConstantValueOpt != null)
		{
			return true;
		}
		switch (expression.Kind)
		{
		case BoundKind.DefaultLiteral:
		case BoundKind.DefaultExpression:
			return true;
		case BoundKind.ObjectCreationExpression:
			return IsValidDefaultValue((BoundObjectCreationExpression)expression);
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)expression;
			if (boundConversion != null && boundConversion.Conversion.IsObjectCreation && boundConversion.Operand is BoundObjectCreationExpression { WasTargetTyped: not false } boundObjectCreationExpression)
			{
				return IsValidDefaultValue(boundObjectCreationExpression);
			}
			return false;
		}
		default:
			return false;
		}
	}

	private static bool IsValidDefaultValue(BoundObjectCreationExpression expression)
	{
		if (expression.Constructor.IsDefaultValueTypeConstructor())
		{
			return expression.InitializerExpressionOpt == null;
		}
		return false;
	}

	internal static MethodSymbol FindContainingGenericMethod(Symbol symbol)
	{
		Symbol symbol2 = symbol;
		while ((object)symbol2 != null)
		{
			if (symbol2.Kind == SymbolKind.Method)
			{
				MethodSymbol methodSymbol = (MethodSymbol)symbol2;
				if (methodSymbol.MethodKind != MethodKind.AnonymousFunction)
				{
					if (!methodSymbol.IsGenericMethod)
					{
						return null;
					}
					return methodSymbol;
				}
			}
			symbol2 = symbol2.ContainingSymbol;
		}
		return null;
	}

	internal static RefKind GetModifiers(SyntaxTokenList modifiers, bool ignoreParams, out SyntaxToken refnessKeyword, out SyntaxToken paramsKeyword, out SyntaxToken thisKeyword, out ScopedKind scope)
	{
		RefKind refKind = RefKind.None;
		bool flag = false;
		refnessKeyword = default(SyntaxToken);
		paramsKeyword = default(SyntaxToken);
		thisKeyword = default(SyntaxToken);
		foreach (SyntaxToken item in modifiers)
		{
			switch (item.Kind())
			{
			case SyntaxKind.OutKeyword:
				if (refKind == RefKind.None)
				{
					refnessKeyword = item;
					refKind = RefKind.Out;
				}
				break;
			case SyntaxKind.RefKeyword:
				if (refKind == RefKind.None)
				{
					refnessKeyword = item;
					refKind = RefKind.Ref;
				}
				break;
			case SyntaxKind.InKeyword:
				if (refKind == RefKind.None)
				{
					refnessKeyword = item;
					refKind = RefKind.In;
				}
				break;
			case SyntaxKind.ParamsKeyword:
				if (!ignoreParams)
				{
					paramsKeyword = item;
				}
				break;
			case SyntaxKind.ThisKeyword:
				thisKeyword = item;
				break;
			case SyntaxKind.ScopedKeyword:
				flag = true;
				break;
			case SyntaxKind.ReadOnlyKeyword:
				if (refKind == RefKind.Ref && refnessKeyword.GetNextToken() == item)
				{
					refKind = RefKind.RefReadOnlyParameter;
				}
				break;
			}
		}
		if (flag)
		{
			scope = ((refKind != RefKind.None) ? ScopedKind.ScopedRef : ScopedKind.ScopedValue);
		}
		else
		{
			scope = ScopedKind.None;
		}
		return refKind;
	}

	internal static ImmutableArray<CustomModifier> ConditionallyCreateInModifiers(RefKind refKind, bool addRefReadOnlyModifier, Binder binder, BindingDiagnosticBag diagnostics, SyntaxNode syntax)
	{
		bool flag = addRefReadOnlyModifier;
		if (flag)
		{
			bool flag2 = refKind - 3 <= RefKind.Ref;
			flag = flag2;
		}
		if (flag)
		{
			return CreateInModifiers(binder, diagnostics, syntax);
		}
		return ImmutableArray<CustomModifier>.Empty;
	}

	internal static ImmutableArray<CustomModifier> CreateInModifiers(Binder binder, BindingDiagnosticBag diagnostics, SyntaxNode syntax)
	{
		return CreateModifiers(WellKnownType.System_Runtime_InteropServices_InAttribute, binder, diagnostics, syntax);
	}

	private static ImmutableArray<CustomModifier> CreateRefReadonlyParameterModifiers(Binder binder, BindingDiagnosticBag diagnostics, SyntaxNode syntax)
	{
		return ImmutableArray.Create(CSharpCustomModifier.CreateOptional(binder.GetWellKnownType(WellKnownType.System_Runtime_CompilerServices_RequiresLocationAttribute, diagnostics, syntax)));
	}

	internal static ImmutableArray<CustomModifier> CreateOutModifiers(Binder binder, BindingDiagnosticBag diagnostics, SyntaxNode syntax)
	{
		return CreateModifiers(WellKnownType.System_Runtime_InteropServices_OutAttribute, binder, diagnostics, syntax);
	}

	private static ImmutableArray<CustomModifier> CreateModifiers(WellKnownType modifier, Binder binder, BindingDiagnosticBag diagnostics, SyntaxNode syntax)
	{
		return ImmutableArray.Create(CSharpCustomModifier.CreateRequired(binder.GetWellKnownType(modifier, diagnostics, syntax)));
	}
}
