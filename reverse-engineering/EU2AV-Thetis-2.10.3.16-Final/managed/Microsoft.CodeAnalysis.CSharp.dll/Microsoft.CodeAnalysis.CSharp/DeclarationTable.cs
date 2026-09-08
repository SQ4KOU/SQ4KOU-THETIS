using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class DeclarationTable
{
	internal sealed class Builder
	{
		private static readonly ObjectPool<Builder> s_builderPool = new ObjectPool<Builder>(() => new Builder());

		private DeclarationTable _table;

		private readonly List<Lazy<RootSingleNamespaceDeclaration>> _addedLazyRootDeclarations;

		private readonly List<Lazy<RootSingleNamespaceDeclaration>> _removedLazyRootDeclarations;

		private Builder()
		{
			_table = Empty;
			_addedLazyRootDeclarations = new List<Lazy<RootSingleNamespaceDeclaration>>();
			_removedLazyRootDeclarations = new List<Lazy<RootSingleNamespaceDeclaration>>();
		}

		public static Builder GetInstance(DeclarationTable table)
		{
			Builder builder = s_builderPool.Allocate();
			builder._table = table;
			return builder;
		}

		public void AddRootDeclaration(Lazy<RootSingleNamespaceDeclaration> lazyRootDeclaration)
		{
			RealizeRemoves();
			_addedLazyRootDeclarations.Add(lazyRootDeclaration);
		}

		public void RemoveRootDeclaration(Lazy<RootSingleNamespaceDeclaration> lazyRootDeclaration)
		{
			RealizeAdds();
			_removedLazyRootDeclarations.Add(lazyRootDeclaration);
		}

		public DeclarationTable ToDeclarationTableAndFree()
		{
			RealizeAdds();
			RealizeRemoves();
			DeclarationTable table = _table;
			_table = Empty;
			s_builderPool.Free(this);
			return table;
		}

		private void RealizeAdds()
		{
			if (_addedLazyRootDeclarations.Count == 0)
			{
				return;
			}
			Lazy<RootSingleNamespaceDeclaration> latestLazyRootDeclaration = _addedLazyRootDeclarations[_addedLazyRootDeclarations.Count - 1];
			if (_addedLazyRootDeclarations.Count == 1)
			{
				if (_table._latestLazyRootDeclaration == null)
				{
					_table = new DeclarationTable(_table._allOlderRootDeclarations, latestLazyRootDeclaration, _table._cache);
				}
				else
				{
					_table = new DeclarationTable(_table._allOlderRootDeclarations.Add(_table._latestLazyRootDeclaration), latestLazyRootDeclaration, null);
				}
			}
			else
			{
				_addedLazyRootDeclarations.RemoveAt(_addedLazyRootDeclarations.Count - 1);
				if (_table._latestLazyRootDeclaration != null)
				{
					_addedLazyRootDeclarations.Insert(0, _table._latestLazyRootDeclaration);
				}
				ImmutableSetWithInsertionOrder<Lazy<RootSingleNamespaceDeclaration>> allOlderRootDeclarations = _table._allOlderRootDeclarations.AddRange(_addedLazyRootDeclarations);
				_table = new DeclarationTable(allOlderRootDeclarations, latestLazyRootDeclaration, null);
			}
			_addedLazyRootDeclarations.Clear();
		}

		private void RealizeRemoves()
		{
			if (_removedLazyRootDeclarations.Count == 0)
			{
				return;
			}
			if (_removedLazyRootDeclarations.Count == 1)
			{
				Lazy<RootSingleNamespaceDeclaration> lazy = _removedLazyRootDeclarations[0];
				if (_table._latestLazyRootDeclaration == lazy)
				{
					_table = new DeclarationTable(_table._allOlderRootDeclarations, null, _table._cache);
				}
				else
				{
					_table = new DeclarationTable(_table._allOlderRootDeclarations.Remove(lazy), _table._latestLazyRootDeclaration, null);
				}
			}
			else
			{
				bool num = _table._latestLazyRootDeclaration != null && _removedLazyRootDeclarations.Contains(_table._latestLazyRootDeclaration);
				ImmutableSetWithInsertionOrder<Lazy<RootSingleNamespaceDeclaration>> allOlderRootDeclarations = _table._allOlderRootDeclarations.RemoveRange(_removedLazyRootDeclarations);
				Lazy<RootSingleNamespaceDeclaration> latestLazyRootDeclaration = (num ? null : _table._latestLazyRootDeclaration);
				_table = new DeclarationTable(allOlderRootDeclarations, latestLazyRootDeclaration, null);
			}
			_removedLazyRootDeclarations.Clear();
		}
	}

	private class Cache
	{
		private readonly DeclarationTable _table;

		private MergedNamespaceDeclaration? _mergedRoot;

		private ISet<string>? _typeNames;

		private ISet<string>? _namespaceNames;

		private ImmutableArray<ReferenceDirective> _referenceDirectives;

		public MergedNamespaceDeclaration MergedRoot
		{
			get
			{
				if (_mergedRoot == null)
				{
					Interlocked.CompareExchange(ref _mergedRoot, MergedNamespaceDeclaration.Create(((IEnumerable<SingleNamespaceDeclaration>)_table._allOlderRootDeclarations.InInsertionOrder.Select((Lazy<RootSingleNamespaceDeclaration> lazyRoot) => lazyRoot.Value)).AsImmutable()), null);
				}
				return _mergedRoot;
			}
		}

		public ISet<string> TypeNames
		{
			get
			{
				if (_typeNames == null)
				{
					Interlocked.CompareExchange(ref _typeNames, GetTypeNames(MergedRoot), null);
				}
				return _typeNames;
			}
		}

		public ISet<string> NamespaceNames
		{
			get
			{
				if (_namespaceNames == null)
				{
					Interlocked.CompareExchange(ref _namespaceNames, GetNamespaceNames(MergedRoot), null);
				}
				return _namespaceNames;
			}
		}

		public ImmutableArray<ReferenceDirective> ReferenceDirectives
		{
			get
			{
				if (_referenceDirectives.IsDefault)
				{
					ImmutableInterlocked.InterlockedInitialize(ref _referenceDirectives, MergedRoot.Declarations.OfType<RootSingleNamespaceDeclaration>().SelectMany((RootSingleNamespaceDeclaration r) => r.ReferenceDirectives).AsImmutable());
				}
				return _referenceDirectives;
			}
		}

		public Cache(DeclarationTable table)
		{
			_table = table;
		}
	}

	private sealed class RootNamespaceLocationComparer : IComparer<SingleNamespaceDeclaration>
	{
		private readonly CSharpCompilation _compilation;

		internal RootNamespaceLocationComparer(CSharpCompilation compilation)
		{
			_compilation = compilation;
		}

		public int Compare(SingleNamespaceDeclaration? x, SingleNamespaceDeclaration? y)
		{
			return _compilation.CompareSourceLocations(x.SyntaxReference, y.SyntaxReference);
		}
	}

	public static readonly DeclarationTable Empty = new DeclarationTable(ImmutableSetWithInsertionOrder<Lazy<RootSingleNamespaceDeclaration>>.Empty, null, null);

	private readonly ImmutableSetWithInsertionOrder<Lazy<RootSingleNamespaceDeclaration>> _allOlderRootDeclarations;

	private readonly Lazy<RootSingleNamespaceDeclaration>? _latestLazyRootDeclaration;

	private readonly Cache _cache;

	private MergedNamespaceDeclaration? _mergedRoot;

	private ICollection<string>? _typeNames;

	private ICollection<string>? _namespaceNames;

	private ICollection<ReferenceDirective>? _referenceDirectives;

	private static readonly Predicate<Declaration> s_isNamespacePredicate = (Declaration d) => d.Kind == DeclarationKind.Namespace;

	private static readonly Predicate<Declaration> s_isTypePredicate = (Declaration d) => d.Kind != DeclarationKind.Namespace;

	public ICollection<string> TypeNames
	{
		get
		{
			if (_typeNames == null)
			{
				Interlocked.CompareExchange(ref _typeNames, GetMergedTypeNames(), null);
			}
			return _typeNames;
		}
	}

	public ICollection<string> NamespaceNames
	{
		get
		{
			if (_namespaceNames == null)
			{
				Interlocked.CompareExchange(ref _namespaceNames, GetMergedNamespaceNames(), null);
			}
			return _namespaceNames;
		}
	}

	public IEnumerable<ReferenceDirective> ReferenceDirectives
	{
		get
		{
			if (_referenceDirectives == null)
			{
				Interlocked.CompareExchange(ref _referenceDirectives, GetMergedReferenceDirectives(), null);
			}
			return _referenceDirectives;
		}
	}

	private DeclarationTable(ImmutableSetWithInsertionOrder<Lazy<RootSingleNamespaceDeclaration>> allOlderRootDeclarations, Lazy<RootSingleNamespaceDeclaration>? latestLazyRootDeclaration, Cache? cache)
	{
		_allOlderRootDeclarations = allOlderRootDeclarations;
		_latestLazyRootDeclaration = latestLazyRootDeclaration;
		_cache = cache ?? new Cache(this);
	}

	public Builder ToBuilder()
	{
		return Builder.GetInstance(this);
	}

	public MergedNamespaceDeclaration GetMergedRoot(CSharpCompilation compilation)
	{
		if (_mergedRoot == null)
		{
			Interlocked.CompareExchange(ref _mergedRoot, CalculateMergedRoot(compilation), null);
		}
		return _mergedRoot;
	}

	internal MergedNamespaceDeclaration CalculateMergedRoot(CSharpCompilation compilation)
	{
		MergedNamespaceDeclaration mergedRoot = _cache.MergedRoot;
		if (_latestLazyRootDeclaration == null)
		{
			return mergedRoot;
		}
		if (mergedRoot == null)
		{
			return MergedNamespaceDeclaration.Create(_latestLazyRootDeclaration.Value);
		}
		ImmutableArray<SingleNamespaceDeclaration> declarations = mergedRoot.Declarations;
		ArrayBuilder<SingleNamespaceDeclaration> instance = ArrayBuilder<SingleNamespaceDeclaration>.GetInstance(declarations.Length + 1);
		instance.AddRange(declarations);
		instance.Add(_latestLazyRootDeclaration.Value);
		if (compilation != null)
		{
			instance.Sort(new RootNamespaceLocationComparer(compilation));
		}
		return MergedNamespaceDeclaration.Create(instance.ToImmutableAndFree());
	}

	private ICollection<string> GetMergedTypeNames()
	{
		ISet<string> typeNames = _cache.TypeNames;
		if (_latestLazyRootDeclaration == null)
		{
			return typeNames;
		}
		return UnionCollection<string>.Create(typeNames, GetTypeNames(_latestLazyRootDeclaration.Value));
	}

	private ICollection<string> GetMergedNamespaceNames()
	{
		ISet<string> namespaceNames = _cache.NamespaceNames;
		if (_latestLazyRootDeclaration == null)
		{
			return namespaceNames;
		}
		return UnionCollection<string>.Create(namespaceNames, GetNamespaceNames(_latestLazyRootDeclaration.Value));
	}

	private ICollection<ReferenceDirective> GetMergedReferenceDirectives()
	{
		ImmutableArray<ReferenceDirective> referenceDirectives = _cache.ReferenceDirectives;
		if (_latestLazyRootDeclaration == null)
		{
			return referenceDirectives;
		}
		return UnionCollection<ReferenceDirective>.Create(referenceDirectives, _latestLazyRootDeclaration.Value.ReferenceDirectives);
	}

	private static ISet<string> GetTypeNames(Declaration declaration)
	{
		return GetNames(declaration, s_isTypePredicate);
	}

	private static ISet<string> GetNamespaceNames(Declaration declaration)
	{
		return GetNames(declaration, s_isNamespacePredicate);
	}

	private static ISet<string> GetNames(Declaration declaration, Predicate<Declaration> predicate)
	{
		HashSet<string> hashSet = new HashSet<string>();
		Stack<Declaration> stack = new Stack<Declaration>();
		stack.Push(declaration);
		while (stack.Count > 0)
		{
			Declaration declaration2 = stack.Pop();
			if (declaration2 != null)
			{
				if (predicate(declaration2))
				{
					hashSet.Add(declaration2.Name);
				}
				foreach (Declaration child in declaration2.Children)
				{
					stack.Push(child);
				}
			}
		}
		return SpecializedCollections.ReadOnlySet(hashSet);
	}

	public static bool ContainsName(MergedNamespaceDeclaration mergedRoot, string name, SymbolFilter filter, CancellationToken cancellationToken)
	{
		return ContainsNameHelper(mergedRoot, (string n) => n == name, filter, (SingleTypeDeclaration t) => t.MemberNames.Value.Contains(name), cancellationToken);
	}

	public static bool ContainsName(MergedNamespaceDeclaration mergedRoot, Func<string, bool> predicate, SymbolFilter filter, CancellationToken cancellationToken)
	{
		return ContainsNameHelper(mergedRoot, predicate, filter, delegate(SingleTypeDeclaration t)
		{
			foreach (string item in t.MemberNames.Value)
			{
				if (predicate(item))
				{
					return true;
				}
			}
			return false;
		}, cancellationToken);
	}

	private static bool ContainsNameHelper(MergedNamespaceDeclaration mergedRoot, Func<string, bool> predicate, SymbolFilter filter, Func<SingleTypeDeclaration, bool> typePredicate, CancellationToken cancellationToken)
	{
		bool flag = (filter & SymbolFilter.Namespace) == SymbolFilter.Namespace;
		bool flag2 = (filter & SymbolFilter.Type) == SymbolFilter.Type;
		bool flag3 = (filter & SymbolFilter.Member) == SymbolFilter.Member;
		Stack<MergedNamespaceOrTypeDeclaration> stack = new Stack<MergedNamespaceOrTypeDeclaration>();
		stack.Push(mergedRoot);
		while (stack.Count > 0)
		{
			cancellationToken.ThrowIfCancellationRequested();
			MergedNamespaceOrTypeDeclaration mergedNamespaceOrTypeDeclaration = stack.Pop();
			if (mergedNamespaceOrTypeDeclaration == null)
			{
				continue;
			}
			if (mergedNamespaceOrTypeDeclaration.Kind == DeclarationKind.Namespace)
			{
				if (flag && predicate(mergedNamespaceOrTypeDeclaration.Name))
				{
					return true;
				}
			}
			else
			{
				if (flag2 && predicate(mergedNamespaceOrTypeDeclaration.Name))
				{
					return true;
				}
				if (flag3)
				{
					foreach (SingleTypeDeclaration declaration in ((MergedTypeDeclaration)mergedNamespaceOrTypeDeclaration).Declarations)
					{
						if (typePredicate(declaration))
						{
							return true;
						}
					}
				}
			}
			foreach (Declaration child in mergedNamespaceOrTypeDeclaration.Children)
			{
				if (child is MergedNamespaceOrTypeDeclaration mergedNamespaceOrTypeDeclaration2 && ((flag3 | flag2) || mergedNamespaceOrTypeDeclaration2.Kind == DeclarationKind.Namespace))
				{
					stack.Push(mergedNamespaceOrTypeDeclaration2);
				}
			}
		}
		return false;
	}
}
