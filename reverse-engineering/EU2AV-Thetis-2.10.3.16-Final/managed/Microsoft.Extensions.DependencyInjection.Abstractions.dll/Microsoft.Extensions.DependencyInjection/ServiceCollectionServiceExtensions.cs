using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionServiceExtensions
{
	public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		System.ExceptionPolyfills.ThrowIfNull(implementationType, "implementationType");
		return Add(services, serviceType, implementationType, ServiceLifetime.Transient);
	}

	public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return Add(services, serviceType, implementationFactory, ServiceLifetime.Transient);
	}

	public static IServiceCollection AddTransient<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		return services.AddTransient(typeof(TService), typeof(TImplementation));
	}

	public static IServiceCollection AddTransient(this IServiceCollection services, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		return services.AddTransient(serviceType, serviceType);
	}

	public static IServiceCollection AddTransient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection services) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		return services.AddTransient(typeof(TService));
	}

	public static IServiceCollection AddTransient<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return services.AddTransient(typeof(TService), implementationFactory);
	}

	public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return services.AddTransient(typeof(TService), implementationFactory);
	}

	public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		System.ExceptionPolyfills.ThrowIfNull(implementationType, "implementationType");
		return Add(services, serviceType, implementationType, ServiceLifetime.Scoped);
	}

	public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return Add(services, serviceType, implementationFactory, ServiceLifetime.Scoped);
	}

	public static IServiceCollection AddScoped<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		return services.AddScoped(typeof(TService), typeof(TImplementation));
	}

	public static IServiceCollection AddScoped(this IServiceCollection services, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		return services.AddScoped(serviceType, serviceType);
	}

	public static IServiceCollection AddScoped<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection services) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		return services.AddScoped(typeof(TService));
	}

	public static IServiceCollection AddScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return services.AddScoped(typeof(TService), implementationFactory);
	}

	public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return services.AddScoped(typeof(TService), implementationFactory);
	}

	public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		System.ExceptionPolyfills.ThrowIfNull(implementationType, "implementationType");
		return Add(services, serviceType, implementationType, ServiceLifetime.Singleton);
	}

	public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return Add(services, serviceType, implementationFactory, ServiceLifetime.Singleton);
	}

	public static IServiceCollection AddSingleton<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		return services.AddSingleton(typeof(TService), typeof(TImplementation));
	}

	public static IServiceCollection AddSingleton(this IServiceCollection services, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		return services.AddSingleton(serviceType, serviceType);
	}

	public static IServiceCollection AddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection services) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		return services.AddSingleton(typeof(TService));
	}

	public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return services.AddSingleton(typeof(TService), implementationFactory);
	}

	public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return services.AddSingleton(typeof(TService), implementationFactory);
	}

	public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, object implementationInstance)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		System.ExceptionPolyfills.ThrowIfNull(implementationInstance, "implementationInstance");
		ServiceDescriptor item = new ServiceDescriptor(serviceType, implementationInstance);
		services.Add(item);
		return services;
	}

	public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, TService implementationInstance) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(implementationInstance, "implementationInstance");
		return services.AddSingleton(typeof(TService), implementationInstance);
	}

	private static IServiceCollection Add(IServiceCollection collection, Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType, ServiceLifetime lifetime)
	{
		ServiceDescriptor item = new ServiceDescriptor(serviceType, implementationType, lifetime);
		collection.Add(item);
		return collection;
	}

	private static IServiceCollection Add(IServiceCollection collection, Type serviceType, Func<IServiceProvider, object> implementationFactory, ServiceLifetime lifetime)
	{
		ServiceDescriptor item = new ServiceDescriptor(serviceType, implementationFactory, lifetime);
		collection.Add(item);
		return collection;
	}

	public static IServiceCollection AddKeyedTransient(this IServiceCollection services, Type serviceType, object? serviceKey, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		System.ExceptionPolyfills.ThrowIfNull(implementationType, "implementationType");
		return AddKeyed(services, serviceType, serviceKey, implementationType, ServiceLifetime.Transient);
	}

	public static IServiceCollection AddKeyedTransient(this IServiceCollection services, Type serviceType, object? serviceKey, Func<IServiceProvider, object?, object> implementationFactory)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return AddKeyed(services, serviceType, serviceKey, implementationFactory, ServiceLifetime.Transient);
	}

	public static IServiceCollection AddKeyedTransient<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services, object? serviceKey) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		return services.AddKeyedTransient(typeof(TService), serviceKey, typeof(TImplementation));
	}

	public static IServiceCollection AddKeyedTransient(this IServiceCollection services, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType, object? serviceKey)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		return services.AddKeyedTransient(serviceType, serviceKey, serviceType);
	}

	public static IServiceCollection AddKeyedTransient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection services, object? serviceKey) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		return services.AddKeyedTransient(typeof(TService), serviceKey);
	}

	public static IServiceCollection AddKeyedTransient<TService>(this IServiceCollection services, object? serviceKey, Func<IServiceProvider, object?, TService> implementationFactory) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return services.AddKeyedTransient(typeof(TService), serviceKey, implementationFactory);
	}

	public static IServiceCollection AddKeyedTransient<TService, TImplementation>(this IServiceCollection services, object? serviceKey, Func<IServiceProvider, object?, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return services.AddKeyedTransient(typeof(TService), serviceKey, implementationFactory);
	}

	public static IServiceCollection AddKeyedScoped(this IServiceCollection services, Type serviceType, object? serviceKey, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		System.ExceptionPolyfills.ThrowIfNull(implementationType, "implementationType");
		return AddKeyed(services, serviceType, serviceKey, implementationType, ServiceLifetime.Scoped);
	}

	public static IServiceCollection AddKeyedScoped(this IServiceCollection services, Type serviceType, object? serviceKey, Func<IServiceProvider, object?, object> implementationFactory)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return AddKeyed(services, serviceType, serviceKey, implementationFactory, ServiceLifetime.Scoped);
	}

	public static IServiceCollection AddKeyedScoped<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services, object? serviceKey) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		return services.AddKeyedScoped(typeof(TService), serviceKey, typeof(TImplementation));
	}

	public static IServiceCollection AddKeyedScoped(this IServiceCollection services, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType, object? serviceKey)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		return services.AddKeyedScoped(serviceType, serviceKey, serviceType);
	}

	public static IServiceCollection AddKeyedScoped<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection services, object? serviceKey) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		return services.AddKeyedScoped(typeof(TService), serviceKey);
	}

	public static IServiceCollection AddKeyedScoped<TService>(this IServiceCollection services, object? serviceKey, Func<IServiceProvider, object?, TService> implementationFactory) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return services.AddKeyedScoped(typeof(TService), serviceKey, implementationFactory);
	}

	public static IServiceCollection AddKeyedScoped<TService, TImplementation>(this IServiceCollection services, object? serviceKey, Func<IServiceProvider, object?, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return services.AddKeyedScoped(typeof(TService), serviceKey, implementationFactory);
	}

	public static IServiceCollection AddKeyedSingleton(this IServiceCollection services, Type serviceType, object? serviceKey, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		System.ExceptionPolyfills.ThrowIfNull(implementationType, "implementationType");
		return AddKeyed(services, serviceType, serviceKey, implementationType, ServiceLifetime.Singleton);
	}

	public static IServiceCollection AddKeyedSingleton(this IServiceCollection services, Type serviceType, object? serviceKey, Func<IServiceProvider, object?, object> implementationFactory)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return AddKeyed(services, serviceType, serviceKey, implementationFactory, ServiceLifetime.Singleton);
	}

	public static IServiceCollection AddKeyedSingleton<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services, object? serviceKey) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		return services.AddKeyedSingleton(typeof(TService), serviceKey, typeof(TImplementation));
	}

	public static IServiceCollection AddKeyedSingleton(this IServiceCollection services, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType, object? serviceKey)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		return services.AddKeyedSingleton(serviceType, serviceKey, serviceType);
	}

	public static IServiceCollection AddKeyedSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection services, object? serviceKey) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		return services.AddKeyedSingleton(typeof(TService), serviceKey, typeof(TService));
	}

	public static IServiceCollection AddKeyedSingleton<TService>(this IServiceCollection services, object? serviceKey, Func<IServiceProvider, object?, TService> implementationFactory) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return services.AddKeyedSingleton(typeof(TService), serviceKey, implementationFactory);
	}

	public static IServiceCollection AddKeyedSingleton<TService, TImplementation>(this IServiceCollection services, object? serviceKey, Func<IServiceProvider, object?, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		return services.AddKeyedSingleton(typeof(TService), serviceKey, implementationFactory);
	}

	public static IServiceCollection AddKeyedSingleton(this IServiceCollection services, Type serviceType, object? serviceKey, object implementationInstance)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		System.ExceptionPolyfills.ThrowIfNull(implementationInstance, "implementationInstance");
		ServiceDescriptor item = new ServiceDescriptor(serviceType, serviceKey, implementationInstance);
		services.Add(item);
		return services;
	}

	public static IServiceCollection AddKeyedSingleton<TService>(this IServiceCollection services, object? serviceKey, TService implementationInstance) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(implementationInstance, "implementationInstance");
		return services.AddKeyedSingleton(typeof(TService), serviceKey, implementationInstance);
	}

	private static IServiceCollection AddKeyed(IServiceCollection collection, Type serviceType, object serviceKey, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType, ServiceLifetime lifetime)
	{
		ServiceDescriptor item = new ServiceDescriptor(serviceType, serviceKey, implementationType, lifetime);
		collection.Add(item);
		return collection;
	}

	private static IServiceCollection AddKeyed(IServiceCollection collection, Type serviceType, object serviceKey, Func<IServiceProvider, object, object> implementationFactory, ServiceLifetime lifetime)
	{
		ServiceDescriptor item = new ServiceDescriptor(serviceType, serviceKey, implementationFactory, lifetime);
		collection.Add(item);
		return collection;
	}
}
