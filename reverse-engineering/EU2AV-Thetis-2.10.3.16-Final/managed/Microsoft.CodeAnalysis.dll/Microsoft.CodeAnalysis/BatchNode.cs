using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class BatchNode<TInput> : IIncrementalGeneratorNode<ImmutableArray<TInput>>
{
	private static readonly string? s_tableType = typeof(ImmutableArray<TInput>).FullName;

	private readonly IIncrementalGeneratorNode<TInput> _sourceNode;

	private readonly IEqualityComparer<ImmutableArray<TInput>>? _comparer;

	private readonly string? _name;

	public BatchNode(IIncrementalGeneratorNode<TInput> sourceNode, IEqualityComparer<ImmutableArray<TInput>>? comparer = null, string? name = null)
	{
		_sourceNode = sourceNode;
		_comparer = comparer;
		_name = name;
	}

	public IIncrementalGeneratorNode<ImmutableArray<TInput>> WithComparer(IEqualityComparer<ImmutableArray<TInput>> comparer)
	{
		return new BatchNode<TInput>(_sourceNode, comparer, _name);
	}

	public IIncrementalGeneratorNode<ImmutableArray<TInput>> WithTrackingName(string name)
	{
		return new BatchNode<TInput>(_sourceNode, _comparer, name);
	}

	private (ImmutableArray<TInput>, ImmutableArray<(IncrementalGeneratorRunStep InputStep, int OutputIndex)>) GetValuesAndInputs(NodeStateTable<TInput> sourceTable, NodeStateTable<ImmutableArray<TInput>>? previousTable, NodeStateTable<ImmutableArray<TInput>>.Builder newTable)
	{
		ArrayBuilder<(IncrementalGeneratorRunStep, int)> arrayBuilder = (newTable.TrackIncrementalSteps ? ArrayBuilder<(IncrementalGeneratorRunStep, int)>.GetInstance() : null);
		int num = 0;
		foreach (NodeStateEntry<TInput> item3 in sourceTable)
		{
			arrayBuilder?.Add((item3.Step, item3.OutputIndex));
			if (item3.State != EntryState.Removed)
			{
				num++;
			}
		}
		ImmutableArray<(IncrementalGeneratorRunStep, int)> item = arrayBuilder?.ToImmutableAndFree() ?? default(ImmutableArray<(IncrementalGeneratorRunStep, int)>);
		return (tryReusePreviousTableValues(num) ?? computeCurrentTableValues(num), item);
		ImmutableArray<TInput> computeCurrentTableValues(int entryCount)
		{
			ArrayBuilder<TInput> instance = ArrayBuilder<TInput>.GetInstance(entryCount);
			foreach (NodeStateEntry<TInput> item4 in sourceTable)
			{
				if (item4.State != EntryState.Removed)
				{
					instance.Add(item4.Item);
				}
			}
			return instance.ToImmutableAndFree();
		}
		ImmutableArray<TInput>? tryReusePreviousTableValues(int entryCount)
		{
			if (previousTable == null)
			{
				return null;
			}
			if (previousTable.Count != 1)
			{
				return null;
			}
			ImmutableArray<TInput> item2 = previousTable.Single().item;
			if (item2.Length != entryCount)
			{
				return null;
			}
			int num2 = 0;
			foreach (NodeStateEntry<TInput> item5 in sourceTable)
			{
				if (item5.State != EntryState.Removed)
				{
					if (!EqualityComparer<TInput>.Default.Equals(item5.Item, item2[num2]))
					{
						return null;
					}
					num2++;
				}
			}
			return item2;
		}
	}

	public NodeStateTable<ImmutableArray<TInput>> UpdateStateTable(DriverStateTable.Builder builder, NodeStateTable<ImmutableArray<TInput>>? previousTable, CancellationToken cancellationToken)
	{
		NodeStateTable<TInput> latestStateTableForNode = builder.GetLatestStateTableForNode(_sourceNode);
		NodeStateTable<ImmutableArray<TInput>>.Builder builder2 = builder.CreateTableBuilder<ImmutableArray<TInput>>(previousTable, _name, _comparer);
		SharedStopwatch sharedStopwatch = SharedStopwatch.StartNew();
		var (value, stepInputs) = GetValuesAndInputs(latestStateTableForNode, previousTable, builder2);
		if (previousTable == null || previousTable.IsEmpty)
		{
			builder2.AddEntry(value, EntryState.Added, sharedStopwatch.Elapsed, stepInputs, EntryState.Added);
		}
		else if ((!latestStateTableForNode.IsCached || !builder2.TryUseCachedEntries(sharedStopwatch.Elapsed, stepInputs)) && !builder2.TryModifyEntry(value, sharedStopwatch.Elapsed, stepInputs, EntryState.Modified))
		{
			builder2.AddEntry(value, EntryState.Added, sharedStopwatch.Elapsed, stepInputs, EntryState.Added);
		}
		NodeStateTable<ImmutableArray<TInput>> nodeStateTable = builder2.ToImmutableAndFree();
		this.LogTables<ImmutableArray<TInput>, TInput>(_name, s_tableType, previousTable, nodeStateTable, latestStateTableForNode);
		return nodeStateTable;
	}

	public void RegisterOutput(IIncrementalGeneratorOutputNode output)
	{
		_sourceNode.RegisterOutput(output);
	}
}
