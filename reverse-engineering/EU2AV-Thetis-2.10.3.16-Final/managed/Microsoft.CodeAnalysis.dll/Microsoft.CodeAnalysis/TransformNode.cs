using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class TransformNode<TInput, TOutput> : IIncrementalGeneratorNode<TOutput>
{
	private static readonly string? s_tableType = typeof(TOutput).FullName;

	private readonly Func<TInput, CancellationToken, ImmutableArray<TOutput>> _func;

	private readonly IEqualityComparer<TOutput>? _comparer;

	private readonly IIncrementalGeneratorNode<TInput> _sourceNode;

	private readonly string? _name;

	private readonly bool _wrapUserFunc;

	public TransformNode(IIncrementalGeneratorNode<TInput> sourceNode, Func<TInput, CancellationToken, TOutput> userFunc, bool wrapUserFunc = false, IEqualityComparer<TOutput>? comparer = null, string? name = null)
		: this(sourceNode, (Func<TInput, CancellationToken, ImmutableArray<TOutput>>)((TInput i, CancellationToken token) => ImmutableArray.Create(userFunc(i, token))), wrapUserFunc, comparer, name)
	{
	}

	public TransformNode(IIncrementalGeneratorNode<TInput> sourceNode, Func<TInput, CancellationToken, ImmutableArray<TOutput>> userFunc, bool wrapUserFunc = false, IEqualityComparer<TOutput>? comparer = null, string? name = null)
	{
		_sourceNode = sourceNode;
		_func = userFunc;
		_wrapUserFunc = wrapUserFunc;
		_comparer = comparer;
		_name = name;
	}

	public IIncrementalGeneratorNode<TOutput> WithComparer(IEqualityComparer<TOutput> comparer)
	{
		return new TransformNode<TInput, TOutput>(_sourceNode, _func, _wrapUserFunc, comparer, _name);
	}

	public IIncrementalGeneratorNode<TOutput> WithTrackingName(string name)
	{
		return new TransformNode<TInput, TOutput>(_sourceNode, _func, _wrapUserFunc, _comparer, name);
	}

	public NodeStateTable<TOutput> UpdateStateTable(DriverStateTable.Builder builder, NodeStateTable<TOutput>? previousTable, CancellationToken cancellationToken)
	{
		NodeStateTable<TInput> latestStateTableForNode = builder.GetLatestStateTableForNode(_sourceNode);
		if (latestStateTableForNode.IsCached && previousTable != null)
		{
			this.LogTables(_name, s_tableType, previousTable, previousTable, latestStateTableForNode);
			if (builder.DriverState.TrackIncrementalSteps)
			{
				return previousTable.CreateCachedTableWithUpdatedSteps(latestStateTableForNode, _name, _comparer);
			}
			return previousTable;
		}
		int totalEntryItemCount = latestStateTableForNode.GetTotalEntryItemCount();
		NodeStateTable<TOutput>.Builder builder2 = builder.CreateTableBuilder(previousTable, _name, _comparer, totalEntryItemCount);
		foreach (NodeStateEntry<TInput> item in latestStateTableForNode)
		{
			ImmutableArray<(IncrementalGeneratorRunStep, int)> stepInputs = (builder2.TrackIncrementalSteps ? ImmutableArray.Create((item.Step, item.OutputIndex)) : default(ImmutableArray<(IncrementalGeneratorRunStep, int)>));
			if (item.State == EntryState.Removed)
			{
				builder2.TryRemoveEntries(TimeSpan.Zero, stepInputs);
			}
			else if (item.State != EntryState.Cached || !builder2.TryUseCachedEntries(TimeSpan.Zero, stepInputs))
			{
				SharedStopwatch sharedStopwatch = SharedStopwatch.StartNew();
				ImmutableArray<TOutput> immutableArray;
				try
				{
					immutableArray = _func(item.Item, cancellationToken);
				}
				catch (Exception ex) when (_wrapUserFunc && !ExceptionUtilities.IsCurrentOperationBeingCancelled(ex, cancellationToken))
				{
					throw new UserFunctionException(ex);
				}
				if (item.State != EntryState.Modified || !builder2.TryModifyEntries(immutableArray, sharedStopwatch.Elapsed, stepInputs, item.State))
				{
					builder2.AddEntries(immutableArray, EntryState.Added, sharedStopwatch.Elapsed, stepInputs, item.State);
				}
			}
		}
		NodeStateTable<TOutput> nodeStateTable = builder2.ToImmutableAndFree();
		this.LogTables(_name, s_tableType, previousTable, nodeStateTable, latestStateTableForNode);
		return nodeStateTable;
	}

	public void RegisterOutput(IIncrementalGeneratorOutputNode output)
	{
		_sourceNode.RegisterOutput(output);
	}
}
