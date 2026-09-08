using System;
using Microsoft.Extensions.DependencyInjection;

namespace Discord.Interactions;

internal class EmptyServiceScope : IServiceScope, IDisposable
{
	public IServiceProvider ServiceProvider => EmptyServiceProvider.Instance;

	public void Dispose()
	{
	}
}
