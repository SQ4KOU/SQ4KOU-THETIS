using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal static class SequenceEqual<TSource>
{
	internal sealed class Observable : Producer<bool, Observable._>
	{
		internal sealed class @_ : IdentitySink<bool>
		{
			private sealed class FirstObserver : IObserver<TSource>
			{
				private readonly @_ _parent;

				public FirstObserver(@_ parent)
				{
					_parent = parent;
				}

				public void OnNext(TSource value)
				{
					lock (_parent._gate)
					{
						if (_parent._qr.Count > 0)
						{
							bool flag = false;
							TSource y = _parent._qr.Dequeue();
							try
							{
								flag = _parent._comparer.Equals(value, y);
							}
							catch (Exception error)
							{
								_parent.ForwardOnError(error);
								return;
							}
							if (!flag)
							{
								_parent.ForwardOnNext(value: false);
								_parent.ForwardOnCompleted();
							}
						}
						else if (_parent._doner)
						{
							_parent.ForwardOnNext(value: false);
							_parent.ForwardOnCompleted();
						}
						else
						{
							_parent._ql.Enqueue(value);
						}
					}
				}

				public void OnError(Exception error)
				{
					lock (_parent._gate)
					{
						_parent.ForwardOnError(error);
					}
				}

				public void OnCompleted()
				{
					lock (_parent._gate)
					{
						_parent._donel = true;
						if (_parent._ql.Count == 0)
						{
							if (_parent._qr.Count > 0)
							{
								_parent.ForwardOnNext(value: false);
								_parent.ForwardOnCompleted();
							}
							else if (_parent._doner)
							{
								_parent.ForwardOnNext(value: true);
								_parent.ForwardOnCompleted();
							}
						}
					}
				}
			}

			private sealed class SecondObserver : IObserver<TSource>
			{
				private readonly @_ _parent;

				public SecondObserver(@_ parent)
				{
					_parent = parent;
				}

				public void OnNext(TSource value)
				{
					lock (_parent._gate)
					{
						if (_parent._ql.Count > 0)
						{
							bool flag = false;
							TSource x = _parent._ql.Dequeue();
							try
							{
								flag = _parent._comparer.Equals(x, value);
							}
							catch (Exception error)
							{
								_parent.ForwardOnError(error);
								return;
							}
							if (!flag)
							{
								_parent.ForwardOnNext(value: false);
								_parent.ForwardOnCompleted();
							}
						}
						else if (_parent._donel)
						{
							_parent.ForwardOnNext(value: false);
							_parent.ForwardOnCompleted();
						}
						else
						{
							_parent._qr.Enqueue(value);
						}
					}
				}

				public void OnError(Exception error)
				{
					lock (_parent._gate)
					{
						_parent.ForwardOnError(error);
					}
				}

				public void OnCompleted()
				{
					lock (_parent._gate)
					{
						_parent._doner = true;
						if (_parent._qr.Count == 0)
						{
							if (_parent._ql.Count > 0)
							{
								_parent.ForwardOnNext(value: false);
								_parent.ForwardOnCompleted();
							}
							else if (_parent._donel)
							{
								_parent.ForwardOnNext(value: true);
								_parent.ForwardOnCompleted();
							}
						}
					}
				}
			}

			private readonly IEqualityComparer<TSource> _comparer;

			private readonly object _gate;

			private readonly Queue<TSource> _ql;

			private readonly Queue<TSource> _qr;

			private bool _donel;

			private bool _doner;

			private SingleAssignmentDisposableValue _second;

			public _(IEqualityComparer<TSource> comparer, IObserver<bool> observer)
				: base(observer)
			{
				_comparer = comparer;
				_gate = new object();
				_ql = new Queue<TSource>();
				_qr = new Queue<TSource>();
			}

			public void Run(Observable parent)
			{
				SetUpstream(parent._first.SubscribeSafe(new FirstObserver(this)));
				_second.Disposable = parent._second.SubscribeSafe(new SecondObserver(this));
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_second.Dispose();
				}
				base.Dispose(disposing);
			}
		}

		private readonly IObservable<TSource> _first;

		private readonly IObservable<TSource> _second;

		private readonly IEqualityComparer<TSource> _comparer;

		public Observable(IObservable<TSource> first, IObservable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			_first = first;
			_second = second;
			_comparer = comparer;
		}

		protected override @_ CreateSink(IObserver<bool> observer)
		{
			return new @_(_comparer, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(this);
		}
	}

	internal sealed class Enumerable : Producer<bool, Enumerable._>
	{
		internal sealed class @_ : Sink<TSource, bool>
		{
			private readonly IEqualityComparer<TSource> _comparer;

			private IEnumerator<TSource>? _enumerator;

			private static readonly IEnumerator<TSource> DisposedEnumerator = MakeDisposedEnumerator();

			public _(IEqualityComparer<TSource> comparer, IObserver<bool> observer)
				: base(observer)
			{
				_comparer = comparer;
			}

			private static IEnumerator<TSource> MakeDisposedEnumerator()
			{
				yield break;
			}

			public void Run(Enumerable parent)
			{
				try
				{
					IEnumerator<TSource> enumerator = parent._second.GetEnumerator();
					if (Interlocked.CompareExchange(ref _enumerator, enumerator, null) != null)
					{
						enumerator.Dispose();
						return;
					}
				}
				catch (Exception error)
				{
					ForwardOnError(error);
					return;
				}
				SetUpstream(parent._first.SubscribeSafe(this));
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					Interlocked.Exchange(ref _enumerator, DisposedEnumerator)?.Dispose();
				}
				base.Dispose(disposing);
			}

			public override void OnNext(TSource value)
			{
				bool flag = false;
				try
				{
					if (_enumerator.MoveNext())
					{
						TSource current = _enumerator.Current;
						flag = _comparer.Equals(value, current);
					}
				}
				catch (Exception error)
				{
					ForwardOnError(error);
					return;
				}
				if (!flag)
				{
					ForwardOnNext(value: false);
					ForwardOnCompleted();
				}
			}

			public override void OnCompleted()
			{
				bool flag;
				try
				{
					flag = _enumerator.MoveNext();
				}
				catch (Exception error)
				{
					ForwardOnError(error);
					return;
				}
				ForwardOnNext(!flag);
				ForwardOnCompleted();
			}
		}

		private readonly IObservable<TSource> _first;

		private readonly IEnumerable<TSource> _second;

		private readonly IEqualityComparer<TSource> _comparer;

		public Enumerable(IObservable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			_first = first;
			_second = second;
			_comparer = comparer;
		}

		protected override @_ CreateSink(IObserver<bool> observer)
		{
			return new @_(_comparer, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(this);
		}
	}
}
