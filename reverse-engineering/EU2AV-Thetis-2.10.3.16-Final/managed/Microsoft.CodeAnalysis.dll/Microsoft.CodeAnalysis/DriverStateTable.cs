using System.Collections.Generic;
using System.Threading;

namespace Microsoft.CodeAnalysis;

internal sealed class DriverStateTable
{
	public sealed class Builder
	{
		private readonly StateTableStore.Builder _stateTableBuilder = new StateTableStore.Builder();

		private readonly DriverStateTable _previousTable;

		private readonly CancellationToken _cancellationToken;

		internal GeneratorDriverState DriverState { get; }

		public Compilation Compilation { get; }

		internal SyntaxStore.Builder SyntaxStore { get; }

		public Builder(Compilation compilation, GeneratorDriverState driverState, SyntaxStore.Builder syntaxStore, CancellationToken cancellationToken = default(CancellationToken))
		{
			Compilation = compilation;
			DriverState = driverState;
			_previousTable = driverState.StateTable;
			_cancellationToken = cancellationToken;
			SyntaxStore = syntaxStore;
		}

		public NodeStateTable<T> GetLatestStateTableForNode<T>(IIncrementalGeneratorNode<T> source)
		{
			if (_stateTableBuilder.TryGetTable(source, out IStateTable table))
			{
				return (NodeStateTable<T>)table;
			}
			NodeStateTable<T> stateTable = _previousTable._tables.GetStateTable<T>(source);
			NodeStateTable<T> nodeStateTable = source.UpdateStateTable(this, stateTable, _cancellationToken);
			_stateTableBuilder.SetTable(source, nodeStateTable);
			return nodeStateTable;
		}

		public NodeStateTable<T>.Builder CreateTableBuilder<T>(NodeStateTable<T>? previousTable, string? stepName, IEqualityComparer<T>? equalityComparer, int? tableCapacity = null)
		{
			if (previousTable == null)
			{
				previousTable = NodeStateTable<T>.Empty;
			}
			return previousTable.ToBuilder(stepName, DriverState.TrackIncrementalSteps, equalityComparer, tableCapacity);
		}

		public DriverStateTable ToImmutable()
		{
			return new DriverStateTable(_stateTableBuilder.ToImmutable());
		}
	}

	private readonly StateTableStore _tables;

	internal static DriverStateTable Empty { get; } = new DriverStateTable(StateTableStore.Empty);

	private DriverStateTable(StateTableStore tables)
	{
		_tables = tables;
	}
}
