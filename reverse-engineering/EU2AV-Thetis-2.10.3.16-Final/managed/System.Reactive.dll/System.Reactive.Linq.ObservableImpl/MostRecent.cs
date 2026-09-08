using System.Diagnostics.CodeAnalysis;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class MostRecent<TSource> : PushToPullAdapter<TSource, TSource>
{
	private sealed class @_ : PushToPullSink<TSource, TSource>
	{
		private volatile NotificationKind _kind;

		private TSource _value;

		private Exception? _error;

		public _(TSource initialValue)
		{
			_kind = NotificationKind.OnNext;
			_value = initialValue;
		}

		public override void OnNext(TSource value)
		{
			_value = value;
			_kind = NotificationKind.OnNext;
		}

		public override void OnError(Exception error)
		{
			Dispose();
			_error = error;
			_kind = NotificationKind.OnError;
		}

		public override void OnCompleted()
		{
			Dispose();
			_kind = NotificationKind.OnCompleted;
		}

		public override bool TryMoveNext([MaybeNullWhen(false)] out TSource current)
		{
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

	private readonly TSource _initialValue;

	public MostRecent(IObservable<TSource> source, TSource initialValue)
		: base(source)
	{
		_initialValue = initialValue;
	}

	protected override PushToPullSink<TSource, TSource> Run()
	{
		return new @_(_initialValue);
	}
}
