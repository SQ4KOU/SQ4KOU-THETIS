using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class MergedNamespaceSymbol : NamespaceSymbol
{
	private readonly NamespaceExtent _extent;

	private readonly ImmutableArray<NamespaceSymbol> _namespacesToMerge;

	private readonly NamespaceSymbol _containingNamespace;

	private readonly string _nameOpt;

	private readonly CachingDictionary<ReadOnlyMemory<char>, Symbol> _cachedLookup;

	private ImmutableArray<Symbol> _allMembers;

	public override string Name => _nameOpt ?? _namespacesToMerge[0].Name;

	internal override NamespaceExtent Extent => _extent;

	public override ImmutableArray<NamespaceSymbol> ConstituentNamespaces => _namespacesToMerge;

	public override Symbol ContainingSymbol => _containingNamespace;

	public override AssemblySymbol ContainingAssembly
	{
		get
		{
			if (_extent.Kind == NamespaceKind.Module)
			{
				return _extent.Module.ContainingAssembly;
			}
			if (_extent.Kind == NamespaceKind.Assembly)
			{
				return _extent.Assembly;
			}
			return null;
		}
	}

	public override ImmutableArray<Location> Locations => _namespacesToMerge.SelectMany((NamespaceSymbol namespaceSymbol) => namespaceSymbol.Locations).AsImmutable();

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => _namespacesToMerge.SelectMany((NamespaceSymbol namespaceSymbol) => namespaceSymbol.DeclaringSyntaxReferences).AsImmutable();

	internal static NamespaceSymbol Create(NamespaceExtent extent, NamespaceSymbol containingNamespace, ImmutableArray<NamespaceSymbol> namespacesToMerge, string nameOpt = null)
	{
		if (namespacesToMerge.Length != 1 || nameOpt != null)
		{
			return new MergedNamespaceSymbol(extent, containingNamespace, namespacesToMerge, nameOpt);
		}
		return namespacesToMerge[0];
	}

	private MergedNamespaceSymbol(NamespaceExtent extent, NamespaceSymbol containingNamespace, ImmutableArray<NamespaceSymbol> namespacesToMerge, string nameOpt)
	{
		_extent = extent;
		_namespacesToMerge = namespacesToMerge;
		_containingNamespace = containingNamespace;
		_cachedLookup = new CachingDictionary<ReadOnlyMemory<char>, Symbol>(SlowGetChildrenOfName, SlowGetChildNames, ReadOnlyMemoryOfCharComparer.Instance);
		_nameOpt = nameOpt;
	}

	internal NamespaceSymbol GetConstituentForCompilation(CSharpCompilation compilation)
	{
		foreach (NamespaceSymbol item in _namespacesToMerge)
		{
			if (item.IsFromCompilation(compilation))
			{
				return item;
			}
		}
		return null;
	}

	internal override void ForceComplete(SourceLocation locationOpt, Predicate<Symbol> filter, CancellationToken cancellationToken)
	{
		foreach (NamespaceSymbol item in _namespacesToMerge)
		{
			cancellationToken.ThrowIfCancellationRequested();
			item.ForceComplete(locationOpt, filter, cancellationToken);
		}
	}

	private ImmutableArray<Symbol> SlowGetChildrenOfName(ReadOnlyMemory<char> name)
	{
		ArrayBuilder<NamespaceSymbol> arrayBuilder = null;
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		foreach (NamespaceSymbol item in _namespacesToMerge)
		{
			foreach (Symbol member in item.GetMembers(name))
			{
				if (member.Kind == SymbolKind.Namespace)
				{
					arrayBuilder = arrayBuilder ?? ArrayBuilder<NamespaceSymbol>.GetInstance();
					arrayBuilder.Add((NamespaceSymbol)member);
				}
				else
				{
					instance.Add(member);
				}
			}
		}
		if (arrayBuilder != null)
		{
			instance.Add(Create(_extent, this, arrayBuilder.ToImmutableAndFree()));
		}
		return instance.ToImmutableAndFree();
	}

	private SegmentedHashSet<ReadOnlyMemory<char>> SlowGetChildNames(IEqualityComparer<ReadOnlyMemory<char>> comparer)
	{
		int num = 0;
		foreach (NamespaceSymbol item in _namespacesToMerge)
		{
			num += item.GetMembersUnordered().Length;
		}
		SegmentedHashSet<ReadOnlyMemory<char>> segmentedHashSet = new SegmentedHashSet<ReadOnlyMemory<char>>(num, comparer);
		foreach (NamespaceSymbol item2 in _namespacesToMerge)
		{
			foreach (Symbol item3 in item2.GetMembersUnordered())
			{
				segmentedHashSet.Add(System.MemoryExtensions.AsMemory(item3.Name));
			}
		}
		return segmentedHashSet;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (_allMembers.IsDefault)
		{
			ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
			_cachedLookup.AddValues(instance);
			_allMembers = instance.ToImmutableAndFree();
		}
		return _allMembers;
	}

	public override ImmutableArray<Symbol> GetMembers(ReadOnlyMemory<char> name)
	{
		return _cachedLookup[name];
	}

	internal sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembersUnordered()
	{
		return ImmutableArray.CreateRange(GetMembersUnordered().OfType<NamedTypeSymbol>());
	}

	public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return ImmutableArray.CreateRange(GetMembers().OfType<NamedTypeSymbol>());
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name)
	{
		return ImmutableArray.CreateRange(_cachedLookup[name].OfType<NamedTypeSymbol>());
	}

	internal override void GetExtensionMethods(ArrayBuilder<MethodSymbol> methods, string name, int arity, LookupOptions options)
	{
		foreach (NamespaceSymbol item in _namespacesToMerge)
		{
			item.GetExtensionMethods(methods, name, arity, options);
		}
	}

	internal sealed override void GetExtensionMembers(ArrayBuilder<Symbol> members, string? name, string? alternativeName, int arity, LookupOptions options, ConsList<FieldSymbol> fieldsBeingBound)
	{
		foreach (Symbol item in GetMembersUnordered())
		{
			if (item is NamedTypeSymbol namedTypeSymbol)
			{
				namedTypeSymbol.GetExtensionMembers(members, name, alternativeName, arity, options, fieldsBeingBound);
			}
		}
	}
}
