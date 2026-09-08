using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class SyntaxStore
{
	public sealed class Builder
	{
		private readonly ImmutableDictionary<SyntaxInputNode, Exception>.Builder _syntaxExceptions = ImmutableDictionary.CreateBuilder<SyntaxInputNode, Exception>();

		private readonly ImmutableDictionary<SyntaxInputNode, TimeSpan>.Builder _syntaxTimes = ImmutableDictionary.CreateBuilder<SyntaxInputNode, TimeSpan>();

		private readonly StateTableStore.Builder _tableBuilder = new StateTableStore.Builder();

		private readonly Compilation _compilation;

		private readonly ImmutableArray<SyntaxInputNode> _syntaxInputNodes;

		private readonly bool _enableTracking;

		private readonly SyntaxStore _previous;

		private readonly CancellationToken _cancellationToken;

		internal Builder(Compilation compilation, ImmutableArray<SyntaxInputNode> syntaxInputNodes, bool enableTracking, SyntaxStore previousStore, CancellationToken cancellationToken)
		{
			_compilation = compilation;
			_syntaxInputNodes = syntaxInputNodes;
			_enableTracking = enableTracking;
			_previous = previousStore;
			_cancellationToken = cancellationToken;
		}

		public IStateTable GetSyntaxInputTable(SyntaxInputNode syntaxInputNode, NodeStateTable<SyntaxTree> syntaxTreeTable)
		{
			if (!_tableBuilder.Contains(syntaxInputNode))
			{
				bool flag = _compilation == _previous._compilation;
				ArrayBuilder<(SyntaxInputNode, ISyntaxInputBuilder)> instance = ArrayBuilder<(SyntaxInputNode, ISyntaxInputBuilder)>.GetInstance(_syntaxInputNodes.Length);
				foreach (SyntaxInputNode syntaxInputNode2 in _syntaxInputNodes)
				{
					if (flag && !_enableTracking && _previous._tables.TryGetValue(syntaxInputNode2, out IStateTable table))
					{
						_tableBuilder.SetTable(syntaxInputNode2, table);
						continue;
					}
					instance.Add((syntaxInputNode2, syntaxInputNode2.GetBuilder(_previous._tables, _enableTracking)));
					_syntaxTimes[syntaxInputNode2] = TimeSpan.Zero;
				}
				if (instance.Count > 0)
				{
					_syntaxTimes[syntaxInputNode] = TimeSpan.Zero;
					foreach (var (tree, entryState2, _, _) in syntaxTreeTable)
					{
						Lazy<SyntaxNode> root = new Lazy<SyntaxNode>(() => tree.GetRoot(_cancellationToken));
						Lazy<SemanticModel> model = ((entryState2 != EntryState.Removed) ? new Lazy<SemanticModel>(() => _compilation.GetSemanticModel(tree)) : null);
						for (int num2 = 0; num2 < instance.Count; num2++)
						{
							SyntaxInputNode item = instance[num2].Item1;
							try
							{
								SharedStopwatch sharedStopwatch = SharedStopwatch.StartNew();
								try
								{
									CancellationToken cancellationToken = _cancellationToken;
									cancellationToken.ThrowIfCancellationRequested();
									instance[num2].Item2.VisitTree(root, entryState2, model, _cancellationToken);
								}
								finally
								{
									TimeSpan elapsed = sharedStopwatch.Elapsed;
									if (item != syntaxInputNode)
									{
										_syntaxTimes[syntaxInputNode] = _syntaxTimes[syntaxInputNode].Subtract(elapsed);
										_syntaxTimes[item] = _syntaxTimes[item].Add(elapsed);
									}
								}
							}
							catch (UserFunctionException value)
							{
								_syntaxExceptions[item] = value;
								instance.RemoveAt(num2);
								num2--;
							}
						}
					}
					foreach (var item2 in instance)
					{
						item2.Item2.SaveStateAndFree(_tableBuilder);
					}
				}
				instance.Free();
			}
			if (!_tableBuilder.TryGetTable(syntaxInputNode, out IStateTable table2))
			{
				throw _syntaxExceptions[syntaxInputNode];
			}
			return table2;
		}

		public TimeSpan GetRuntimeAdjustment(ImmutableArray<SyntaxInputNode> inputNodes)
		{
			TimeSpan result = TimeSpan.Zero;
			foreach (SyntaxInputNode item in inputNodes)
			{
				if (_syntaxTimes.TryGetValue(item, out var value))
				{
					result = result.Add(value);
				}
			}
			return result;
		}

		public SyntaxStore ToImmutable()
		{
			return new SyntaxStore(_tableBuilder.ToImmutable(), _compilation);
		}
	}

	private readonly StateTableStore _tables;

	private readonly Compilation? _compilation;

	internal static readonly SyntaxStore Empty = new SyntaxStore(StateTableStore.Empty, null);

	private SyntaxStore(StateTableStore tables, Compilation? compilation)
	{
		_tables = tables;
		_compilation = compilation;
	}

	public Builder ToBuilder(Compilation compilation, ImmutableArray<SyntaxInputNode> syntaxInputNodes, bool enableTracking, CancellationToken cancellationToken)
	{
		return new Builder(compilation, syntaxInputNodes, enableTracking, this, cancellationToken);
	}
}
