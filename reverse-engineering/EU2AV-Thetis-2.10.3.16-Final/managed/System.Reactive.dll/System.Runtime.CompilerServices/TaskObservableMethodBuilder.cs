using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Security;

namespace System.Runtime.CompilerServices;

public struct TaskObservableMethodBuilder<T>
{
	internal sealed class TaskObservable : ITaskObservable<T>, IObservable<T>, ITaskObservableAwaiter<T>, INotifyCompletion
	{
		private readonly AsyncSubject<T>? _subject;

		private readonly T? _result;

		private readonly Exception? _exception;

		public bool IsCompleted => _subject?.IsCompleted ?? true;

		public TaskObservable()
		{
			_subject = new AsyncSubject<T>();
		}

		public TaskObservable(T result)
		{
			_result = result;
		}

		public TaskObservable(Exception exception)
		{
			_exception = exception;
		}

		public void SetResult(T result)
		{
			if (IsCompleted)
			{
				throw new InvalidOperationException();
			}
			_subject.OnNext(result);
			_subject.OnCompleted();
		}

		public void SetException(Exception exception)
		{
			if (IsCompleted)
			{
				throw new InvalidOperationException();
			}
			_subject.OnError(exception);
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			if (_subject != null)
			{
				return _subject.Subscribe(observer);
			}
			if (_exception != null)
			{
				observer.OnError(_exception);
				return Disposable.Empty;
			}
			observer.OnNext(_result);
			return Disposable.Empty;
		}

		public ITaskObservableAwaiter<T> GetAwaiter()
		{
			return this;
		}

		public T GetResult()
		{
			if (_subject != null)
			{
				return _subject.GetResult();
			}
			_exception?.Throw();
			return _result;
		}

		public void OnCompleted(Action continuation)
		{
			if (_subject != null)
			{
				_subject.OnCompleted(continuation);
			}
			else
			{
				continuation();
			}
		}
	}

	private IAsyncStateMachine _stateMachine;

	private TaskObservable _inner;

	public ITaskObservable<T> Task => _inner ?? (_inner = new TaskObservable());

	public static TaskObservableMethodBuilder<T> Create()
	{
		return default(TaskObservableMethodBuilder<T>);
	}

	public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
	{
		if (stateMachine == null)
		{
			throw new ArgumentNullException("stateMachine");
		}
		stateMachine.MoveNext();
	}

	public void SetStateMachine(IAsyncStateMachine stateMachine)
	{
		if (_stateMachine != null)
		{
			throw new InvalidOperationException();
		}
		_stateMachine = stateMachine ?? throw new ArgumentNullException("stateMachine");
	}

	public void SetResult(T result)
	{
		if (_inner == null)
		{
			_inner = new TaskObservable(result);
		}
		else
		{
			_inner.SetResult(result);
		}
	}

	public void SetException(Exception exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		if (_inner == null)
		{
			_inner = new TaskObservable(exception);
		}
		else
		{
			_inner.SetException(exception);
		}
	}

	public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
	{
		try
		{
			if (_stateMachine == null)
			{
				_ = Task;
				_stateMachine = stateMachine;
				_stateMachine.SetStateMachine(_stateMachine);
			}
			awaiter.OnCompleted(_stateMachine.MoveNext);
		}
		catch (Exception exception)
		{
			Rethrow(exception);
		}
	}

	[SecuritySafeCritical]
	public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
	{
		try
		{
			if (_stateMachine == null)
			{
				_ = Task;
				_stateMachine = stateMachine;
				_stateMachine.SetStateMachine(_stateMachine);
			}
			awaiter.UnsafeOnCompleted(_stateMachine.MoveNext);
		}
		catch (Exception exception)
		{
			Rethrow(exception);
		}
	}

	private static void Rethrow(Exception exception)
	{
		Scheduler.Default.Schedule(exception, delegate(Exception ex, Action<Exception> recurse)
		{
			ex.Throw();
		});
	}
}
