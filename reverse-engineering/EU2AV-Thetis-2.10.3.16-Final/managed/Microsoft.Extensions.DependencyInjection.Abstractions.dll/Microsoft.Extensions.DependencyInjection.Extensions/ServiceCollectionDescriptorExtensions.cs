using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection.Extensions;

public static class ServiceCollectionDescriptorExtensions
{
	public static IServiceCollection Add(this IServiceCollection collection, ServiceDescriptor descriptor)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(descriptor, "descriptor");
		collection.Add(descriptor);
		return collection;
	}

	public static IServiceCollection Add(this IServiceCollection collection, IEnumerable<ServiceDescriptor> descriptors)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(descriptors, "descriptors");
		foreach (ServiceDescriptor descriptor in descriptors)
		{
			collection.Add(descriptor);
		}
		return collection;
	}

	public static void TryAdd(this IServiceCollection collection, ServiceDescriptor descriptor)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(descriptor, "descriptor");
		int count = collection.Count;
		for (int i = 0; i < count; i++)
		{
			if (collection[i].ServiceType == descriptor.ServiceType && object.Equals(collection[i].ServiceKey, descriptor.ServiceKey))
			{
				return;
			}
		}
		collection.Add(descriptor);
	}

	public static void TryAdd(this IServiceCollection collection, IEnumerable<ServiceDescriptor> descriptors)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(descriptors, "descriptors");
		foreach (ServiceDescriptor descriptor in descriptors)
		{
			collection.TryAdd(descriptor);
		}
	}

	public static void TryAddTransient(this IServiceCollection collection, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type service)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		ServiceDescriptor descriptor = ServiceDescriptor.Transient(service, service);
		collection.TryAdd(descriptor);
	}

	public static void TryAddTransient(this IServiceCollection collection, Type service, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		System.ExceptionPolyfills.ThrowIfNull(implementationType, "implementationType");
		ServiceDescriptor descriptor = ServiceDescriptor.Transient(service, implementationType);
		collection.TryAdd(descriptor);
	}

	public static void TryAddTransient(this IServiceCollection collection, Type service, Func<IServiceProvider, object> implementationFactory)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		ServiceDescriptor descriptor = ServiceDescriptor.Transient(service, implementationFactory);
		collection.TryAdd(descriptor);
	}

	public static void TryAddTransient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection collection) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		collection.TryAddTransient(typeof(TService), typeof(TService));
	}

	public static void TryAddTransient<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection collection) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		collection.TryAddTransient(typeof(TService), typeof(TImplementation));
	}

	public static void TryAddTransient<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
	{
		services.TryAdd(ServiceDescriptor.Transient(implementationFactory));
	}

	public static void TryAddScoped(this IServiceCollection collection, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type service)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		ServiceDescriptor descriptor = ServiceDescriptor.Scoped(service, service);
		collection.TryAdd(descriptor);
	}

	public static void TryAddScoped(this IServiceCollection collection, Type service, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		System.ExceptionPolyfills.ThrowIfNull(implementationType, "implementationType");
		ServiceDescriptor descriptor = ServiceDescriptor.Scoped(service, implementationType);
		collection.TryAdd(descriptor);
	}

	public static void TryAddScoped(this IServiceCollection collection, Type service, Func<IServiceProvider, object> implementationFactory)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		ServiceDescriptor descriptor = ServiceDescriptor.Scoped(service, implementationFactory);
		collection.TryAdd(descriptor);
	}

	public static void TryAddScoped<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection collection) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		collection.TryAddScoped(typeof(TService), typeof(TService));
	}

	public static void TryAddScoped<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection collection) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		collection.TryAddScoped(typeof(TService), typeof(TImplementation));
	}

	public static void TryAddScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
	{
		services.TryAdd(ServiceDescriptor.Scoped(implementationFactory));
	}

	public static void TryAddSingleton(this IServiceCollection collection, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type service)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		ServiceDescriptor descriptor = ServiceDescriptor.Singleton(service, service);
		collection.TryAdd(descriptor);
	}

	public static void TryAddSingleton(this IServiceCollection collection, Type service, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		System.ExceptionPolyfills.ThrowIfNull(implementationType, "implementationType");
		ServiceDescriptor descriptor = ServiceDescriptor.Singleton(service, implementationType);
		collection.TryAdd(descriptor);
	}

	public static void TryAddSingleton(this IServiceCollection collection, Type service, Func<IServiceProvider, object> implementationFactory)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		ServiceDescriptor descriptor = ServiceDescriptor.Singleton(service, implementationFactory);
		collection.TryAdd(descriptor);
	}

	public static void TryAddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection collection) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		collection.TryAddSingleton(typeof(TService), typeof(TService));
	}

	public static void TryAddSingleton<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection collection) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		collection.TryAddSingleton(typeof(TService), typeof(TImplementation));
	}

	public static void TryAddSingleton<TService>(this IServiceCollection collection, TService instance) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(instance, "instance");
		ServiceDescriptor descriptor = ServiceDescriptor.Singleton(typeof(TService), instance);
		collection.TryAdd(descriptor);
	}

	public static void TryAddSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
	{
		services.TryAdd(ServiceDescriptor.Singleton(implementationFactory));
	}

	public static void TryAddEnumerable(this IServiceCollection services, ServiceDescriptor descriptor)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(descriptor, "descriptor");
		Type implementationType = descriptor.GetImplementationType();
		if (implementationType == typeof(object) || implementationType == descriptor.ServiceType)
		{
			throw new ArgumentException(System.SR.Format(System.SR.TryAddIndistinguishableTypeToEnumerable, implementationType, descriptor.ServiceType), "descriptor");
		}
		int count = services.Count;
		for (int i = 0; i < count; i++)
		{
			ServiceDescriptor serviceDescriptor = services[i];
			if (serviceDescriptor.ServiceType == descriptor.ServiceType && serviceDescriptor.GetImplementationType() == implementationType && object.Equals(serviceDescriptor.ServiceKey, descriptor.ServiceKey))
			{
				return;
			}
		}
		services.Add(descriptor);
	}

	public static void TryAddEnumerable(this IServiceCollection services, IEnumerable<ServiceDescriptor> descriptors)
	{
		System.ExceptionPolyfills.ThrowIfNull(services, "services");
		System.ExceptionPolyfills.ThrowIfNull(descriptors, "descriptors");
		foreach (ServiceDescriptor descriptor in descriptors)
		{
			services.TryAddEnumerable(descriptor);
		}
	}

	public static IServiceCollection Replace(this IServiceCollection collection, ServiceDescriptor descriptor)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(descriptor, "descriptor");
		int count = collection.Count;
		for (int i = 0; i < count; i++)
		{
			if (collection[i].ServiceType == descriptor.ServiceType && object.Equals(collection[i].ServiceKey, descriptor.ServiceKey))
			{
				collection.RemoveAt(i);
				break;
			}
		}
		collection.Add(descriptor);
		return collection;
	}

	public static IServiceCollection RemoveAll<T>(this IServiceCollection collection)
	{
		return collection.RemoveAll(typeof(T));
	}

	public static IServiceCollection RemoveAll(this IServiceCollection collection, Type serviceType)
	{
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		for (int num = collection.Count - 1; num >= 0; num--)
		{
			ServiceDescriptor serviceDescriptor = collection[num];
			if (serviceDescriptor.ServiceType == serviceType && serviceDescriptor.ServiceKey == null)
			{
				collection.RemoveAt(num);
			}
		}
		return collection;
	}

	public static void TryAddKeyedTransient(this IServiceCollection collection, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type service, object? serviceKey)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		ServiceDescriptor descriptor = ServiceDescriptor.KeyedTransient(service, serviceKey, service);
		collection.TryAdd(descriptor);
	}

	public static void TryAddKeyedTransient(this IServiceCollection collection, Type service, object? serviceKey, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		System.ExceptionPolyfills.ThrowIfNull(implementationType, "implementationType");
		ServiceDescriptor descriptor = ServiceDescriptor.KeyedTransient(service, serviceKey, implementationType);
		collection.TryAdd(descriptor);
	}

	public static void TryAddKeyedTransient(this IServiceCollection collection, Type service, object? serviceKey, Func<IServiceProvider, object?, object> implementationFactory)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		ServiceDescriptor descriptor = ServiceDescriptor.KeyedTransient(service, serviceKey, implementationFactory);
		collection.TryAdd(descriptor);
	}

	public static void TryAddKeyedTransient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection collection, object? serviceKey) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		collection.TryAddKeyedTransient(typeof(TService), serviceKey, typeof(TService));
	}

	public static void TryAddKeyedTransient<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection collection, object? serviceKey) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		collection.TryAddKeyedTransient(typeof(TService), serviceKey, typeof(TImplementation));
	}

	public static void TryAddKeyedTransient<TService>(this IServiceCollection services, object? serviceKey, Func<IServiceProvider, object?, TService> implementationFactory) where TService : class
	{
		services.TryAdd(ServiceDescriptor.KeyedTransient(serviceKey, implementationFactory));
	}

	public static void TryAddKeyedScoped(this IServiceCollection collection, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type service, object? serviceKey)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		ServiceDescriptor descriptor = ServiceDescriptor.KeyedScoped(service, serviceKey, service);
		collection.TryAdd(descriptor);
	}

	public static void TryAddKeyedScoped(this IServiceCollection collection, Type service, object? serviceKey, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		System.ExceptionPolyfills.ThrowIfNull(implementationType, "implementationType");
		ServiceDescriptor descriptor = ServiceDescriptor.KeyedScoped(service, serviceKey, implementationType);
		collection.TryAdd(descriptor);
	}

	public static void TryAddKeyedScoped(this IServiceCollection collection, Type service, object? serviceKey, Func<IServiceProvider, object?, object> implementationFactory)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		ServiceDescriptor descriptor = ServiceDescriptor.KeyedScoped(service, serviceKey, implementationFactory);
		collection.TryAdd(descriptor);
	}

	public static void TryAddKeyedScoped<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection collection, object? serviceKey) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		collection.TryAddKeyedScoped(typeof(TService), serviceKey, typeof(TService));
	}

	public static void TryAddKeyedScoped<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection collection, object? serviceKey) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		collection.TryAddKeyedScoped(typeof(TService), serviceKey, typeof(TImplementation));
	}

	public static void TryAddKeyedScoped<TService>(this IServiceCollection services, object? serviceKey, Func<IServiceProvider, object?, TService> implementationFactory) where TService : class
	{
		services.TryAdd(ServiceDescriptor.KeyedScoped(serviceKey, implementationFactory));
	}

	public static void TryAddKeyedSingleton(this IServiceCollection collection, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type service, object? serviceKey)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		ServiceDescriptor descriptor = ServiceDescriptor.KeyedSingleton(service, serviceKey, service);
		collection.TryAdd(descriptor);
	}

	public static void TryAddKeyedSingleton(this IServiceCollection collection, Type service, object? serviceKey, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		System.ExceptionPolyfills.ThrowIfNull(implementationType, "implementationType");
		ServiceDescriptor descriptor = ServiceDescriptor.KeyedSingleton(service, serviceKey, implementationType);
		collection.TryAdd(descriptor);
	}

	public static void TryAddKeyedSingleton(this IServiceCollection collection, Type service, object? serviceKey, Func<IServiceProvider, object?, object> implementationFactory)
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(service, "service");
		System.ExceptionPolyfills.ThrowIfNull(implementationFactory, "implementationFactory");
		ServiceDescriptor descriptor = ServiceDescriptor.KeyedSingleton(service, serviceKey, implementationFactory);
		collection.TryAdd(descriptor);
	}

	public static void TryAddKeyedSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection collection, object? serviceKey) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		collection.TryAddKeyedSingleton(typeof(TService), serviceKey, typeof(TService));
	}

	public static void TryAddKeyedSingleton<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection collection, object? serviceKey) where TService : class where TImplementation : class, TService
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		collection.TryAddKeyedSingleton(typeof(TService), serviceKey, typeof(TImplementation));
	}

	public static void TryAddKeyedSingleton<TService>(this IServiceCollection collection, object? serviceKey, TService instance) where TService : class
	{
		System.ExceptionPolyfills.ThrowIfNull(collection, "collection");
		System.ExceptionPolyfills.ThrowIfNull(instance, "instance");
		ServiceDescriptor descriptor = ServiceDescriptor.KeyedSingleton(typeof(TService), serviceKey, instance);
		collection.TryAdd(descriptor);
	}

	public static void TryAddKeyedSingleton<TService>(this IServiceCollection services, object? serviceKey, Func<IServiceProvider, object?, TService> implementationFactory) where TService : class
	{
		services.TryAdd(ServiceDescriptor.KeyedSingleton(serviceKey, implementationFactory));
	}

	public static IServiceCollection RemoveAllKeyed<T>(this IServiceCollection collection, object? serviceKey)
	{
		return collection.RemoveAllKeyed(typeof(T), serviceKey);
	}

	public static IServiceCollection RemoveAllKeyed(this IServiceCollection collection, Type serviceType, object? serviceKey)
	{
		System.ExceptionPolyfills.ThrowIfNull(serviceType, "serviceType");
		for (int num = collection.Count - 1; num >= 0; num--)
		{
			ServiceDescriptor serviceDescriptor = collection[num];
			if (serviceDescriptor.ServiceType == serviceType && object.Equals(serviceDescriptor.ServiceKey, serviceKey))
			{
				collection.RemoveAt(num);
			}
		}
		return collection;
	}
}
