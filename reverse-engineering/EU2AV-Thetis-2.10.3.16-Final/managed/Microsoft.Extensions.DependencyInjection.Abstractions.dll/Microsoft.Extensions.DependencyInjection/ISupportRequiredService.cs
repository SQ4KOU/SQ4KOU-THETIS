using System;

namespace Microsoft.Extensions.DependencyInjection;

public interface ISupportRequiredService
{
	object GetRequiredService(Type serviceType);
}
