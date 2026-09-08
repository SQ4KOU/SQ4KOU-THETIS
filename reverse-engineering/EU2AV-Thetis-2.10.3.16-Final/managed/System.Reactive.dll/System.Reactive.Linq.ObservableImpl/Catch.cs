using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Catch<TSource> : Producer<TSource, Catch<TSource>._>
{
	internal sealed class @_ : TailRecursiveSink<TSource>
	{
		private Exception? _lastException;

		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}

		protected override IEnumerable<IObservable<TSource>>? Extract(IObservable<TSource> source)
		{
			if (source is Catch<TSource> obj)
			{
				return obj._sources;
			}
			return null;
		}

		public override void OnError(Exception error)
		{
			_lastException = error;
			Recurse();
		}

		protected override void Done()
		{
			if (_lastException != null)
			{
				ForwardOnError(_lastException);
			}
			else
			{
				ForwardOnCompleted();
			}
		}

		protected override bool Fail(Exception error)
		{
			OnError(error);
			return true;
		}
	}

	private readonly IEnumerable<IObservable<TSource>> _sources;

	public Catch(IEnumerable<IObservable<TSource>> sources)
	{
		_sources = sources;
	}

	protected override @_ CreateSink(IObserver<TSource> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_sources);
	}
}
internal sealed class Catch<TSource, TException> : Producer<TSource, Catch<TSource, TException>._> where TException : Exception
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private readonly Func<TException, IObservable<TSource>> _handler;

		private bool _once;

		private SerialDisposableValue _subscription;

		public _(Func<TException, IObservable<TSource>> handler, IObserver<TSource> observer)
			: base(observer)
		{
			_handler = handler;
		}

		public override void Run(IObservable<TSource> source)
		{
			_subscription.TrySetFirst(source.SubscribeSafe(this));
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_subscription.Dispose();
			}
			base.Dispose(disposing);
		}

		public override void OnError(Exception error)
		{
			if (!Volatile.Read(ref _once) && error is TException arg)
			{
				IObservable<TSource> source;
				try
				{
					source = _handler(arg);
				}
				catch (Exception error2)
				{
					ForwardOnError(error2);
					return;
				}
				Volatile.Write(ref _once, value: true);
				_subscription.Disposable = source.SubscribeSafe(this);
			}
			else
			{
				ForwardOnError(error);
			}
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly Func<TException, IObservable<TSource>> _handler;

	public Catch(IObservable<TSource> source, Func<TException, IObservable<TSource>> handler)
	{
		_source = source;
		_handler = handler;
	}

	protected override @_ CreateSink(IObserver<TSource> observer)
	{
		return new @_(_handler, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
