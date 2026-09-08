using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Next<TSource> : PushToPullAdapter<TSource, TSource>
{
	private sealed class @_ : PushToPullSink<TSource, TSource>
	{
		private readonly object _gate;

		private readonly SemaphoreSlim _semaphore;

		private bool _waiting;

		private NotificationKind _kind;

		private TSource? _value;

		private Exception? _error;

		public _()
		{
			_gate = new object();
			_semaphore = new SemaphoreSlim(0, 1);
		}

		public override void OnNext(TSource value)
		{
			lock (_gate)
			{
				if (_waiting)
				{
					_value = value;
					_kind = NotificationKind.OnNext;
					_semaphore.Release();
				}
				_waiting = false;
			}
		}

		public override void OnError(Exception error)
		{
			Dispose();
			lock (_gate)
			{
				_error = error;
				_kind = NotificationKind.OnError;
				if (_waiting)
				{
					_semaphore.Release();
				}
				_waiting = false;
			}
		}

		public override void OnCompleted()
		{
			Dispose();
			lock (_gate)
			{
				_kind = NotificationKind.OnCompleted;
				if (_waiting)
				{
					_semaphore.Release();
				}
				_waiting = false;
			}
		}

		public override bool TryMoveNext([MaybeNullWhen(false)] out TSource current)
		{
			bool flag = false;
			lock (_gate)
			{
				_waiting = true;
				flag = _kind != NotificationKind.OnNext;
			}
			if (!flag)
			{
				_semaphore.Wait();
			}
			switch (_kind)
			{
			case NotificationKind.OnNext:
				current = _value;
				return true;
			case NotificationKind.OnError:
				_error.Throw();
				break;
			}
			current = default(TSource);
			return false;
		}
	}

	public Next(IObservable<TSource> source)
		: base(source)
	{
	}

	protected override PushToPullSink<TSource, TSource> Run()
	{
		return new @_();
	}
}
