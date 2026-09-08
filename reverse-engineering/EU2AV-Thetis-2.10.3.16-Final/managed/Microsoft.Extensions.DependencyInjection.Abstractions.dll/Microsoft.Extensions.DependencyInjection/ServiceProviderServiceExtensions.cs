using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceProviderServiceExtensions
{
	public static T? GetService<T>(this IServiceProvider provider)
	{
		System.ExceptionPolyfills.ThrowIfNull(provider, "provider");
		return (T)provider.GetService(typeof(T));
	}

	public static object GetRequiredService(this IServiceProvider provider, Type serviceType)
	{
		System.ExceptionPolyfills.ThrowIfNull(provider, "provider");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		if (provider is ISupportRequiredService supportRequiredService)
		{
			return supportRequiredService.GetRequiredService(serviceType);
		}
		return provider.GetService(serviceType) ?? throw new InvalidOperationException(System.SR.Format(System.SR.NoServiceRegistered, serviceType));
	}

	public static T GetRequiredService<T>(this IServiceProvider provider) where T : notnull
	{
		System.ExceptionPolyfills.ThrowIfNull(provider, "provider");
		return (T)provider.GetRequiredService(typeof(T));
	}

	public static IEnumerable<T> GetServices<T>(this IServiceProvider provider)
	{
		System.ExceptionPolyfills.ThrowIfNull(provider, "provider");
		return provider.GetRequiredService<IEnumerable<T>>();
	}

	[RequiresDynamicCode("The native code for an IEnumerable<serviceType> might not be available at runtime.")]
	public static IEnumerable<object?> GetServices(this IServiceProvider provider, Type serviceType)
	{
		System.ExceptionPolyfills.ThrowIfNull(provider, "provider");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		Type serviceType2 = typeof(IEnumerable<>).MakeGenericType(serviceType);
		return (IEnumerable<object>)provider.GetRequiredService(serviceType2);
	}

	public static IServiceScope CreateScope(this IServiceProvider provider)
	{
		return provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
	}

	public static AsyncServiceScope CreateAsyncScope(this IServiceProvider provider)
	{
		return new AsyncServiceScope(provider.CreateScope());
	}

	public static AsyncServiceScope CreateAsyncScope(this IServiceScopeFactory serviceScopeFactory)
	{
		return new AsyncServiceScope(serviceScopeFactory.CreateScope());
	}
}
