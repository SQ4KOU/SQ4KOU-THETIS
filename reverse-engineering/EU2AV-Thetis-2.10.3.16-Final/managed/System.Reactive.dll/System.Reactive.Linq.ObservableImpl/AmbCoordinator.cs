using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class AmbCoordinator<T> : IDisposable
{
	internal sealed class InnerObserver : IdentitySink<T>
	{
		private readonly AmbCoordinator<T> _parent;

		private readonly int _index;

		private bool _won;

		public InnerObserver(AmbCoordinator<T> parent, int index)
			: base(parent._downstream)
		{
			_parent = parent;
			_index = index;
		}

		public override void OnCompleted()
		{
			if (_won)
			{
				ForwardOnCompleted();
			}
			else if (_parent.TryWin(_index))
			{
				_won = true;
				ForwardOnCompleted();
			}
			else
			{
				Dispose();
			}
		}

		public override void OnError(Exception error)
		{
			if (_won)
			{
				ForwardOnError(error);
			}
			else if (_parent.TryWin(_index))
			{
				_won = true;
				ForwardOnError(error);
			}
			else
			{
				Dispose();
			}
		}

		public override void OnNext(T value)
		{
			if (_won)
			{
				ForwardOnNext(value);
			}
			else if (_parent.TryWin(_index))
			{
				_won = true;
				ForwardOnNext(value);
			}
			else
			{
				Dispose();
			}
		}
	}

	private readonly IObserver<T> _downstream;

	private readonly InnerObserver?[] _observers;

	private int _winner;

	internal AmbCoordinator(IObserver<T> downstream, int n)
	{
		_downstream = downstream;
		InnerObserver[] array = new InnerObserver[n];
		for (int i = 0; i < n; i++)
		{
			array[i] = new InnerObserver(this, i);
		}
		_observers = array;
		Volatile.Write(ref _winner, -1);
	}

	internal static IDisposable Create(IObserver<T> observer, IObservable<T>[] sources)
	{
		int num = sources.Length;
		switch (num)
		{
		case 0:
			observer.OnCompleted();
			return Disposable.Empty;
		case 1:
			return sources[0].Subscribe(observer);
		default:
		{
			AmbCoordinator<T> ambCoordinator = new AmbCoordinator<T>(observer, num);
			ambCoordinator.Subscribe(sources);
			return ambCoordinator;
		}
		}
	}

	internal void Subscribe(IObservable<T>[] sources)
	{
		for (int i = 0; i < _observers.Length; i++)
		{
			InnerObserver innerObserver = Volatile.Read(ref _observers[i]);
			if (innerObserver != null)
			{
				innerObserver.Run(sources[i]);
				continue;
			}
			break;
		}
	}

	public void Dispose()
	{
		for (int i = 0; i < _observers.Length; i++)
		{
			Interlocked.Exchange(ref _observers[i], null)?.Dispose();
		}
	}

	private bool TryWin(int index)
	{
		if (Volatile.Read(ref _winner) == -1 && Interlocked.CompareExchange(ref _winner, index, -1) == -1)
		{
			for (int i = 0; i < _observers.Length; i++)
			{
				if (index != i)
				{
					Interlocked.Exchange(ref _observers[i], null)?.Dispose();
				}
			}
			return true;
		}
		return false;
	}
}
