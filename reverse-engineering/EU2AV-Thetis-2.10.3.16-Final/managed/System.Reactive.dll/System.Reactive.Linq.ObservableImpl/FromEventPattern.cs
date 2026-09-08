using System.Reactive.Concurrency;
using System.Reflection;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class FromEventPattern
{
	public sealed class Impl<TDelegate, TEventArgs> : ClassicEventProducer<TDelegate, EventPattern<TEventArgs>>
	{
		private readonly Func<EventHandler<TEventArgs>, TDelegate>? _conversion;

		public Impl(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
			: base(addHandler, removeHandler, scheduler)
		{
		}

		public Impl(Func<EventHandler<TEventArgs>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
			: base(addHandler, removeHandler, scheduler)
		{
			_conversion = conversion;
		}

		protected override TDelegate GetHandler(Action<EventPattern<TEventArgs>> onNext)
		{
			if (_conversion == null)
			{
				return ReflectionUtils.CreateDelegate<TDelegate>((Action<object, TEventArgs>)delegate(object sender, TEventArgs eventArgs)
				{
					onNext(new EventPattern<TEventArgs>(sender, eventArgs));
				}, typeof(Action<object, TEventArgs>).GetMethod("Invoke"));
			}
			return _conversion(delegate(object sender, TEventArgs eventArgs)
			{
				onNext(new EventPattern<TEventArgs>(sender, eventArgs));
			});
		}
	}

	public sealed class Impl<TDelegate, TSender, TEventArgs> : ClassicEventProducer<TDelegate, EventPattern<TSender, TEventArgs>>
	{
		public Impl(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
			: base(addHandler, removeHandler, scheduler)
		{
		}

		protected override TDelegate GetHandler(Action<EventPattern<TSender, TEventArgs>> onNext)
		{
			return ReflectionUtils.CreateDelegate<TDelegate>((Action<TSender, TEventArgs>)delegate(TSender sender, TEventArgs eventArgs)
			{
				onNext(new EventPattern<TSender, TEventArgs>(sender, eventArgs));
			}, typeof(Action<TSender, TEventArgs>).GetMethod("Invoke"));
		}
	}

	public sealed class Handler<TSender, TEventArgs, TResult> : EventProducer<Delegate, TResult>
	{
		private sealed class RemoveHandlerDisposable : IDisposable
		{
			private Action? _removeHandler;

			public RemoveHandlerDisposable(Action removeHandler)
			{
				Volatile.Write(ref _removeHandler, removeHandler);
			}

			public void Dispose()
			{
				try
				{
					Interlocked.Exchange(ref _removeHandler, null)?.Invoke();
				}
				catch (TargetInvocationException ex) when (ex.InnerException != null)
				{
					throw ex.InnerException;
				}
			}
		}

		private readonly object? _target;

		private readonly Type _delegateType;

		private readonly MethodInfo _addMethod;

		private readonly MethodInfo _removeMethod;

		private readonly Func<TSender, TEventArgs, TResult> _getResult;

		private readonly bool _isWinRT;

		public Handler(object? target, Type delegateType, MethodInfo addMethod, MethodInfo removeMethod, Func<TSender, TEventArgs, TResult> getResult, bool isWinRT, IScheduler scheduler)
			: base(scheduler)
		{
			_isWinRT = isWinRT;
			_target = target;
			_delegateType = delegateType;
			_addMethod = addMethod;
			_removeMethod = removeMethod;
			_getResult = getResult;
		}

		protected override Delegate GetHandler(Action<TResult> onNext)
		{
			Action<TSender, TEventArgs> o = delegate(TSender sender, TEventArgs eventArgs)
			{
				onNext(_getResult(sender, eventArgs));
			};
			return ReflectionUtils.CreateDelegate(_delegateType, o, typeof(Action<TSender, TEventArgs>).GetMethod("Invoke"));
		}

		protected override IDisposable AddHandler(Delegate handler)
		{
			Action removeHandler;
			try
			{
				removeHandler = ((!_isWinRT) ? AddHandlerCore(handler) : AddHandlerCoreWinRT(handler));
			}
			catch (TargetInvocationException ex) when (ex.InnerException != null)
			{
				throw ex.InnerException;
			}
			return new RemoveHandlerDisposable(removeHandler);
		}

		private Action AddHandlerCore(Delegate handler)
		{
			_addMethod.Invoke(_target, new object[1] { handler });
			return delegate
			{
				_removeMethod.Invoke(_target, new object[1] { handler });
			};
		}

		private Action AddHandlerCoreWinRT(Delegate handler)
		{
			object token = _addMethod.Invoke(_target, new object[1] { handler });
			return delegate
			{
				_removeMethod.Invoke(_target, new object[1] { token });
			};
		}
	}
}
