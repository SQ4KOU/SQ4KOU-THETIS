using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class SourceOutputNode<TInput> : IIncrementalGeneratorOutputNode, IIncrementalGeneratorNode<(IEnumerable<GeneratedSourceText>, IEnumerable<Diagnostic>)>
{
	private static readonly string? s_tableType = typeof((IEnumerable<GeneratedSourceText>, IEnumerable<Diagnostic>)).FullName;

	private readonly IIncrementalGeneratorNode<TInput> _source;

	private readonly Action<SourceProductionContext, TInput, CancellationToken> _action;

	private readonly IncrementalGeneratorOutputKind _outputKind;

	private readonly string _sourceExtension;

	public IncrementalGeneratorOutputKind Kind => _outputKind;

	public SourceOutputNode(IIncrementalGeneratorNode<TInput> source, Action<SourceProductionContext, TInput, CancellationToken> action, IncrementalGeneratorOutputKind outputKind, string sourceExtension)
	{
		_source = source;
		_action = action;
		_outputKind = outputKind;
		_sourceExtension = sourceExtension;
	}

	public NodeStateTable<(IEnumerable<GeneratedSourceText>, IEnumerable<Diagnostic>)> UpdateStateTable(DriverStateTable.Builder graphState, NodeStateTable<(IEnumerable<GeneratedSourceText>, IEnumerable<Diagnostic>)>? previousTable, CancellationToken cancellationToken)
	{
		string text = ((Kind == IncrementalGeneratorOutputKind.Source) ? "SourceOutput" : "ImplementationSourceOutput");
		NodeStateTable<TInput> latestStateTableForNode = graphState.GetLatestStateTableForNode(_source);
		if (latestStateTableForNode.IsCached && previousTable != null)
		{
			this.LogTables(text, s_tableType, previousTable, previousTable, latestStateTableForNode);
			if (graphState.DriverState.TrackIncrementalSteps)
			{
				return previousTable.CreateCachedTableWithUpdatedSteps(latestStateTableForNode, text, null);
			}
			return previousTable;
		}
		NodeStateTable<(IEnumerable<GeneratedSourceText>, IEnumerable<Diagnostic>)>.Builder builder = graphState.CreateTableBuilder(previousTable, text, null);
		foreach (NodeStateEntry<TInput> item in latestStateTableForNode)
		{
			ImmutableArray<(IncrementalGeneratorRunStep, int)> stepInputs = (builder.TrackIncrementalSteps ? ImmutableArray.Create((item.Step, item.OutputIndex)) : default(ImmutableArray<(IncrementalGeneratorRunStep, int)>));
			if (item.State == EntryState.Removed)
			{
				builder.TryRemoveEntries(TimeSpan.Zero, stepInputs);
			}
			else
			{
				if (item.State == EntryState.Cached && builder.TryUseCachedEntries(TimeSpan.Zero, stepInputs))
				{
					continue;
				}
				AdditionalSourcesCollection additionalSourcesCollection = new AdditionalSourcesCollection(_sourceExtension);
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				SourceProductionContext arg = new SourceProductionContext(additionalSourcesCollection, instance, graphState.Compilation, graphState.DriverState.ChecksumAlgorithm, cancellationToken);
				try
				{
					SharedStopwatch sharedStopwatch = SharedStopwatch.StartNew();
					_action(arg, item.Item, cancellationToken);
					(ImmutableArray<GeneratedSourceText>, ImmutableArray<Diagnostic>) tuple = (additionalSourcesCollection.ToImmutable(), instance.ToReadOnly());
					if (item.State != EntryState.Modified)
					{
						goto IL_0191;
					}
					(ImmutableArray<GeneratedSourceText>, ImmutableArray<Diagnostic>) tuple2 = tuple;
					if (!builder.TryModifyEntry((tuple2.Item1, tuple2.Item2), sharedStopwatch.Elapsed, stepInputs, item.State))
					{
						goto IL_0191;
					}
					goto end_IL_011c;
					IL_0191:
					tuple2 = tuple;
					builder.AddEntry((tuple2.Item1, tuple2.Item2), EntryState.Added, sharedStopwatch.Elapsed, stepInputs, EntryState.Added);
					end_IL_011c:;
				}
				finally
				{
					additionalSourcesCollection.Free();
					instance.Free();
				}
			}
		}
		NodeStateTable<(IEnumerable<GeneratedSourceText>, IEnumerable<Diagnostic>)> nodeStateTable = builder.ToImmutableAndFree();
		this.LogTables<(IEnumerable<GeneratedSourceText>, IEnumerable<Diagnostic>), TInput>(text, s_tableType, previousTable, nodeStateTable, latestStateTableForNode);
		return nodeStateTable;
	}

	IIncrementalGeneratorNode<(IEnumerable<GeneratedSourceText>, IEnumerable<Diagnostic>)> IIncrementalGeneratorNode<(IEnumerable<GeneratedSourceText>, IEnumerable<Diagnostic>)>.WithComparer(IEqualityComparer<(IEnumerable<GeneratedSourceText>, IEnumerable<Diagnostic>)> comparer)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/SourceGeneration/Nodes/SourceOutputNode.cs", 92);
	}

	public IIncrementalGeneratorNode<(IEnumerable<GeneratedSourceText>, IEnumerable<Diagnostic>)> WithTrackingName(string name)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/SourceGeneration/Nodes/SourceOutputNode.cs", 94);
	}

	void IIncrementalGeneratorNode<(IEnumerable<GeneratedSourceText>, IEnumerable<Diagnostic>)>.RegisterOutput(IIncrementalGeneratorOutputNode output)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/SourceGeneration/Nodes/SourceOutputNode.cs", 96);
	}

	public void AppendOutputs(IncrementalExecutionContext context, CancellationToken cancellationToken)
	{
		NodeStateTable<(IEnumerable<GeneratedSourceText>, IEnumerable<Diagnostic>)> latestStateTableForNode = context.TableBuilder.GetLatestStateTableForNode(this);
		foreach (NodeStateEntry<(IEnumerable<GeneratedSourceText>, IEnumerable<Diagnostic>)> item in latestStateTableForNode)
		{
			item.Deconstruct(out (IEnumerable<GeneratedSourceText>, IEnumerable<Diagnostic>) Item, out EntryState State, out int _, out IncrementalGeneratorRunStep _);
			var (enumerable, diagnostics) = Item;
			if (State == EntryState.Removed)
			{
				continue;
			}
			foreach (GeneratedSourceText item2 in enumerable)
			{
				try
				{
					context.Sources.Add(item2.HintName, item2.Text);
				}
				catch (ArgumentException innerException)
				{
					throw new UserFunctionException(innerException);
				}
			}
			context.Diagnostics.AddRange(diagnostics);
		}
		if (context.GeneratorRunStateBuilder.RecordingExecutedSteps)
		{
			context.GeneratorRunStateBuilder.RecordStepsFromOutputNodeUpdate(latestStateTableForNode);
		}
	}
}
