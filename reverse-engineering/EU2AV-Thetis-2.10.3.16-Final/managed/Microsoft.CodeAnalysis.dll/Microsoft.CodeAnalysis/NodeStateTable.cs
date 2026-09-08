using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal sealed class NodeStateTable<T> : IStateTable
{
	public struct Enumerator
	{
		private readonly NodeStateTable<T> _stateTable;

		private int _nextStatesIndex;

		private int _nextInputEntryIndex;

		private IncrementalGeneratorRunStep? _step;

		private TableEntry _inputEntry;

		private NodeStateEntry<T> _current;

		public NodeStateEntry<T> Current => _current;

		public Enumerator(NodeStateTable<T> stateTable)
		{
			_nextInputEntryIndex = 0;
			_step = null;
			_inputEntry = default(TableEntry);
			_current = default(NodeStateEntry<T>);
			_stateTable = stateTable;
			_nextStatesIndex = 0;
			UpdateAfterNextStatesIndexModification();
		}

		public bool MoveNext()
		{
			while (_nextStatesIndex < _stateTable._states.Length)
			{
				if (_nextInputEntryIndex < _inputEntry.Count)
				{
					_current = new NodeStateEntry<T>(_inputEntry.GetItem(_nextInputEntryIndex), _inputEntry.GetState(_nextInputEntryIndex), _nextInputEntryIndex, _step);
					_nextInputEntryIndex++;
					return true;
				}
				_nextStatesIndex++;
				UpdateAfterNextStatesIndexModification();
			}
			return false;
		}

		private void UpdateAfterNextStatesIndexModification()
		{
			_nextInputEntryIndex = 0;
			if (_nextStatesIndex < _stateTable._states.Length)
			{
				_step = (_stateTable.HasTrackedSteps ? _stateTable.Steps[_nextStatesIndex] : null);
				_inputEntry = _stateTable._states[_nextStatesIndex];
			}
		}
	}

	public sealed class Builder
	{
		private readonly ArrayBuilder<TableEntry> _states;

		private readonly NodeStateTable<T> _previous;

		private readonly string? _name;

		private readonly IEqualityComparer<T> _equalityComparer;

		private readonly ArrayBuilder<IncrementalGeneratorRunStep>? _steps;

		private int _insertedCount;

		[MemberNotNullWhen(true, "_steps")]
		public bool TrackIncrementalSteps
		{
			[MemberNotNullWhen(true, "_steps")]
			get
			{
				return _steps != null;
			}
		}

		public int Count => _states.Count;

		public IReadOnlyList<IncrementalGeneratorRunStep> Steps
		{
			get
			{
				IReadOnlyList<IncrementalGeneratorRunStep> steps = _steps;
				return (IReadOnlyList<IncrementalGeneratorRunStep>)(steps ?? ((object)ImmutableArray<IncrementalGeneratorRunStep>.Empty));
			}
		}

		internal Builder(NodeStateTable<T> previous, string? name, bool stepTrackingEnabled, IEqualityComparer<T>? equalityComparer, int? tableCapacity)
		{
			_states = ArrayBuilder<TableEntry>.GetInstance(tableCapacity ?? previous.GetTotalEntryItemCount());
			_previous = previous;
			_name = name;
			_equalityComparer = equalityComparer ?? WrappedUserComparer<T>.Default;
			if (stepTrackingEnabled)
			{
				_steps = ArrayBuilder<IncrementalGeneratorRunStep>.GetInstance();
			}
		}

		public bool TryRemoveEntries(TimeSpan elapsedTime, ImmutableArray<(IncrementalGeneratorRunStep InputStep, int OutputIndex)> stepInputs)
		{
			if (!TryGetPreviousEntry(out var previousEntry))
			{
				return false;
			}
			TableEntry item = previousEntry.AsRemovedDueToInputRemoval();
			_states.Add(item);
			RecordStepInfoForLastEntry(elapsedTime, stepInputs, EntryState.Removed);
			return true;
		}

		public bool TryRemoveEntries(TimeSpan elapsedTime, ImmutableArray<(IncrementalGeneratorRunStep InputStep, int OutputIndex)> stepInputs, out OneOrMany<T> entries)
		{
			if (!TryRemoveEntries(elapsedTime, stepInputs))
			{
				entries = default(OneOrMany<T>);
				return false;
			}
			ArrayBuilder<TableEntry> states = _states;
			entries = states[states.Count - 1].Items;
			return true;
		}

		public bool TryUseCachedEntries(TimeSpan elapsedTime, ImmutableArray<(IncrementalGeneratorRunStep InputStep, int OutputIndex)> stepInputs)
		{
			if (!TryGetPreviousEntry(out var previousEntry))
			{
				return false;
			}
			_states.Add(previousEntry);
			RecordStepInfoForLastEntry(elapsedTime, stepInputs, EntryState.Cached);
			return true;
		}

		internal bool TryUseCachedEntries(TimeSpan elapsedTime, ImmutableArray<(IncrementalGeneratorRunStep InputStep, int OutputIndex)> stepInputs, out TableEntry entry)
		{
			if (!TryUseCachedEntries(elapsedTime, stepInputs))
			{
				entry = default(TableEntry);
				return false;
			}
			ArrayBuilder<TableEntry> states = _states;
			entry = states[states.Count - 1];
			return true;
		}

		public bool TryModifyEntry(T value, TimeSpan elapsedTime, ImmutableArray<(IncrementalGeneratorRunStep InputStep, int OutputIndex)> stepInputs, EntryState overallInputState)
		{
			if (!TryGetPreviousEntry(out var previousEntry))
			{
				return false;
			}
			if (previousEntry.Count == 0)
			{
				return false;
			}
			var (one, state, _) = GetModifiedItemAndState(previousEntry.GetItem(0), value);
			_states.Add(new TableEntry(OneOrMany.Create(one), state));
			RecordStepInfoForLastEntry(elapsedTime, stepInputs, overallInputState);
			return true;
		}

		public bool TryModifyEntries(ImmutableArray<T> outputs, TimeSpan elapsedTime, ImmutableArray<(IncrementalGeneratorRunStep InputStep, int OutputIndex)> stepInputs, EntryState overallInputState)
		{
			if (!TryGetPreviousEntry(out var previousEntry))
			{
				return false;
			}
			if (previousEntry.Count == 0 && outputs.Length == 0)
			{
				_states.Add(previousEntry);
				if (TrackIncrementalSteps)
				{
					RecordStepInfoForLastEntry(elapsedTime, stepInputs, EntryState.Cached);
				}
				return true;
			}
			int capacity = Math.Max(previousEntry.Count, outputs.Length);
			TableEntry.Builder builder = ((previousEntry.Count == outputs.Length) ? null : new TableEntry.Builder(capacity));
			int num = Math.Min(previousEntry.Count, outputs.Length);
			for (int i = 0; i < num; i++)
			{
				T item = previousEntry.GetItem(i);
				EntryState state = previousEntry.GetState(i);
				T replacement = outputs[i];
				var (item2, entryState, flag) = GetModifiedItemAndState(item, replacement);
				if (builder != null)
				{
					builder.Add(item2, entryState);
				}
				else if (!flag || entryState != state)
				{
					builder = new TableEntry.Builder(capacity);
					for (int j = 0; j < i; j++)
					{
						builder.Add(previousEntry.GetItem(j), previousEntry.GetState(j));
					}
					builder.Add(item2, entryState);
				}
			}
			for (int k = num; k < previousEntry.Count; k++)
			{
				builder.Add(previousEntry.GetItem(k), EntryState.Removed);
			}
			for (int l = num; l < outputs.Length; l++)
			{
				builder.Add(outputs[l], EntryState.Added);
			}
			_states.Add(builder?.ToImmutableAndFree() ?? previousEntry);
			RecordStepInfoForLastEntry(elapsedTime, stepInputs, overallInputState);
			return true;
		}

		public bool TryModifyEntries(ImmutableArray<T> outputs, TimeSpan elapsedTime, ImmutableArray<(IncrementalGeneratorRunStep InputStep, int OutputIndex)> stepInputs, EntryState overallInputState, out TableEntry entry)
		{
			if (!TryModifyEntries(outputs, elapsedTime, stepInputs, overallInputState))
			{
				entry = default(TableEntry);
				return false;
			}
			ArrayBuilder<TableEntry> states = _states;
			entry = states[states.Count - 1];
			return true;
		}

		public void AddEntry(T value, EntryState state, TimeSpan elapsedTime, ImmutableArray<(IncrementalGeneratorRunStep InputStep, int OutputIndex)> stepInputs, EntryState overallInputState)
		{
			_states.Add(new TableEntry(OneOrMany.Create(value), state));
			_insertedCount += ((state == EntryState.Added) ? 1 : 0);
			RecordStepInfoForLastEntry(elapsedTime, stepInputs, overallInputState);
		}

		public TableEntry AddEntries(ImmutableArray<T> values, EntryState state, TimeSpan elapsedTime, ImmutableArray<(IncrementalGeneratorRunStep InputStep, int OutputIndex)> stepInputs, EntryState overallInputState)
		{
			TableEntry tableEntry = new TableEntry(OneOrMany.Create(values), state);
			_states.Add(tableEntry);
			_insertedCount += ((state == EntryState.Added) ? 1 : 0);
			RecordStepInfoForLastEntry(elapsedTime, stepInputs, overallInputState);
			return tableEntry;
		}

		private bool TryGetPreviousEntry(out TableEntry previousEntry)
		{
			int num = _states.Count - _insertedCount;
			bool flag = _previous._states.Length > num;
			previousEntry = (flag ? _previous._states[num] : default(TableEntry));
			return flag;
		}

		private void RecordStepInfoForLastEntry(TimeSpan elapsedTime, ImmutableArray<(IncrementalGeneratorRunStep InputStep, int OutputIndex)> stepInputs, EntryState overallInputState)
		{
			if (TrackIncrementalSteps)
			{
				ArrayBuilder<TableEntry> states = _states;
				TableEntry tableEntry = states[states.Count - 1];
				ArrayBuilder<(object, IncrementalStepRunReason)> instance = ArrayBuilder<(object, IncrementalStepRunReason)>.GetInstance(tableEntry.Count);
				for (int i = 0; i < tableEntry.Count; i++)
				{
					instance.Add((tableEntry.GetItem(i), AsStepState(overallInputState, tableEntry.GetState(i))));
				}
				_steps.Add(new IncrementalGeneratorRunStep(_name, stepInputs, instance.ToImmutableAndFree(), elapsedTime));
			}
		}

		private static IncrementalStepRunReason AsStepState(EntryState inputState, EntryState outputState)
		{
			switch (inputState)
			{
			case EntryState.Added:
				if (outputState != EntryState.Added)
				{
					break;
				}
				return IncrementalStepRunReason.New;
			case EntryState.Modified:
				switch (outputState)
				{
				case EntryState.Modified:
					return IncrementalStepRunReason.Modified;
				case EntryState.Cached:
					return IncrementalStepRunReason.Unchanged;
				case EntryState.Removed:
					return IncrementalStepRunReason.Removed;
				case EntryState.Added:
					return IncrementalStepRunReason.New;
				}
				break;
			case EntryState.Cached:
				if (outputState != EntryState.Cached)
				{
					break;
				}
				return IncrementalStepRunReason.Cached;
			case EntryState.Removed:
				if (outputState != EntryState.Removed)
				{
					break;
				}
				return IncrementalStepRunReason.Removed;
			}
			throw ExceptionUtilities.UnexpectedValue((inputState, outputState));
		}

		public NodeStateTable<T> ToImmutableAndFree()
		{
			if (_states.Count == 0)
			{
				_states.Free();
				return NodeStateTable<T>.Empty;
			}
			ImmutableArray<TableEntry> immutableArray;
			if (_states.Count == _previous.Count && _states.SequenceEqual(_previous._states, (TableEntry e1, TableEntry e2) => e1.Matches(e2, _equalityComparer)))
			{
				immutableArray = _previous._states;
				_states.Free();
			}
			else
			{
				immutableArray = _states.ToImmutableAndFree();
			}
			return new NodeStateTable<T>(immutableArray, TrackIncrementalSteps ? _steps.ToImmutableAndFree() : default(ImmutableArray<IncrementalGeneratorRunStep>), TrackIncrementalSteps, immutableArray.All((TableEntry s) => s.IsCached) && _previous.GetTotalEntryItemCount() == immutableArray.Sum((TableEntry s) => s.Count));
		}

		private (T chosen, EntryState state, bool chosePrevious) GetModifiedItemAndState(T previous, T replacement)
		{
			if (!_equalityComparer.Equals(previous, replacement))
			{
				return (chosen: replacement, state: EntryState.Modified, chosePrevious: false);
			}
			return (chosen: previous, state: EntryState.Cached, chosePrevious: true);
		}
	}

	internal readonly struct TableEntry
	{
		public struct Enumerator(TableEntry tableEntry)
		{
			private readonly TableEntry _entry = tableEntry;

			private int _index = -1;

			public T Current => _entry.GetItem(_index);

			public bool MoveNext()
			{
				_index++;
				return _index < _entry.Count;
			}
		}

		public sealed class Builder
		{
			private readonly ArrayBuilder<T> _items;

			private ArrayBuilder<EntryState>? _states;

			private EntryState? _currentState;

			private bool _anyRemoved;

			private readonly int _requestedCapacity;

			public Builder(int capacity)
			{
				_items = ArrayBuilder<T>.GetInstance(capacity);
				_requestedCapacity = capacity;
			}

			public void Add(T item, EntryState state)
			{
				_items.Add(item);
				_anyRemoved |= state == EntryState.Removed;
				if (!_currentState.HasValue)
				{
					_currentState = state;
				}
				else if (_states != null)
				{
					_states.Add(state);
				}
				else if (_currentState != state)
				{
					_states = ArrayBuilder<EntryState>.GetInstance(_requestedCapacity);
					int i = 0;
					for (int num = _items.Count - 1; i < num; i++)
					{
						_states.Add(_currentState.Value);
					}
					_states.Add(state);
				}
			}

			public TableEntry ToImmutableAndFree()
			{
				return new TableEntry(_items.ToOneOrManyAndFree(), _states?.ToImmutableAndFree() ?? TableEntry.GetSingleArray(_currentState.Value), _anyRemoved);
			}
		}

		private static readonly ImmutableArray<EntryState> s_allAddedEntries = ImmutableArray.Create(EntryState.Added);

		private static readonly ImmutableArray<EntryState> s_allCachedEntries = ImmutableArray.Create(EntryState.Cached);

		private static readonly ImmutableArray<EntryState> s_allModifiedEntries = ImmutableArray.Create(EntryState.Modified);

		private static readonly ImmutableArray<EntryState> s_allRemovedEntries = ImmutableArray.Create(EntryState.Removed);

		private static readonly ImmutableArray<EntryState> s_allRemovedDueToInputRemoval = ImmutableArray.Create(EntryState.Removed);

		private readonly OneOrMany<T> _items;

		private readonly bool _anyRemoved;

		private readonly ImmutableArray<EntryState> _states;

		public bool IsCached
		{
			get
			{
				if (!(_states == s_allCachedEntries))
				{
					return _states.All((EntryState s) => s == EntryState.Cached);
				}
				return true;
			}
		}

		public bool IsRemovedDueToInputRemoval => _states == s_allRemovedDueToInputRemoval;

		public int Count => _items.Count;

		public OneOrMany<T> Items => _items;

		public TableEntry(OneOrMany<T> items, EntryState state)
			: this(items, GetSingleArray(state), state == EntryState.Removed)
		{
		}

		private TableEntry(OneOrMany<T> items, ImmutableArray<EntryState> states, bool anyRemoved)
		{
			_items = items;
			_states = states;
			_anyRemoved = anyRemoved;
		}

		public bool Matches(TableEntry entry, IEqualityComparer<T> equalityComparer)
		{
			if (!_states.SequenceEqual(entry._states))
			{
				return false;
			}
			if (Count != entry.Count)
			{
				return false;
			}
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				if (!equalityComparer.Equals(GetItem(i), entry.GetItem(i)))
				{
					return false;
				}
			}
			return true;
		}

		public T GetItem(int index)
		{
			return _items[index];
		}

		public EntryState GetState(int index)
		{
			if (_states.Length != 1)
			{
				return _states[index];
			}
			return _states[0];
		}

		public TableEntry AsCached()
		{
			if (!_anyRemoved)
			{
				return new TableEntry(_items, s_allCachedEntries, anyRemoved: false);
			}
			ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance();
			for (int i = 0; i < Count; i++)
			{
				if (GetState(i) != EntryState.Removed)
				{
					instance.Add(GetItem(i));
				}
			}
			return new TableEntry(OneOrMany.Create(instance.ToImmutableArray()), s_allCachedEntries, anyRemoved: false);
		}

		public TableEntry AsRemovedDueToInputRemoval()
		{
			return new TableEntry(_items, s_allRemovedDueToInputRemoval, anyRemoved: true);
		}

		private static ImmutableArray<EntryState> GetSingleArray(EntryState state)
		{
			return state switch
			{
				EntryState.Added => s_allAddedEntries, 
				EntryState.Cached => s_allCachedEntries, 
				EntryState.Modified => s_allModifiedEntries, 
				EntryState.Removed => s_allRemovedEntries, 
				_ => throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/SourceGeneration/Nodes/NodeStateTable.cs", 663), 
			};
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}
	}

	private readonly ImmutableArray<TableEntry> _states;

	internal static NodeStateTable<T> Empty { get; } = new NodeStateTable<T>(ImmutableArray<TableEntry>.Empty, ImmutableArray<IncrementalGeneratorRunStep>.Empty, hasTrackedSteps: true, isCached: false);

	public int Count => _states.Length;

	public bool IsCached { get; }

	public bool IsEmpty => _states.IsEmpty;

	public bool HasTrackedSteps { get; }

	public ImmutableArray<IncrementalGeneratorRunStep> Steps { get; }

	private NodeStateTable(ImmutableArray<TableEntry> states, ImmutableArray<IncrementalGeneratorRunStep> steps, bool hasTrackedSteps, bool isCached)
	{
		_states = states;
		Steps = steps;
		IsCached = isCached;
		HasTrackedSteps = hasTrackedSteps;
	}

	public int GetTotalEntryItemCount()
	{
		return _states.Sum((TableEntry e) => e.Count);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	public NodeStateTable<T> AsCached()
	{
		if (IsCached)
		{
			return this;
		}
		ArrayBuilder<TableEntry> instance = ArrayBuilder<TableEntry>.GetInstance(_states.Count((TableEntry e) => !e.IsRemovedDueToInputRemoval));
		foreach (TableEntry state in _states)
		{
			if (!state.IsRemovedDueToInputRemoval)
			{
				instance.Add(state.AsCached());
			}
		}
		return new NodeStateTable<T>(instance.ToImmutableAndFree(), ImmutableArray<IncrementalGeneratorRunStep>.Empty, hasTrackedSteps: false, isCached: true);
	}

	IStateTable IStateTable.AsCached()
	{
		return AsCached();
	}

	public (T item, IncrementalGeneratorRunStep? step) Single()
	{
		ImmutableArray<TableEntry> states = _states;
		T item = states[states.Length - 1].GetItem(0);
		object item2;
		if (!HasTrackedSteps)
		{
			item2 = null;
		}
		else
		{
			ImmutableArray<IncrementalGeneratorRunStep> steps = Steps;
			item2 = steps[steps.Length - 1];
		}
		return (item: item, step: (IncrementalGeneratorRunStep)item2);
	}

	public Builder ToBuilder(string? stepName, bool stepTrackingEnabled, IEqualityComparer<T>? equalityComparer = null, int? tableCapacity = null)
	{
		return new Builder(this, stepName, stepTrackingEnabled, equalityComparer, tableCapacity);
	}

	public NodeStateTable<T> CreateCachedTableWithUpdatedSteps<TInput>(NodeStateTable<TInput> inputTable, string? stepName, IEqualityComparer<T>? equalityComparer)
	{
		Builder builder = ToBuilder(stepName, stepTrackingEnabled: true, equalityComparer);
		foreach (NodeStateEntry<TInput> item in inputTable)
		{
			ImmutableArray<(IncrementalGeneratorRunStep, int)> stepInputs = ImmutableArray.Create((item.Step, item.OutputIndex));
			builder.TryUseCachedEntries(TimeSpan.Zero, stepInputs);
		}
		return builder.ToImmutableAndFree();
	}

	public string GetPackedStates()
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		foreach (TableEntry state in _states)
		{
			for (int i = 0; i < state.Count; i++)
			{
				char value = state.GetState(i) switch
				{
					EntryState.Added => 'A', 
					EntryState.Removed => 'R', 
					EntryState.Modified => 'M', 
					EntryState.Cached => 'C', 
					_ => throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/SourceGeneration/Nodes/NodeStateTable.cs", 214), 
				};
				instance.Builder.Append(value);
			}
			instance.Builder.Append(',');
		}
		return instance.ToStringAndFree();
	}
}
