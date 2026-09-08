using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class CSharpSymbolMatcher : SymbolMatcher
{
	private sealed class Visitor : CSharpSymbolVisitor<Symbol?>
	{
		private sealed class SymbolComparer
		{
			private readonly Visitor _matcher;

			private readonly DeepTranslator? _deepTranslator;

			public SymbolComparer(Visitor matcher, DeepTranslator? deepTranslator)
			{
				_matcher = matcher;
				_deepTranslator = deepTranslator;
			}

			public bool Equals(TypeSymbol source, TypeSymbol other)
			{
				if ((object)source == other)
				{
					return true;
				}
				TypeSymbol obj = (TypeSymbol)_matcher.Visit(source);
				TypeSymbol t = ((_deepTranslator != null) ? ((TypeSymbol)_deepTranslator.Visit(other)) : other);
				return obj?.Equals(t, TypeCompareKind.IgnoreDynamicAndTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes) ?? false;
			}
		}

		private readonly SynthesizedTypeMaps _otherSynthesizedTypes;

		private readonly SourceAssemblySymbol _sourceAssembly;

		private readonly AssemblySymbol _otherAssembly;

		private readonly IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> _otherSynthesizedMembers;

		private readonly IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> _otherDeletedMembers;

		private readonly SymbolComparer _comparer;

		private readonly ConcurrentDictionary<Symbol, Symbol?> _matches = new ConcurrentDictionary<Symbol, Symbol>(ReferenceEqualityComparer.Instance);

		private readonly ConcurrentDictionary<ISymbolInternal, IReadOnlyDictionary<string, ImmutableArray<ISymbolInternal>>> _otherMembers = new ConcurrentDictionary<ISymbolInternal, IReadOnlyDictionary<string, ImmutableArray<ISymbolInternal>>>(ReferenceEqualityComparer.Instance);

		public Visitor(SourceAssemblySymbol sourceAssembly, AssemblySymbol otherAssembly, SynthesizedTypeMaps otherSynthesizedTypes, IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> otherSynthesizedMembers, IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> otherDeletedMembers, DeepTranslator? deepTranslator)
		{
			_otherSynthesizedTypes = otherSynthesizedTypes;
			_sourceAssembly = sourceAssembly;
			_otherAssembly = otherAssembly;
			_otherSynthesizedMembers = otherSynthesizedMembers;
			_otherDeletedMembers = otherDeletedMembers;
			_comparer = new SymbolComparer(this, deepTranslator);
		}

		public override Symbol DefaultVisit(Symbol symbol)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Emitter/EditAndContinue/CSharpSymbolMatcher.cs", 150);
		}

		public override Symbol? Visit(Symbol symbol)
		{
			return _matches.GetOrAdd(symbol, base.Visit);
		}

		public override Symbol? VisitArrayType(ArrayTypeSymbol symbol)
		{
			TypeSymbol typeSymbol = (TypeSymbol)Visit(symbol.ElementType);
			if ((object)typeSymbol == null)
			{
				return null;
			}
			ImmutableArray<CustomModifier> customModifiers = VisitCustomModifiers(symbol.ElementTypeWithAnnotations.CustomModifiers);
			if (symbol.IsSZArray)
			{
				return ArrayTypeSymbol.CreateSZArray(_otherAssembly, symbol.ElementTypeWithAnnotations.WithTypeAndModifiers(typeSymbol, customModifiers));
			}
			return ArrayTypeSymbol.CreateMDArray(_otherAssembly, symbol.ElementTypeWithAnnotations.WithTypeAndModifiers(typeSymbol, customModifiers), symbol.Rank, symbol.Sizes, symbol.LowerBounds);
		}

		public override Symbol? VisitEvent(EventSymbol symbol)
		{
			return VisitNamedTypeMember(symbol, AreEventsEqual);
		}

		public override Symbol? VisitField(FieldSymbol symbol)
		{
			return VisitNamedTypeMember(symbol, AreFieldsEqual);
		}

		public override Symbol? VisitMethod(MethodSymbol symbol)
		{
			return VisitNamedTypeMember(symbol, AreMethodsEqual);
		}

		public override Symbol? VisitModule(ModuleSymbol module)
		{
			AssemblySymbol assemblySymbol = (AssemblySymbol)Visit(module.ContainingAssembly);
			if ((object)assemblySymbol == null)
			{
				return null;
			}
			if (module.Ordinal == 0)
			{
				return assemblySymbol.Modules[0];
			}
			for (int i = 1; i < assemblySymbol.Modules.Length; i++)
			{
				ModuleSymbol moduleSymbol = assemblySymbol.Modules[i];
				if (StringComparer.Ordinal.Equals(moduleSymbol.Name, module.Name))
				{
					return moduleSymbol;
				}
			}
			return null;
		}

		public override Symbol? VisitAssembly(AssemblySymbol assembly)
		{
			if (assembly.IsLinked)
			{
				return assembly;
			}
			if (IdentityEqualIgnoringVersionWildcard(assembly, _sourceAssembly))
			{
				return _otherAssembly;
			}
			foreach (AssemblySymbol referencedAssemblySymbol in _otherAssembly.Modules[0].ReferencedAssemblySymbols)
			{
				if (IdentityEqualIgnoringVersionWildcard(assembly, referencedAssemblySymbol))
				{
					return referencedAssemblySymbol;
				}
			}
			return null;
		}

		private static bool IdentityEqualIgnoringVersionWildcard(AssemblySymbol left, AssemblySymbol right)
		{
			AssemblyIdentity identity = left.Identity;
			AssemblyIdentity identity2 = right.Identity;
			if (AssemblyIdentityComparer.SimpleNameComparer.Equals(identity.Name, identity2.Name) && (left.AssemblyVersionPattern ?? identity.Version).Equals(right.AssemblyVersionPattern ?? identity2.Version))
			{
				return AssemblyIdentity.EqualIgnoringNameAndVersion(identity, identity2);
			}
			return false;
		}

		public override Symbol? VisitNamespace(NamespaceSymbol @namespace)
		{
			Symbol symbol = Visit(@namespace.ContainingSymbol);
			if ((object)symbol == null)
			{
				return null;
			}
			return symbol.Kind switch
			{
				SymbolKind.NetModule => ((ModuleSymbol)symbol).GlobalNamespace, 
				SymbolKind.Namespace => FindMatchingMember(symbol, @namespace, AreNamespacesEqual), 
				_ => throw ExceptionUtilities.UnexpectedValue(symbol.Kind), 
			};
		}

		public override Symbol VisitDynamicType(DynamicTypeSymbol symbol)
		{
			return _otherAssembly.GetSpecialType(SpecialType.System_Object);
		}

		public override Symbol? VisitNamedType(NamedTypeSymbol sourceType)
		{
			NamedTypeSymbol originalDefinition = sourceType.OriginalDefinition;
			if ((object)originalDefinition != sourceType)
			{
				CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
				ImmutableArray<TypeWithAnnotations> allTypeArguments = sourceType.GetAllTypeArguments(ref useSiteInfo);
				NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)Visit(originalDefinition);
				if ((object)namedTypeSymbol == null)
				{
					return null;
				}
				ImmutableArray<TypeParameterSymbol> allTypeParameters = namedTypeSymbol.GetAllTypeParameters();
				bool translationFailed = false;
				ImmutableArray<TypeWithAnnotations> to = allTypeArguments.SelectAsArray(delegate(TypeWithAnnotations t, Visitor v)
				{
					TypeSymbol typeSymbol = (TypeSymbol)v.Visit(t.Type);
					if ((object)typeSymbol == null)
					{
						translationFailed = true;
						typeSymbol = t.Type;
					}
					return t.WithTypeAndModifiers(typeSymbol, v.VisitCustomModifiers(t.CustomModifiers));
				}, this);
				if (translationFailed)
				{
					return null;
				}
				return new TypeMap(allTypeParameters, to, allowAlpha: true).SubstituteNamedType(namedTypeSymbol);
			}
			Symbol symbol = Visit(sourceType.ContainingSymbol);
			if ((object)symbol == null)
			{
				return null;
			}
			switch (symbol.Kind)
			{
			case SymbolKind.Namespace:
				if (sourceType is AnonymousTypeManager.AnonymousTypeTemplateSymbol template)
				{
					TryGetAnonymousTypeValue(template, out var otherType);
					return (NamedTypeSymbol)(otherType.Type?.GetInternalSymbol());
				}
				if (sourceType is AnonymousTypeManager.AnonymousDelegateTemplateSymbol anonymousDelegateTemplateSymbol)
				{
					if (anonymousDelegateTemplateSymbol.HasIndexedName)
					{
						TryGetAnonymousTypeValue(anonymousDelegateTemplateSymbol, out var otherType2);
						return (NamedTypeSymbol)(otherType2.Type?.GetInternalSymbol());
					}
					TryGetAnonymousDelegateValue(anonymousDelegateTemplateSymbol, out var otherDelegateSymbol);
					return (NamedTypeSymbol)(otherDelegateSymbol.Delegate?.GetInternalSymbol());
				}
				if (sourceType.IsAnonymousType)
				{
					return Visit(AnonymousTypeManager.TranslateAnonymousTypeSymbol(sourceType));
				}
				return FindMatchingMember(symbol, sourceType, AreNamedTypesEqual);
			case SymbolKind.NamedType:
				return FindMatchingMember(symbol, sourceType, AreNamedTypesEqual);
			default:
				throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
			}
		}

		public override Symbol VisitParameter(ParameterSymbol parameter)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Emitter/EditAndContinue/CSharpSymbolMatcher.cs", 385);
		}

		public override Symbol? VisitPointerType(PointerTypeSymbol symbol)
		{
			TypeSymbol typeSymbol = (TypeSymbol)Visit(symbol.PointedAtType);
			if ((object)typeSymbol == null)
			{
				return null;
			}
			ImmutableArray<CustomModifier> customModifiers = VisitCustomModifiers(symbol.PointedAtTypeWithAnnotations.CustomModifiers);
			return new PointerTypeSymbol(symbol.PointedAtTypeWithAnnotations.WithTypeAndModifiers(typeSymbol, customModifiers));
		}

		public override Symbol? VisitFunctionPointerType(FunctionPointerTypeSymbol symbol)
		{
			FunctionPointerMethodSymbol signature = symbol.Signature;
			TypeSymbol typeSymbol = (TypeSymbol)Visit(signature.ReturnType);
			if ((object)typeSymbol == null)
			{
				return null;
			}
			ImmutableArray<CustomModifier> refCustomModifiers = VisitCustomModifiers(signature.RefCustomModifiers);
			TypeWithAnnotations substitutedReturnType = signature.ReturnTypeWithAnnotations.WithTypeAndModifiers(typeSymbol, VisitCustomModifiers(signature.ReturnTypeWithAnnotations.CustomModifiers));
			ImmutableArray<TypeWithAnnotations> substitutedParameterTypes = ImmutableArray<TypeWithAnnotations>.Empty;
			ImmutableArray<ImmutableArray<CustomModifier>> paramRefCustomModifiers = default(ImmutableArray<ImmutableArray<CustomModifier>>);
			if (signature.ParameterCount > 0)
			{
				ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance(signature.ParameterCount);
				ArrayBuilder<ImmutableArray<CustomModifier>> instance2 = ArrayBuilder<ImmutableArray<CustomModifier>>.GetInstance(signature.ParameterCount);
				foreach (ParameterSymbol parameter in signature.Parameters)
				{
					TypeSymbol typeSymbol2 = (TypeSymbol)Visit(parameter.Type);
					if ((object)typeSymbol2 == null)
					{
						instance.Free();
						instance2.Free();
						return null;
					}
					instance2.Add(VisitCustomModifiers(parameter.RefCustomModifiers));
					instance.Add(parameter.TypeWithAnnotations.WithTypeAndModifiers(typeSymbol2, VisitCustomModifiers(parameter.TypeWithAnnotations.CustomModifiers)));
				}
				substitutedParameterTypes = instance.ToImmutableAndFree();
				paramRefCustomModifiers = instance2.ToImmutableAndFree();
			}
			return symbol.SubstituteTypeSymbol(substitutedReturnType, substitutedParameterTypes, refCustomModifiers, paramRefCustomModifiers);
		}

		public override Symbol? VisitProperty(PropertySymbol symbol)
		{
			return VisitNamedTypeMember(symbol, ArePropertiesEqual);
		}

		public override Symbol VisitTypeParameter(TypeParameterSymbol symbol)
		{
			if (symbol is IndexedTypeParameterSymbol result)
			{
				return result;
			}
			Symbol symbol2 = Visit(symbol.ContainingSymbol);
			ImmutableArray<TypeParameterSymbol> typeParameters;
			switch (symbol2.Kind)
			{
			case SymbolKind.ErrorType:
			case SymbolKind.NamedType:
				typeParameters = ((NamedTypeSymbol)symbol2).TypeParameters;
				break;
			case SymbolKind.Method:
				typeParameters = ((MethodSymbol)symbol2).TypeParameters;
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(symbol2.Kind);
			}
			ImmutableArray<TypeParameterSymbol> immutableArray = typeParameters;
			return immutableArray[symbol.Ordinal];
		}

		private ImmutableArray<CustomModifier> VisitCustomModifiers(ImmutableArray<CustomModifier> modifiers)
		{
			return modifiers.SelectAsArray(VisitCustomModifier);
		}

		private CustomModifier VisitCustomModifier(CustomModifier modifier)
		{
			NamedTypeSymbol modifier2 = (NamedTypeSymbol)Visit(((CSharpCustomModifier)modifier).ModifierSymbol);
			if (!modifier.IsOptional)
			{
				return CSharpCustomModifier.CreateRequired(modifier2);
			}
			return CSharpCustomModifier.CreateOptional(modifier2);
		}

		private bool TryGetAnonymousDelegateValue(AnonymousTypeManager.AnonymousDelegateTemplateSymbol delegateSymbol, out SynthesizedDelegateValue otherDelegateSymbol)
		{
			SynthesizedDelegateKey key = new SynthesizedDelegateKey(delegateSymbol.MetadataName);
			return _otherSynthesizedTypes.AnonymousDelegates.TryGetValue(key, out otherDelegateSymbol);
		}

		internal bool TryGetAnonymousTypeValue(AnonymousTypeManager.AnonymousTypeOrDelegateTemplateSymbol template, out AnonymousTypeValue otherType)
		{
			if (template is AnonymousTypeManager.AnonymousTypeTemplateSymbol anonymousTypeTemplateSymbol)
			{
				return _otherSynthesizedTypes.AnonymousTypes.TryGetValue(anonymousTypeTemplateSymbol.GetAnonymousTypeKey(), out otherType);
			}
			AnonymousTypeManager.AnonymousDelegateTemplateSymbol anonymousDelegateTemplateSymbol = (AnonymousTypeManager.AnonymousDelegateTemplateSymbol)template;
			otherType = default(AnonymousTypeValue);
			AnonymousDelegateWithIndexedNamePartialKey key = new AnonymousDelegateWithIndexedNamePartialKey(anonymousDelegateTemplateSymbol.Arity, anonymousDelegateTemplateSymbol.DelegateInvokeMethod.ParameterCount);
			if (_otherSynthesizedTypes.AnonymousDelegatesWithIndexedNames.TryGetValue(key, out var value))
			{
				return TryGetMatchingDelegateWithIndexedName(anonymousDelegateTemplateSymbol, value, out otherType);
			}
			return false;
		}

		internal bool TryGetMatchingDelegateWithIndexedName(AnonymousTypeManager.AnonymousDelegateTemplateSymbol delegateTemplate, ImmutableArray<AnonymousTypeValue> values, out AnonymousTypeValue match)
		{
			foreach (AnonymousTypeValue item in values)
			{
				NamedTypeSymbol otherType = (NamedTypeSymbol)item.Type.GetInternalSymbol();
				if (isCorrespondingAnonymousDelegate(delegateTemplate, otherType))
				{
					match = item;
					return true;
				}
			}
			match = default(AnonymousTypeValue);
			return false;
			bool isCorrespondingAnonymousDelegate(NamedTypeSymbol type, NamedTypeSymbol namedTypeSymbol)
			{
				type = SubstituteTypeParameters(type);
				namedTypeSymbol = SubstituteTypeParameters(namedTypeSymbol);
				MethodSymbol delegateInvokeMethod = type.DelegateInvokeMethod;
				if ((object)delegateInvokeMethod != null)
				{
					MethodSymbol delegateInvokeMethod2 = namedTypeSymbol.DelegateInvokeMethod;
					if ((object)delegateInvokeMethod2 != null && delegateInvokeMethod.Parameters.SequenceEqual(delegateInvokeMethod2.Parameters, (ParameterSymbol x, ParameterSymbol y) => isCorrespondingType(x.TypeWithAnnotations, y.TypeWithAnnotations) && x.ExplicitDefaultConstantValue == y.ExplicitDefaultConstantValue && x.IsParamsArray == y.IsParamsArray && x.IsParamsCollection == y.IsParamsCollection))
					{
						return isCorrespondingType(delegateInvokeMethod.ReturnTypeWithAnnotations, delegateInvokeMethod2.ReturnTypeWithAnnotations);
					}
				}
				return false;
			}
			bool isCorrespondingType(TypeWithAnnotations type, TypeWithAnnotations expectedType)
			{
				return type.WithTypeAndModifiers((TypeSymbol)Visit(type.Type), VisitCustomModifiers(type.CustomModifiers)).Equals(expectedType, TypeCompareKind.CLRSignatureCompareOptions);
			}
		}

		private Symbol? VisitNamedTypeMember<T>(T member, Func<T, T, bool> predicate) where T : Symbol
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)Visit(member.ContainingType);
			if ((object)namedTypeSymbol == null)
			{
				return null;
			}
			return FindMatchingMember(namedTypeSymbol, member, predicate);
		}

		private T? FindMatchingMember<T>(ISymbolInternal otherTypeOrNamespace, T sourceMember, Func<T, T, bool> predicate) where T : Symbol
		{
			if (_otherMembers.GetOrAdd(otherTypeOrNamespace, GetAllEmittedMembers).TryGetValue(sourceMember.MetadataName, out ImmutableArray<ISymbolInternal> value))
			{
				foreach (ISymbolInternal item in value)
				{
					if (item is T val && predicate(sourceMember, val))
					{
						return val;
					}
				}
			}
			return null;
		}

		private bool AreArrayTypesEqual(ArrayTypeSymbol type, ArrayTypeSymbol other)
		{
			if (type.HasSameShapeAs(other))
			{
				return AreTypesEqual(type.ElementType, other.ElementType);
			}
			return false;
		}

		private bool AreEventsEqual(EventSymbol @event, EventSymbol other)
		{
			return true;
		}

		private bool AreFieldsEqual(FieldSymbol field, FieldSymbol other)
		{
			return _comparer.Equals(field.Type, other.Type);
		}

		private bool AreMethodsEqual(MethodSymbol method, MethodSymbol other)
		{
			method = SubstituteTypeParameters(method);
			other = SubstituteTypeParameters(other);
			if (_comparer.Equals(method.ReturnType, other.ReturnType) && method.RefKind.Equals(other.RefKind) && method.Parameters.SequenceEqual(other.Parameters, AreParametersEqual))
			{
				return method.TypeParameters.SequenceEqual(other.TypeParameters, AreTypesEqual);
			}
			return false;
		}

		private static MethodSymbol SubstituteTypeParameters(MethodSymbol method)
		{
			int length = method.TypeParameters.Length;
			if (length == 0)
			{
				return method;
			}
			return method.Construct(IndexedTypeParameterSymbol.Take(length));
		}

		private bool AreNamedTypesEqual(NamedTypeSymbol type, NamedTypeSymbol other)
		{
			return type.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics.SequenceEqual(other.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics, AreTypesEqual);
		}

		private static NamedTypeSymbol SubstituteTypeParameters(NamedTypeSymbol type)
		{
			int length = type.TypeParameters.Length;
			if (length == 0)
			{
				return type;
			}
			return type.Construct(IndexedTypeParameterSymbol.Take(length));
		}

		private bool AreNamespacesEqual(NamespaceSymbol @namespace, NamespaceSymbol other)
		{
			return true;
		}

		private bool AreParametersEqual(ParameterSymbol parameter, ParameterSymbol other)
		{
			if (parameter.RefKind == RefKind.None == (other.RefKind == RefKind.None))
			{
				return _comparer.Equals(parameter.Type, other.Type);
			}
			return false;
		}

		private bool ArePointerTypesEqual(PointerTypeSymbol type, PointerTypeSymbol other)
		{
			return AreTypesEqual(type.PointedAtType, other.PointedAtType);
		}

		private bool AreFunctionPointerTypesEqual(FunctionPointerTypeSymbol type, FunctionPointerTypeSymbol other)
		{
			FunctionPointerMethodSymbol signature = type.Signature;
			FunctionPointerMethodSymbol signature2 = other.Signature;
			if (signature.RefKind != signature2.RefKind || !AreTypesEqual(signature.ReturnTypeWithAnnotations, signature2.ReturnTypeWithAnnotations))
			{
				return false;
			}
			return signature.Parameters.SequenceEqual(signature2.Parameters, AreFunctionPointerParametersEqual);
		}

		private bool AreFunctionPointerParametersEqual(ParameterSymbol param, ParameterSymbol otherParam)
		{
			if (param.RefKind == otherParam.RefKind)
			{
				return AreTypesEqual(param.TypeWithAnnotations, otherParam.TypeWithAnnotations);
			}
			return false;
		}

		[Conditional("DEBUG")]
		private static void ValidateFunctionPointerParamOrReturn(TypeWithAnnotations type, RefKind refKind, ImmutableArray<CustomModifier> refCustomModifiers, bool allowOut)
		{
		}

		private bool ArePropertiesEqual(PropertySymbol property, PropertySymbol other)
		{
			if (_comparer.Equals(property.Type, other.Type) && property.RefKind.Equals(other.RefKind))
			{
				return property.Parameters.SequenceEqual(other.Parameters, AreParametersEqual);
			}
			return false;
		}

		private static bool AreTypeParametersEqual(TypeParameterSymbol type, TypeParameterSymbol other)
		{
			return true;
		}

		private bool AreTypesEqual(TypeWithAnnotations type, TypeWithAnnotations other)
		{
			return AreTypesEqual(type.Type, other.Type);
		}

		private bool AreTypesEqual(TypeSymbol type, TypeSymbol other)
		{
			if (type.Kind != other.Kind)
			{
				return false;
			}
			switch (type.Kind)
			{
			case SymbolKind.ArrayType:
				return AreArrayTypesEqual((ArrayTypeSymbol)type, (ArrayTypeSymbol)other);
			case SymbolKind.PointerType:
				return ArePointerTypesEqual((PointerTypeSymbol)type, (PointerTypeSymbol)other);
			case SymbolKind.FunctionPointerType:
				return AreFunctionPointerTypesEqual((FunctionPointerTypeSymbol)type, (FunctionPointerTypeSymbol)other);
			case SymbolKind.ErrorType:
			case SymbolKind.NamedType:
				return AreNamedTypesEqual((NamedTypeSymbol)type, (NamedTypeSymbol)other);
			case SymbolKind.TypeParameter:
				return AreTypeParametersEqual((TypeParameterSymbol)type, (TypeParameterSymbol)other);
			default:
				throw ExceptionUtilities.UnexpectedValue(type.Kind);
			}
		}

		private IReadOnlyDictionary<string, ImmutableArray<ISymbolInternal>> GetAllEmittedMembers(ISymbolInternal symbol)
		{
			ArrayBuilder<ISymbolInternal> instance = ArrayBuilder<ISymbolInternal>.GetInstance();
			if (symbol.Kind == SymbolKind.NamedType)
			{
				NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)symbol;
				instance.AddRange(namedTypeSymbol.GetEventsToEmit());
				instance.AddRange(namedTypeSymbol.GetFieldsToEmit());
				instance.AddRange(namedTypeSymbol.GetMethodsToEmit());
				instance.AddRange(namedTypeSymbol.GetTypeMembers());
				instance.AddRange(namedTypeSymbol.GetPropertiesToEmit());
			}
			else
			{
				instance.AddRange(((NamespaceSymbol)symbol).GetMembers());
			}
			if (_otherSynthesizedMembers.TryGetValue(symbol, out ImmutableArray<ISymbolInternal> value))
			{
				instance.AddRange(value);
			}
			if (_otherDeletedMembers.TryGetValue(symbol, out ImmutableArray<ISymbolInternal> value2))
			{
				instance.AddRange(value2);
			}
			Dictionary<string, ImmutableArray<ISymbolInternal>> result = instance.ToDictionary((ISymbolInternal s) => s.MetadataName, StringOrdinalComparer.Instance);
			instance.Free();
			return result;
		}
	}

	internal sealed class DeepTranslator : CSharpSymbolVisitor<Symbol>
	{
		private readonly ConcurrentDictionary<Symbol, Symbol> _matches;

		private readonly NamedTypeSymbol _systemObject;

		public DeepTranslator(NamedTypeSymbol systemObject)
		{
			_matches = new ConcurrentDictionary<Symbol, Symbol>(ReferenceEqualityComparer.Instance);
			_systemObject = systemObject;
		}

		public override Symbol DefaultVisit(Symbol symbol)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Emitter/EditAndContinue/CSharpSymbolMatcher.cs", 865);
		}

		public override Symbol Visit(Symbol symbol)
		{
			return _matches.GetOrAdd(symbol, base.Visit(symbol));
		}

		public override Symbol VisitArrayType(ArrayTypeSymbol symbol)
		{
			TypeSymbol typeSymbol = (TypeSymbol)Visit(symbol.ElementType);
			ImmutableArray<CustomModifier> customModifiers = VisitCustomModifiers(symbol.ElementTypeWithAnnotations.CustomModifiers);
			if (symbol.IsSZArray)
			{
				return ArrayTypeSymbol.CreateSZArray(symbol.BaseTypeNoUseSiteDiagnostics.ContainingAssembly, symbol.ElementTypeWithAnnotations.WithTypeAndModifiers(typeSymbol, customModifiers));
			}
			return ArrayTypeSymbol.CreateMDArray(symbol.BaseTypeNoUseSiteDiagnostics.ContainingAssembly, symbol.ElementTypeWithAnnotations.WithTypeAndModifiers(typeSymbol, customModifiers), symbol.Rank, symbol.Sizes, symbol.LowerBounds);
		}

		public override Symbol VisitDynamicType(DynamicTypeSymbol symbol)
		{
			return _systemObject;
		}

		public override Symbol VisitNamedType(NamedTypeSymbol type)
		{
			NamedTypeSymbol originalDefinition = type.OriginalDefinition;
			if ((object)originalDefinition != type)
			{
				CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
				ImmutableArray<TypeWithAnnotations> to = type.GetAllTypeArguments(ref useSiteInfo).SelectAsArray((TypeWithAnnotations t, DeepTranslator v) => t.WithTypeAndModifiers((TypeSymbol)v.Visit(t.Type), v.VisitCustomModifiers(t.CustomModifiers)), this);
				NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)Visit(originalDefinition);
				return new TypeMap(namedTypeSymbol.GetAllTypeParameters(), to, allowAlpha: true).SubstituteNamedType(namedTypeSymbol);
			}
			if (type.IsAnonymousType)
			{
				return Visit(AnonymousTypeManager.TranslateAnonymousTypeSymbol(type));
			}
			return type;
		}

		public override Symbol VisitPointerType(PointerTypeSymbol symbol)
		{
			TypeSymbol typeSymbol = (TypeSymbol)Visit(symbol.PointedAtType);
			ImmutableArray<CustomModifier> customModifiers = VisitCustomModifiers(symbol.PointedAtTypeWithAnnotations.CustomModifiers);
			return new PointerTypeSymbol(symbol.PointedAtTypeWithAnnotations.WithTypeAndModifiers(typeSymbol, customModifiers));
		}

		public override Symbol VisitFunctionPointerType(FunctionPointerTypeSymbol symbol)
		{
			FunctionPointerMethodSymbol signature = symbol.Signature;
			TypeSymbol typeSymbol = (TypeSymbol)Visit(signature.ReturnType);
			TypeWithAnnotations substitutedReturnType = signature.ReturnTypeWithAnnotations.WithTypeAndModifiers(typeSymbol, VisitCustomModifiers(signature.ReturnTypeWithAnnotations.CustomModifiers));
			ImmutableArray<CustomModifier> refCustomModifiers = VisitCustomModifiers(signature.RefCustomModifiers);
			ImmutableArray<TypeWithAnnotations> substitutedParameterTypes = ImmutableArray<TypeWithAnnotations>.Empty;
			ImmutableArray<ImmutableArray<CustomModifier>> paramRefCustomModifiers = default(ImmutableArray<ImmutableArray<CustomModifier>>);
			if (signature.ParameterCount > 0)
			{
				ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance(signature.ParameterCount);
				ArrayBuilder<ImmutableArray<CustomModifier>> instance2 = ArrayBuilder<ImmutableArray<CustomModifier>>.GetInstance(signature.ParameterCount);
				foreach (ParameterSymbol parameter in signature.Parameters)
				{
					TypeSymbol typeSymbol2 = (TypeSymbol)Visit(parameter.Type);
					instance.Add(parameter.TypeWithAnnotations.WithTypeAndModifiers(typeSymbol2, VisitCustomModifiers(parameter.TypeWithAnnotations.CustomModifiers)));
					instance2.Add(VisitCustomModifiers(parameter.RefCustomModifiers));
				}
				substitutedParameterTypes = instance.ToImmutableAndFree();
				paramRefCustomModifiers = instance2.ToImmutableAndFree();
			}
			return symbol.SubstituteTypeSymbol(substitutedReturnType, substitutedParameterTypes, refCustomModifiers, paramRefCustomModifiers);
		}

		public override Symbol VisitTypeParameter(TypeParameterSymbol symbol)
		{
			return symbol;
		}

		private ImmutableArray<CustomModifier> VisitCustomModifiers(ImmutableArray<CustomModifier> modifiers)
		{
			return modifiers.SelectAsArray(VisitCustomModifier);
		}

		private CustomModifier VisitCustomModifier(CustomModifier modifier)
		{
			NamedTypeSymbol modifier2 = (NamedTypeSymbol)Visit(((CSharpCustomModifier)modifier).ModifierSymbol);
			if (!modifier.IsOptional)
			{
				return CSharpCustomModifier.CreateRequired(modifier2);
			}
			return CSharpCustomModifier.CreateOptional(modifier2);
		}
	}

	private readonly Visitor _visitor;

	public CSharpSymbolMatcher(SourceAssemblySymbol sourceAssembly, SourceAssemblySymbol otherAssembly, SynthesizedTypeMaps otherSynthesizedTypes, IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> otherSynthesizedMembers, IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> otherDeletedMembers)
	{
		_visitor = new Visitor(sourceAssembly, otherAssembly, otherSynthesizedTypes, otherSynthesizedMembers, otherDeletedMembers, new DeepTranslator(otherAssembly.GetSpecialType(SpecialType.System_Object)));
	}

	public CSharpSymbolMatcher(SourceAssemblySymbol sourceAssembly, PEAssemblySymbol otherAssembly, SynthesizedTypeMaps otherSynthesizedTypes)
	{
		_visitor = new Visitor(sourceAssembly, otherAssembly, otherSynthesizedTypes, SpecializedCollections.EmptyReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>>(), SpecializedCollections.EmptyReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>>(), null);
	}

	public override IDefinition? MapDefinition(IDefinition definition)
	{
		if (definition.GetInternalSymbol() is Symbol symbol)
		{
			return (IDefinition)(_visitor.Visit(symbol)?.GetCciAdapter());
		}
		return null;
	}

	public override INamespace? MapNamespace(INamespace @namespace)
	{
		if (@namespace.GetInternalSymbol() is NamespaceSymbol symbol)
		{
			return (INamespace)(_visitor.Visit(symbol)?.GetCciAdapter());
		}
		return null;
	}

	public override ITypeReference? MapReference(ITypeReference reference)
	{
		if (reference.GetInternalSymbol() is Symbol symbol)
		{
			return (ITypeReference)(_visitor.Visit(symbol)?.GetCciAdapter());
		}
		return null;
	}

	internal bool TryGetAnonymousTypeValue(AnonymousTypeManager.AnonymousTypeOrDelegateTemplateSymbol template, out AnonymousTypeValue typeValue)
	{
		return _visitor.TryGetAnonymousTypeValue(template, out typeValue);
	}

	protected override bool TryGetMatchingDelegateWithIndexedName(INamedTypeSymbolInternal delegateTemplate, ImmutableArray<AnonymousTypeValue> values, out AnonymousTypeValue match)
	{
		return _visitor.TryGetMatchingDelegateWithIndexedName((AnonymousTypeManager.AnonymousDelegateTemplateSymbol)delegateTemplate, values, out match);
	}
}
