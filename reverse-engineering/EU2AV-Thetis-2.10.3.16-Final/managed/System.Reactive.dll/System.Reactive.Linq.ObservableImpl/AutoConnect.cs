using System.Reactive.Subjects;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class AutoConnect<T> : IObservable<T>
{
	private readonly IConnectableObservable<T> _source;

	private readonly int _minObservers;

	private readonly Action<IDisposable>? _onConnect;

	private int _count;

	internal AutoConnect(IConnectableObservable<T> source, int minObservers, Action<IDisposable>? onConnect)
	{
		_source = source;
		_minObservers = minObservers;
		_onConnect = onConnect;
	}

	public IDisposable Subscribe(IObserver<T> observer)
	{
		IDisposable result = _source.Subscribe(observer);
		if (Volatile.Read(ref _count) < _minObservers && Interlocked.Increment(ref _count) == _minObservers)
		{
			IDisposable obj = _source.Connect();
			Action<IDisposable>? onConnect = _onConnect;
			if (onConnect == null)
			{
				return result;
			}
			onConnect(obj);
		}
		return result;
	}
}
