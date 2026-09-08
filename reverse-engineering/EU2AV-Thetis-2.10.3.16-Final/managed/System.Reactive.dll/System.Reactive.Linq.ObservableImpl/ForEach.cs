using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class ForEach<TSource>
{
	public abstract class ObserverBase : ManualResetEventSlim, IObserver<TSource>
	{
		private Exception? _exception;

		private int _stopped;

		public Exception? Error => _exception;

		protected abstract void OnNextCore(TSource value);

		public void OnNext(TSource value)
		{
			if (Volatile.Read(ref _stopped) == 0)
			{
				try
				{
					OnNextCore(value);
				}
				catch (Exception error)
				{
					OnError(error);
				}
			}
		}

		public void OnError(Exception error)
		{
			if (Interlocked.Exchange(ref _stopped, 1) == 0)
			{
				_exception = error;
				Set();
			}
		}

		public void OnCompleted()
		{
			if (Interlocked.Exchange(ref _stopped, 1) == 0)
			{
				Set();
			}
		}
	}

	public sealed class Observer : ObserverBase
	{
		private readonly Action<TSource> _onNext;

		public Observer(Action<TSource> onNext)
		{
			_onNext = onNext;
		}

		protected override void OnNextCore(TSource value)
		{
			_onNext(value);
		}
	}

	public sealed class ObserverIndexed : ObserverBase
	{
		private readonly Action<TSource, int> _onNext;

		private int _index;

		public ObserverIndexed(Action<TSource, int> onNext)
		{
			_onNext = onNext;
		}

		protected override void OnNextCore(TSource value)
		{
			_onNext(value, checked(_index++));
		}
	}
}
