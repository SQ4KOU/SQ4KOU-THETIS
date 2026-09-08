using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive.Linq.ObservableImpl;

internal static class SelectMany<TSource, TCollection, TResult>
{
	internal sealed class ObservableSelector : Producer<TResult, ObservableSelector._>
	{
		internal sealed class @_ : Sink<TSource, TResult>
		{
			private sealed class InnerObserver : SafeObserver<TCollection>
			{
				private readonly @_ _parent;

				private readonly TSource _value;

				public InnerObserver(@_ parent, TSource value)
				{
					_parent = parent;
					_value = value;
				}

				public override void OnNext(TCollection value)
				{
					TResult value2;
					try
					{
						value2 = _parent._resultSelector(_value, value);
					}
					catch (Exception error)
					{
						lock (_parent._gate)
						{
							_parent.ForwardOnError(error);
							return;
						}
					}
					lock (_parent._gate)
					{
						_parent.ForwardOnNext(value2);
					}
				}

				public override void OnError(Exception error)
				{
					lock (_parent._gate)
					{
						_parent.ForwardOnError(error);
					}
				}

				public override void OnCompleted()
				{
					_parent._group.Remove(this);
					if (_parent._isStopped && _parent._group.Count == 0)
					{
						lock (_parent._gate)
						{
							_parent.ForwardOnCompleted();
						}
					}
				}
			}

			private readonly object _gate = new object();

			private readonly CompositeDisposable _group = new CompositeDisposable();

			private readonly Func<TSource, IObservable<TCollection>> _collectionSelector;

			private readonly Func<TSource, TCollection, TResult> _resultSelector;

			private volatile bool _isStopped;

			public _(ObservableSelector parent, IObserver<TResult> observer)
				: base(observer)
			{
				_collectionSelector = parent._collectionSelector;
				_resultSelector = parent._resultSelector;
			}

			public override void OnNext(TSource value)
			{
				IObservable<TCollection> source;
				try
				{
					source = _collectionSelector(value);
				}
				catch (Exception error)
				{
					lock (_gate)
					{
						ForwardOnError(error);
						return;
					}
				}
				InnerObserver innerObserver = new InnerObserver(this, value);
				_group.Add(innerObserver);
				innerObserver.SetResource(source.SubscribeSafe(innerObserver));
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				_isStopped = true;
				if (_group.Count == 0)
				{
					lock (_gate)
					{
						ForwardOnCompleted();
						return;
					}
				}
				DisposeUpstream();
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				if (disposing)
				{
					_group.Dispose();
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Func<TSource, IObservable<TCollection>> _collectionSelector;

		private readonly Func<TSource, TCollection, TResult> _resultSelector;

		public ObservableSelector(IObservable<TSource> source, Func<TSource, IObservable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
		{
			_source = source;
			_collectionSelector = collectionSelector;
			_resultSelector = resultSelector;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class ObservableSelectorIndexed : Producer<TResult, ObservableSelectorIndexed._>
	{
		internal sealed class @_ : Sink<TSource, TResult>
		{
			private sealed class InnerObserver : SafeObserver<TCollection>
			{
				private readonly @_ _parent;

				private readonly TSource _value;

				private readonly int _valueIndex;

				private int _index;

				public InnerObserver(@_ parent, TSource value, int index)
				{
					_parent = parent;
					_value = value;
					_valueIndex = index;
				}

				public override void OnNext(TCollection value)
				{
					TResult value2;
					try
					{
						value2 = _parent._resultSelector(_value, _valueIndex, value, checked(_index++));
					}
					catch (Exception error)
					{
						lock (_parent._gate)
						{
							_parent.ForwardOnError(error);
							return;
						}
					}
					lock (_parent._gate)
					{
						_parent.ForwardOnNext(value2);
					}
				}

				public override void OnError(Exception error)
				{
					lock (_parent._gate)
					{
						_parent.ForwardOnError(error);
					}
				}

				public override void OnCompleted()
				{
					_parent._group.Remove(this);
					if (_parent._isStopped && _parent._group.Count == 0)
					{
						lock (_parent._gate)
						{
							_parent.ForwardOnCompleted();
						}
					}
				}
			}

			private readonly object _gate = new object();

			private readonly CompositeDisposable _group = new CompositeDisposable();

			private readonly Func<TSource, int, IObservable<TCollection>> _collectionSelector;

			private readonly Func<TSource, int, TCollection, int, TResult> _resultSelector;

			private volatile bool _isStopped;

			private int _index;

			public _(ObservableSelectorIndexed parent, IObserver<TResult> observer)
				: base(observer)
			{
				_collectionSelector = parent._collectionSelector;
				_resultSelector = parent._resultSelector;
			}

			public override void OnNext(TSource value)
			{
				int num;
				IObservable<TCollection> source;
				try
				{
					num = checked(_index++);
					source = _collectionSelector(value, num);
				}
				catch (Exception error)
				{
					lock (_gate)
					{
						ForwardOnError(error);
						return;
					}
				}
				InnerObserver innerObserver = new InnerObserver(this, value, num);
				_group.Add(innerObserver);
				innerObserver.SetResource(source.SubscribeSafe(innerObserver));
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				_isStopped = true;
				if (_group.Count == 0)
				{
					lock (_gate)
					{
						ForwardOnCompleted();
						return;
					}
				}
				DisposeUpstream();
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				if (disposing)
				{
					_group.Dispose();
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Func<TSource, int, IObservable<TCollection>> _collectionSelector;

		private readonly Func<TSource, int, TCollection, int, TResult> _resultSelector;

		public ObservableSelectorIndexed(IObservable<TSource> source, Func<TSource, int, IObservable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
		{
			_source = source;
			_collectionSelector = collectionSelector;
			_resultSelector = resultSelector;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class EnumerableSelector : Producer<TResult, EnumerableSelector._>
	{
		internal sealed class @_ : Sink<TSource, TResult>
		{
			private readonly Func<TSource, IEnumerable<TCollection>> _collectionSelector;

			private readonly Func<TSource, TCollection, TResult> _resultSelector;

			public _(EnumerableSelector parent, IObserver<TResult> observer)
				: base(observer)
			{
				_collectionSelector = parent._collectionSelector;
				_resultSelector = parent._resultSelector;
			}

			public override void OnNext(TSource value)
			{
				IEnumerable<TCollection> enumerable;
				try
				{
					enumerable = _collectionSelector(value);
				}
				catch (Exception error)
				{
					ForwardOnError(error);
					return;
				}
				IEnumerator<TCollection> enumerator;
				try
				{
					enumerator = enumerable.GetEnumerator();
				}
				catch (Exception error2)
				{
					ForwardOnError(error2);
					return;
				}
				using (enumerator)
				{
					bool flag = true;
					while (flag)
					{
						TResult value2 = default(TResult);
						try
						{
							flag = enumerator.MoveNext();
							if (flag)
							{
								value2 = _resultSelector(value, enumerator.Current);
							}
						}
						catch (Exception error3)
						{
							ForwardOnError(error3);
							break;
						}
						if (flag)
						{
							ForwardOnNext(value2);
						}
					}
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Func<TSource, IEnumerable<TCollection>> _collectionSelector;

		private readonly Func<TSource, TCollection, TResult> _resultSelector;

		public EnumerableSelector(IObservable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
		{
			_source = source;
			_collectionSelector = collectionSelector;
			_resultSelector = resultSelector;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class EnumerableSelectorIndexed : Producer<TResult, EnumerableSelectorIndexed._>
	{
		internal sealed class @_ : Sink<TSource, TResult>
		{
			private readonly Func<TSource, int, IEnumerable<TCollection>> _collectionSelector;

			private readonly Func<TSource, int, TCollection, int, TResult> _resultSelector;

			private int _index;

			public _(EnumerableSelectorIndexed parent, IObserver<TResult> observer)
				: base(observer)
			{
				_collectionSelector = parent._collectionSelector;
				_resultSelector = parent._resultSelector;
			}

			public override void OnNext(TSource value)
			{
				checked
				{
					int arg;
					IEnumerable<TCollection> enumerable;
					try
					{
						arg = _index++;
						enumerable = _collectionSelector(value, arg);
					}
					catch (Exception error)
					{
						ForwardOnError(error);
						return;
					}
					IEnumerator<TCollection> enumerator;
					try
					{
						enumerator = enumerable.GetEnumerator();
					}
					catch (Exception error2)
					{
						ForwardOnError(error2);
						return;
					}
					using (enumerator)
					{
						int num = 0;
						bool flag = true;
						while (flag)
						{
							TResult value2 = default(TResult);
							try
							{
								flag = enumerator.MoveNext();
								if (flag)
								{
									value2 = _resultSelector(value, arg, enumerator.Current, num++);
								}
							}
							catch (Exception error3)
							{
								ForwardOnError(error3);
								break;
							}
							if (flag)
							{
								ForwardOnNext(value2);
							}
						}
					}
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Func<TSource, int, IEnumerable<TCollection>> _collectionSelector;

		private readonly Func<TSource, int, TCollection, int, TResult> _resultSelector;

		public EnumerableSelectorIndexed(IObservable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
		{
			_source = source;
			_collectionSelector = collectionSelector;
			_resultSelector = resultSelector;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class TaskSelector : Producer<TResult, TaskSelector._>
	{
		internal sealed class @_ : Sink<TSource, TResult>
		{
			private readonly object _gate = new object();

			private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

			private readonly Func<TSource, CancellationToken, Task<TCollection>> _collectionSelector;

			private readonly Func<TSource, TCollection, TResult> _resultSelector;

			private volatile int _count;

			public _(TaskSelector parent, IObserver<TResult> observer)
				: base(observer)
			{
				_collectionSelector = parent._collectionSelector;
				_resultSelector = parent._resultSelector;
			}

			public override void Run(IObservable<TSource> source)
			{
				_count = 1;
				base.Run(source);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_cancel.Cancel();
				}
				base.Dispose(disposing);
			}

			public override void OnNext(TSource value)
			{
				Task<TCollection> task;
				try
				{
					Interlocked.Increment(ref _count);
					task = _collectionSelector(value, _cancel.Token);
				}
				catch (Exception error)
				{
					lock (_gate)
					{
						ForwardOnError(error);
						return;
					}
				}
				if (task.IsCompleted)
				{
					OnCompletedTask(value, task);
					return;
				}
				task.ContinueWithState(delegate(Task<TCollection> t, (@_ @this, TSource value) tuple)
				{
					tuple.@this.OnCompletedTask(tuple.value, t);
				}, (this, value), _cancel.Token);
			}

			private void OnCompletedTask(TSource value, Task<TCollection> task)
			{
				switch (task.Status)
				{
				case TaskStatus.RanToCompletion:
				{
					TResult value2;
					try
					{
						value2 = _resultSelector(value, task.Result);
					}
					catch (Exception error)
					{
						lock (_gate)
						{
							ForwardOnError(error);
							break;
						}
					}
					lock (_gate)
					{
						ForwardOnNext(value2);
					}
					OnCompleted();
					break;
				}
				case TaskStatus.Faulted:
					lock (_gate)
					{
						ForwardOnError(task.GetSingleException());
						break;
					}
				case TaskStatus.Canceled:
					if (!_cancel.IsCancellationRequested)
					{
						lock (_gate)
						{
							ForwardOnError(new TaskCanceledException(task));
							break;
						}
					}
					break;
				}
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				if (Interlocked.Decrement(ref _count) == 0)
				{
					lock (_gate)
					{
						ForwardOnCompleted();
					}
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Func<TSource, CancellationToken, Task<TCollection>> _collectionSelector;

		private readonly Func<TSource, TCollection, TResult> _resultSelector;

		public TaskSelector(IObservable<TSource> source, Func<TSource, CancellationToken, Task<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
		{
			_source = source;
			_collectionSelector = collectionSelector;
			_resultSelector = resultSelector;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class TaskSelectorIndexed : Producer<TResult, TaskSelectorIndexed._>
	{
		internal sealed class @_ : Sink<TSource, TResult>
		{
			private readonly object _gate = new object();

			private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

			private readonly Func<TSource, int, CancellationToken, Task<TCollection>> _collectionSelector;

			private readonly Func<TSource, int, TCollection, TResult> _resultSelector;

			private volatile int _count;

			private int _index;

			public _(TaskSelectorIndexed parent, IObserver<TResult> observer)
				: base(observer)
			{
				_collectionSelector = parent._collectionSelector;
				_resultSelector = parent._resultSelector;
			}

			public override void Run(IObservable<TSource> source)
			{
				_count = 1;
				base.Run(source);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_cancel.Cancel();
				}
				base.Dispose(disposing);
			}

			public override void OnNext(TSource value)
			{
				int num;
				Task<TCollection> task;
				try
				{
					num = checked(_index++);
					Interlocked.Increment(ref _count);
					task = _collectionSelector(value, num, _cancel.Token);
				}
				catch (Exception error)
				{
					lock (_gate)
					{
						ForwardOnError(error);
						return;
					}
				}
				if (task.IsCompleted)
				{
					OnCompletedTask(value, num, task);
					return;
				}
				task.ContinueWithState(delegate(Task<TCollection> t, (@_ @this, TSource value, int index) tuple)
				{
					tuple.@this.OnCompletedTask(tuple.value, tuple.index, t);
				}, (this, value, num), _cancel.Token);
			}

			private void OnCompletedTask(TSource value, int index, Task<TCollection> task)
			{
				switch (task.Status)
				{
				case TaskStatus.RanToCompletion:
				{
					TResult value2;
					try
					{
						value2 = _resultSelector(value, index, task.Result);
					}
					catch (Exception error)
					{
						lock (_gate)
						{
							ForwardOnError(error);
							break;
						}
					}
					lock (_gate)
					{
						ForwardOnNext(value2);
					}
					OnCompleted();
					break;
				}
				case TaskStatus.Faulted:
					lock (_gate)
					{
						ForwardOnError(task.GetSingleException());
						break;
					}
				case TaskStatus.Canceled:
					if (!_cancel.IsCancellationRequested)
					{
						lock (_gate)
						{
							ForwardOnError(new TaskCanceledException(task));
							break;
						}
					}
					break;
				}
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				if (Interlocked.Decrement(ref _count) == 0)
				{
					lock (_gate)
					{
						ForwardOnCompleted();
					}
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Func<TSource, int, CancellationToken, Task<TCollection>> _collectionSelector;

		private readonly Func<TSource, int, TCollection, TResult> _resultSelector;

		public TaskSelectorIndexed(IObservable<TSource> source, Func<TSource, int, CancellationToken, Task<TCollection>> collectionSelector, Func<TSource, int, TCollection, TResult> resultSelector)
		{
			_source = source;
			_collectionSelector = collectionSelector;
			_resultSelector = resultSelector;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}
}
internal static class SelectMany<TSource, TResult>
{
	internal class ObservableSelector : Producer<TResult, ObservableSelector._>
	{
		internal class @_ : Sink<TSource, TResult>
		{
			private sealed class InnerObserver : SafeObserver<TResult>
			{
				private readonly @_ _parent;

				public InnerObserver(@_ parent)
				{
					_parent = parent;
				}

				public override void OnNext(TResult value)
				{
					lock (_parent._gate)
					{
						_parent.ForwardOnNext(value);
					}
				}

				public override void OnError(Exception error)
				{
					lock (_parent._gate)
					{
						_parent.ForwardOnError(error);
					}
				}

				public override void OnCompleted()
				{
					_parent._group.Remove(this);
					if (_parent._isStopped && _parent._group.Count == 0)
					{
						lock (_parent._gate)
						{
							_parent.ForwardOnCompleted();
						}
					}
				}
			}

			protected readonly object _gate = new object();

			private readonly Func<TSource, IObservable<TResult>> _selector;

			private readonly CompositeDisposable _group = new CompositeDisposable();

			private volatile bool _isStopped;

			public _(ObservableSelector parent, IObserver<TResult> observer)
				: base(observer)
			{
				_selector = parent._selector;
			}

			public override void OnNext(TSource value)
			{
				IObservable<TResult> inner;
				try
				{
					inner = _selector(value);
				}
				catch (Exception error)
				{
					lock (_gate)
					{
						ForwardOnError(error);
						return;
					}
				}
				SubscribeInner(inner);
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				Final();
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				if (disposing)
				{
					_group.Dispose();
				}
			}

			protected void Final()
			{
				_isStopped = true;
				if (_group.Count == 0)
				{
					lock (_gate)
					{
						ForwardOnCompleted();
						return;
					}
				}
				DisposeUpstream();
			}

			protected void SubscribeInner(IObservable<TResult> inner)
			{
				InnerObserver innerObserver = new InnerObserver(this);
				_group.Add(innerObserver);
				innerObserver.SetResource(inner.SubscribeSafe(innerObserver));
			}
		}

		protected readonly IObservable<TSource> _source;

		protected readonly Func<TSource, IObservable<TResult>> _selector;

		public ObservableSelector(IObservable<TSource> source, Func<TSource, IObservable<TResult>> selector)
		{
			_source = source;
			_selector = selector;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class ObservableSelectors : ObservableSelector
	{
		internal new sealed class @_ : ObservableSelector._
		{
			private readonly Func<Exception, IObservable<TResult>> _selectorOnError;

			private readonly Func<IObservable<TResult>> _selectorOnCompleted;

			public _(ObservableSelectors parent, IObserver<TResult> observer)
				: base((ObservableSelector)parent, observer)
			{
				_selectorOnError = parent._selectorOnError;
				_selectorOnCompleted = parent._selectorOnCompleted;
			}

			public override void OnError(Exception error)
			{
				if (_selectorOnError != null)
				{
					IObservable<TResult> inner;
					try
					{
						inner = _selectorOnError(error);
					}
					catch (Exception error2)
					{
						lock (_gate)
						{
							ForwardOnError(error2);
							return;
						}
					}
					SubscribeInner(inner);
					Final();
				}
				else
				{
					base.OnError(error);
				}
			}

			public override void OnCompleted()
			{
				if (_selectorOnCompleted != null)
				{
					IObservable<TResult> inner;
					try
					{
						inner = _selectorOnCompleted();
					}
					catch (Exception error)
					{
						lock (_gate)
						{
							ForwardOnError(error);
							return;
						}
					}
					SubscribeInner(inner);
				}
				Final();
			}
		}

		private readonly Func<Exception, IObservable<TResult>> _selectorOnError;

		private readonly Func<IObservable<TResult>> _selectorOnCompleted;

		public ObservableSelectors(IObservable<TSource> source, Func<TSource, IObservable<TResult>> selector, Func<Exception, IObservable<TResult>> selectorOnError, Func<IObservable<TResult>> selectorOnCompleted)
			: base(source, selector)
		{
			_selectorOnError = selectorOnError;
			_selectorOnCompleted = selectorOnCompleted;
		}

		protected override ObservableSelector._ CreateSink(IObserver<TResult> observer)
		{
			return new @_(this, observer);
		}
	}

	internal class ObservableSelectorIndexed : Producer<TResult, ObservableSelectorIndexed._>
	{
		internal class @_ : Sink<TSource, TResult>
		{
			private sealed class InnerObserver : SafeObserver<TResult>
			{
				private readonly @_ _parent;

				public InnerObserver(@_ parent)
				{
					_parent = parent;
				}

				public override void OnNext(TResult value)
				{
					lock (_parent._gate)
					{
						_parent.ForwardOnNext(value);
					}
				}

				public override void OnError(Exception error)
				{
					lock (_parent._gate)
					{
						_parent.ForwardOnError(error);
					}
				}

				public override void OnCompleted()
				{
					_parent._group.Remove(this);
					if (_parent._isStopped && _parent._group.Count == 0)
					{
						lock (_parent._gate)
						{
							_parent.ForwardOnCompleted();
						}
					}
				}
			}

			private readonly object _gate = new object();

			private readonly CompositeDisposable _group = new CompositeDisposable();

			protected readonly Func<TSource, int, IObservable<TResult>> _selector;

			private int _index;

			private volatile bool _isStopped;

			public _(ObservableSelectorIndexed parent, IObserver<TResult> observer)
				: base(observer)
			{
				_selector = parent._selector;
			}

			public override void OnNext(TSource value)
			{
				IObservable<TResult> inner;
				try
				{
					inner = _selector(value, checked(_index++));
				}
				catch (Exception error)
				{
					lock (_gate)
					{
						ForwardOnError(error);
						return;
					}
				}
				SubscribeInner(inner);
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				Final();
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				if (disposing)
				{
					_group.Dispose();
				}
			}

			protected void Final()
			{
				_isStopped = true;
				if (_group.Count == 0)
				{
					lock (_gate)
					{
						ForwardOnCompleted();
						return;
					}
				}
				DisposeUpstream();
			}

			protected void SubscribeInner(IObservable<TResult> inner)
			{
				InnerObserver innerObserver = new InnerObserver(this);
				_group.Add(innerObserver);
				innerObserver.SetResource(inner.SubscribeSafe(innerObserver));
			}
		}

		protected readonly IObservable<TSource> _source;

		protected readonly Func<TSource, int, IObservable<TResult>> _selector;

		public ObservableSelectorIndexed(IObservable<TSource> source, Func<TSource, int, IObservable<TResult>> selector)
		{
			_source = source;
			_selector = selector;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class ObservableSelectorsIndexed : ObservableSelectorIndexed
	{
		internal new sealed class @_ : ObservableSelectorIndexed._
		{
			private readonly object _gate = new object();

			private readonly Func<Exception, IObservable<TResult>> _selectorOnError;

			private readonly Func<IObservable<TResult>> _selectorOnCompleted;

			public _(ObservableSelectorsIndexed parent, IObserver<TResult> observer)
				: base((ObservableSelectorIndexed)parent, observer)
			{
				_selectorOnError = parent._selectorOnError;
				_selectorOnCompleted = parent._selectorOnCompleted;
			}

			public override void OnError(Exception error)
			{
				if (_selectorOnError != null)
				{
					IObservable<TResult> inner;
					try
					{
						inner = _selectorOnError(error);
					}
					catch (Exception error2)
					{
						lock (_gate)
						{
							ForwardOnError(error2);
							return;
						}
					}
					SubscribeInner(inner);
					Final();
				}
				else
				{
					base.OnError(error);
				}
			}

			public override void OnCompleted()
			{
				if (_selectorOnCompleted != null)
				{
					IObservable<TResult> inner;
					try
					{
						inner = _selectorOnCompleted();
					}
					catch (Exception error)
					{
						lock (_gate)
						{
							ForwardOnError(error);
							return;
						}
					}
					SubscribeInner(inner);
				}
				Final();
			}
		}

		private readonly Func<Exception, IObservable<TResult>> _selectorOnError;

		private readonly Func<IObservable<TResult>> _selectorOnCompleted;

		public ObservableSelectorsIndexed(IObservable<TSource> source, Func<TSource, int, IObservable<TResult>> selector, Func<Exception, IObservable<TResult>> selectorOnError, Func<IObservable<TResult>> selectorOnCompleted)
			: base(source, selector)
		{
			_selectorOnError = selectorOnError;
			_selectorOnCompleted = selectorOnCompleted;
		}

		protected override ObservableSelectorIndexed._ CreateSink(IObserver<TResult> observer)
		{
			return new @_(this, observer);
		}
	}

	internal sealed class EnumerableSelector : Producer<TResult, EnumerableSelector._>
	{
		internal sealed class @_ : Sink<TSource, TResult>
		{
			private readonly Func<TSource, IEnumerable<TResult>> _selector;

			public _(EnumerableSelector parent, IObserver<TResult> observer)
				: base(observer)
			{
				_selector = parent._selector;
			}

			public override void OnNext(TSource value)
			{
				IEnumerable<TResult> enumerable;
				try
				{
					enumerable = _selector(value);
				}
				catch (Exception error)
				{
					ForwardOnError(error);
					return;
				}
				IEnumerator<TResult> enumerator;
				try
				{
					enumerator = enumerable.GetEnumerator();
				}
				catch (Exception error2)
				{
					ForwardOnError(error2);
					return;
				}
				using (enumerator)
				{
					bool flag = true;
					while (flag)
					{
						TResult value2 = default(TResult);
						try
						{
							flag = enumerator.MoveNext();
							if (flag)
							{
								value2 = enumerator.Current;
							}
						}
						catch (Exception error3)
						{
							ForwardOnError(error3);
							break;
						}
						if (flag)
						{
							ForwardOnNext(value2);
						}
					}
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Func<TSource, IEnumerable<TResult>> _selector;

		public EnumerableSelector(IObservable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
		{
			_source = source;
			_selector = selector;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class EnumerableSelectorIndexed : Producer<TResult, EnumerableSelectorIndexed._>
	{
		internal sealed class @_ : Sink<TSource, TResult>
		{
			private readonly Func<TSource, int, IEnumerable<TResult>> _selector;

			private int _index;

			public _(EnumerableSelectorIndexed parent, IObserver<TResult> observer)
				: base(observer)
			{
				_selector = parent._selector;
			}

			public override void OnNext(TSource value)
			{
				IEnumerable<TResult> enumerable;
				try
				{
					enumerable = _selector(value, checked(_index++));
				}
				catch (Exception error)
				{
					ForwardOnError(error);
					return;
				}
				IEnumerator<TResult> enumerator;
				try
				{
					enumerator = enumerable.GetEnumerator();
				}
				catch (Exception error2)
				{
					ForwardOnError(error2);
					return;
				}
				using (enumerator)
				{
					bool flag = true;
					while (flag)
					{
						TResult value2 = default(TResult);
						try
						{
							flag = enumerator.MoveNext();
							if (flag)
							{
								value2 = enumerator.Current;
							}
						}
						catch (Exception error3)
						{
							ForwardOnError(error3);
							break;
						}
						if (flag)
						{
							ForwardOnNext(value2);
						}
					}
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Func<TSource, int, IEnumerable<TResult>> _selector;

		public EnumerableSelectorIndexed(IObservable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
		{
			_source = source;
			_selector = selector;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class TaskSelector : Producer<TResult, TaskSelector._>
	{
		internal sealed class @_ : Sink<TSource, TResult>
		{
			private readonly object _gate = new object();

			private readonly CancellationTokenSource _cts = new CancellationTokenSource();

			private readonly Func<TSource, CancellationToken, Task<TResult>> _selector;

			private volatile int _count;

			public _(TaskSelector parent, IObserver<TResult> observer)
				: base(observer)
			{
				_selector = parent._selector;
			}

			public override void Run(IObservable<TSource> source)
			{
				_count = 1;
				base.Run(source);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_cts.Cancel();
				}
				base.Dispose(disposing);
			}

			public override void OnNext(TSource value)
			{
				Task<TResult> task;
				try
				{
					Interlocked.Increment(ref _count);
					task = _selector(value, _cts.Token);
				}
				catch (Exception error)
				{
					lock (_gate)
					{
						ForwardOnError(error);
						return;
					}
				}
				if (task.IsCompleted)
				{
					OnCompletedTask(task);
					return;
				}
				task.ContinueWith(delegate(Task<TResult> closureTask, object thisObject)
				{
					((@_)thisObject).OnCompletedTask(closureTask);
				}, this, _cts.Token);
			}

			private void OnCompletedTask(Task<TResult> task)
			{
				switch (task.Status)
				{
				case TaskStatus.RanToCompletion:
					lock (_gate)
					{
						ForwardOnNext(task.Result);
					}
					OnCompleted();
					break;
				case TaskStatus.Faulted:
					lock (_gate)
					{
						ForwardOnError(task.GetSingleException());
						break;
					}
				case TaskStatus.Canceled:
					if (!_cts.IsCancellationRequested)
					{
						lock (_gate)
						{
							ForwardOnError(new TaskCanceledException(task));
							break;
						}
					}
					break;
				}
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				if (Interlocked.Decrement(ref _count) == 0)
				{
					lock (_gate)
					{
						ForwardOnCompleted();
					}
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Func<TSource, CancellationToken, Task<TResult>> _selector;

		public TaskSelector(IObservable<TSource> source, Func<TSource, CancellationToken, Task<TResult>> selector)
		{
			_source = source;
			_selector = selector;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class TaskSelectorIndexed : Producer<TResult, TaskSelectorIndexed._>
	{
		internal sealed class @_ : Sink<TSource, TResult>
		{
			private readonly object _gate = new object();

			private readonly CancellationTokenSource _cts = new CancellationTokenSource();

			private readonly Func<TSource, int, CancellationToken, Task<TResult>> _selector;

			private volatile int _count;

			private int _index;

			public _(TaskSelectorIndexed parent, IObserver<TResult> observer)
				: base(observer)
			{
				_selector = parent._selector;
			}

			public override void Run(IObservable<TSource> source)
			{
				_count = 1;
				base.Run(source);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_cts.Cancel();
				}
				base.Dispose(disposing);
			}

			public override void OnNext(TSource value)
			{
				Task<TResult> task;
				try
				{
					Interlocked.Increment(ref _count);
					task = _selector(value, checked(_index++), _cts.Token);
				}
				catch (Exception error)
				{
					lock (_gate)
					{
						ForwardOnError(error);
						return;
					}
				}
				if (task.IsCompleted)
				{
					OnCompletedTask(task);
					return;
				}
				task.ContinueWith(delegate(Task<TResult> closureTask, object thisObject)
				{
					((@_)thisObject).OnCompletedTask(closureTask);
				}, this, _cts.Token);
			}

			private void OnCompletedTask(Task<TResult> task)
			{
				switch (task.Status)
				{
				case TaskStatus.RanToCompletion:
					lock (_gate)
					{
						ForwardOnNext(task.Result);
					}
					OnCompleted();
					break;
				case TaskStatus.Faulted:
					lock (_gate)
					{
						ForwardOnError(task.GetSingleException());
						break;
					}
				case TaskStatus.Canceled:
					if (!_cts.IsCancellationRequested)
					{
						lock (_gate)
						{
							ForwardOnError(new TaskCanceledException(task));
							break;
						}
					}
					break;
				}
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				if (Interlocked.Decrement(ref _count) == 0)
				{
					lock (_gate)
					{
						ForwardOnCompleted();
					}
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Func<TSource, int, CancellationToken, Task<TResult>> _selector;

		public TaskSelectorIndexed(IObservable<TSource> source, Func<TSource, int, CancellationToken, Task<TResult>> selector)
		{
			_source = source;
			_selector = selector;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}
}
