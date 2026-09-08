using System.Reactive.Concurrency;
using System.Windows.Threading;

namespace System.Reactive.Linq;

public static class DispatcherObservable
{
	public static IObservable<TSource> ObserveOn<TSource>(this IObservable<TSource> source, Dispatcher dispatcher)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (dispatcher == null)
		{
			throw new ArgumentNullException("dispatcher");
		}
		return ObserveOn_(source, dispatcher);
	}

	public static IObservable<TSource> ObserveOn<TSource>(this IObservable<TSource> source, Dispatcher dispatcher, DispatcherPriority priority)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (dispatcher == null)
		{
			throw new ArgumentNullException("dispatcher");
		}
		return ObserveOn_(source, dispatcher, priority);
	}

	public static IObservable<TSource> ObserveOn<TSource>(this IObservable<TSource> source, DispatcherScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return ObserveOn_(source, scheduler.Dispatcher, scheduler.Priority);
	}

	public static IObservable<TSource> ObserveOn<TSource>(this IObservable<TSource> source, DispatcherObject dispatcherObject)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (dispatcherObject == null)
		{
			throw new ArgumentNullException("dispatcherObject");
		}
		return ObserveOn_(source, dispatcherObject.Dispatcher);
	}

	public static IObservable<TSource> ObserveOn<TSource>(this IObservable<TSource> source, DispatcherObject dispatcherObject, DispatcherPriority priority)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (dispatcherObject == null)
		{
			throw new ArgumentNullException("dispatcherObject");
		}
		return ObserveOn_(source, dispatcherObject.Dispatcher, priority);
	}

	public static IObservable<TSource> ObserveOnDispatcher<TSource>(this IObservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return ObserveOn_(source, DispatcherScheduler.Current.Dispatcher);
	}

	public static IObservable<TSource> ObserveOnDispatcher<TSource>(this IObservable<TSource> source, DispatcherPriority priority)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return ObserveOn_(source, DispatcherScheduler.Current.Dispatcher, priority);
	}

	private static IObservable<TSource> ObserveOn_<TSource>(IObservable<TSource> source, Dispatcher dispatcher, DispatcherPriority priority)
	{
		return Synchronization.ObserveOn(source, new DispatcherSynchronizationContext(dispatcher, priority));
	}

	private static IObservable<TSource> ObserveOn_<TSource>(IObservable<TSource> source, Dispatcher dispatcher)
	{
		return Synchronization.ObserveOn(source, new DispatcherSynchronizationContext(dispatcher));
	}

	public static IObservable<TSource> SubscribeOn<TSource>(this IObservable<TSource> source, Dispatcher dispatcher)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (dispatcher == null)
		{
			throw new ArgumentNullException("dispatcher");
		}
		return SubscribeOn_(source, dispatcher);
	}

	public static IObservable<TSource> SubscribeOn<TSource>(this IObservable<TSource> source, Dispatcher dispatcher, DispatcherPriority priority)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (dispatcher == null)
		{
			throw new ArgumentNullException("dispatcher");
		}
		return SubscribeOn_(source, dispatcher, priority);
	}

	public static IObservable<TSource> SubscribeOn<TSource>(this IObservable<TSource> source, DispatcherScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return SubscribeOn_(source, scheduler.Dispatcher, scheduler.Priority);
	}

	public static IObservable<TSource> SubscribeOn<TSource>(this IObservable<TSource> source, DispatcherObject dispatcherObject)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (dispatcherObject == null)
		{
			throw new ArgumentNullException("dispatcherObject");
		}
		return SubscribeOn_(source, dispatcherObject.Dispatcher);
	}

	public static IObservable<TSource> SubscribeOn<TSource>(this IObservable<TSource> source, DispatcherObject dispatcherObject, DispatcherPriority priority)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (dispatcherObject == null)
		{
			throw new ArgumentNullException("dispatcherObject");
		}
		return SubscribeOn_(source, dispatcherObject.Dispatcher, priority);
	}

	public static IObservable<TSource> SubscribeOnDispatcher<TSource>(this IObservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return SubscribeOn_(source, DispatcherScheduler.Current.Dispatcher);
	}

	public static IObservable<TSource> SubscribeOnDispatcher<TSource>(this IObservable<TSource> source, DispatcherPriority priority)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return SubscribeOn_(source, DispatcherScheduler.Current.Dispatcher, priority);
	}

	private static IObservable<TSource> SubscribeOn_<TSource>(IObservable<TSource> source, Dispatcher dispatcher, DispatcherPriority priority)
	{
		return Synchronization.SubscribeOn(source, new DispatcherSynchronizationContext(dispatcher, priority));
	}

	private static IObservable<TSource> SubscribeOn_<TSource>(IObservable<TSource> source, Dispatcher dispatcher)
	{
		return Synchronization.SubscribeOn(source, new DispatcherSynchronizationContext(dispatcher));
	}
}
