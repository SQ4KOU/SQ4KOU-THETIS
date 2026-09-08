using System;

namespace Microsoft.Extensions.DependencyInjection;

public interface IServiceProviderIsKeyedService : IServiceProviderIsService
{
	bool IsKeyedService(Type serviceType, object? serviceKey);
}
