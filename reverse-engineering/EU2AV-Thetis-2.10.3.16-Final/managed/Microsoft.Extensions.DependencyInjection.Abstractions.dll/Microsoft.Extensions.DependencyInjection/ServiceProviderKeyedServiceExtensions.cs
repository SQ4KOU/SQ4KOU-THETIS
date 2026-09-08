using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceProviderKeyedServiceExtensions
{
	public static T? GetKeyedService<T>(this IServiceProvider provider, object? serviceKey)
	{
		System.ExceptionPolyfills.ThrowIfNull(provider, "provider");
		if (provider is IKeyedServiceProvider keyedServiceProvider)
		{
			return (T)keyedServiceProvider.GetKeyedService(typeof(T), serviceKey);
		}
		throw new InvalidOperationException(System.SR.KeyedServicesNotSupported);
	}

	public static object? GetKeyedService(this IServiceProvider provider, Type serviceType, object? serviceKey)
	{
		System.ExceptionPolyfills.ThrowIfNull(provider, "provider");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		if (provider is IKeyedServiceProvider keyedServiceProvider)
		{
			return keyedServiceProvider.GetKeyedService(serviceType, serviceKey);
		}
		throw new InvalidOperationException(System.SR.KeyedServicesNotSupported);
	}

	public static object GetRequiredKeyedService(this IServiceProvider provider, Type serviceType, object? serviceKey)
	{
		System.ExceptionPolyfills.ThrowIfNull(provider, "provider");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		if (provider is IKeyedServiceProvider keyedServiceProvider)
		{
			return keyedServiceProvider.GetRequiredKeyedService(serviceType, serviceKey);
		}
		throw new InvalidOperationException(System.SR.KeyedServicesNotSupported);
	}

	public static T GetRequiredKeyedService<T>(this IServiceProvider provider, object? serviceKey) where T : notnull
	{
		System.ExceptionPolyfills.ThrowIfNull(provider, "provider");
		return (T)provider.GetRequiredKeyedService(typeof(T), serviceKey);
	}

	public static IEnumerable<T> GetKeyedServices<T>(this IServiceProvider provider, object? serviceKey)
	{
		System.ExceptionPolyfills.ThrowIfNull(provider, "provider");
		return provider.GetRequiredKeyedService<IEnumerable<T>>(serviceKey);
	}

	[RequiresDynamicCode("The native code for an IEnumerable<serviceType> might not be available at runtime.")]
	public static IEnumerable<object?> GetKeyedServices(this IServiceProvider provider, Type serviceType, object? serviceKey)
	{
		System.ExceptionPolyfills.ThrowIfNull(provider, "provider");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		Type serviceType2 = typeof(IEnumerable<>).MakeGenericType(serviceType);
		return (IEnumerable<object>)provider.GetRequiredKeyedService(serviceType2, serviceKey);
	}
}
