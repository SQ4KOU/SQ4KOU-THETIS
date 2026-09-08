using System.Threading;

namespace Microsoft.CodeAnalysis;

public readonly struct GeneratorFilterContext
{
	public ISourceGenerator Generator { get; }

	public CancellationToken CancellationToken { get; }

	internal GeneratorFilterContext(ISourceGenerator generator, CancellationToken cancellationToken)
	{
		Generator = generator;
		CancellationToken = cancellationToken;
	}
}
