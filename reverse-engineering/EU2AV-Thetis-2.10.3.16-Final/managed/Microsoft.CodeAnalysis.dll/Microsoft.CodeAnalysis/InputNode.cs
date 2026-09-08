using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class InputNode<T> : IIncrementalGeneratorNode<T>
{
	private static readonly string? s_tableType = typeof(T).FullName;

	private readonly Func<DriverStateTable.Builder, ImmutableArray<T>> _getInput;

	private readonly Action<IIncrementalGeneratorOutputNode> _registerOutput;

	private readonly IEqualityComparer<T> _inputComparer;

	private readonly IEqualityComparer<T>? _comparer;

	private readonly string? _name;

	public InputNode(Func<DriverStateTable.Builder, ImmutableArray<T>> getInput, IEqualityComparer<T>? inputComparer = null)
		: this(getInput, (Action<IIncrementalGeneratorOutputNode>?)null, inputComparer, (IEqualityComparer<T>?)null, (string?)null)
	{
	}

	private InputNode(Func<DriverStateTable.Builder, ImmutableArray<T>> getInput, Action<IIncrementalGeneratorOutputNode>? registerOutput, IEqualityComparer<T>? inputComparer = null, IEqualityComparer<T>? comparer = null, string? name = null)
	{
		_getInput = getInput;
		_comparer = comparer;
		_inputComparer = inputComparer ?? EqualityComparer<T>.Default;
		_registerOutput = registerOutput ?? ((Action<IIncrementalGeneratorOutputNode>)delegate
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/SourceGeneration/Nodes/InputNode.cs", 40);
		});
		_name = name;
	}

	public NodeStateTable<T> UpdateStateTable(DriverStateTable.Builder graphState, NodeStateTable<T>? previousTable, CancellationToken cancellationToken)
	{
		SharedStopwatch sharedStopwatch = SharedStopwatch.StartNew();
		ImmutableArray<T> inputs = _getInput(graphState);
		TimeSpan elapsed = sharedStopwatch.Elapsed;
		HashSet<T> hashSet = ((_inputComparer == EqualityComparer<T>.Default) ? PooledHashSet<T>.GetInstance() : new HashSet<T>(_inputComparer));
		foreach (T item2 in inputs)
		{
			hashSet.Add(item2);
		}
		NodeStateTable<T>.Builder builder = graphState.CreateTableBuilder(previousTable, _name, _comparer);
		ImmutableArray<(IncrementalGeneratorRunStep, int)> stepInputs = (builder.TrackIncrementalSteps ? ImmutableArray<(IncrementalGeneratorRunStep, int)>.Empty : default(ImmutableArray<(IncrementalGeneratorRunStep, int)>));
		if (previousTable != null)
		{
			int num = 0;
			foreach (var (item, _, _, _) in previousTable)
			{
				if (hashSet.Remove(item))
				{
					builder.TryUseCachedEntries(elapsed, stepInputs);
				}
				else if (inputs.Length == previousTable.Count)
				{
					builder.TryModifyEntry(inputs[num], elapsed, stepInputs, EntryState.Modified);
					hashSet.Remove(inputs[num]);
				}
				else
				{
					builder.TryRemoveEntries(elapsed, stepInputs);
				}
				num++;
			}
		}
		foreach (T item3 in hashSet)
		{
			builder.AddEntry(item3, EntryState.Added, elapsed, stepInputs, EntryState.Added);
		}
		NodeStateTable<T> nodeStateTable = builder.ToImmutableAndFree();
		LogTables(previousTable, nodeStateTable, inputs);
		(hashSet as PooledHashSet<T>)?.Free();
		return nodeStateTable;
	}

	public IIncrementalGeneratorNode<T> WithComparer(IEqualityComparer<T> comparer)
	{
		return new InputNode<T>(_getInput, _registerOutput, _inputComparer, comparer, _name);
	}

	public IIncrementalGeneratorNode<T> WithTrackingName(string name)
	{
		return new InputNode<T>(_getInput, _registerOutput, _inputComparer, _comparer, name);
	}

	public InputNode<T> WithRegisterOutput(Action<IIncrementalGeneratorOutputNode> registerOutput)
	{
		return new InputNode<T>(_getInput, registerOutput, _inputComparer, _comparer, _name);
	}

	public void RegisterOutput(IIncrementalGeneratorOutputNode output)
	{
		_registerOutput(output);
	}

	private void LogTables(NodeStateTable<T>? previousTable, NodeStateTable<T> newTable, ImmutableArray<T> inputs)
	{
		if (CodeAnalysisEventSource.Log.IsEnabled())
		{
			NodeStateTable<T>.Builder builder = NodeStateTable<T>.Empty.ToBuilder(_name, stepTrackingEnabled: false, null, inputs.Length);
			foreach (T item in inputs)
			{
				builder.AddEntry(item, EntryState.Added, TimeSpan.Zero, default(ImmutableArray<(IncrementalGeneratorRunStep, int)>), EntryState.Added);
			}
			NodeStateTable<T> inputTable = builder.ToImmutableAndFree();
			this.LogTables(_name, s_tableType, previousTable, newTable, inputTable);
		}
	}
}
