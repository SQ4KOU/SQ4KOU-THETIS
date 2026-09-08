using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class CombineNode<TInput1, TInput2> : IIncrementalGeneratorNode<(TInput1, TInput2)>
{
	private static readonly string? s_tableType = typeof((TInput1, TInput2)).FullName;

	private readonly IIncrementalGeneratorNode<TInput1> _input1;

	private readonly IIncrementalGeneratorNode<TInput2> _input2;

	private readonly IEqualityComparer<(TInput1, TInput2)>? _comparer;

	private readonly string? _name;

	public CombineNode(IIncrementalGeneratorNode<TInput1> input1, IIncrementalGeneratorNode<TInput2> input2, IEqualityComparer<(TInput1, TInput2)>? comparer = null, string? name = null)
	{
		_input1 = input1;
		_input2 = input2;
		_comparer = comparer;
		_name = name;
	}

	public NodeStateTable<(TInput1, TInput2)> UpdateStateTable(DriverStateTable.Builder graphState, NodeStateTable<(TInput1, TInput2)>? previousTable, CancellationToken cancellationToken)
	{
		NodeStateTable<TInput1> latestStateTableForNode = graphState.GetLatestStateTableForNode(_input1);
		NodeStateTable<TInput2> latestStateTableForNode2 = graphState.GetLatestStateTableForNode(_input2);
		if (latestStateTableForNode.IsCached && latestStateTableForNode2.IsCached && previousTable != null)
		{
			this.LogTables<(TInput1, TInput2), TInput1, TInput2>(_name, s_tableType, previousTable, previousTable, latestStateTableForNode, latestStateTableForNode2);
			if (graphState.DriverState.TrackIncrementalSteps)
			{
				return RecordStepsForCachedTable(graphState, previousTable, latestStateTableForNode, latestStateTableForNode2);
			}
			return previousTable;
		}
		int totalEntryItemCount = latestStateTableForNode.GetTotalEntryItemCount();
		NodeStateTable<(TInput1, TInput2)>.Builder builder = graphState.CreateTableBuilder<(TInput1, TInput2)>(previousTable, _name, _comparer, totalEntryItemCount);
		bool isCached = latestStateTableForNode2.IsCached;
		(TInput2 item, IncrementalGeneratorRunStep? step) tuple = latestStateTableForNode2.Single();
		TInput2 item = tuple.item;
		IncrementalGeneratorRunStep item2 = tuple.step;
		foreach (NodeStateEntry<TInput1> item3 in latestStateTableForNode)
		{
			SharedStopwatch sharedStopwatch = SharedStopwatch.StartNew();
			ImmutableArray<(IncrementalGeneratorRunStep, int)> stepInputs = (builder.TrackIncrementalSteps ? ImmutableArray.Create<(IncrementalGeneratorRunStep, int)>((item3.Step, item3.OutputIndex), (item2, 0)) : default(ImmutableArray<(IncrementalGeneratorRunStep, int)>));
			EntryState entryState = ((item3.State != EntryState.Cached) ? item3.State : ((!isCached) ? EntryState.Modified : EntryState.Cached));
			EntryState entryState2 = entryState;
			(TInput1, TInput2) value = (item3.Item, item);
			if (entryState2 != EntryState.Modified || _comparer == null || !builder.TryModifyEntry(value, sharedStopwatch.Elapsed, stepInputs, entryState2))
			{
				builder.AddEntry(value, entryState2, sharedStopwatch.Elapsed, stepInputs, entryState2);
			}
		}
		NodeStateTable<(TInput1, TInput2)> nodeStateTable = builder.ToImmutableAndFree();
		this.LogTables<(TInput1, TInput2), TInput1, TInput2>(_name, s_tableType, previousTable, nodeStateTable, latestStateTableForNode, latestStateTableForNode2);
		return nodeStateTable;
	}

	private NodeStateTable<(TInput1, TInput2)> RecordStepsForCachedTable(DriverStateTable.Builder graphState, NodeStateTable<(TInput1, TInput2)> previousTable, NodeStateTable<TInput1> input1Table, NodeStateTable<TInput2> input2Table)
	{
		NodeStateTable<(TInput1, TInput2)>.Builder builder = graphState.CreateTableBuilder<(TInput1, TInput2)>(previousTable, _name, _comparer);
		IncrementalGeneratorRunStep item = input2Table.Single().step;
		foreach (NodeStateEntry<TInput1> item2 in input1Table)
		{
			ImmutableArray<(IncrementalGeneratorRunStep, int)> stepInputs = ImmutableArray.Create<(IncrementalGeneratorRunStep, int)>((item2.Step, item2.OutputIndex), (item, 0));
			builder.TryUseCachedEntries(TimeSpan.Zero, stepInputs);
		}
		return builder.ToImmutableAndFree();
	}

	public IIncrementalGeneratorNode<(TInput1, TInput2)> WithComparer(IEqualityComparer<(TInput1, TInput2)> comparer)
	{
		return new CombineNode<TInput1, TInput2>(_input1, _input2, comparer, _name);
	}

	public IIncrementalGeneratorNode<(TInput1, TInput2)> WithTrackingName(string name)
	{
		return new CombineNode<TInput1, TInput2>(_input1, _input2, _comparer, name);
	}

	public void RegisterOutput(IIncrementalGeneratorOutputNode output)
	{
		_input1.RegisterOutput(output);
		_input2.RegisterOutput(output);
	}
}
