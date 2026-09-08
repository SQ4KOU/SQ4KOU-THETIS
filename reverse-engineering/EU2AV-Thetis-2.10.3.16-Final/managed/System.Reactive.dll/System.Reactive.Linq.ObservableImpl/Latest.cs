using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Latest<TSource> : PushToPullAdapter<TSource, TSource>
{
	private sealed class @_ : PushToPullSink<TSource, TSource>
	{
		private readonly object _gate;

		private readonly SemaphoreSlim _semaphore;

		private bool _notificationAvailable;

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
			bool flag = false;
			lock (_gate)
			{
				flag = !_notificationAvailable;
				_notificationAvailable = true;
				_kind = NotificationKind.OnNext;
				_value = value;
			}
			if (flag)
			{
				_semaphore.Release();
			}
		}

		public override void OnError(Exception error)
		{
			Dispose();
			bool flag = false;
			lock (_gate)
			{
				flag = !_notificationAvailable;
				_notificationAvailable = true;
				_kind = NotificationKind.OnError;
				_error = error;
			}
			if (flag)
			{
				_semaphore.Release();
			}
		}

		public override void OnCompleted()
		{
			Dispose();
			bool flag = false;
			lock (_gate)
			{
				flag = !_notificationAvailable;
				_notificationAvailable = true;
				_kind = NotificationKind.OnCompleted;
			}
			if (flag)
			{
				_semaphore.Release();
			}
		}

		public override bool TryMoveNext([MaybeNullWhen(false)] out TSource current)
		{
			TSource val = default(TSource);
			Exception exception = null;
			_semaphore.Wait();
			NotificationKind kind;
			lock (_gate)
			{
				kind = _kind;
				switch (kind)
				{
				case NotificationKind.OnNext:
					val = _value;
					break;
				case NotificationKind.OnError:
					exception = _error;
					break;
				}
				_notificationAvailable = false;
			}
			switch (kind)
			{
			case NotificationKind.OnNext:
				current = val;
				return true;
			case NotificationKind.OnError:
				exception.Throw();
				break;
			}
			current = default(TSource);
			return false;
		}
	}

	public Latest(IObservable<TSource> source)
		: base(source)
	{
	}

	protected override PushToPullSink<TSource, TSource> Run()
	{
		return new @_();
	}
}
