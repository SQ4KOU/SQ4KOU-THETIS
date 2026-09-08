using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Threading;

namespace System.Reactive.PlatformServices;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class SystemClock
{
	private static readonly Lazy<ISystemClock> ServiceSystemClock = new Lazy<ISystemClock>(InitializeSystemClock);

	private static readonly Lazy<INotifySystemClockChanged> ServiceSystemClockChanged = new Lazy<INotifySystemClockChanged>(InitializeSystemClockChanged);

	internal static readonly HashSet<WeakReference<LocalScheduler>> SystemClockChanged = new HashSet<WeakReference<LocalScheduler>>();

	private static IDisposable? _systemClockChangedHandlerCollector;

	private static int _refCount;

	public static DateTimeOffset UtcNow => ServiceSystemClock.Value.UtcNow;

	public static void AddRef()
	{
		if (Interlocked.Increment(ref _refCount) == 1)
		{
			ServiceSystemClockChanged.Value.SystemClockChanged += OnSystemClockChanged;
		}
	}

	public static void Release()
	{
		if (Interlocked.Decrement(ref _refCount) == 0)
		{
			ServiceSystemClockChanged.Value.SystemClockChanged -= OnSystemClockChanged;
		}
	}

	internal static void OnSystemClockChanged(object? sender, SystemClockChangedEventArgs e)
	{
		lock (SystemClockChanged)
		{
			foreach (WeakReference<LocalScheduler> item in new List<WeakReference<LocalScheduler>>(SystemClockChanged))
			{
				if (item.TryGetTarget(out var target))
				{
					target.SystemClockChanged(sender, e);
				}
			}
		}
	}

	private static ISystemClock InitializeSystemClock()
	{
		return PlatformEnlightenmentProvider.Current.GetService<ISystemClock>(Array.Empty<object>()) ?? new DefaultSystemClock();
	}

	private static INotifySystemClockChanged InitializeSystemClockChanged()
	{
		return PlatformEnlightenmentProvider.Current.GetService<INotifySystemClockChanged>(Array.Empty<object>()) ?? new DefaultSystemClockMonitor();
	}

	internal static void Register(LocalScheduler scheduler)
	{
		lock (SystemClockChanged)
		{
			SystemClockChanged.Add(new WeakReference<LocalScheduler>(scheduler));
			if (SystemClockChanged.Count == 1)
			{
				_systemClockChangedHandlerCollector = ConcurrencyAbstractionLayer.Current.StartPeriodicTimer(CollectHandlers, TimeSpan.FromSeconds(30.0));
			}
			else if (SystemClockChanged.Count % 64 == 0)
			{
				CollectHandlers();
			}
		}
	}

	private static void CollectHandlers()
	{
		lock (SystemClockChanged)
		{
			HashSet<WeakReference<LocalScheduler>> hashSet = null;
			foreach (WeakReference<LocalScheduler> item in SystemClockChanged)
			{
				if (!item.TryGetTarget(out var _))
				{
					if (hashSet == null)
					{
						hashSet = new HashSet<WeakReference<LocalScheduler>>();
					}
					hashSet.Add(item);
				}
			}
			if (hashSet != null)
			{
				foreach (WeakReference<LocalScheduler> item2 in hashSet)
				{
					SystemClockChanged.Remove(item2);
				}
			}
			if (SystemClockChanged.Count == 0)
			{
				_systemClockChangedHandlerCollector?.Dispose();
				_systemClockChangedHandlerCollector = null;
			}
		}
	}
}
