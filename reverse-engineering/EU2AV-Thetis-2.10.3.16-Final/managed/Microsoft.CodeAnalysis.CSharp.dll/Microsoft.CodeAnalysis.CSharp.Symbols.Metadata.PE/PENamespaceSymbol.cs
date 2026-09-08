using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal abstract class PENamespaceSymbol : NamespaceSymbol
{
	protected Dictionary<ReadOnlyMemory<char>, PENestedNamespaceSymbol>? lazyNamespaces;

	protected Dictionary<ReadOnlyMemory<char>, ImmutableArray<PENamedTypeSymbol>>? lazyTypes;

	private Dictionary<string, TypeDefinitionHandle>? _lazyNoPiaLocalTypes;

	private ImmutableArray<PENamedTypeSymbol> _lazyFlattenedTypes;

	private ImmutableArray<Symbol> _lazyFlattenedNamespacesAndTypes;

	internal sealed override NamespaceExtent Extent => new NamespaceExtent(ContainingPEModule);

	public sealed override ImmutableArray<Location> Locations => ContainingPEModule.MetadataLocation.Cast<MetadataLocation, Location>();

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	internal abstract PEModuleSymbol ContainingPEModule { get; }

	public sealed override ImmutableArray<Symbol> GetMembers()
	{
		if (_lazyFlattenedNamespacesAndTypes.IsDefault)
		{
			EnsureAllMembersLoaded();
			ImmutableInterlocked.InterlockedExchange(ref _lazyFlattenedNamespacesAndTypes, calculateMembers());
		}
		return _lazyFlattenedNamespacesAndTypes;
		ImmutableArray<Symbol> calculateMembers()
		{
			ImmutableArray<NamedTypeSymbol> memberTypesPrivate = GetMemberTypesPrivate();
			if (lazyNamespaces.Count == 0)
			{
				return StaticCast<Symbol>.From(memberTypesPrivate);
			}
			ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(memberTypesPrivate.Length + lazyNamespaces.Count);
			instance.AddRange(memberTypesPrivate);
			foreach (KeyValuePair<ReadOnlyMemory<char>, PENestedNamespaceSymbol> lazyNamespace in lazyNamespaces)
			{
				instance.Add(lazyNamespace.Value);
			}
			return instance.ToImmutableAndFree();
		}
	}

	private ImmutableArray<NamedTypeSymbol> GetMemberTypesPrivate()
	{
		if (_lazyFlattenedTypes.IsDefault)
		{
			ImmutableArray<PENamedTypeSymbol> value = lazyTypes.Flatten();
			ImmutableInterlocked.InterlockedExchange(ref _lazyFlattenedTypes, value);
		}
		return StaticCast<NamedTypeSymbol>.From(_lazyFlattenedTypes);
	}

	internal override NamespaceSymbol? GetNestedNamespace(ReadOnlyMemory<char> name)
	{
		EnsureAllMembersLoaded();
		if (lazyNamespaces.TryGetValue(name, out PENestedNamespaceSymbol value))
		{
			return value;
		}
		return null;
	}

	public sealed override ImmutableArray<Symbol> GetMembers(ReadOnlyMemory<char> name)
	{
		EnsureAllMembersLoaded();
		PENestedNamespaceSymbol value = null;
		ImmutableArray<PENamedTypeSymbol> value2;
		if (lazyNamespaces.TryGetValue(name, out value))
		{
			if (lazyTypes.TryGetValue(name, out value2))
			{
				return StaticCast<Symbol>.From(value2).Add(value);
			}
			return ImmutableArray.Create((Symbol)value);
		}
		if (lazyTypes.TryGetValue(name, out value2))
		{
			return StaticCast<Symbol>.From(value2);
		}
		return ImmutableArray<Symbol>.Empty;
	}

	public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		EnsureAllMembersLoaded();
		return GetMemberTypesPrivate();
	}

	public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name)
	{
		EnsureAllMembersLoaded();
		if (!lazyTypes.TryGetValue(name, out ImmutableArray<PENamedTypeSymbol> value))
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}
		return StaticCast<NamedTypeSymbol>.From(value);
	}

	public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name, int arity)
	{
		return GetTypeMembers(name).WhereAsArray((NamedTypeSymbol type, int num) => type.Arity == num, arity);
	}

	[MemberNotNull("lazyTypes")]
	[MemberNotNull("lazyNamespaces")]
	protected abstract void EnsureAllMembersLoaded();

	[MemberNotNull("lazyTypes")]
	[MemberNotNull("lazyNamespaces")]
	protected void LoadAllMembers(IEnumerable<IGrouping<string, TypeDefinitionHandle>> typesByNS)
	{
		IEnumerable<IGrouping<string, TypeDefinitionHandle>> types = null;
		IEnumerable<KeyValuePair<string, IEnumerable<IGrouping<string, TypeDefinitionHandle>>>> namespaces = null;
		bool isGlobalNamespace = IsGlobalNamespace;
		MetadataHelpers.GetInfoForImmediateNamespaceMembers(isGlobalNamespace, (!isGlobalNamespace) ? GetQualifiedNameLength() : 0, typesByNS, StringComparer.Ordinal, out types, out namespaces);
		LazyInitializeNamespaces(namespaces);
		LazyInitializeTypes(types);
	}

	private int GetQualifiedNameLength()
	{
		int num = Name.Length;
		NamespaceSymbol containingNamespace = ContainingNamespace;
		while ((object)containingNamespace != null && !containingNamespace.IsGlobalNamespace)
		{
			num += containingNamespace.Name.Length + 1;
			containingNamespace = containingNamespace.ContainingNamespace;
		}
		return num;
	}

	[MemberNotNull("lazyNamespaces")]
	private void LazyInitializeNamespaces(IEnumerable<KeyValuePair<string, IEnumerable<IGrouping<string, TypeDefinitionHandle>>>> childNamespaces)
	{
		if (lazyNamespaces != null)
		{
			return;
		}
		Dictionary<ReadOnlyMemory<char>, PENestedNamespaceSymbol> dictionary = new Dictionary<ReadOnlyMemory<char>, PENestedNamespaceSymbol>(ReadOnlyMemoryOfCharComparer.Instance);
		foreach (KeyValuePair<string, IEnumerable<IGrouping<string, TypeDefinitionHandle>>> childNamespace in childNamespaces)
		{
			PENestedNamespaceSymbol pENestedNamespaceSymbol = new PENestedNamespaceSymbol(childNamespace.Key, this, childNamespace.Value);
			dictionary.Add(System.MemoryExtensions.AsMemory(pENestedNamespaceSymbol.Name), pENestedNamespaceSymbol);
		}
		Interlocked.CompareExchange(ref lazyNamespaces, dictionary, null);
	}

	[MemberNotNull("lazyTypes")]
	private void LazyInitializeTypes(IEnumerable<IGrouping<string, TypeDefinitionHandle>> typeGroups)
	{
		if (lazyTypes != null)
		{
			return;
		}
		PEModuleSymbol containingPEModule = ContainingPEModule;
		ArrayBuilder<PENamedTypeSymbol> instance = ArrayBuilder<PENamedTypeSymbol>.GetInstance();
		bool flag = !containingPEModule.Module.ContainsNoPiaLocalTypes();
		Dictionary<string, TypeDefinitionHandle> dictionary = null;
		foreach (IGrouping<string, TypeDefinitionHandle> typeGroup in typeGroups)
		{
			foreach (TypeDefinitionHandle item in typeGroup)
			{
				if (flag || !containingPEModule.Module.IsNoPiaLocalType(item))
				{
					instance.Add(PENamedTypeSymbol.Create(containingPEModule, this, item, typeGroup.Key));
					continue;
				}
				try
				{
					string typeDefNameOrThrow = containingPEModule.Module.GetTypeDefNameOrThrow(item);
					if (dictionary == null)
					{
						dictionary = new Dictionary<string, TypeDefinitionHandle>(StringOrdinalComparer.Instance);
					}
					dictionary[typeDefNameOrThrow] = item;
				}
				catch (BadImageFormatException)
				{
				}
			}
		}
		Dictionary<ReadOnlyMemory<char>, ImmutableArray<PENamedTypeSymbol>> dictionary2 = instance.ToDictionary((PENamedTypeSymbol c) => System.MemoryExtensions.AsMemory(c.Name), ReadOnlyMemoryOfCharComparer.Instance);
		instance.Free();
		if (dictionary != null)
		{
			Interlocked.CompareExchange(ref _lazyNoPiaLocalTypes, dictionary, null);
		}
		if (Interlocked.CompareExchange(ref lazyTypes, dictionary2, null) == null)
		{
			containingPEModule.OnNewTypeDeclarationsLoaded(dictionary2);
		}
	}

	internal NamedTypeSymbol? UnifyIfNoPiaLocalType(ref MetadataTypeName emittedTypeName)
	{
		EnsureAllMembersLoaded();
		bool isNoPiaLocalType;
		if (_lazyNoPiaLocalTypes != null && _lazyNoPiaLocalTypes.TryGetValue(emittedTypeName.TypeName, out var value))
		{
			return (NamedTypeSymbol)new MetadataDecoder(ContainingPEModule).GetTypeOfToken(value, out isNoPiaLocalType);
		}
		return null;
	}
}
