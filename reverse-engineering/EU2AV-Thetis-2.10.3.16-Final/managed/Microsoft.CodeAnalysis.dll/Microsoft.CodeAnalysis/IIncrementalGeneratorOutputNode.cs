using System.Threading;

namespace Microsoft.CodeAnalysis;

internal interface IIncrementalGeneratorOutputNode
{
	IncrementalGeneratorOutputKind Kind { get; }

	void AppendOutputs(IncrementalExecutionContext context, CancellationToken cancellationToken);
}
