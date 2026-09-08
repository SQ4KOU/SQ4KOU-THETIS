using System;

namespace Microsoft.Extensions.DependencyInjection;

public interface IKeyedServiceProvider : IServiceProvider
{
	object? GetKeyedService(Type serviceType, object? serviceKey);

	object GetRequiredKeyedService(Type serviceType, object? serviceKey);
}
