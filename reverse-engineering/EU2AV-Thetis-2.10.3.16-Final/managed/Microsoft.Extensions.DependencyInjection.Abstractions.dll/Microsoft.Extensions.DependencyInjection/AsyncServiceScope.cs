using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection;

[DebuggerDisplay("{ServiceProvider,nq}")]
public readonly struct AsyncServiceScope : IServiceScope, IDisposable, IAsyncDisposable
{
	private readonly IServiceScope _serviceScope;

	public IServiceProvider ServiceProvider => _serviceScope.ServiceProvider;

	public AsyncServiceScope(IServiceScope serviceScope)
	{
		System.ExceptionPolyfills.ThrowIfNull(serviceScope, "serviceScope");
		_serviceScope = serviceScope;
	}

	public void Dispose()
	{
		_serviceScope.Dispose();
	}

	public ValueTask DisposeAsync()
	{
		if (_serviceScope is IAsyncDisposable asyncDisposable)
		{
			return asyncDisposable.DisposeAsync();
		}
		_serviceScope.Dispose();
		return default(ValueTask);
	}
}
