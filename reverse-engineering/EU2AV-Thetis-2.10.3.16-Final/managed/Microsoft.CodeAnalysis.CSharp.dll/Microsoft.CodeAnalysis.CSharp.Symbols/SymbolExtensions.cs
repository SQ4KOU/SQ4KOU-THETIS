using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class SymbolExtensions
{
	private static readonly Func<TypeSymbol, Symbol, bool, bool> s_hasInvalidTypeParameterFunc = (TypeSymbol type, Symbol containingSymbol, bool unused) => HasInvalidTypeParameter(type, containingSymbol);

	internal static bool HasParamsParameter(this Symbol member)
	{
		ImmutableArray<ParameterSymbol> parameters = member.GetParameters();
		if (!parameters.IsEmpty)
		{
			return parameters.Last().IsParams;
		}
		return false;
	}

	internal static ImmutableArray<ParameterSymbol> GetParameters(this Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).Parameters, 
			SymbolKind.Property => ((PropertySymbol)member).Parameters, 
			SymbolKind.Event => ImmutableArray<ParameterSymbol>.Empty, 
			_ => throw ExceptionUtilities.UnexpectedValue(member.Kind), 
		};
	}

	internal static ImmutableArray<TypeWithAnnotations> GetParameterTypes(this Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).ParameterTypesWithAnnotations, 
			SymbolKind.Property => ((PropertySymbol)member).ParameterTypesWithAnnotations, 
			SymbolKind.Event => ImmutableArray<TypeWithAnnotations>.Empty, 
			_ => throw ExceptionUtilities.UnexpectedValue(member.Kind), 
		};
	}

	internal static bool GetIsVararg(this Symbol member)
	{
		switch (member.Kind)
		{
		case SymbolKind.Method:
			return ((MethodSymbol)member).IsVararg;
		case SymbolKind.Event:
		case SymbolKind.Property:
			return false;
		default:
			throw ExceptionUtilities.UnexpectedValue(member.Kind);
		}
	}

	internal static bool IsExtensionBlockMember(this Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).IsExtensionBlockMember(), 
			SymbolKind.Property => ((PropertySymbol)member).IsExtensionBlockMember(), 
			_ => false, 
		};
	}

	internal static bool IsExtensionBlockMember(this MethodSymbol member)
	{
		if ((object)member != null && member.ContainingSymbol is NamedTypeSymbol { IsExtension: not false })
		{
			return !(member.OriginalDefinition is SynthesizedExtensionMarker);
		}
		return false;
	}

	internal static bool IsExtensionBlockMember(this PropertySymbol member)
	{
		if (member.ContainingSymbol is NamedTypeSymbol namedTypeSymbol)
		{
			return namedTypeSymbol.IsExtension;
		}
		return false;
	}

	internal static bool TryGetInstanceExtensionParameter(this Symbol symbol, [NotNullWhen(true)] out ParameterSymbol? extensionParameter)
	{
		if ((object)symbol != null && symbol.IsExtensionBlockMember() && !symbol.IsStatic)
		{
			ParameterSymbol extensionParameter2 = symbol.ContainingType.ExtensionParameter;
			if ((object)extensionParameter2 != null)
			{
				extensionParameter = extensionParameter2;
				return true;
			}
		}
		extensionParameter = null;
		return false;
	}

	internal static int GetMemberArityIncludingExtension(this Symbol member)
	{
		if (member.IsExtensionBlockMember())
		{
			return member.ContainingType.Arity + member.GetMemberArity();
		}
		return member.GetMemberArity();
	}

	internal static ImmutableArray<TypeParameterSymbol> GetTypeParametersIncludingExtension<TMember>(this TMember member) where TMember : Symbol
	{
		if (member is MethodSymbol methodSymbol)
		{
			if (!methodSymbol.IsExtensionBlockMember())
			{
				return methodSymbol.TypeParameters;
			}
			return methodSymbol.ContainingType.TypeParameters.Concat(methodSymbol.TypeParameters);
		}
		if (member is PropertySymbol propertySymbol)
		{
			return propertySymbol.ContainingType.TypeParameters;
		}
		throw ExceptionUtilities.UnexpectedValue(member);
	}

	internal static Dictionary<TypeParameterSymbol, int>? MakeAdjustedTypeParameterOrdinalsIfNeeded<TMember>(this TMember member, ImmutableArray<TypeParameterSymbol> originalTypeParameters) where TMember : Symbol
	{
		if (member is MethodSymbol methodSymbol)
		{
			Dictionary<TypeParameterSymbol, int> dictionary = null;
			if (methodSymbol.IsExtensionBlockMember() && methodSymbol.Arity > 0 && methodSymbol.ContainingType.Arity > 0)
			{
				dictionary = new Dictionary<TypeParameterSymbol, int>(ReferenceEqualityComparer.Instance);
				for (int i = 0; i < originalTypeParameters.Length; i++)
				{
					dictionary.Add(originalTypeParameters[i], i);
				}
			}
			return dictionary;
		}
		if (member is PropertySymbol)
		{
			return null;
		}
		throw ExceptionUtilities.UnexpectedValue(member);
	}

	internal static ImmutableArray<ParameterSymbol> GetParametersIncludingExtensionParameter(this Symbol symbol, bool skipExtensionIfStatic)
	{
		if ((!skipExtensionIfStatic || !symbol.IsStatic) && symbol.IsExtensionBlockMember())
		{
			ParameterSymbol extensionParameter = symbol.ContainingType.ExtensionParameter;
			if ((object)extensionParameter != null)
			{
				ParameterSymbol parameterSymbol = extensionParameter;
				ImmutableArray<ParameterSymbol> parameters = symbol.GetParameters();
				int num = 0;
				ParameterSymbol[] array = new ParameterSymbol[1 + parameters.Length];
				array[num] = parameterSymbol;
				num++;
				ReadOnlySpan<ParameterSymbol> readOnlySpan = parameters.AsSpan();
				readOnlySpan.CopyTo(new Span<ParameterSymbol>(array).Slice(num, readOnlySpan.Length));
				num += readOnlySpan.Length;
				return ImmutableCollectionsMarshal.AsImmutableArray(array);
			}
		}
		return symbol.GetParameters();
	}

	internal static int GetParameterCountIncludingExtensionParameter(this Symbol symbol)
	{
		bool flag = symbol.IsExtensionBlockMember() && (object)symbol.ContainingType.ExtensionParameter != null;
		return symbol.GetParameterCount() + (flag ? 1 : 0);
	}

	internal static TMember ConstructIncludingExtension<TMember>(this TMember member, ImmutableArray<TypeWithAnnotations> typeArguments) where TMember : Symbol
	{
		MethodSymbol methodSymbol = member as MethodSymbol;
		if ((object)methodSymbol != null)
		{
			if (methodSymbol.IsExtensionBlockMember())
			{
				NamedTypeSymbol namedTypeSymbol = methodSymbol.ContainingType;
				if (namedTypeSymbol.Arity > 0)
				{
					namedTypeSymbol = namedTypeSymbol.Construct(typeArguments.Slice(0, namedTypeSymbol.Arity));
					methodSymbol = methodSymbol.AsMember(namedTypeSymbol);
				}
				if (methodSymbol.Arity > 0)
				{
					MethodSymbol methodSymbol2 = methodSymbol;
					int arity = namedTypeSymbol.Arity;
					return (TMember)(Symbol)methodSymbol2.Construct(typeArguments.Slice(arity, typeArguments.Length - arity));
				}
				return (TMember)(Symbol)methodSymbol;
			}
			return (TMember)(Symbol)methodSymbol.Construct(typeArguments);
		}
		if (member is PropertySymbol { ContainingType: var containingType } propertySymbol)
		{
			NamedTypeSymbol newOwner = containingType.Construct(typeArguments);
			PropertySymbol propertySymbol2 = propertySymbol.AsMember(newOwner);
			return (TMember)(Symbol)propertySymbol2;
		}
		throw ExceptionUtilities.UnexpectedValue(member);
	}

	internal static Symbol? GetReducedAndFilteredSymbol(this Symbol member, ImmutableArray<TypeWithAnnotations> typeArguments, TypeSymbol receiverType, CSharpCompilation compilation, bool checkFullyInferred)
	{
		if (member is MethodSymbol methodSymbol)
		{
			MethodSymbol methodSymbol2;
			if (!typeArguments.IsDefaultOrEmpty && methodSymbol.GetMemberArityIncludingExtension() == typeArguments.Length)
			{
				methodSymbol2 = methodSymbol.ConstructIncludingExtension(typeArguments);
				if (!checkConstraintsIncludingExtension(methodSymbol2, compilation, methodSymbol.ContainingAssembly.CorLibrary.TypeConversions))
				{
					return null;
				}
			}
			else
			{
				methodSymbol2 = methodSymbol;
			}
			if ((object)receiverType != null)
			{
				if (methodSymbol.IsExtensionMethod)
				{
					methodSymbol2 = methodSymbol2.ReduceExtensionMethod(receiverType, compilation, out var wasFullyInferred);
					if (checkFullyInferred && !wasFullyInferred)
					{
						return null;
					}
				}
				else
				{
					methodSymbol2 = (MethodSymbol)SourceNamedTypeSymbol.ReduceExtensionMember(compilation, methodSymbol2, receiverType, out var wasExtensionFullyInferred);
					if (checkFullyInferred && (!wasExtensionFullyInferred || ((object)methodSymbol2 != null && methodSymbol2.IsGenericMethod && typeArguments.IsDefaultOrEmpty)))
					{
						return null;
					}
				}
			}
			return methodSymbol2;
		}
		if (member is PropertySymbol extensionMember)
		{
			PropertySymbol result = (PropertySymbol)SourceNamedTypeSymbol.ReduceExtensionMember(compilation, extensionMember, receiverType, out var wasExtensionFullyInferred2);
			if (checkFullyInferred && !wasExtensionFullyInferred2)
			{
				return null;
			}
			return result;
		}
		throw ExceptionUtilities.UnexpectedValue(member.Kind);
		static bool checkConstraintsIncludingExtension(MethodSymbol symbol, CSharpCompilation currentCompilation, TypeConversions conversions)
		{
			return symbol.CheckConstraints(new ConstraintsHelper.CheckConstraintsArgs(currentCompilation, conversions, includeNullability: false, NoLocation.Singleton, BindingDiagnosticBag.Discarded, CompoundUseSiteInfo<AssemblySymbol>.Discarded));
		}
	}

	internal static ImmutableArray<RefKind> GetParameterRefKinds(this Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).ParameterRefKinds, 
			SymbolKind.Property => ((PropertySymbol)member).ParameterRefKinds, 
			SymbolKind.Event => default(ImmutableArray<RefKind>), 
			_ => throw ExceptionUtilities.UnexpectedValue(member.Kind), 
		};
	}

	internal static int GetParameterCount(this Symbol member)
	{
		switch (member.Kind)
		{
		case SymbolKind.Method:
			return ((MethodSymbol)member).ParameterCount;
		case SymbolKind.Property:
			return ((PropertySymbol)member).ParameterCount;
		case SymbolKind.Event:
		case SymbolKind.Field:
			return 0;
		default:
			throw ExceptionUtilities.UnexpectedValue(member.Kind);
		}
	}

	internal static bool HasParameterContainingPointerType(this Symbol member)
	{
		foreach (TypeWithAnnotations parameterType in member.GetParameterTypes())
		{
			if (parameterType.Type.ContainsPointerOrFunctionPointer())
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsEventOrPropertyWithImplementableNonPublicAccessor(this Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Property:
		{
			PropertySymbol propertySymbol = (PropertySymbol)symbol;
			if (!isImplementableAndNotPublic(propertySymbol.GetMethod))
			{
				return isImplementableAndNotPublic(propertySymbol.SetMethod);
			}
			return true;
		}
		case SymbolKind.Event:
		{
			EventSymbol eventSymbol = (EventSymbol)symbol;
			if (!isImplementableAndNotPublic(eventSymbol.AddMethod))
			{
				return isImplementableAndNotPublic(eventSymbol.RemoveMethod);
			}
			return true;
		}
		default:
			return false;
		}
		static bool isImplementableAndNotPublic(MethodSymbol accessor)
		{
			if (accessor.IsImplementable())
			{
				return accessor.DeclaredAccessibility != Accessibility.Public;
			}
			return false;
		}
	}

	public static bool IsImplementable(this MethodSymbol methodOpt)
	{
		if ((object)methodOpt != null && !methodOpt.IsSealed)
		{
			if (!methodOpt.IsAbstract)
			{
				return methodOpt.IsVirtual;
			}
			return true;
		}
		return false;
	}

	public static bool IsAccessor(this MethodSymbol methodSymbol)
	{
		return (object)methodSymbol.AssociatedSymbol != null;
	}

	public static bool IsAccessor(this Symbol symbol)
	{
		if (symbol.Kind == SymbolKind.Method)
		{
			return ((MethodSymbol)symbol).IsAccessor();
		}
		return false;
	}

	public static bool IsIndexedPropertyAccessor(this MethodSymbol methodSymbol)
	{
		return methodSymbol.AssociatedSymbol?.IsIndexedProperty() ?? false;
	}

	public static bool IsOperator(this MethodSymbol methodSymbol)
	{
		if (methodSymbol.MethodKind != MethodKind.UserDefinedOperator)
		{
			return methodSymbol.MethodKind == MethodKind.Conversion;
		}
		return true;
	}

	public static bool IsOperator(this Symbol symbol)
	{
		if (symbol.Kind == SymbolKind.Method)
		{
			return ((MethodSymbol)symbol).IsOperator();
		}
		return false;
	}

	public static bool IsIndexer(this Symbol symbol)
	{
		if (symbol.Kind == SymbolKind.Property)
		{
			return ((PropertySymbol)symbol).IsIndexer;
		}
		return false;
	}

	public static bool IsIndexedProperty(this Symbol symbol)
	{
		if (symbol.Kind == SymbolKind.Property)
		{
			return ((PropertySymbol)symbol).IsIndexedProperty;
		}
		return false;
	}

	public static bool IsUserDefinedConversion(this Symbol symbol)
	{
		if (symbol.Kind == SymbolKind.Method)
		{
			return ((MethodSymbol)symbol).MethodKind == MethodKind.Conversion;
		}
		return false;
	}

	public static int CustomModifierCount(this MethodSymbol method)
	{
		int num = 0;
		TypeWithAnnotations returnTypeWithAnnotations = method.ReturnTypeWithAnnotations;
		num += returnTypeWithAnnotations.CustomModifiers.Length + method.RefCustomModifiers.Length;
		num += returnTypeWithAnnotations.Type.CustomModifierCount();
		foreach (ParameterSymbol parameter in method.Parameters)
		{
			TypeWithAnnotations typeWithAnnotations = parameter.TypeWithAnnotations;
			num += typeWithAnnotations.CustomModifiers.Length + parameter.RefCustomModifiers.Length;
			num += typeWithAnnotations.Type.CustomModifierCount();
		}
		return num;
	}

	public static int CustomModifierCount(this Symbol m)
	{
		switch (m.Kind)
		{
		case SymbolKind.ArrayType:
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
		case SymbolKind.PointerType:
		case SymbolKind.TypeParameter:
		case SymbolKind.FunctionPointerType:
			return ((TypeSymbol)m).CustomModifierCount();
		case SymbolKind.Event:
			return ((EventSymbol)m).CustomModifierCount();
		case SymbolKind.Method:
			return ((MethodSymbol)m).CustomModifierCount();
		case SymbolKind.Property:
			return ((PropertySymbol)m).CustomModifierCount();
		default:
			throw ExceptionUtilities.UnexpectedValue(m.Kind);
		}
	}

	public static int CustomModifierCount(this EventSymbol e)
	{
		return e.Type.CustomModifierCount();
	}

	public static int CustomModifierCount(this PropertySymbol property)
	{
		int num = 0;
		TypeWithAnnotations typeWithAnnotations = property.TypeWithAnnotations;
		num += typeWithAnnotations.CustomModifiers.Length + property.RefCustomModifiers.Length;
		num += typeWithAnnotations.Type.CustomModifierCount();
		foreach (ParameterSymbol parameter in property.Parameters)
		{
			TypeWithAnnotations typeWithAnnotations2 = parameter.TypeWithAnnotations;
			num += typeWithAnnotations2.CustomModifiers.Length + parameter.RefCustomModifiers.Length;
			num += typeWithAnnotations2.Type.CustomModifierCount();
		}
		return num;
	}

	internal static Symbol SymbolAsMember(this Symbol s, NamedTypeSymbol newOwner)
	{
		return s.Kind switch
		{
			SymbolKind.Field => ((FieldSymbol)s).AsMember(newOwner), 
			SymbolKind.Method => ((MethodSymbol)s).AsMember(newOwner), 
			SymbolKind.NamedType => ((NamedTypeSymbol)s).AsMember(newOwner), 
			SymbolKind.Property => ((PropertySymbol)s).AsMember(newOwner), 
			SymbolKind.Event => ((EventSymbol)s).AsMember(newOwner), 
			_ => throw ExceptionUtilities.UnexpectedValue(s.Kind), 
		};
	}

	internal static int GetMemberArity(this Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Method:
			return ((MethodSymbol)symbol).Arity;
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
			return ((NamedTypeSymbol)symbol).Arity;
		default:
			return 0;
		}
	}

	internal static NamespaceOrTypeSymbol OfMinimalArity(this IEnumerable<NamespaceOrTypeSymbol> symbols)
	{
		NamespaceOrTypeSymbol result = null;
		int num = int.MaxValue;
		foreach (NamespaceOrTypeSymbol symbol in symbols)
		{
			int memberArity = symbol.GetMemberArity();
			if (memberArity < num)
			{
				num = memberArity;
				result = symbol;
			}
		}
		return result;
	}

	internal static ImmutableArray<TypeParameterSymbol> GetMemberTypeParameters(this Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Method:
			return ((MethodSymbol)symbol).TypeParameters;
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
			return ((NamedTypeSymbol)symbol).TypeParameters;
		case SymbolKind.Event:
		case SymbolKind.Field:
		case SymbolKind.Property:
			return ImmutableArray<TypeParameterSymbol>.Empty;
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
	}

	internal static ImmutableArray<TypeSymbol> GetMemberTypeArgumentsNoUseSiteDiagnostics(this Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Method:
			return ((MethodSymbol)symbol).TypeArgumentsWithAnnotations.SelectAsArray(TypeMap.AsTypeSymbol);
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
			return ((NamedTypeSymbol)symbol).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics.SelectAsArray(TypeMap.AsTypeSymbol);
		case SymbolKind.Event:
		case SymbolKind.Field:
		case SymbolKind.Property:
			return ImmutableArray<TypeSymbol>.Empty;
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
	}

	internal static bool IsConstructor(this MethodSymbol method)
	{
		MethodKind methodKind = method.MethodKind;
		if (methodKind == MethodKind.Constructor || methodKind == MethodKind.StaticConstructor)
		{
			return true;
		}
		return false;
	}

	internal static bool HasThisConstructorInitializer(this MethodSymbol method, out ConstructorInitializerSyntax initializerSyntax)
	{
		if ((object)method != null && method.MethodKind == MethodKind.Constructor && method is SourceMemberMethodSymbol { SyntaxNode: ConstructorDeclarationSyntax syntaxNode } && syntaxNode.Initializer?.Kind() == SyntaxKind.ThisConstructorInitializer)
		{
			initializerSyntax = syntaxNode.Initializer;
			return true;
		}
		initializerSyntax = null;
		return false;
	}

	internal static bool IncludeFieldInitializersInBody(this MethodSymbol methodSymbol)
	{
		if (methodSymbol.IsConstructor() && (!methodSymbol.HasThisConstructorInitializer(out var initializerSyntax) || methodSymbol.ContainingType.IsDefaultValueTypeConstructor(initializerSyntax)) && !(methodSymbol is SynthesizedRecordCopyCtor))
		{
			return !Binder.IsUserDefinedRecordCopyConstructor(methodSymbol);
		}
		return false;
	}

	internal static bool IsDefaultValueTypeConstructor(this NamedTypeSymbol type, ConstructorInitializerSyntax initializerSyntax)
	{
		if (initializerSyntax.ArgumentList.Arguments.Count > 0 || !type.IsValueType)
		{
			return false;
		}
		bool flag = false;
		bool result = false;
		foreach (MethodSymbol instanceConstructor in type.InstanceConstructors)
		{
			if (instanceConstructor.ParameterCount == 0)
			{
				if (flag)
				{
					return false;
				}
				flag = true;
				result = instanceConstructor.IsDefaultValueTypeConstructor();
			}
		}
		return result;
	}

	internal static bool IsParameterlessConstructor(this MethodSymbol method)
	{
		if (method.MethodKind == MethodKind.Constructor)
		{
			return method.ParameterCount == 0;
		}
		return false;
	}

	internal static bool IsDefaultValueTypeConstructor(this MethodSymbol method)
	{
		if (method.IsImplicitlyDeclared)
		{
			NamedTypeSymbol containingType = method.ContainingType;
			if ((object)containingType != null && containingType.IsValueType)
			{
				return method.IsParameterlessConstructor();
			}
		}
		return false;
	}

	internal static bool ShouldEmit(this MethodSymbol method)
	{
		if (method.IsDefaultValueTypeConstructor())
		{
			return false;
		}
		if (method is SynthesizedStaticConstructor synthesizedStaticConstructor && !synthesizedStaticConstructor.ShouldEmit())
		{
			return false;
		}
		if (method.IsPartialMember() && (object)method.PartialImplementationPart == null)
		{
			return false;
		}
		return true;
	}

	internal static MethodSymbol GetOwnOrInheritedAddMethod(this EventSymbol @event)
	{
		while ((object)@event != null)
		{
			MethodSymbol addMethod = @event.AddMethod;
			if ((object)addMethod != null)
			{
				return addMethod;
			}
			@event = (@event.IsOverride ? @event.OverriddenEvent : null);
		}
		return null;
	}

	internal static MethodSymbol GetOwnOrInheritedRemoveMethod(this EventSymbol @event)
	{
		while ((object)@event != null)
		{
			MethodSymbol removeMethod = @event.RemoveMethod;
			if ((object)removeMethod != null)
			{
				return removeMethod;
			}
			@event = (@event.IsOverride ? @event.OverriddenEvent : null);
		}
		return null;
	}

	internal static bool IsExplicitInterfaceImplementation(this Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).IsExplicitInterfaceImplementation, 
			SymbolKind.Property => ((PropertySymbol)member).IsExplicitInterfaceImplementation, 
			SymbolKind.Event => ((EventSymbol)member).IsExplicitInterfaceImplementation, 
			_ => false, 
		};
	}

	internal static bool IsPartialMember(this Symbol member)
	{
		if (member is SourceOrdinaryMethodSymbol sourceOrdinaryMethodSymbol)
		{
			if (sourceOrdinaryMethodSymbol.IsPartial)
			{
				goto IL_007c;
			}
		}
		else if (member is SourcePropertySymbol sourcePropertySymbol)
		{
			if (sourcePropertySymbol.IsPartial)
			{
				goto IL_007c;
			}
		}
		else if (member is SourcePropertyAccessorSymbol sourcePropertyAccessorSymbol)
		{
			if (sourcePropertyAccessorSymbol.IsPartial)
			{
				goto IL_007c;
			}
		}
		else if (member is SourceConstructorSymbol sourceConstructorSymbol)
		{
			if (sourceConstructorSymbol.IsPartial)
			{
				goto IL_007c;
			}
		}
		else if (member is SourceEventSymbol sourceEventSymbol)
		{
			if (sourceEventSymbol.IsPartial)
			{
				goto IL_007c;
			}
		}
		else if (member is SourceEventAccessorSymbol { IsPartial: not false })
		{
			goto IL_007c;
		}
		return false;
		IL_007c:
		return true;
	}

	internal static bool IsPartialImplementation(this Symbol member)
	{
		if (member is SourceOrdinaryMethodSymbol sourceOrdinaryMethodSymbol)
		{
			if (sourceOrdinaryMethodSymbol.IsPartialImplementation)
			{
				goto IL_007c;
			}
		}
		else if (member is SourcePropertySymbol sourcePropertySymbol)
		{
			if (sourcePropertySymbol.IsPartialImplementation)
			{
				goto IL_007c;
			}
		}
		else if (member is SourcePropertyAccessorSymbol sourcePropertyAccessorSymbol)
		{
			if (sourcePropertyAccessorSymbol.IsPartialImplementation)
			{
				goto IL_007c;
			}
		}
		else if (member is SourceConstructorSymbol sourceConstructorSymbol)
		{
			if (sourceConstructorSymbol.IsPartialImplementation)
			{
				goto IL_007c;
			}
		}
		else if (member is SourceEventSymbol sourceEventSymbol)
		{
			if (sourceEventSymbol.IsPartialImplementation)
			{
				goto IL_007c;
			}
		}
		else if (member is SourceEventAccessorSymbol { IsPartialImplementation: not false })
		{
			goto IL_007c;
		}
		return false;
		IL_007c:
		return true;
	}

	internal static bool IsPartialDefinition(this Symbol member)
	{
		if (member is SourceOrdinaryMethodSymbol sourceOrdinaryMethodSymbol)
		{
			if (sourceOrdinaryMethodSymbol.IsPartialDefinition)
			{
				goto IL_007c;
			}
		}
		else if (member is SourcePropertySymbol sourcePropertySymbol)
		{
			if (sourcePropertySymbol.IsPartialDefinition)
			{
				goto IL_007c;
			}
		}
		else if (member is SourcePropertyAccessorSymbol sourcePropertyAccessorSymbol)
		{
			if (sourcePropertyAccessorSymbol.IsPartialDefinition)
			{
				goto IL_007c;
			}
		}
		else if (member is SourceConstructorSymbol sourceConstructorSymbol)
		{
			if (sourceConstructorSymbol.IsPartialDefinition)
			{
				goto IL_007c;
			}
		}
		else if (member is SourceEventSymbol sourceEventSymbol)
		{
			if (sourceEventSymbol.IsPartialDefinition)
			{
				goto IL_007c;
			}
		}
		else if (member is SourceEventAccessorSymbol { IsPartialDefinition: not false })
		{
			goto IL_007c;
		}
		return false;
		IL_007c:
		return true;
	}

	internal static Symbol? GetPartialImplementationPart(this Symbol member)
	{
		if (!(member is MethodSymbol methodSymbol))
		{
			if (!(member is SourcePropertySymbol sourcePropertySymbol))
			{
				if (member is SourceEventSymbol sourceEventSymbol)
				{
					return sourceEventSymbol.PartialImplementationPart;
				}
				return null;
			}
			return sourcePropertySymbol.PartialImplementationPart;
		}
		return methodSymbol.PartialImplementationPart;
	}

	internal static Symbol? GetPartialDefinitionPart(this Symbol member)
	{
		if (!(member is MethodSymbol methodSymbol))
		{
			if (!(member is SourcePropertySymbol sourcePropertySymbol))
			{
				if (member is SourceEventSymbol sourceEventSymbol)
				{
					return sourceEventSymbol.PartialDefinitionPart;
				}
				return null;
			}
			return sourcePropertySymbol.PartialDefinitionPart;
		}
		return methodSymbol.PartialDefinitionPart;
	}

	internal static bool ContainsTupleNames(this Symbol member)
	{
		switch (member.Kind)
		{
		case SymbolKind.Method:
		{
			MethodSymbol methodSymbol = (MethodSymbol)member;
			if (!methodSymbol.ReturnType.ContainsTupleNames())
			{
				return methodSymbol.Parameters.Any((ParameterSymbol p) => p.Type.ContainsTupleNames());
			}
			return true;
		}
		case SymbolKind.Property:
			return ((PropertySymbol)member).Type.ContainsTupleNames();
		case SymbolKind.Event:
			return ((EventSymbol)member).Type.ContainsTupleNames();
		default:
			throw ExceptionUtilities.UnexpectedValue(member.Kind);
		}
	}

	internal static ImmutableArray<Symbol> GetExplicitInterfaceImplementations(this Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).ExplicitInterfaceImplementations.Cast<MethodSymbol, Symbol>(), 
			SymbolKind.Property => ((PropertySymbol)member).ExplicitInterfaceImplementations.Cast<PropertySymbol, Symbol>(), 
			SymbolKind.Event => ((EventSymbol)member).ExplicitInterfaceImplementations.Cast<EventSymbol, Symbol>(), 
			_ => ImmutableArray<Symbol>.Empty, 
		};
	}

	internal static Symbol GetOverriddenMember(this Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).OverriddenMethod, 
			SymbolKind.Property => ((PropertySymbol)member).OverriddenProperty, 
			SymbolKind.Event => ((EventSymbol)member).OverriddenEvent, 
			_ => throw ExceptionUtilities.UnexpectedValue(member.Kind), 
		};
	}

	internal static Symbol GetLeastOverriddenMember(this Symbol member, NamedTypeSymbol accessingTypeOpt)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).GetConstructedLeastOverriddenMethod(accessingTypeOpt, requireSameReturnType: false), 
			SymbolKind.Property => ((PropertySymbol)member).GetLeastOverriddenProperty(accessingTypeOpt), 
			SymbolKind.Event => ((EventSymbol)member).GetLeastOverriddenEvent(accessingTypeOpt), 
			_ => member, 
		};
	}

	internal static bool IsFieldOrFieldLikeEvent(this Symbol member, out FieldSymbol field)
	{
		switch (member.Kind)
		{
		case SymbolKind.Field:
			field = (FieldSymbol)member;
			return true;
		case SymbolKind.Event:
			field = ((EventSymbol)member).AssociatedField;
			return (object)field != null;
		default:
			field = null;
			return false;
		}
	}

	internal static string GetMemberCallerName(this Symbol member)
	{
		if (member.Kind == SymbolKind.Method)
		{
			member = ((MethodSymbol)member).AssociatedSymbol ?? member;
		}
		if (!member.IsIndexer())
		{
			if (!member.IsExplicitInterfaceImplementation())
			{
				return member.Name;
			}
			return ExplicitInterfaceHelpers.GetMemberNameWithoutInterfaceName(member.Name);
		}
		return member.MetadataName;
	}

	public static bool IsCompilationOutputWinMdObj(this Symbol symbol)
	{
		CSharpCompilation declaringCompilation = symbol.DeclaringCompilation;
		if (declaringCompilation != null)
		{
			return declaringCompilation.Options.OutputKind == OutputKind.WindowsRuntimeMetadata;
		}
		return false;
	}

	public static NamedTypeSymbol ConstructIfGeneric(this NamedTypeSymbol type, ImmutableArray<TypeWithAnnotations> typeArguments)
	{
		if (!type.TypeParameters.IsEmpty)
		{
			return type.Construct(typeArguments, unbound: false);
		}
		return type;
	}

	public static bool IsNestedType([NotNullWhen(true)] this Symbol? symbol)
	{
		if (symbol is NamedTypeSymbol)
		{
			return (object)symbol.ContainingType != null;
		}
		return false;
	}

	public static bool IsAccessibleViaInheritance(this NamedTypeSymbol superType, NamedTypeSymbol subType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		NamedTypeSymbol originalDefinition = superType.OriginalDefinition;
		NamedTypeSymbol namedTypeSymbol = subType;
		while ((object)namedTypeSymbol != null)
		{
			if ((object)namedTypeSymbol.OriginalDefinition == originalDefinition)
			{
				return true;
			}
			namedTypeSymbol = namedTypeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
		}
		if (originalDefinition.IsInterface)
		{
			foreach (NamedTypeSymbol item in subType.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo))
			{
				if ((object)item.OriginalDefinition == originalDefinition)
				{
					return true;
				}
			}
		}
		if (superType.TypeKind == TypeKind.Submission)
		{
			return subType.TypeKind == TypeKind.Submission;
		}
		return false;
	}

	public static bool IsNoMoreVisibleThan(this Symbol symbol, TypeSymbol type, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return type.IsAtLeastAsVisibleAs(symbol, ref useSiteInfo);
	}

	public static bool IsNoMoreVisibleThan(this Symbol symbol, TypeWithAnnotations type, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return type.IsAtLeastAsVisibleAs(symbol, ref useSiteInfo);
	}

	internal static void AddUseSiteInfo(this Symbol? symbol, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, bool addDiagnostics = true)
	{
		if ((object)symbol != null && useSiteInfo.AccumulatesDiagnostics)
		{
			UseSiteInfo<AssemblySymbol> useSiteInfo2 = symbol.GetUseSiteInfo();
			if (addDiagnostics)
			{
				useSiteInfo.AddDiagnostics(useSiteInfo2);
			}
			useSiteInfo.AddDependencies(useSiteInfo2);
		}
	}

	public static LocalizableErrorArgument GetKindText(this Symbol symbol)
	{
		return symbol.Kind.Localize();
	}

	internal static NamespaceOrTypeSymbol? ContainingNamespaceOrType(this Symbol symbol)
	{
		Symbol containingSymbol = symbol.ContainingSymbol;
		if ((object)containingSymbol != null)
		{
			SymbolKind kind = containingSymbol.Kind;
			if (kind == SymbolKind.ErrorType || (uint)(kind - 11) <= 1u)
			{
				return (NamespaceOrTypeSymbol)containingSymbol;
			}
		}
		return null;
	}

	internal static Symbol? ContainingNonLambdaMember(this Symbol? containingMember)
	{
		while ((object)containingMember != null && containingMember.Kind == SymbolKind.Method)
		{
			MethodSymbol methodSymbol = (MethodSymbol)containingMember;
			if (methodSymbol.MethodKind != MethodKind.AnonymousFunction && methodSymbol.MethodKind != MethodKind.LocalFunction)
			{
				break;
			}
			containingMember = containingMember.ContainingSymbol;
		}
		return containingMember;
	}

	internal static ParameterSymbol? EnclosingThisSymbol(this Symbol containingMember)
	{
		Symbol symbol = containingMember;
		NamedTypeSymbol namedTypeSymbol;
		while (true)
		{
			MethodSymbol methodSymbol;
			switch (symbol.Kind)
			{
			case SymbolKind.Method:
				methodSymbol = (MethodSymbol)symbol;
				if (methodSymbol.MethodKind == MethodKind.AnonymousFunction || methodSymbol.MethodKind == MethodKind.LocalFunction)
				{
					goto IL_0032;
				}
				return methodSymbol.ThisParameter;
			case SymbolKind.Field:
				namedTypeSymbol = symbol.ContainingType;
				break;
			case SymbolKind.NamedType:
				namedTypeSymbol = (NamedTypeSymbol)symbol;
				break;
			default:
				return null;
			}
			break;
			IL_0032:
			symbol = methodSymbol.ContainingSymbol;
		}
		if (!namedTypeSymbol.IsScriptClass)
		{
			return null;
		}
		return namedTypeSymbol.InstanceConstructors.Single().ThisParameter;
	}

	public static Symbol ConstructedFrom(this Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
			return ((NamedTypeSymbol)symbol).ConstructedFrom;
		case SymbolKind.Method:
			return ((MethodSymbol)symbol).ConstructedFrom;
		default:
			return symbol;
		}
	}

	public static bool IsContainingSymbolOfAllTypeParameters(this Symbol containingSymbol, TypeSymbol type)
	{
		return (object)type.VisitType(s_hasInvalidTypeParameterFunc, containingSymbol) == null;
	}

	public static bool IsContainingSymbolOfAllTypeParameters(this Symbol containingSymbol, ImmutableArray<TypeSymbol> types)
	{
		return types.All(containingSymbol.IsContainingSymbolOfAllTypeParameters);
	}

	private static bool HasInvalidTypeParameter(TypeSymbol type, Symbol? containingSymbol)
	{
		if (type.TypeKind == TypeKind.TypeParameter)
		{
			Symbol containingSymbol2 = type.ContainingSymbol;
			while ((object)containingSymbol != null && containingSymbol.Kind != SymbolKind.Namespace)
			{
				if (containingSymbol == containingSymbol2)
				{
					return false;
				}
				containingSymbol = containingSymbol.ContainingSymbol;
			}
			return true;
		}
		return false;
	}

	public static bool IsTypeOrTypeAlias(this Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.ArrayType:
		case SymbolKind.DynamicType:
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
		case SymbolKind.PointerType:
		case SymbolKind.TypeParameter:
		case SymbolKind.FunctionPointerType:
			return true;
		case SymbolKind.Alias:
			return ((AliasSymbol)symbol).Target.IsTypeOrTypeAlias();
		default:
			return false;
		}
	}

	internal static bool CompilationAllowsUnsafe(this Symbol symbol)
	{
		return symbol.DeclaringCompilation.Options.AllowUnsafe;
	}

	internal static void CheckUnsafeModifier(this Symbol symbol, DeclarationModifiers modifiers, BindingDiagnosticBag diagnostics)
	{
		symbol.CheckUnsafeModifier(modifiers, symbol.GetFirstLocation(), diagnostics);
	}

	internal static void CheckUnsafeModifier(this Symbol symbol, DeclarationModifiers modifiers, Location errorLocation, BindingDiagnosticBag diagnostics)
	{
		symbol.CheckUnsafeModifier(modifiers, errorLocation, diagnostics.DiagnosticBag);
	}

	internal static void CheckUnsafeModifier(this Symbol symbol, DeclarationModifiers modifiers, Location errorLocation, DiagnosticBag? diagnostics)
	{
		if (diagnostics != null && (modifiers & DeclarationModifiers.Unsafe) == DeclarationModifiers.Unsafe && !symbol.CompilationAllowsUnsafe())
		{
			diagnostics.Add(ErrorCode.ERR_IllegalUnsafe, errorLocation);
		}
	}

	public static bool IsHiddenByCodeAnalysisEmbeddedAttribute(this Symbol symbol)
	{
		NamedTypeSymbol namedTypeSymbol = ((symbol.Kind == SymbolKind.NamedType) ? ((NamedTypeSymbol)symbol) : symbol.ContainingType);
		if ((object)namedTypeSymbol == null)
		{
			return false;
		}
		while ((object)namedTypeSymbol.ContainingType != null)
		{
			namedTypeSymbol = namedTypeSymbol.ContainingType;
		}
		return namedTypeSymbol.HasCodeAnalysisEmbeddedAttribute;
	}

	public static bool MustCallMethodsDirectly(this Symbol symbol)
	{
		return symbol.Kind switch
		{
			SymbolKind.Property => ((PropertySymbol)symbol).MustCallMethodsDirectly, 
			SymbolKind.Event => ((EventSymbol)symbol).MustCallMethodsDirectly, 
			_ => false, 
		};
	}

	public static int GetArity(this Symbol? symbol)
	{
		if ((object)symbol != null)
		{
			switch (symbol.Kind)
			{
			case SymbolKind.NamedType:
				return ((NamedTypeSymbol)symbol).Arity;
			case SymbolKind.Method:
				return ((MethodSymbol)symbol).Arity;
			}
		}
		return 0;
	}

	internal static CSharpSyntaxNode GetNonNullSyntaxNode(this Symbol? symbol)
	{
		if ((object)symbol != null)
		{
			SyntaxReference syntaxReference = symbol.DeclaringSyntaxReferences.FirstOrDefault();
			if (syntaxReference == null && symbol.IsImplicitlyDeclared)
			{
				Symbol containingSymbol = symbol.ContainingSymbol;
				if ((object)containingSymbol != null)
				{
					syntaxReference = containingSymbol.DeclaringSyntaxReferences.FirstOrDefault();
				}
			}
			if (syntaxReference != null)
			{
				return (CSharpSyntaxNode)syntaxReference.GetSyntax();
			}
		}
		return (CSharpSyntaxNode)CSharpSyntaxTree.Dummy.GetRoot();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static Symbol? EnsureCSharpSymbolOrNull(this ISymbol? symbol, string paramName)
	{
		if (!(symbol is Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.Symbol symbol2))
		{
			if (symbol != null)
			{
				throw new ArgumentException(CSharpResources.NotACSharpSymbol, paramName);
			}
			return null;
		}
		return symbol2.UnderlyingSymbol;
	}

	[return: NotNullIfNotNull("symbol")]
	internal static AssemblySymbol? EnsureCSharpSymbolOrNull(this IAssemblySymbol? symbol, string paramName)
	{
		return (AssemblySymbol)((ISymbol?)symbol).EnsureCSharpSymbolOrNull(paramName);
	}

	[return: NotNullIfNotNull("symbol")]
	internal static NamespaceOrTypeSymbol? EnsureCSharpSymbolOrNull(this INamespaceOrTypeSymbol? symbol, string paramName)
	{
		return (NamespaceOrTypeSymbol)((ISymbol?)symbol).EnsureCSharpSymbolOrNull(paramName);
	}

	[return: NotNullIfNotNull("symbol")]
	internal static NamespaceSymbol? EnsureCSharpSymbolOrNull(this INamespaceSymbol? symbol, string paramName)
	{
		return (NamespaceSymbol)((ISymbol?)symbol).EnsureCSharpSymbolOrNull(paramName);
	}

	[return: NotNullIfNotNull("symbol")]
	internal static TypeSymbol? EnsureCSharpSymbolOrNull(this ITypeSymbol? symbol, string paramName)
	{
		return (TypeSymbol)((ISymbol?)symbol).EnsureCSharpSymbolOrNull(paramName);
	}

	[return: NotNullIfNotNull("symbol")]
	internal static NamedTypeSymbol? EnsureCSharpSymbolOrNull(this INamedTypeSymbol? symbol, string paramName)
	{
		return (NamedTypeSymbol)((ISymbol?)symbol).EnsureCSharpSymbolOrNull(paramName);
	}

	[return: NotNullIfNotNull("symbol")]
	internal static TypeParameterSymbol? EnsureCSharpSymbolOrNull(this ITypeParameterSymbol? symbol, string paramName)
	{
		return (TypeParameterSymbol)((ISymbol?)symbol).EnsureCSharpSymbolOrNull(paramName);
	}

	[return: NotNullIfNotNull("symbol")]
	internal static EventSymbol? EnsureCSharpSymbolOrNull(this IEventSymbol? symbol, string paramName)
	{
		return (EventSymbol)((ISymbol?)symbol).EnsureCSharpSymbolOrNull(paramName);
	}

	internal static TypeWithAnnotations GetTypeOrReturnType(this Symbol symbol)
	{
		symbol.GetTypeOrReturnType(out RefKind _, out TypeWithAnnotations returnType, out ImmutableArray<CustomModifier> _);
		return returnType;
	}

	internal static FlowAnalysisAnnotations GetFlowAnalysisAnnotations(this PropertySymbol property)
	{
		FlowAnalysisAnnotations flowAnalysisAnnotations = property.GetOwnOrInheritedGetMethod()?.ReturnTypeFlowAnalysisAnnotations ?? FlowAnalysisAnnotations.None;
		FlowAnalysisAnnotations? flowAnalysisAnnotations2 = property.GetOwnOrInheritedSetMethod()?.Parameters.Last().FlowAnalysisAnnotations;
		if (flowAnalysisAnnotations2.HasValue)
		{
			FlowAnalysisAnnotations valueOrDefault = flowAnalysisAnnotations2.GetValueOrDefault();
			flowAnalysisAnnotations |= valueOrDefault;
		}
		else if (property is SourcePropertySymbolBase sourcePropertySymbolBase)
		{
			if (sourcePropertySymbolBase.HasAllowNull)
			{
				flowAnalysisAnnotations |= FlowAnalysisAnnotations.AllowNull;
			}
			if (sourcePropertySymbolBase.HasDisallowNull)
			{
				flowAnalysisAnnotations |= FlowAnalysisAnnotations.DisallowNull;
			}
		}
		return flowAnalysisAnnotations;
	}

	internal static FlowAnalysisAnnotations GetFlowAnalysisAnnotations(this Symbol? symbol)
	{
		if (!(symbol is MethodSymbol { ReturnTypeFlowAnalysisAnnotations: var returnTypeFlowAnalysisAnnotations }))
		{
			if (!(symbol is PropertySymbol property))
			{
				if (!(symbol is ParameterSymbol { FlowAnalysisAnnotations: var flowAnalysisAnnotations }))
				{
					if (!(symbol is FieldSymbol { FlowAnalysisAnnotations: var flowAnalysisAnnotations2 }))
					{
						return FlowAnalysisAnnotations.None;
					}
					return flowAnalysisAnnotations2;
				}
				return flowAnalysisAnnotations;
			}
			return property.GetFlowAnalysisAnnotations();
		}
		return returnTypeFlowAnalysisAnnotations;
	}

	internal static void GetTypeOrReturnType(this Symbol symbol, out RefKind refKind, out TypeWithAnnotations returnType, out ImmutableArray<CustomModifier> refCustomModifiers)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Field:
		{
			FieldSymbol fieldSymbol = (FieldSymbol)symbol;
			refKind = RefKind.None;
			returnType = fieldSymbol.TypeWithAnnotations;
			refCustomModifiers = ImmutableArray<CustomModifier>.Empty;
			break;
		}
		case SymbolKind.Method:
		{
			MethodSymbol methodSymbol = (MethodSymbol)symbol;
			refKind = methodSymbol.RefKind;
			returnType = methodSymbol.ReturnTypeWithAnnotations;
			refCustomModifiers = methodSymbol.RefCustomModifiers;
			break;
		}
		case SymbolKind.Property:
		{
			PropertySymbol propertySymbol = (PropertySymbol)symbol;
			refKind = propertySymbol.RefKind;
			returnType = propertySymbol.TypeWithAnnotations;
			refCustomModifiers = propertySymbol.RefCustomModifiers;
			break;
		}
		case SymbolKind.Event:
		{
			EventSymbol eventSymbol = (EventSymbol)symbol;
			refKind = RefKind.None;
			returnType = eventSymbol.TypeWithAnnotations;
			refCustomModifiers = ImmutableArray<CustomModifier>.Empty;
			break;
		}
		case SymbolKind.Local:
		{
			LocalSymbol localSymbol = (LocalSymbol)symbol;
			refKind = localSymbol.RefKind;
			returnType = localSymbol.TypeWithAnnotations;
			refCustomModifiers = ImmutableArray<CustomModifier>.Empty;
			break;
		}
		case SymbolKind.Parameter:
		{
			ParameterSymbol parameterSymbol = (ParameterSymbol)symbol;
			refKind = parameterSymbol.RefKind;
			returnType = parameterSymbol.TypeWithAnnotations;
			refCustomModifiers = parameterSymbol.RefCustomModifiers;
			break;
		}
		case SymbolKind.ErrorType:
			refKind = RefKind.None;
			returnType = TypeWithAnnotations.Create((TypeSymbol)symbol);
			refCustomModifiers = ImmutableArray<CustomModifier>.Empty;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
	}

	internal static bool IsImplementableInterfaceMember(this Symbol symbol)
	{
		if (!symbol.IsSealed && (symbol.IsAbstract || symbol.IsVirtual))
		{
			return symbol.ContainingType?.IsInterface ?? false;
		}
		return false;
	}

	internal static bool RequiresInstanceReceiver(this Symbol symbol)
	{
		return symbol.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)symbol).RequiresInstanceReceiver, 
			SymbolKind.Property => ((PropertySymbol)symbol).RequiresInstanceReceiver, 
			SymbolKind.Field => ((FieldSymbol)symbol).RequiresInstanceReceiver, 
			SymbolKind.Event => ((EventSymbol)symbol).RequiresInstanceReceiver, 
			_ => throw new ArgumentException("only methods, properties, fields and events can take a receiver", "symbol"), 
		};
	}

	[return: NotNullIfNotNull("symbol")]
	private static TISymbol? GetPublicSymbol<TISymbol>(this Symbol? symbol) where TISymbol : class, ISymbol
	{
		return (TISymbol)(symbol?.ISymbol);
	}

	[return: NotNullIfNotNull("symbol")]
	internal static ISymbol? GetPublicSymbol(this Symbol? symbol)
	{
		return symbol.GetPublicSymbol<ISymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static IMethodSymbol? GetPublicSymbol(this MethodSymbol? symbol)
	{
		return symbol.GetPublicSymbol<IMethodSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static IPropertySymbol? GetPublicSymbol(this PropertySymbol? symbol)
	{
		return symbol.GetPublicSymbol<IPropertySymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static INamedTypeSymbol? GetPublicSymbol(this NamedTypeSymbol? symbol)
	{
		return symbol.GetPublicSymbol<INamedTypeSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static INamespaceSymbol? GetPublicSymbol(this NamespaceSymbol? symbol)
	{
		return symbol.GetPublicSymbol<INamespaceSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static ITypeSymbol? GetPublicSymbol(this TypeSymbol? symbol)
	{
		return symbol.GetPublicSymbol<ITypeSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static ILocalSymbol? GetPublicSymbol(this LocalSymbol? symbol)
	{
		return symbol.GetPublicSymbol<ILocalSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static IAssemblySymbol? GetPublicSymbol(this AssemblySymbol? symbol)
	{
		return symbol.GetPublicSymbol<IAssemblySymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static INamespaceOrTypeSymbol? GetPublicSymbol(this NamespaceOrTypeSymbol? symbol)
	{
		return symbol.GetPublicSymbol<INamespaceOrTypeSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static IDiscardSymbol? GetPublicSymbol(this DiscardSymbol? symbol)
	{
		return symbol.GetPublicSymbol<IDiscardSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static IFieldSymbol? GetPublicSymbol(this FieldSymbol? symbol)
	{
		return symbol.GetPublicSymbol<IFieldSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static IParameterSymbol? GetPublicSymbol(this ParameterSymbol? symbol)
	{
		return symbol.GetPublicSymbol<IParameterSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static IRangeVariableSymbol? GetPublicSymbol(this RangeVariableSymbol? symbol)
	{
		return symbol.GetPublicSymbol<IRangeVariableSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static ILabelSymbol? GetPublicSymbol(this LabelSymbol? symbol)
	{
		return symbol.GetPublicSymbol<ILabelSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static IAliasSymbol? GetPublicSymbol(this AliasSymbol? symbol)
	{
		return symbol.GetPublicSymbol<IAliasSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static IModuleSymbol? GetPublicSymbol(this ModuleSymbol? symbol)
	{
		return symbol.GetPublicSymbol<IModuleSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static ITypeParameterSymbol? GetPublicSymbol(this TypeParameterSymbol? symbol)
	{
		return symbol.GetPublicSymbol<ITypeParameterSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static IArrayTypeSymbol? GetPublicSymbol(this ArrayTypeSymbol? symbol)
	{
		return symbol.GetPublicSymbol<IArrayTypeSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static IPointerTypeSymbol? GetPublicSymbol(this PointerTypeSymbol? symbol)
	{
		return symbol.GetPublicSymbol<IPointerTypeSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static IFunctionPointerTypeSymbol? GetPublicSymbol(this FunctionPointerTypeSymbol? symbol)
	{
		return symbol.GetPublicSymbol<IFunctionPointerTypeSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static IEventSymbol? GetPublicSymbol(this EventSymbol? symbol)
	{
		return symbol.GetPublicSymbol<IEventSymbol>();
	}

	internal static IEnumerable<ISymbol?> GetPublicSymbols(this IEnumerable<Symbol?> symbols)
	{
		return symbols.Select((Symbol p) => p.GetPublicSymbol<ISymbol>());
	}

	private static ImmutableArray<TISymbol> GetPublicSymbols<TISymbol>(this ImmutableArray<Symbol> symbols) where TISymbol : class, ISymbol
	{
		if (symbols.IsDefault)
		{
			return default(ImmutableArray<TISymbol>);
		}
		return symbols.SelectAsArray((Symbol p) => p.GetPublicSymbol<TISymbol>());
	}

	internal static ImmutableArray<ISymbol> GetPublicSymbols(this ImmutableArray<Symbol> symbols)
	{
		return symbols.GetPublicSymbols<ISymbol>();
	}

	internal static ImmutableArray<IPropertySymbol> GetPublicSymbols(this ImmutableArray<PropertySymbol> symbols)
	{
		return StaticCast<Symbol>.From(symbols).GetPublicSymbols<IPropertySymbol>();
	}

	internal static ImmutableArray<ITypeSymbol> GetPublicSymbols(this ImmutableArray<TypeSymbol> symbols)
	{
		return StaticCast<Symbol>.From(symbols).GetPublicSymbols<ITypeSymbol>();
	}

	internal static ImmutableArray<INamedTypeSymbol> GetPublicSymbols(this ImmutableArray<NamedTypeSymbol> symbols)
	{
		return StaticCast<Symbol>.From(symbols).GetPublicSymbols<INamedTypeSymbol>();
	}

	internal static ImmutableArray<ILocalSymbol> GetPublicSymbols(this ImmutableArray<LocalSymbol> symbols)
	{
		return StaticCast<Symbol>.From(symbols).GetPublicSymbols<ILocalSymbol>();
	}

	internal static ImmutableArray<IEventSymbol> GetPublicSymbols(this ImmutableArray<EventSymbol> symbols)
	{
		return StaticCast<Symbol>.From(symbols).GetPublicSymbols<IEventSymbol>();
	}

	internal static ImmutableArray<ITypeParameterSymbol> GetPublicSymbols(this ImmutableArray<TypeParameterSymbol> symbols)
	{
		return StaticCast<Symbol>.From(symbols).GetPublicSymbols<ITypeParameterSymbol>();
	}

	internal static ImmutableArray<IParameterSymbol> GetPublicSymbols(this ImmutableArray<ParameterSymbol> symbols)
	{
		return StaticCast<Symbol>.From(symbols).GetPublicSymbols<IParameterSymbol>();
	}

	internal static ImmutableArray<IMethodSymbol> GetPublicSymbols(this ImmutableArray<MethodSymbol> symbols)
	{
		return StaticCast<Symbol>.From(symbols).GetPublicSymbols<IMethodSymbol>();
	}

	internal static ImmutableArray<IAssemblySymbol> GetPublicSymbols(this ImmutableArray<AssemblySymbol> symbols)
	{
		return StaticCast<Symbol>.From(symbols).GetPublicSymbols<IAssemblySymbol>();
	}

	internal static ImmutableArray<IFieldSymbol> GetPublicSymbols(this ImmutableArray<FieldSymbol> symbols)
	{
		return StaticCast<Symbol>.From(symbols).GetPublicSymbols<IFieldSymbol>();
	}

	internal static ImmutableArray<INamespaceSymbol> GetPublicSymbols(this ImmutableArray<NamespaceSymbol> symbols)
	{
		return StaticCast<Symbol>.From(symbols).GetPublicSymbols<INamespaceSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static TSymbol? GetSymbol<TSymbol>(this ISymbol? symbol) where TSymbol : Symbol
	{
		return (TSymbol)(((Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.Symbol)symbol)?.UnderlyingSymbol);
	}

	[return: NotNullIfNotNull("symbol")]
	internal static Symbol? GetSymbol(this ISymbol? symbol)
	{
		return symbol.GetSymbol<Symbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static TypeSymbol? GetSymbol(this ITypeSymbol? symbol)
	{
		return symbol.GetSymbol<TypeSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static NamedTypeSymbol? GetSymbol(this INamedTypeSymbol? symbol)
	{
		return symbol.GetSymbol<NamedTypeSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static AliasSymbol? GetSymbol(this IAliasSymbol? symbol)
	{
		return symbol.GetSymbol<AliasSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static LocalSymbol? GetSymbol(this ILocalSymbol? symbol)
	{
		return symbol.GetSymbol<LocalSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static AssemblySymbol? GetSymbol(this IAssemblySymbol? symbol)
	{
		return symbol.GetSymbol<AssemblySymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static MethodSymbol? GetSymbol(this IMethodSymbol? symbol)
	{
		return symbol.GetSymbol<MethodSymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static PropertySymbol? GetSymbol(this IPropertySymbol? symbol)
	{
		return symbol.GetSymbol<PropertySymbol>();
	}

	[return: NotNullIfNotNull("symbol")]
	internal static FunctionPointerTypeSymbol? GetSymbol(this IFunctionPointerTypeSymbol? symbol)
	{
		return symbol.GetSymbol<FunctionPointerTypeSymbol>();
	}

	internal static bool IsRequired(this Symbol symbol)
	{
		if (symbol is FieldSymbol fieldSymbol)
		{
			if (fieldSymbol.IsRequired)
			{
				goto IL_0026;
			}
		}
		else if (symbol is PropertySymbol { IsRequired: not false })
		{
			goto IL_0026;
		}
		return false;
		IL_0026:
		return true;
	}

	internal static bool ShouldCheckRequiredMembers(this MethodSymbol method)
	{
		if ((object)method != null && method.MethodKind == MethodKind.Constructor)
		{
			return !method.HasSetsRequiredMembers;
		}
		return false;
	}

	internal static int GetOverloadResolutionPriority(this Symbol symbol)
	{
		if (!(symbol is MethodSymbol methodSymbol))
		{
			return ((PropertySymbol)symbol).OverloadResolutionPriority;
		}
		return methodSymbol.OverloadResolutionPriority;
	}

	internal static bool IsExtensionParameter(this ParameterSymbol parameter)
	{
		if (parameter.ContainingSymbol is NamedTypeSymbol namedTypeSymbol)
		{
			return namedTypeSymbol.IsExtension;
		}
		return false;
	}

	internal static bool IsExtensionParameterImplementation(this ParameterSymbol parameter)
	{
		if (parameter.ContainingSymbol is SourceExtensionImplementationMethodSymbol sourceExtensionImplementationMethodSymbol && !sourceExtensionImplementationMethodSymbol.UnderlyingMethod.IsStatic)
		{
			return parameter.Ordinal == 0;
		}
		return false;
	}
}
