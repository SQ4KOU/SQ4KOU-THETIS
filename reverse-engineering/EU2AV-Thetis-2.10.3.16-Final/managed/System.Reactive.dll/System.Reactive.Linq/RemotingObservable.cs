using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Security;
using System.Threading;

namespace System.Reactive.Linq;

public static class RemotingObservable
{
	[Serializable]
	private class SerializableObservable<T> : IObservable<T>
	{
		private readonly RemotableObservable<T> _remotableObservable;

		public SerializableObservable(RemotableObservable<T> remotableObservable)
		{
			_remotableObservable = remotableObservable;
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			ISafeObserver<T> safeObserver = SafeObserver<T>.Wrap(observer);
			IDisposable disposable = _remotableObservable.Subscribe(new RemotableObserver<T>(safeObserver));
			safeObserver.SetResource(disposable);
			return disposable;
		}
	}

	private class RemotableObserver<T> : MarshalByRefObject, IObserver<T>, ISponsor
	{
		private readonly IObserver<T> _underlyingObserver;

		public RemotableObserver(IObserver<T> underlyingObserver)
		{
			_underlyingObserver = underlyingObserver;
		}

		public void OnNext(T value)
		{
			_underlyingObserver.OnNext(value);
		}

		public void OnError(Exception exception)
		{
			try
			{
				_underlyingObserver.OnError(exception);
			}
			finally
			{
				Unregister();
			}
		}

		public void OnCompleted()
		{
			try
			{
				_underlyingObserver.OnCompleted();
			}
			finally
			{
				Unregister();
			}
		}

		[SecuritySafeCritical]
		private void Unregister()
		{
			((ILease)RemotingServices.GetLifetimeService(this))?.Unregister(this);
		}

		[SecurityCritical]
		public override object InitializeLifetimeService()
		{
			ILease obj = (ILease)base.InitializeLifetimeService();
			obj.Register(this);
			return obj;
		}

		[SecurityCritical]
		TimeSpan ISponsor.Renewal(ILease lease)
		{
			return lease.InitialLeaseTime;
		}
	}

	[Serializable]
	private sealed class RemotableObservable<T> : MarshalByRefObject, IObservable<T>
	{
		private sealed class RemotableSubscription : MarshalByRefObject, IDisposable, ISponsor
		{
			private IDisposable _underlyingSubscription;

			public RemotableSubscription(IDisposable underlyingSubscription)
			{
				_underlyingSubscription = underlyingSubscription;
			}

			public void Dispose()
			{
				using (Interlocked.Exchange(ref _underlyingSubscription, Disposable.Empty))
				{
					Unregister();
				}
			}

			[SecuritySafeCritical]
			private void Unregister()
			{
				((ILease)RemotingServices.GetLifetimeService(this))?.Unregister(this);
			}

			[SecurityCritical]
			public override object InitializeLifetimeService()
			{
				ILease obj = (ILease)base.InitializeLifetimeService();
				obj.Register(this);
				return obj;
			}

			[SecurityCritical]
			TimeSpan ISponsor.Renewal(ILease lease)
			{
				return lease.InitialLeaseTime;
			}
		}

		private readonly IObservable<T> _underlyingObservable;

		private readonly ILease? _lease;

		public RemotableObservable(IObservable<T> underlyingObservable, ILease? lease)
		{
			_underlyingObservable = underlyingObservable;
			_lease = lease;
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			return new RemotableSubscription(_underlyingObservable.Subscribe(observer));
		}

		[SecurityCritical]
		public override object? InitializeLifetimeService()
		{
			return _lease;
		}
	}

	public static IObservable<TSource> Remotable<TSource>(this IObservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return Remotable_(source);
	}

	public static IObservable<TSource> Remotable<TSource>(this IObservable<TSource> source, ILease lease)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return Remotable_(source, lease);
	}

	public static IQbservable<TSource> Remotable<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource> Remotable<TSource>(this IQbservable<TSource> source, ILease lease)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(lease, typeof(ILease))));
	}

	private static IObservable<TSource> Remotable_<TSource>(IObservable<TSource> source)
	{
		return new SerializableObservable<TSource>(new RemotableObservable<TSource>(source, null));
	}

	private static IObservable<TSource> Remotable_<TSource>(IObservable<TSource> source, ILease? lease)
	{
		return new SerializableObservable<TSource>(new RemotableObservable<TSource>(source, lease));
	}
}
