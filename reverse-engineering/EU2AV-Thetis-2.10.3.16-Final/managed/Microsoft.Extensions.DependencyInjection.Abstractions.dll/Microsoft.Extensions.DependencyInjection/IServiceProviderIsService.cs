using System;

namespace Microsoft.Extensions.DependencyInjection;

public interface IServiceProviderIsService
{
	bool IsService(Type serviceType);
}
