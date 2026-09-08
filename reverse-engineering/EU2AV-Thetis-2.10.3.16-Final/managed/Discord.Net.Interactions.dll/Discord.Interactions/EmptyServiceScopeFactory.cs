using Microsoft.Extensions.DependencyInjection;

namespace Discord.Interactions;

internal class EmptyServiceScopeFactory : IServiceScopeFactory
{
	public static EmptyServiceScopeFactory Instance => new EmptyServiceScopeFactory();

	public IServiceScope CreateScope()
	{
		return new EmptyServiceScope();
	}
}
