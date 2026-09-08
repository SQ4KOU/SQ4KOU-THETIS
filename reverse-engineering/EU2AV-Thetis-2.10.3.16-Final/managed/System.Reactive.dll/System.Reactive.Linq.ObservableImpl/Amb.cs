using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Amb<TSource> : Producer<TSource, Amb<TSource>.AmbCoordinator>
{
	internal sealed class AmbCoordinator : IDisposable
	{
		private sealed class AmbObserver : IdentitySink<TSource>
		{
			private readonly AmbCoordinator _parent;

			private readonly bool _isLeft;

			private bool _iwon;

			public AmbObserver(IObserver<TSource> downstream, AmbCoordinator parent, bool isLeft)
				: base(downstream)
			{
				_parent = parent;
				_isLeft = isLeft;
			}

			public override void OnCompleted()
			{
				if (_iwon)
				{
					ForwardOnCompleted();
				}
				else if (_parent.TryWin(_isLeft))
				{
					_iwon = true;
					ForwardOnCompleted();
				}
				else
				{
					Dispose();
				}
			}

			public override void OnError(Exception error)
			{
				if (_iwon)
				{
					ForwardOnError(error);
				}
				else if (_parent.TryWin(_isLeft))
				{
					_iwon = true;
					ForwardOnError(error);
				}
				else
				{
					Dispose();
				}
			}

			public override void OnNext(TSource value)
			{
				if (_iwon)
				{
					ForwardOnNext(value);
				}
				else if (_parent.TryWin(_isLeft))
				{
					_iwon = true;
					ForwardOnNext(value);
				}
			}
		}

		private readonly AmbObserver _leftObserver;

		private readonly AmbObserver _rightObserver;

		private int _winner;

		public AmbCoordinator(IObserver<TSource> observer)
		{
			_leftObserver = new AmbObserver(observer, this, isLeft: true);
			_rightObserver = new AmbObserver(observer, this, isLeft: false);
		}

		public void Run(IObservable<TSource> left, IObservable<TSource> right)
		{
			_leftObserver.Run(left);
			_rightObserver.Run(right);
		}

		public void Dispose()
		{
			_leftObserver.Dispose();
			_rightObserver.Dispose();
		}

		public bool TryWin(bool isLeft)
		{
			int num = (isLeft ? 1 : 2);
			if (Volatile.Read(ref _winner) == num)
			{
				return true;
			}
			if (Interlocked.CompareExchange(ref _winner, num, 0) == 0)
			{
				(isLeft ? _rightObserver : _leftObserver).Dispose();
				return true;
			}
			return false;
		}
	}

	private readonly IObservable<TSource> _left;

	private readonly IObservable<TSource> _right;

	public Amb(IObservable<TSource> left, IObservable<TSource> right)
	{
		_left = left;
		_right = right;
	}

	protected override AmbCoordinator CreateSink(IObserver<TSource> observer)
	{
		return new AmbCoordinator(observer);
	}

	protected override void Run(AmbCoordinator sink)
	{
		sink.Run(_left, _right);
	}
}
