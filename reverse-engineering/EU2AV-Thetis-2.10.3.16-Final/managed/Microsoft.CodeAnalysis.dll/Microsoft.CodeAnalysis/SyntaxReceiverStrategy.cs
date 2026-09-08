using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class SyntaxReceiverStrategy<T> : ISyntaxSelectionStrategy<T>
{
	private sealed class Builder : ISyntaxInputBuilder
	{
		private readonly object _key;

		private readonly NodeStateTable<ISyntaxContextReceiver?>.Builder _nodeStateTable;

		private readonly ISyntaxContextReceiver? _receiver;

		private readonly GeneratorSyntaxWalker? _walker;

		private TimeSpan lastElapsedTime;

		private bool TrackIncrementalSteps => _nodeStateTable.TrackIncrementalSteps;

		public Builder(SyntaxReceiverStrategy<T> owner, object key, StateTableStore driverStateTable, bool trackIncrementalSteps)
		{
			_key = key;
			_nodeStateTable = driverStateTable.GetStateTableOrEmpty<ISyntaxContextReceiver>(_key).ToBuilder(null, trackIncrementalSteps);
			try
			{
				_receiver = owner._receiverCreator();
			}
			catch (Exception innerException)
			{
				throw new UserFunctionException(innerException);
			}
			if (_receiver != null)
			{
				_walker = new GeneratorSyntaxWalker(_receiver, owner._syntaxHelper);
			}
		}

		public void SaveStateAndFree(StateTableStore.Builder tables)
		{
			_nodeStateTable.AddEntry(_receiver, EntryState.Modified, lastElapsedTime, TrackIncrementalSteps ? ImmutableArray<(IncrementalGeneratorRunStep, int)>.Empty : default(ImmutableArray<(IncrementalGeneratorRunStep, int)>), EntryState.Modified);
			tables.SetTable(_key, _nodeStateTable.ToImmutableAndFree());
		}

		public void VisitTree(Lazy<SyntaxNode> root, EntryState state, Lazy<SemanticModel>? model, CancellationToken cancellationToken)
		{
			if (_walker == null || state == EntryState.Removed)
			{
				return;
			}
			try
			{
				SharedStopwatch sharedStopwatch = SharedStopwatch.StartNew();
				_walker.VisitWithModel(model, root.Value);
				if (TrackIncrementalSteps)
				{
					lastElapsedTime = sharedStopwatch.Elapsed;
				}
			}
			catch (Exception ex) when (!ExceptionUtilities.IsCurrentOperationBeingCancelled(ex, cancellationToken))
			{
				throw new UserFunctionException(ex);
			}
		}
	}

	private readonly SyntaxContextReceiverCreator _receiverCreator;

	private readonly Action<IIncrementalGeneratorOutputNode> _registerOutput;

	private readonly ISyntaxHelper _syntaxHelper;

	public SyntaxReceiverStrategy(SyntaxContextReceiverCreator receiverCreator, Action<IIncrementalGeneratorOutputNode> registerOutput, ISyntaxHelper syntaxHelper)
	{
		_receiverCreator = receiverCreator;
		_registerOutput = registerOutput;
		_syntaxHelper = syntaxHelper;
	}

	public ISyntaxInputBuilder GetBuilder(StateTableStore table, object key, bool trackIncrementalSteps, string? name, IEqualityComparer<T> comparer)
	{
		return new Builder(this, key, table, trackIncrementalSteps);
	}
}
