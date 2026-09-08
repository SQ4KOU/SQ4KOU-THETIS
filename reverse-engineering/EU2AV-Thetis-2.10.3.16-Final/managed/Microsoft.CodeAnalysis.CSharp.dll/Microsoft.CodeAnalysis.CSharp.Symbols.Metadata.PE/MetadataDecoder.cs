using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.ErrorReporting;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal class MetadataDecoder : MetadataDecoder<PEModuleSymbol, TypeSymbol, MethodSymbol, FieldSymbol, Symbol>
{
	private readonly PENamedTypeSymbol _typeContextOpt;

	private readonly PEMethodSymbol _methodContextOpt;

	internal PEModuleSymbol ModuleSymbol => moduleSymbol;

	public MetadataDecoder(PEModuleSymbol moduleSymbol, PENamedTypeSymbol context)
		: this(moduleSymbol, context, null)
	{
	}

	public MetadataDecoder(PEModuleSymbol moduleSymbol, PEMethodSymbol context)
		: this(moduleSymbol, (PENamedTypeSymbol)context.ContainingType, context)
	{
	}

	public MetadataDecoder(PEModuleSymbol moduleSymbol)
		: this(moduleSymbol, null, null)
	{
	}

	private MetadataDecoder(PEModuleSymbol moduleSymbol, PENamedTypeSymbol typeContextOpt, PEMethodSymbol methodContextOpt)
		: base(moduleSymbol.Module, (moduleSymbol.ContainingAssembly is PEAssemblySymbol) ? moduleSymbol.ContainingAssembly.Identity : null, (SymbolFactory<PEModuleSymbol, TypeSymbol>)SymbolFactory.Instance, moduleSymbol)
	{
		_typeContextOpt = typeContextOpt;
		_methodContextOpt = methodContextOpt;
	}

	protected override TypeSymbol GetGenericMethodTypeParamSymbol(int position)
	{
		if ((object)_methodContextOpt == null)
		{
			return new UnsupportedMetadataTypeSymbol();
		}
		ImmutableArray<TypeParameterSymbol> typeParameters = _methodContextOpt.TypeParameters;
		if (typeParameters.Length <= position)
		{
			return new UnsupportedMetadataTypeSymbol();
		}
		return typeParameters[position];
	}

	protected override TypeSymbol GetGenericTypeParamSymbol(int position)
	{
		PENamedTypeSymbol pENamedTypeSymbol = _typeContextOpt;
		while ((object)pENamedTypeSymbol != null && pENamedTypeSymbol.MetadataArity - pENamedTypeSymbol.Arity > position)
		{
			pENamedTypeSymbol = pENamedTypeSymbol.ContainingSymbol as PENamedTypeSymbol;
		}
		if ((object)pENamedTypeSymbol == null || pENamedTypeSymbol.MetadataArity <= position)
		{
			return new UnsupportedMetadataTypeSymbol();
		}
		position -= pENamedTypeSymbol.MetadataArity - pENamedTypeSymbol.Arity;
		return pENamedTypeSymbol.TypeParameters[position];
	}

	protected override ConcurrentDictionary<TypeDefinitionHandle, TypeSymbol> GetTypeHandleToTypeMap()
	{
		return moduleSymbol.TypeHandleToTypeMap;
	}

	protected override ConcurrentDictionary<TypeReferenceHandle, TypeSymbol> GetTypeRefHandleToTypeMap()
	{
		return moduleSymbol.TypeRefHandleToTypeMap;
	}

	protected override TypeSymbol LookupNestedTypeDefSymbol(TypeSymbol container, ref MetadataTypeName emittedName)
	{
		return container.LookupMetadataType(ref emittedName) ?? new MissingMetadataTypeSymbol.Nested((NamedTypeSymbol)container, ref emittedName);
	}

	protected override TypeSymbol LookupTopLevelTypeDefSymbol(int referencedAssemblyIndex, ref MetadataTypeName emittedName)
	{
		AssemblySymbol referencedAssemblySymbol = moduleSymbol.GetReferencedAssemblySymbol(referencedAssemblyIndex);
		if ((object)referencedAssemblySymbol == null)
		{
			return new UnsupportedMetadataTypeSymbol();
		}
		try
		{
			return referencedAssemblySymbol.LookupDeclaredOrForwardedTopLevelMetadataType(ref emittedName, null);
		}
		catch (Exception exception) when (FatalError.ReportAndPropagate(exception))
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/MetadataDecoder.cs", 148);
		}
	}

	protected override TypeSymbol LookupTopLevelTypeDefSymbol(string moduleName, ref MetadataTypeName emittedName, out bool isNoPiaLocalType)
	{
		foreach (ModuleSymbol module in moduleSymbol.ContainingAssembly.Modules)
		{
			if (string.Equals(module.Name, moduleName, StringComparison.OrdinalIgnoreCase))
			{
				if ((object)module == moduleSymbol)
				{
					return moduleSymbol.LookupTopLevelMetadataTypeWithNoPiaLocalTypeUnification(ref emittedName, out isNoPiaLocalType);
				}
				isNoPiaLocalType = false;
				return module.LookupTopLevelMetadataType(ref emittedName) ?? new MissingMetadataTypeSymbol.TopLevel(module, ref emittedName);
			}
		}
		isNoPiaLocalType = false;
		return new MissingMetadataTypeSymbol.TopLevel(new MissingModuleSymbolWithName(moduleSymbol.ContainingAssembly, moduleName), ref emittedName, SpecialType.None);
	}

	protected override TypeSymbol LookupTopLevelTypeDefSymbol(ref MetadataTypeName emittedName, out bool isNoPiaLocalType)
	{
		return moduleSymbol.LookupTopLevelMetadataTypeWithNoPiaLocalTypeUnification(ref emittedName, out isNoPiaLocalType);
	}

	protected override int GetIndexOfReferencedAssembly(AssemblyIdentity identity)
	{
		ImmutableArray<AssemblyIdentity> referencedAssemblies = moduleSymbol.GetReferencedAssemblies();
		for (int i = 0; i < referencedAssemblies.Length; i++)
		{
			if (identity.Equals(referencedAssemblies[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static bool IsOrClosedOverATypeFromAssemblies(TypeSymbol symbol, ImmutableArray<AssemblySymbol> assemblies)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.TypeParameter:
			return false;
		case SymbolKind.ArrayType:
			return IsOrClosedOverATypeFromAssemblies(((ArrayTypeSymbol)symbol).ElementType, assemblies);
		case SymbolKind.PointerType:
			return IsOrClosedOverATypeFromAssemblies(((PointerTypeSymbol)symbol).PointedAtType, assemblies);
		case SymbolKind.DynamicType:
			return false;
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)symbol;
			AssemblySymbol containingAssembly = symbol.OriginalDefinition.ContainingAssembly;
			if ((object)containingAssembly != null)
			{
				for (int i = 0; i < assemblies.Length; i++)
				{
					if ((object)containingAssembly == assemblies[i])
					{
						return true;
					}
				}
			}
			do
			{
				ImmutableArray<TypeWithAnnotations> typeArgumentsWithAnnotationsNoUseSiteDiagnostics = namedTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics;
				int length = typeArgumentsWithAnnotationsNoUseSiteDiagnostics.Length;
				for (int i = 0; i < length; i++)
				{
					if (IsOrClosedOverATypeFromAssemblies(typeArgumentsWithAnnotationsNoUseSiteDiagnostics[i].Type, assemblies))
					{
						return true;
					}
				}
				namedTypeSymbol = namedTypeSymbol.ContainingType;
			}
			while ((object)namedTypeSymbol != null);
			return false;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
	}

	protected override TypeSymbol SubstituteNoPiaLocalType(TypeDefinitionHandle typeDef, ref MetadataTypeName name, string interfaceGuid, string scope, string identifier)
	{
		TypeSymbol value;
		try
		{
			bool flag = Module.IsInterfaceOrThrow(typeDef);
			TypeSymbol baseType = null;
			if (!flag)
			{
				EntityHandle baseTypeOfTypeOrThrow = Module.GetBaseTypeOfTypeOrThrow(typeDef);
				if (!baseTypeOfTypeOrThrow.IsNil)
				{
					baseType = GetTypeOfToken(baseTypeOfTypeOrThrow);
				}
			}
			value = SubstituteNoPiaLocalType(ref name, flag, baseType, interfaceGuid, scope, identifier, moduleSymbol.ContainingAssembly);
		}
		catch (BadImageFormatException exception)
		{
			value = GetUnsupportedMetadataTypeSymbol(exception);
		}
		return GetTypeHandleToTypeMap().GetOrAdd(typeDef, value);
	}

	internal static NamedTypeSymbol SubstituteNoPiaLocalType(ref MetadataTypeName name, bool isInterface, TypeSymbol baseType, string? interfaceGuid, string? scope, string? identifier, AssemblySymbol referringAssembly)
	{
		NamedTypeSymbol namedTypeSymbol = null;
		Guid result = default(Guid);
		bool flag = false;
		Guid result2 = default(Guid);
		bool flag2 = false;
		if (isInterface && interfaceGuid != null)
		{
			flag = Guid.TryParse(interfaceGuid, out result);
			if (flag)
			{
				scope = null;
				identifier = null;
			}
		}
		if (scope != null)
		{
			flag2 = Guid.TryParse(scope, out result2);
		}
		foreach (AssemblySymbol noPiaResolutionAssembly in referringAssembly.GetNoPiaResolutionAssemblies())
		{
			if ((object)noPiaResolutionAssembly == referringAssembly)
			{
				continue;
			}
			NamedTypeSymbol namedTypeSymbol2 = noPiaResolutionAssembly.LookupDeclaredTopLevelMetadataType(ref name);
			if ((object)namedTypeSymbol2 == null || namedTypeSymbol2.DeclaredAccessibility != Accessibility.Public)
			{
				continue;
			}
			bool flag3 = false;
			Guid result3 = default(Guid);
			string guidString;
			switch (namedTypeSymbol2.TypeKind)
			{
			case TypeKind.Interface:
				if (!isInterface)
				{
					continue;
				}
				if (namedTypeSymbol2.GetGuidString(out guidString) && guidString != null)
				{
					flag3 = Guid.TryParse(guidString, out result3);
				}
				break;
			case TypeKind.Delegate:
			case TypeKind.Enum:
			case TypeKind.Struct:
			{
				if (isInterface)
				{
					continue;
				}
				SpecialType specialType = namedTypeSymbol2.BaseTypeNoUseSiteDiagnostics?.SpecialType ?? SpecialType.None;
				if (specialType == SpecialType.None || specialType != (baseType?.SpecialType ?? SpecialType.None))
				{
					continue;
				}
				break;
			}
			default:
				continue;
			}
			if (flag | flag3)
			{
				if (!flag || !flag3 || result3 != result)
				{
					continue;
				}
			}
			else
			{
				if (!flag2 || identifier == null || !identifier.Equals(name.FullName))
				{
					continue;
				}
				flag3 = false;
				if (noPiaResolutionAssembly.GetGuidString(out guidString) && guidString != null)
				{
					flag3 = Guid.TryParse(guidString, out result3);
				}
				if (!flag3 || result2 != result3)
				{
					continue;
				}
			}
			if ((object)namedTypeSymbol != null)
			{
				namedTypeSymbol = new NoPiaAmbiguousCanonicalTypeSymbol(referringAssembly, namedTypeSymbol, namedTypeSymbol2);
				break;
			}
			namedTypeSymbol = namedTypeSymbol2;
		}
		if ((object)namedTypeSymbol == null)
		{
			namedTypeSymbol = new NoPiaMissingCanonicalTypeSymbol(referringAssembly, name.FullName, interfaceGuid, scope, identifier);
		}
		return namedTypeSymbol;
	}

	protected override MethodSymbol FindMethodSymbolInType(TypeSymbol typeSymbol, MethodDefinitionHandle targetMethodDef)
	{
		if (typeSymbol is PENamedTypeSymbol pENamedTypeSymbol && (object)pENamedTypeSymbol.ContainingPEModule == moduleSymbol)
		{
			foreach (Symbol item in typeSymbol.GetMembersUnordered())
			{
				if (item is PEMethodSymbol pEMethodSymbol && pEMethodSymbol.Handle == targetMethodDef)
				{
					return pEMethodSymbol;
				}
			}
		}
		else if (!(typeSymbol is ErrorTypeSymbol))
		{
			return (MethodSymbol)new MemberRefMetadataDecoder(moduleSymbol, typeSymbol).FindMember(targetMethodDef, methodsOnly: true);
		}
		return null;
	}

	protected override FieldSymbol FindFieldSymbolInType(TypeSymbol typeSymbol, FieldDefinitionHandle fieldDef)
	{
		foreach (Symbol item in typeSymbol.GetMembersUnordered())
		{
			if (item is PEFieldSymbol pEFieldSymbol && pEFieldSymbol.Handle == fieldDef)
			{
				return pEFieldSymbol;
			}
		}
		return null;
	}

	internal override Symbol GetSymbolForMemberRef(MemberReferenceHandle memberRef, TypeSymbol scope = null, bool methodsOnly = false)
	{
		TypeSymbol typeSymbol = GetMemberRefTypeSymbol(memberRef);
		if ((object)typeSymbol == null)
		{
			return null;
		}
		if ((object)scope != null)
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			if (!TypeSymbol.Equals(scope, typeSymbol, TypeCompareKind.ConsiderEverything) && !(typeSymbol.IsInterfaceType() ? (scope.AllInterfacesNoUseSiteDiagnostics.IndexOf((NamedTypeSymbol)typeSymbol, 0, SymbolEqualityComparer.CLRSignature) != -1) : scope.IsDerivedFrom(typeSymbol, TypeCompareKind.CLRSignatureCompareOptions, ref useSiteInfo)))
			{
				return null;
			}
		}
		if (!typeSymbol.IsTupleType)
		{
			typeSymbol = TupleTypeDecoder.DecodeTupleTypesIfApplicable(typeSymbol, default(ImmutableArray<string>));
		}
		Symbol symbol = new MemberRefMetadataDecoder(moduleSymbol, typeSymbol.OriginalDefinition).FindMember(memberRef, methodsOnly);
		if ((object)symbol != null && !typeSymbol.IsDefinition)
		{
			return symbol.SymbolAsMember((NamedTypeSymbol)typeSymbol);
		}
		return symbol;
	}

	protected override void EnqueueTypeSymbolInterfacesAndBaseTypes(Queue<TypeDefinitionHandle> typeDefsToSearch, Queue<TypeSymbol> typeSymbolsToSearch, TypeSymbol typeSymbol)
	{
		foreach (NamedTypeSymbol item in typeSymbol.InterfacesNoUseSiteDiagnostics())
		{
			EnqueueTypeSymbol(typeDefsToSearch, typeSymbolsToSearch, item);
		}
		EnqueueTypeSymbol(typeDefsToSearch, typeSymbolsToSearch, typeSymbol.BaseTypeNoUseSiteDiagnostics);
	}

	protected override void EnqueueTypeSymbol(Queue<TypeDefinitionHandle> typeDefsToSearch, Queue<TypeSymbol> typeSymbolsToSearch, TypeSymbol typeSymbol)
	{
		if ((object)typeSymbol != null)
		{
			if (typeSymbol is PENamedTypeSymbol pENamedTypeSymbol && (object)pENamedTypeSymbol.ContainingPEModule == moduleSymbol)
			{
				typeDefsToSearch.Enqueue(pENamedTypeSymbol.Handle);
			}
			else
			{
				typeSymbolsToSearch.Enqueue(typeSymbol);
			}
		}
	}

	protected override MethodDefinitionHandle GetMethodHandle(MethodSymbol method)
	{
		if (method is PEMethodSymbol pEMethodSymbol && (object)pEMethodSymbol.ContainingModule == moduleSymbol)
		{
			return pEMethodSymbol.Handle;
		}
		return default(MethodDefinitionHandle);
	}
}
