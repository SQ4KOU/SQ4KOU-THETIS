using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class PredicateSyntaxStrategy<T> : ISyntaxSelectionStrategy<T>
{
	private sealed class Builder : ISyntaxInputBuilder
	{
		private readonly PredicateSyntaxStrategy<T> _owner;

		private readonly string? _name;

		private readonly IEqualityComparer<T> _comparer;

		private readonly object _key;

		private readonly NodeStateTable<SyntaxNode>.Builder _filterTable;

		private readonly NodeStateTable<T>.Builder _transformTable;

		public Builder(PredicateSyntaxStrategy<T> owner, object key, StateTableStore table, bool trackIncrementalSteps, string? name, IEqualityComparer<T> comparer)
		{
			_owner = owner;
			_name = name;
			_comparer = comparer;
			_key = key;
			_filterTable = table.GetStateTableOrEmpty<SyntaxNode>(_owner._filterKey).ToBuilder(null, trackIncrementalSteps, ReferenceEqualityComparer.Instance);
			_transformTable = table.GetStateTableOrEmpty<T>(_key).ToBuilder(_name, trackIncrementalSteps, _comparer);
		}

		public void SaveStateAndFree(StateTableStore.Builder tables)
		{
			tables.SetTable(_owner._filterKey, _filterTable.ToImmutableAndFree());
			tables.SetTable(_key, _transformTable.ToImmutableAndFree());
		}

		public void VisitTree(Lazy<SyntaxNode> root, EntryState state, Lazy<SemanticModel>? model, CancellationToken cancellationToken)
		{
			ImmutableArray<(IncrementalGeneratorRunStep, int)> stepInputs = (_filterTable.TrackIncrementalSteps ? ImmutableArray<(IncrementalGeneratorRunStep, int)>.Empty : default(ImmutableArray<(IncrementalGeneratorRunStep, int)>));
			if (state == EntryState.Removed)
			{
				if (_filterTable.TryRemoveEntries(TimeSpan.Zero, stepInputs, out OneOrMany<SyntaxNode> entries))
				{
					for (int i = 0; i < entries.Count; i++)
					{
						_transformTable.TryRemoveEntries(TimeSpan.Zero, stepInputs);
					}
				}
				return;
			}
			if (state != EntryState.Cached || !_filterTable.TryUseCachedEntries(TimeSpan.Zero, stepInputs, out NodeStateTable<SyntaxNode>.TableEntry entry))
			{
				SharedStopwatch sharedStopwatch = SharedStopwatch.StartNew();
				ImmutableArray<SyntaxNode> immutableArray = getFilteredNodes(root.Value, _owner._filterFunc, cancellationToken);
				if (state != EntryState.Modified || !_filterTable.TryModifyEntries(immutableArray, sharedStopwatch.Elapsed, stepInputs, state, out entry))
				{
					entry = _filterTable.AddEntries(immutableArray, state, sharedStopwatch.Elapsed, stepInputs, state);
				}
			}
			for (int j = 0; j < entry.Count; j++)
			{
				if (entry.GetState(j) == EntryState.Removed)
				{
					_transformTable.TryRemoveEntries(TimeSpan.Zero, stepInputs);
					continue;
				}
				SharedStopwatch sharedStopwatch2 = SharedStopwatch.StartNew();
				GeneratorSyntaxContext arg = new GeneratorSyntaxContext(entry.GetItem(j), model, _owner._syntaxHelper);
				T value = _owner._transformFunc(arg, cancellationToken);
				EntryState entryState = ((state == EntryState.Cached) ? EntryState.Modified : state);
				if (entryState == EntryState.Added || !_transformTable.TryModifyEntry(value, sharedStopwatch2.Elapsed, stepInputs, entryState))
				{
					_transformTable.AddEntry(value, EntryState.Added, sharedStopwatch2.Elapsed, stepInputs, EntryState.Added);
				}
			}
			static ImmutableArray<SyntaxNode> getFilteredNodes(SyntaxNode syntaxNode, Func<SyntaxNode, CancellationToken, bool> func, CancellationToken token)
			{
				ArrayBuilder<SyntaxNode> arrayBuilder = null;
				foreach (SyntaxNode item in syntaxNode.DescendantNodesAndSelf())
				{
					token.ThrowIfCancellationRequested();
					if (func(item, token))
					{
						(arrayBuilder ?? (arrayBuilder = ArrayBuilder<SyntaxNode>.GetInstance())).Add(item);
					}
				}
				return arrayBuilder.ToImmutableOrEmptyAndFree();
			}
		}
	}

	private readonly Func<GeneratorSyntaxContext, CancellationToken, T> _transformFunc;

	private readonly ISyntaxHelper _syntaxHelper;

	private readonly Func<SyntaxNode, CancellationToken, bool> _filterFunc;

	private readonly object _filterKey = new object();

	internal PredicateSyntaxStrategy(Func<SyntaxNode, CancellationToken, bool> filterFunc, Func<GeneratorSyntaxContext, CancellationToken, T> transformFunc, ISyntaxHelper syntaxHelper)
	{
		_transformFunc = transformFunc;
		_syntaxHelper = syntaxHelper;
		_filterFunc = filterFunc;
	}

	public ISyntaxInputBuilder GetBuilder(StateTableStore table, object key, bool trackIncrementalSteps, string? name, IEqualityComparer<T> comparer)
	{
		return new Builder(this, key, table, trackIncrementalSteps, name, comparer);
	}
}
