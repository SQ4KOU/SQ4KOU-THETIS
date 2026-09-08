using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis;

internal sealed class StateTableStore
{
	public sealed class Builder
	{
		private readonly ImmutableSegmentedDictionary<object, IStateTable>.Builder _tableBuilder = ImmutableSegmentedDictionary.CreateBuilder<object, IStateTable>();

		public bool Contains(object key)
		{
			return _tableBuilder.ContainsKey(key);
		}

		public bool TryGetTable(object key, [NotNullWhen(true)] out IStateTable? table)
		{
			return _tableBuilder.TryGetValue(key, out table);
		}

		public void SetTable(object key, IStateTable table)
		{
			_tableBuilder[key] = table;
		}

		public StateTableStore ToImmutable()
		{
			foreach (KeyValuePair<object, IStateTable> item in _tableBuilder)
			{
				IStateTable stateTable = item.Value.AsCached();
				if (stateTable != item.Value)
				{
					SegmentedCollectionsMarshal.GetValueRefOrNullRef(_tableBuilder, item.Key) = stateTable;
				}
			}
			return new StateTableStore(_tableBuilder.ToImmutable());
		}
	}

	private readonly ImmutableSegmentedDictionary<object, IStateTable> _tables;

	public static readonly StateTableStore Empty = new StateTableStore(ImmutableSegmentedDictionary<object, IStateTable>.Empty);

	private StateTableStore(ImmutableSegmentedDictionary<object, IStateTable> tables)
	{
		_tables = tables;
	}

	public bool TryGetValue(object key, [NotNullWhen(true)] out IStateTable? table)
	{
		return _tables.TryGetValue(key, out table);
	}

	public NodeStateTable<T> GetStateTableOrEmpty<T>(object input)
	{
		return GetStateTable<T>(input) ?? NodeStateTable<T>.Empty;
	}

	public NodeStateTable<T>? GetStateTable<T>(object input)
	{
		if (TryGetValue(input, out IStateTable table))
		{
			return (NodeStateTable<T>)table;
		}
		return null;
	}
}
