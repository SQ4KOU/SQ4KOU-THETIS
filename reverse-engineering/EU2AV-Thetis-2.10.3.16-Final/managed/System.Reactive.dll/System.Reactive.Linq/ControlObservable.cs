using System.Reactive.Concurrency;
using System.Windows.Forms;

namespace System.Reactive.Linq;

public static class ControlObservable
{
	public static IObservable<TSource> SubscribeOn<TSource>(this IObservable<TSource> source, Control control)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		return Synchronization.SubscribeOn(source, new ControlScheduler(control));
	}

	public static IObservable<TSource> ObserveOn<TSource>(this IObservable<TSource> source, Control control)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		return Synchronization.ObserveOn(source, new ControlScheduler(control));
	}
}
