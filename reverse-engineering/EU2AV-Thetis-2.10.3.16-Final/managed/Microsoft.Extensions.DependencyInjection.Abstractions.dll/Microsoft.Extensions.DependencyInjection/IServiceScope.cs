using System;

namespace Microsoft.Extensions.DependencyInjection;

public interface IServiceScope : IDisposable
{
	IServiceProvider ServiceProvider { get; }
}
