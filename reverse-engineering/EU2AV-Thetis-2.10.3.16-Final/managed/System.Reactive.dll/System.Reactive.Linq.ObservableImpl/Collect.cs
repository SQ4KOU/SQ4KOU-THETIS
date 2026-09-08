using System.Diagnostics.CodeAnalysis;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Collect<TSource, TResult> : PushToPullAdapter<TSource, TResult>
{
	private sealed class @_ : PushToPullSink<TSource, TResult>
	{
		private readonly object _gate;

		private readonly Func<TResult, TSource, TResult> _merge;

		private readonly Func<TResult, TResult> _getNewCollector;

		private TResult _collector;

		private Exception? _error;

		private bool _hasCompleted;

		private bool _done;

		public _(Func<TResult, TSource, TResult> merge, Func<TResult, TResult> getNewCollector, TResult collector)
		{
			_gate = new object();
			_merge = merge;
			_getNewCollector = getNewCollector;
			_collector = collector;
		}

		public override void OnNext(TSource value)
		{
			lock (_gate)
			{
				try
				{
					_collector = _merge(_collector, value);
				}
				catch (Exception error)
				{
					_error = error;
					Dispose();
				}
			}
		}

		public override void OnError(Exception error)
		{
			Dispose();
			lock (_gate)
			{
				_error = error;
			}
		}

		public override void OnCompleted()
		{
			Dispose();
			lock (_gate)
			{
				_hasCompleted = true;
			}
		}

		public override bool TryMoveNext([MaybeNullWhen(false)] out TResult current)
		{
			lock (_gate)
			{
				Exception error = _error;
				if (error != null)
				{
					current = default(TResult);
					_collector = default(TResult);
					error.Throw();
				}
				else if (_hasCompleted)
				{
					if (_done)
					{
						current = default(TResult);
						_collector = default(TResult);
						return false;
					}
					current = _collector;
					_done = true;
				}
				else
				{
					current = _collector;
					try
					{
						_collector = _getNewCollector(current);
					}
					catch
					{
						Dispose();
						throw;
					}
				}
				return true;
			}
		}
	}

	private readonly Func<TResult> _getInitialCollector;

	private readonly Func<TResult, TSource, TResult> _merge;

	private readonly Func<TResult, TResult> _getNewCollector;

	public Collect(IObservable<TSource> source, Func<TResult> getInitialCollector, Func<TResult, TSource, TResult> merge, Func<TResult, TResult> getNewCollector)
		: base(source)
	{
		_getInitialCollector = getInitialCollector;
		_merge = merge;
		_getNewCollector = getNewCollector;
	}

	protected override PushToPullSink<TSource, TResult> Run()
	{
		return new @_(_merge, _getNewCollector, _getInitialCollector());
	}
}
