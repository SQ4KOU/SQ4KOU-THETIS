using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Concurrency;

namespace System.Reactive;

[Serializable]
public abstract class Notification<T> : IEquatable<Notification<T>>
{
	[Serializable]
	[DebuggerDisplay("OnNext({Value})")]
	internal sealed class OnNextNotification : Notification<T>
	{
		public override T Value { get; }

		public override Exception? Exception => null;

		public override bool HasValue => true;

		public override NotificationKind Kind => NotificationKind.OnNext;

		public OnNextNotification(T value)
		{
			Value = value;
		}

		public override int GetHashCode()
		{
			T value = Value;
			if (value == null)
			{
				return 0;
			}
			return value.GetHashCode();
		}

		public override bool Equals(Notification<T>? other)
		{
			if ((object)this == other)
			{
				return true;
			}
			if ((object)other == null)
			{
				return false;
			}
			if (other.Kind != NotificationKind.OnNext)
			{
				return false;
			}
			return EqualityComparer<T>.Default.Equals(Value, other.Value);
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "OnNext({0})", Value);
		}

		public override void Accept(IObserver<T> observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException("observer");
			}
			observer.OnNext(Value);
		}

		public override TResult Accept<TResult>(IObserver<T, TResult> observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException("observer");
			}
			return observer.OnNext(Value);
		}

		public override void Accept(Action<T> onNext, Action<Exception> onError, Action onCompleted)
		{
			if (onNext == null)
			{
				throw new ArgumentNullException("onNext");
			}
			if (onError == null)
			{
				throw new ArgumentNullException("onError");
			}
			if (onCompleted == null)
			{
				throw new ArgumentNullException("onCompleted");
			}
			onNext(Value);
		}

		public override TResult Accept<TResult>(Func<T, TResult> onNext, Func<Exception, TResult> onError, Func<TResult> onCompleted)
		{
			if (onNext == null)
			{
				throw new ArgumentNullException("onNext");
			}
			if (onError == null)
			{
				throw new ArgumentNullException("onError");
			}
			if (onCompleted == null)
			{
				throw new ArgumentNullException("onCompleted");
			}
			return onNext(Value);
		}
	}

	[Serializable]
	[DebuggerDisplay("OnError({Exception})")]
	internal sealed class OnErrorNotification : Notification<T>
	{
		public override T Value
		{
			get
			{
				Exception.Throw();
				return default(T);
			}
		}

		public override Exception Exception { get; }

		public override bool HasValue => false;

		public override NotificationKind Kind => NotificationKind.OnError;

		public OnErrorNotification(Exception exception)
		{
			Exception = exception;
		}

		public override int GetHashCode()
		{
			return Exception.GetHashCode();
		}

		public override bool Equals(Notification<T>? other)
		{
			if ((object)this == other)
			{
				return true;
			}
			if ((object)other == null)
			{
				return false;
			}
			if (other.Kind != NotificationKind.OnError)
			{
				return false;
			}
			return object.Equals(Exception, other.Exception);
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "OnError({0})", Exception.GetType().FullName);
		}

		public override void Accept(IObserver<T> observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException("observer");
			}
			observer.OnError(Exception);
		}

		public override TResult Accept<TResult>(IObserver<T, TResult> observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException("observer");
			}
			return observer.OnError(Exception);
		}

		public override void Accept(Action<T> onNext, Action<Exception> onError, Action onCompleted)
		{
			if (onNext == null)
			{
				throw new ArgumentNullException("onNext");
			}
			if (onError == null)
			{
				throw new ArgumentNullException("onError");
			}
			if (onCompleted == null)
			{
				throw new ArgumentNullException("onCompleted");
			}
			onError(Exception);
		}

		public override TResult Accept<TResult>(Func<T, TResult> onNext, Func<Exception, TResult> onError, Func<TResult> onCompleted)
		{
			if (onNext == null)
			{
				throw new ArgumentNullException("onNext");
			}
			if (onError == null)
			{
				throw new ArgumentNullException("onError");
			}
			if (onCompleted == null)
			{
				throw new ArgumentNullException("onCompleted");
			}
			return onError(Exception);
		}
	}

	[Serializable]
	[DebuggerDisplay("OnCompleted()")]
	internal sealed class OnCompletedNotification : Notification<T>
	{
		internal static readonly Notification<T> Instance = new Notification<T>.OnCompletedNotification();

		public override T Value
		{
			get
			{
				throw new InvalidOperationException(Strings_Core.COMPLETED_NO_VALUE);
			}
		}

		public override Exception? Exception => null;

		public override bool HasValue => false;

		public override NotificationKind Kind => NotificationKind.OnCompleted;

		private OnCompletedNotification()
		{
		}

		public override int GetHashCode()
		{
			return typeof(T).GetHashCode() ^ 0x213E;
		}

		public override bool Equals(Notification<T>? other)
		{
			if ((object)this == other)
			{
				return true;
			}
			if ((object)other == null)
			{
				return false;
			}
			return other.Kind == NotificationKind.OnCompleted;
		}

		public override string ToString()
		{
			return "OnCompleted()";
		}

		public override void Accept(IObserver<T> observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException("observer");
			}
			observer.OnCompleted();
		}

		public override TResult Accept<TResult>(IObserver<T, TResult> observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException("observer");
			}
			return observer.OnCompleted();
		}

		public override void Accept(Action<T> onNext, Action<Exception> onError, Action onCompleted)
		{
			if (onNext == null)
			{
				throw new ArgumentNullException("onNext");
			}
			if (onError == null)
			{
				throw new ArgumentNullException("onError");
			}
			if (onCompleted == null)
			{
				throw new ArgumentNullException("onCompleted");
			}
			onCompleted();
		}

		public override TResult Accept<TResult>(Func<T, TResult> onNext, Func<Exception, TResult> onError, Func<TResult> onCompleted)
		{
			if (onNext == null)
			{
				throw new ArgumentNullException("onNext");
			}
			if (onError == null)
			{
				throw new ArgumentNullException("onError");
			}
			if (onCompleted == null)
			{
				throw new ArgumentNullException("onCompleted");
			}
			return onCompleted();
		}
	}

	private sealed class NotificationToObservable : ObservableBase<T>
	{
		private readonly IScheduler _scheduler;

		private readonly Notification<T> _parent;

		public NotificationToObservable(IScheduler scheduler, Notification<T> parent)
		{
			_scheduler = scheduler;
			_parent = parent;
		}

		protected override IDisposable SubscribeCore(IObserver<T> observer)
		{
			return _scheduler.ScheduleAction((_parent, observer), delegate((Notification<T> _parent, IObserver<T> observer) state)
			{
				var (notification, observer2) = state;
				notification.Accept(observer2);
				if (notification.Kind == NotificationKind.OnNext)
				{
					observer2.OnCompleted();
				}
			});
		}
	}

	public abstract T Value { get; }

	public abstract bool HasValue { get; }

	public abstract Exception? Exception { get; }

	public abstract NotificationKind Kind { get; }

	protected internal Notification()
	{
	}

	public abstract bool Equals(Notification<T>? other);

	public static bool operator ==(Notification<T> left, Notification<T> right)
	{
		if ((object)left == right)
		{
			return true;
		}
		if ((object)left == null || (object)right == null)
		{
			return false;
		}
		return left.Equals(right);
	}

	public static bool operator !=(Notification<T> left, Notification<T> right)
	{
		return !(left == right);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as Notification<T>);
	}

	public abstract void Accept(IObserver<T> observer);

	public abstract TResult Accept<TResult>(IObserver<T, TResult> observer);

	public abstract void Accept(Action<T> onNext, Action<Exception> onError, Action onCompleted);

	public abstract TResult Accept<TResult>(Func<T, TResult> onNext, Func<Exception, TResult> onError, Func<TResult> onCompleted);

	public IObservable<T> ToObservable()
	{
		return ToObservable(ImmediateScheduler.Instance);
	}

	public IObservable<T> ToObservable(IScheduler scheduler)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return new NotificationToObservable(scheduler, this);
	}
}
public static class Notification
{
	public static Notification<T> CreateOnNext<T>(T value)
	{
		return new Notification<T>.OnNextNotification(value);
	}

	public static Notification<T> CreateOnError<T>(Exception error)
	{
		if (error == null)
		{
			throw new ArgumentNullException("error");
		}
		return new Notification<T>.OnErrorNotification(error);
	}

	public static Notification<T> CreateOnCompleted<T>()
	{
		return Notification<T>.OnCompletedNotification.Instance;
	}
}
