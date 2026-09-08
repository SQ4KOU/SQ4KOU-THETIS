using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class HostOutputNode<TInput> : IIncrementalGeneratorOutputNode, IIncrementalGeneratorNode<ImmutableArray<(string, object)>>
{
	private readonly IIncrementalGeneratorNode<TInput> _source;

	private readonly Action<HostOutputProductionContext, TInput, CancellationToken> _action;

	public IncrementalGeneratorOutputKind Kind => IncrementalGeneratorOutputKind.Host;

	public HostOutputNode(IIncrementalGeneratorNode<TInput> source, Action<HostOutputProductionContext, TInput, CancellationToken> action)
	{
		_source = source;
		_action = action;
	}

	public NodeStateTable<ImmutableArray<(string, object)>> UpdateStateTable(DriverStateTable.Builder graphState, NodeStateTable<ImmutableArray<(string, object)>>? previousTable, CancellationToken cancellationToken)
	{
		string stepName = "HostOutput";
		NodeStateTable<TInput> latestStateTableForNode = graphState.GetLatestStateTableForNode(_source);
		if (latestStateTableForNode.IsCached && previousTable != null)
		{
			if (graphState.DriverState.TrackIncrementalSteps)
			{
				return previousTable.CreateCachedTableWithUpdatedSteps(latestStateTableForNode, stepName, EqualityComparer<ImmutableArray<(string, object)>>.Default);
			}
			return previousTable;
		}
		NodeStateTable<ImmutableArray<(string, object)>>.Builder builder = graphState.CreateTableBuilder<ImmutableArray<(string, object)>>(previousTable, stepName, EqualityComparer<ImmutableArray<(string, object)>>.Default);
		foreach (NodeStateEntry<TInput> item in latestStateTableForNode)
		{
			ImmutableArray<(IncrementalGeneratorRunStep, int)> stepInputs = (builder.TrackIncrementalSteps ? ImmutableArray.Create((item.Step, item.OutputIndex)) : default(ImmutableArray<(IncrementalGeneratorRunStep, int)>));
			if (item.State == EntryState.Removed)
			{
				builder.TryRemoveEntries(TimeSpan.Zero, stepInputs);
			}
			else if (item.State != EntryState.Cached || !builder.TryUseCachedEntries(TimeSpan.Zero, stepInputs))
			{
				ArrayBuilder<(string, object)> instance = ArrayBuilder<(string, object)>.GetInstance();
				HostOutputProductionContext arg = new HostOutputProductionContext(instance, cancellationToken);
				SharedStopwatch sharedStopwatch = SharedStopwatch.StartNew();
				_action(arg, item.Item, cancellationToken);
				builder.AddEntry(instance.ToImmutableAndFree(), EntryState.Added, sharedStopwatch.Elapsed, stepInputs, EntryState.Added);
			}
		}
		return builder.ToImmutableAndFree();
	}

	public void AppendOutputs(IncrementalExecutionContext context, CancellationToken cancellationToken)
	{
		NodeStateTable<ImmutableArray<(string, object)>> latestStateTableForNode = context.TableBuilder.GetLatestStateTableForNode(this);
		foreach (var (immutableArray2, entryState2, _, _) in latestStateTableForNode)
		{
			if (entryState2 == EntryState.Removed)
			{
				continue;
			}
			foreach (var (key, value) in immutableArray2)
			{
				try
				{
					context.HostOutputBuilder.Add(key, value);
				}
				catch (ArgumentException innerException)
				{
					throw new UserFunctionException(innerException);
				}
			}
		}
		if (context.GeneratorRunStateBuilder.RecordingExecutedSteps)
		{
			context.GeneratorRunStateBuilder.RecordStepsFromOutputNodeUpdate(latestStateTableForNode);
		}
	}

	IIncrementalGeneratorNode<ImmutableArray<(string, object)>> IIncrementalGeneratorNode<ImmutableArray<(string, object)>>.WithComparer(IEqualityComparer<ImmutableArray<(string, object)>> comparer)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/SourceGeneration/Nodes/HostOutputNode.cs", 98);
	}

	public IIncrementalGeneratorNode<ImmutableArray<(string, object)>> WithTrackingName(string name)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/SourceGeneration/Nodes/HostOutputNode.cs", 100);
	}

	void IIncrementalGeneratorNode<ImmutableArray<(string, object)>>.RegisterOutput(IIncrementalGeneratorOutputNode output)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/SourceGeneration/Nodes/HostOutputNode.cs", 102);
	}
}
