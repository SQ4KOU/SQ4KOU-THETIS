using System.Collections.Generic;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Case<TValue, TResult> : Producer<TResult, Case<TValue, TResult>._>, IEvaluatableObservable<TResult> where TValue : notnull
{
	internal sealed class @_ : IdentitySink<TResult>
	{
		public _(IObserver<TResult> observer)
			: base(observer)
		{
		}

		public void Run(Case<TValue, TResult> parent)
		{
			IObservable<TResult> source;
			try
			{
				source = parent.Eval();
			}
			catch (Exception error)
			{
				ForwardOnError(error);
				return;
			}
			Run(source);
		}
	}

	private readonly Func<TValue> _selector;

	private readonly IDictionary<TValue, IObservable<TResult>> _sources;

	private readonly IObservable<TResult> _defaultSource;

	public Case(Func<TValue> selector, IDictionary<TValue, IObservable<TResult>> sources, IObservable<TResult> defaultSource)
	{
		_selector = selector;
		_sources = sources;
		_defaultSource = defaultSource;
	}

	public IObservable<TResult> Eval()
	{
		if (_sources.TryGetValue(_selector(), out IObservable<TResult> value))
		{
			return value;
		}
		return _defaultSource;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(this);
	}
}
