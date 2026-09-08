namespace Microsoft.Extensions.DependencyInjection;

public interface IServiceScopeFactory
{
	IServiceScope CreateScope();
}
