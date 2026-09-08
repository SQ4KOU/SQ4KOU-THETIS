using System;
using Microsoft.Extensions.DependencyInjection;

namespace Discord.Interactions;

internal class EmptyServiceProvider : IServiceProvider
{
	public static EmptyServiceProvider Instance => new EmptyServiceProvider();

	public object GetService(Type serviceType)
	{
		if (serviceType == typeof(IServiceScopeFactory))
		{
			return EmptyServiceScopeFactory.Instance;
		}
		return null;
	}
}
