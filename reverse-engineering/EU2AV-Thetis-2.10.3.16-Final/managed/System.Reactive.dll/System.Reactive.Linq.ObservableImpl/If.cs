namespace System.Reactive.Linq.ObservableImpl;

internal sealed class If<TResult> : Producer<TResult, If<TResult>._>, IEvaluatableObservable<TResult>
{
	internal sealed class @_ : IdentitySink<TResult>
	{
		private readonly If<TResult> _parent;

		public _(If<TResult> parent, IObserver<TResult> observer)
			: base(observer)
		{
			_parent = parent;
		}

		public void Run()
		{
			IObservable<TResult> source;
			try
			{
				source = _parent.Eval();
			}
			catch (Exception error)
			{
				ForwardOnError(error);
				return;
			}
			SetUpstream(source.SubscribeSafe(this));
		}
	}

	private readonly Func<bool> _condition;

	private readonly IObservable<TResult> _thenSource;

	private readonly IObservable<TResult> _elseSource;

	public If(Func<bool> condition, IObservable<TResult> thenSource, IObservable<TResult> elseSource)
	{
		_condition = condition;
		_thenSource = thenSource;
		_elseSource = elseSource;
	}

	public IObservable<TResult> Eval()
	{
		if (!_condition())
		{
			return _elseSource;
		}
		return _thenSource;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(this, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run();
	}
}
