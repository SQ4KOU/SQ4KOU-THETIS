using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class ModifierUtils
{
	internal static DeclarationModifiers MakeAndCheckNonTypeMemberModifiers(bool isOrdinaryMethod, bool isForInterfaceMember, SyntaxTokenList modifiers, DeclarationModifiers defaultAccess, DeclarationModifiers allowedModifiers, Location errorLocation, BindingDiagnosticBag diagnostics, out bool modifierErrors, out bool hasExplicitAccessModifier)
	{
		DeclarationModifiers modifiers2 = modifiers.ToDeclarationModifiers(isForTypeDeclaration: false, diagnostics.DiagnosticBag ?? new DiagnosticBag(), isOrdinaryMethod);
		modifiers2 = CheckModifiers(isForTypeDeclaration: false, isForInterfaceMember, modifiers2, allowedModifiers, errorLocation, diagnostics, modifiers, out modifierErrors);
		SyntaxToken syntax = modifiers.FirstOrDefault(SyntaxKind.ReadOnlyKeyword);
		SyntaxNode parent = syntax.Parent;
		if ((parent is MethodDeclarationSyntax || parent is AccessorDeclarationSyntax || parent is BasePropertyDeclarationSyntax) ? true : false)
		{
			modifierErrors |= !MessageID.IDS_FeatureReadOnlyMembers.CheckFeatureAvailability(diagnostics, syntax);
		}
		hasExplicitAccessModifier = (modifiers2 & DeclarationModifiers.AccessibilityMask) != 0;
		if (!hasExplicitAccessModifier)
		{
			modifiers2 |= defaultAccess;
		}
		return modifiers2;
	}

	internal static DeclarationModifiers CheckModifiers(bool isForTypeDeclaration, bool isForInterfaceMember, DeclarationModifiers modifiers, DeclarationModifiers allowedModifiers, Location errorLocation, BindingDiagnosticBag diagnostics, SyntaxTokenList? modifierTokens, out bool modifierErrors)
	{
		modifierErrors = false;
		DeclarationModifiers declarationModifiers = DeclarationModifiers.None;
		if (!isForTypeDeclaration && (modifiers & allowedModifiers & DeclarationModifiers.Static) != DeclarationModifiers.None)
		{
			declarationModifiers = ((!isForInterfaceMember) ? (allowedModifiers & (DeclarationModifiers.Abstract | DeclarationModifiers.Virtual | DeclarationModifiers.Override)) : (allowedModifiers & DeclarationModifiers.Override));
			allowedModifiers &= ~declarationModifiers;
		}
		DeclarationModifiers declarationModifiers2 = modifiers & ~allowedModifiers;
		DeclarationModifiers result = modifiers & allowedModifiers;
		for (; declarationModifiers2 != DeclarationModifiers.None; modifierErrors = true)
		{
			DeclarationModifiers declarationModifiers3 = declarationModifiers2 & ~(declarationModifiers2 - 1);
			declarationModifiers2 &= ~declarationModifiers3;
			switch (declarationModifiers3)
			{
			case DeclarationModifiers.Partial:
				ReportPartialError(errorLocation, diagnostics, modifierTokens);
				continue;
			case DeclarationModifiers.Abstract:
			case DeclarationModifiers.Virtual:
			case DeclarationModifiers.Override:
				if ((declarationModifiers & declarationModifiers3) != DeclarationModifiers.None)
				{
					diagnostics.Add(ErrorCode.ERR_StaticNotVirtual, errorLocation, ConvertSingleModifierToSyntaxText(declarationModifiers3));
					continue;
				}
				break;
			}
			diagnostics.Add(ErrorCode.ERR_BadMemberFlag, errorLocation, ConvertSingleModifierToSyntaxText(declarationModifiers3));
		}
		modifierErrors |= checkFeature(DeclarationModifiers.PrivateProtected, MessageID.IDS_FeaturePrivateProtected) | checkFeature(DeclarationModifiers.Required, MessageID.IDS_FeatureRequiredMembers) | checkFeature(DeclarationModifiers.File, MessageID.IDS_FeatureFileTypes) | checkFeature(DeclarationModifiers.Async, MessageID.IDS_FeatureAsync);
		return result;
		bool checkFeature(DeclarationModifiers modifier, MessageID featureID)
		{
			if ((result & modifier) != DeclarationModifiers.None)
			{
				return !Binder.CheckFeatureAvailability(errorLocation.SourceTree, featureID, diagnostics, errorLocation);
			}
			return false;
		}
	}

	internal static void CheckScopedModifierAvailability(CSharpSyntaxNode syntax, SyntaxToken modifier, BindingDiagnosticBag diagnostics)
	{
		CSDiagnosticInfo featureAvailabilityDiagnosticInfo = MessageID.IDS_FeatureRefFields.GetFeatureAvailabilityDiagnosticInfo((CSharpParseOptions)syntax.SyntaxTree.Options);
		if (featureAvailabilityDiagnosticInfo != null)
		{
			diagnostics.Add(featureAvailabilityDiagnosticInfo, modifier.GetLocation());
		}
	}

	private static void ReportPartialError(Location errorLocation, BindingDiagnosticBag diagnostics, SyntaxTokenList? modifierTokens)
	{
		if (modifierTokens.HasValue)
		{
			SyntaxToken syntaxToken = modifierTokens.Value.FirstOrDefault(SyntaxKind.PartialKeyword);
			if (syntaxToken != default(SyntaxToken))
			{
				diagnostics.Add(ErrorCode.ERR_PartialMisplaced, syntaxToken.GetLocation());
				return;
			}
		}
		diagnostics.Add(ErrorCode.ERR_PartialMisplaced, errorLocation);
	}

	internal static void ReportDefaultInterfaceImplementationModifiers(bool hasBody, DeclarationModifiers modifiers, DeclarationModifiers defaultInterfaceImplementationModifiers, Location errorLocation, BindingDiagnosticBag diagnostics)
	{
		if ((modifiers & defaultInterfaceImplementationModifiers) == 0)
		{
			return;
		}
		LanguageVersion languageVersion = ((CSharpParseOptions)errorLocation.SourceTree.Options).LanguageVersion;
		if ((modifiers & defaultInterfaceImplementationModifiers & DeclarationModifiers.Static) != DeclarationModifiers.None && (modifiers & defaultInterfaceImplementationModifiers & (DeclarationModifiers.Abstract | DeclarationModifiers.Sealed | DeclarationModifiers.Virtual)) != DeclarationModifiers.None)
		{
			DeclarationModifiers declarationModifiers = DeclarationModifiers.Abstract | DeclarationModifiers.Sealed | DeclarationModifiers.Virtual;
			if ((modifiers & defaultInterfaceImplementationModifiers & DeclarationModifiers.Sealed) != DeclarationModifiers.None && (modifiers & defaultInterfaceImplementationModifiers & (DeclarationModifiers.Abstract | DeclarationModifiers.Virtual)) != DeclarationModifiers.None)
			{
				diagnostics.Add(ErrorCode.ERR_BadMemberFlag, errorLocation, ConvertSingleModifierToSyntaxText(DeclarationModifiers.Sealed));
				declarationModifiers = (DeclarationModifiers)((uint)declarationModifiers & 0xFFFFFFFDu);
			}
			LanguageVersion languageVersion2 = MessageID.IDS_FeatureStaticAbstractMembersInInterfaces.RequiredVersion();
			if (languageVersion < languageVersion2)
			{
				ReportUnsupportedModifiersForLanguageVersion(modifiers, declarationModifiers, errorLocation, diagnostics, languageVersion, languageVersion2);
			}
		}
		else if (hasBody)
		{
			if ((modifiers & defaultInterfaceImplementationModifiers & DeclarationModifiers.Static) != DeclarationModifiers.None)
			{
				Binder.CheckFeatureAvailability(errorLocation.SourceTree, MessageID.IDS_DefaultInterfaceImplementation, diagnostics, errorLocation);
			}
		}
		else
		{
			LanguageVersion languageVersion2 = MessageID.IDS_DefaultInterfaceImplementation.RequiredVersion();
			if (languageVersion < languageVersion2)
			{
				ReportUnsupportedModifiersForLanguageVersion(modifiers, defaultInterfaceImplementationModifiers, errorLocation, diagnostics, languageVersion, languageVersion2);
			}
		}
	}

	internal static void ReportUnsupportedModifiersForLanguageVersion(DeclarationModifiers modifiers, DeclarationModifiers unsupportedModifiers, Location errorLocation, BindingDiagnosticBag diagnostics, LanguageVersion availableVersion, LanguageVersion requiredVersion)
	{
		DeclarationModifiers declarationModifiers = modifiers & unsupportedModifiers;
		CSharpRequiredLanguageVersion cSharpRequiredLanguageVersion = new CSharpRequiredLanguageVersion(requiredVersion);
		string text = availableVersion.ToDisplayString();
		while (declarationModifiers != DeclarationModifiers.None)
		{
			DeclarationModifiers declarationModifiers2 = declarationModifiers & ~(declarationModifiers - 1);
			declarationModifiers &= ~declarationModifiers2;
			diagnostics.Add(ErrorCode.ERR_InvalidModifierForLanguageVersion, errorLocation, ConvertSingleModifierToSyntaxText(declarationModifiers2), text, cSharpRequiredLanguageVersion);
		}
	}

	internal static void CheckFeatureAvailabilityForStaticAbstractMembersInInterfacesIfNeeded(DeclarationModifiers mods, bool isExplicitInterfaceImplementation, Location location, BindingDiagnosticBag diagnostics)
	{
		if (isExplicitInterfaceImplementation && (mods & DeclarationModifiers.Static) != DeclarationModifiers.None)
		{
			LanguageVersion languageVersion = ((CSharpParseOptions)location.SourceTree.Options).LanguageVersion;
			LanguageVersion languageVersion2 = MessageID.IDS_FeatureStaticAbstractMembersInInterfaces.RequiredVersion();
			if (languageVersion < languageVersion2)
			{
				ReportUnsupportedModifiersForLanguageVersion(mods, DeclarationModifiers.Static, location, diagnostics, languageVersion, languageVersion2);
			}
		}
	}

	internal static void CheckFeatureAvailabilityForPartialEventsAndConstructors(Location location, BindingDiagnosticBag diagnostics)
	{
		LanguageVersion languageVersion = ((CSharpParseOptions)location.SourceTree.Options).LanguageVersion;
		LanguageVersion languageVersion2 = MessageID.IDS_FeaturePartialEventsAndConstructors.RequiredVersion();
		if (languageVersion < languageVersion2)
		{
			ReportUnsupportedModifiersForLanguageVersion(DeclarationModifiers.Partial, DeclarationModifiers.Partial, location, diagnostics, languageVersion, languageVersion2);
		}
	}

	internal static DeclarationModifiers AdjustModifiersForAnInterfaceMember(DeclarationModifiers mods, bool hasBody, bool isExplicitInterfaceImplementation, bool forMethod)
	{
		bool flag = (mods & DeclarationModifiers.Partial) == 0 || !forMethod;
		if ((mods & DeclarationModifiers.AccessibilityMask) == 0)
		{
			mods = ((!(!isExplicitInterfaceImplementation & flag)) ? (mods | DeclarationModifiers.Private) : (mods | DeclarationModifiers.Public));
		}
		if (isExplicitInterfaceImplementation)
		{
			if ((mods & DeclarationModifiers.Abstract) != DeclarationModifiers.None)
			{
				mods |= DeclarationModifiers.Sealed;
			}
		}
		else if ((mods & DeclarationModifiers.Static) != DeclarationModifiers.None)
		{
			mods = (DeclarationModifiers)((uint)mods & 0xFFFFFFFDu);
		}
		else if (((mods & (DeclarationModifiers.Abstract | DeclarationModifiers.Private | DeclarationModifiers.Virtual)) == 0) & flag)
		{
			mods = ((!hasBody && (mods & (DeclarationModifiers.Sealed | DeclarationModifiers.Extern | DeclarationModifiers.Partial)) == 0) ? (mods | DeclarationModifiers.Abstract) : (((mods & DeclarationModifiers.Sealed) != DeclarationModifiers.None) ? ((DeclarationModifiers)((uint)mods & 0xFFFFFFFDu)) : (mods | DeclarationModifiers.Virtual)));
		}
		return mods;
	}

	internal static string ConvertSingleModifierToSyntaxText(DeclarationModifiers modifier)
	{
		return modifier switch
		{
			DeclarationModifiers.Abstract => SyntaxFacts.GetText(SyntaxKind.AbstractKeyword), 
			DeclarationModifiers.Sealed => SyntaxFacts.GetText(SyntaxKind.SealedKeyword), 
			DeclarationModifiers.Static => SyntaxFacts.GetText(SyntaxKind.StaticKeyword), 
			DeclarationModifiers.New => SyntaxFacts.GetText(SyntaxKind.NewKeyword), 
			DeclarationModifiers.Public => SyntaxFacts.GetText(SyntaxKind.PublicKeyword), 
			DeclarationModifiers.Protected => SyntaxFacts.GetText(SyntaxKind.ProtectedKeyword), 
			DeclarationModifiers.Internal => SyntaxFacts.GetText(SyntaxKind.InternalKeyword), 
			DeclarationModifiers.ProtectedInternal => SyntaxFacts.GetText(SyntaxKind.ProtectedKeyword) + " " + SyntaxFacts.GetText(SyntaxKind.InternalKeyword), 
			DeclarationModifiers.Private => SyntaxFacts.GetText(SyntaxKind.PrivateKeyword), 
			DeclarationModifiers.PrivateProtected => SyntaxFacts.GetText(SyntaxKind.PrivateKeyword) + " " + SyntaxFacts.GetText(SyntaxKind.ProtectedKeyword), 
			DeclarationModifiers.ReadOnly => SyntaxFacts.GetText(SyntaxKind.ReadOnlyKeyword), 
			DeclarationModifiers.Const => SyntaxFacts.GetText(SyntaxKind.ConstKeyword), 
			DeclarationModifiers.Volatile => SyntaxFacts.GetText(SyntaxKind.VolatileKeyword), 
			DeclarationModifiers.Extern => SyntaxFacts.GetText(SyntaxKind.ExternKeyword), 
			DeclarationModifiers.Partial => SyntaxFacts.GetText(SyntaxKind.PartialKeyword), 
			DeclarationModifiers.Unsafe => SyntaxFacts.GetText(SyntaxKind.UnsafeKeyword), 
			DeclarationModifiers.Fixed => SyntaxFacts.GetText(SyntaxKind.FixedKeyword), 
			DeclarationModifiers.Virtual => SyntaxFacts.GetText(SyntaxKind.VirtualKeyword), 
			DeclarationModifiers.Override => SyntaxFacts.GetText(SyntaxKind.OverrideKeyword), 
			DeclarationModifiers.Async => SyntaxFacts.GetText(SyntaxKind.AsyncKeyword), 
			DeclarationModifiers.Ref => SyntaxFacts.GetText(SyntaxKind.RefKeyword), 
			DeclarationModifiers.Required => SyntaxFacts.GetText(SyntaxKind.RequiredKeyword), 
			DeclarationModifiers.Scoped => SyntaxFacts.GetText(SyntaxKind.ScopedKeyword), 
			DeclarationModifiers.File => SyntaxFacts.GetText(SyntaxKind.FileKeyword), 
			_ => throw ExceptionUtilities.UnexpectedValue(modifier), 
		};
	}

	private static DeclarationModifiers ToDeclarationModifier(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.AbstractKeyword => DeclarationModifiers.Abstract, 
			SyntaxKind.AsyncKeyword => DeclarationModifiers.Async, 
			SyntaxKind.SealedKeyword => DeclarationModifiers.Sealed, 
			SyntaxKind.StaticKeyword => DeclarationModifiers.Static, 
			SyntaxKind.NewKeyword => DeclarationModifiers.New, 
			SyntaxKind.PublicKeyword => DeclarationModifiers.Public, 
			SyntaxKind.ProtectedKeyword => DeclarationModifiers.Protected, 
			SyntaxKind.InternalKeyword => DeclarationModifiers.Internal, 
			SyntaxKind.PrivateKeyword => DeclarationModifiers.Private, 
			SyntaxKind.ExternKeyword => DeclarationModifiers.Extern, 
			SyntaxKind.ReadOnlyKeyword => DeclarationModifiers.ReadOnly, 
			SyntaxKind.PartialKeyword => DeclarationModifiers.Partial, 
			SyntaxKind.UnsafeKeyword => DeclarationModifiers.Unsafe, 
			SyntaxKind.VirtualKeyword => DeclarationModifiers.Virtual, 
			SyntaxKind.OverrideKeyword => DeclarationModifiers.Override, 
			SyntaxKind.ConstKeyword => DeclarationModifiers.Const, 
			SyntaxKind.FixedKeyword => DeclarationModifiers.Fixed, 
			SyntaxKind.VolatileKeyword => DeclarationModifiers.Volatile, 
			SyntaxKind.RefKeyword => DeclarationModifiers.Ref, 
			SyntaxKind.RequiredKeyword => DeclarationModifiers.Required, 
			SyntaxKind.ScopedKeyword => DeclarationModifiers.Scoped, 
			SyntaxKind.FileKeyword => DeclarationModifiers.File, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	public static void CheckForDuplicateModifiers(SyntaxTokenList modifiers, DiagnosticBag diagnostics)
	{
		GetDeclarationModifiersAndCheckForDuplicateModifiers(modifiers, diagnostics);
	}

	private static DeclarationModifiers GetDeclarationModifiersAndCheckForDuplicateModifiers(SyntaxTokenList modifiers, DiagnosticBag diagnostics)
	{
		DeclarationModifiers declarationModifiers = DeclarationModifiers.None;
		bool seenNoDuplicates = true;
		foreach (SyntaxToken item in modifiers)
		{
			DeclarationModifiers declarationModifiers2 = ToDeclarationModifier(item.ContextualKind());
			ReportDuplicateModifiers(item, declarationModifiers2, declarationModifiers, ref seenNoDuplicates, diagnostics);
			declarationModifiers |= declarationModifiers2;
		}
		return declarationModifiers;
	}

	public static DeclarationModifiers ToDeclarationModifiers(this SyntaxTokenList modifiers, bool isForTypeDeclaration, DiagnosticBag diagnostics, bool isOrdinaryMethod = false)
	{
		DeclarationModifiers declarationModifiers = GetDeclarationModifiersAndCheckForDuplicateModifiers(modifiers, diagnostics);
		if ((declarationModifiers & DeclarationModifiers.Partial) == DeclarationModifiers.Partial)
		{
			int num = modifiers.IndexOf(SyntaxKind.PartialKeyword);
			SyntaxToken syntax = modifiers[num];
			(isForTypeDeclaration ? MessageID.IDS_FeaturePartialTypes : MessageID.IDS_FeaturePartialMethod).CheckFeatureAvailability(diagnostics, syntax);
			bool num2 = num == modifiers.Count - 1;
			bool flag = isOrdinaryMethod && num == modifiers.Count - 2 && modifiers[num + 1].ContextualKind() == SyntaxKind.AsyncKeyword;
			if (!num2 && !flag)
			{
				diagnostics.Add(ErrorCode.ERR_PartialMisplaced, syntax.GetLocation());
			}
		}
		switch (declarationModifiers & DeclarationModifiers.AccessibilityMask)
		{
		case DeclarationModifiers.Protected | DeclarationModifiers.Internal:
			declarationModifiers = (DeclarationModifiers)((uint)declarationModifiers & 0xFFFFFC0Fu);
			declarationModifiers |= DeclarationModifiers.ProtectedInternal;
			break;
		case DeclarationModifiers.Protected | DeclarationModifiers.Private:
			declarationModifiers = (DeclarationModifiers)((uint)declarationModifiers & 0xFFFFFC0Fu);
			declarationModifiers |= DeclarationModifiers.PrivateProtected;
			break;
		}
		return declarationModifiers;
	}

	private static void ReportDuplicateModifiers(SyntaxToken modifierToken, DeclarationModifiers modifierKind, DeclarationModifiers allModifiers, ref bool seenNoDuplicates, DiagnosticBag diagnostics)
	{
		if ((allModifiers & modifierKind) != DeclarationModifiers.None && seenNoDuplicates)
		{
			diagnostics.Add(ErrorCode.ERR_DuplicateModifier, modifierToken.GetLocation(), SyntaxFacts.GetText(modifierToken.Kind()));
			seenNoDuplicates = false;
		}
	}

	internal static bool CheckAccessibility(DeclarationModifiers modifiers, Symbol symbol, bool isExplicitInterfaceImplementation, BindingDiagnosticBag diagnostics, Location errorLocation)
	{
		if (!IsValidAccessibility(modifiers))
		{
			diagnostics.Add(ErrorCode.ERR_BadMemberProtection, errorLocation);
			return true;
		}
		if (!isExplicitInterfaceImplementation && (symbol.Kind != SymbolKind.Method || (modifiers & DeclarationModifiers.Partial) == 0) && (modifiers & DeclarationModifiers.Static) == 0)
		{
			DeclarationModifiers declarationModifiers = modifiers & DeclarationModifiers.AccessibilityMask;
			if (declarationModifiers == DeclarationModifiers.Protected || declarationModifiers == DeclarationModifiers.ProtectedInternal || declarationModifiers == DeclarationModifiers.PrivateProtected)
			{
				NamedTypeSymbol containingType = symbol.ContainingType;
				if ((object)containingType != null && containingType.IsInterface && !symbol.ContainingAssembly.RuntimeSupportsDefaultInterfaceImplementation)
				{
					diagnostics.Add(ErrorCode.ERR_RuntimeDoesNotSupportProtectedAccessForInterfaceMember, errorLocation);
					return true;
				}
			}
		}
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo;
		bool result;
		if ((modifiers & DeclarationModifiers.Required) != DeclarationModifiers.None)
		{
			useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, symbol.ContainingAssembly);
			result = false;
			if (!(symbol is FieldSymbol))
			{
				if (symbol is PropertySymbol { SetMethod: var setMethod })
				{
					if ((object)setMethod == null)
					{
						goto IL_0110;
					}
					if (!setMethod.IsAsRestrictive(symbol.ContainingType, ref useSiteInfo))
					{
						goto IL_00e2;
					}
				}
			}
			else
			{
				if (!symbol.IsAsRestrictive(symbol.ContainingType, ref useSiteInfo))
				{
					goto IL_00e2;
				}
				if ((modifiers & DeclarationModifiers.ReadOnly) != DeclarationModifiers.None)
				{
					goto IL_0110;
				}
			}
			goto IL_012a;
		}
		return false;
		IL_00e2:
		diagnostics.Add(ErrorCode.ERR_RequiredMemberCannotBeLessVisibleThanContainingType, errorLocation, symbol, symbol.ContainingType);
		result = true;
		goto IL_012a;
		IL_0110:
		diagnostics.Add(ErrorCode.ERR_RequiredMemberMustBeSettable, errorLocation, symbol);
		result = true;
		goto IL_012a;
		IL_012a:
		diagnostics.Add(errorLocation, useSiteInfo);
		return result;
	}

	internal static Accessibility EffectiveAccessibility(DeclarationModifiers modifiers)
	{
		return (modifiers & DeclarationModifiers.AccessibilityMask) switch
		{
			DeclarationModifiers.None => Accessibility.NotApplicable, 
			DeclarationModifiers.Private => Accessibility.Private, 
			DeclarationModifiers.Protected => Accessibility.Protected, 
			DeclarationModifiers.Internal => Accessibility.Internal, 
			DeclarationModifiers.Public => Accessibility.Public, 
			DeclarationModifiers.ProtectedInternal => Accessibility.ProtectedOrInternal, 
			DeclarationModifiers.PrivateProtected => Accessibility.ProtectedAndInternal, 
			_ => Accessibility.Public, 
		};
	}

	internal static bool IsValidAccessibility(DeclarationModifiers modifiers)
	{
		switch (modifiers & DeclarationModifiers.AccessibilityMask)
		{
		case DeclarationModifiers.None:
		case DeclarationModifiers.Public:
		case DeclarationModifiers.Protected:
		case DeclarationModifiers.Internal:
		case DeclarationModifiers.ProtectedInternal:
		case DeclarationModifiers.Private:
		case DeclarationModifiers.PrivateProtected:
			return true;
		default:
			return false;
		}
	}
}
