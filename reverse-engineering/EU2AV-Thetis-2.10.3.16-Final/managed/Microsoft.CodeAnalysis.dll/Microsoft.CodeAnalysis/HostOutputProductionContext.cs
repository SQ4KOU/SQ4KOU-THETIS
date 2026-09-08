using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

[Experimental("RSEXPERIMENTAL004", UrlFormat = "https://github.com/dotnet/roslyn/issues/74753")]
public readonly struct HostOutputProductionContext
{
	internal readonly ArrayBuilder<(string, object)> Outputs;

	public CancellationToken CancellationToken { get; }

	internal HostOutputProductionContext(ArrayBuilder<(string, object)> outputs, CancellationToken cancellationToken)
	{
		Outputs = outputs;
		CancellationToken = cancellationToken;
	}

	public void AddOutput(string name, object value)
	{
		Outputs.Add((name, value));
	}
}
