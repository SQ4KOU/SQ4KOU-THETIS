using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Multicast<TSource, TIntermediate, TResult> : Producer<TResult, Multicast<TSource, TIntermediate, TResult>._>
{
	internal sealed class @_ : IdentitySink<TResult>
	{
		private SingleAssignmentDisposableValue _connection;

		public _(IObserver<TResult> observer)
			: base(observer)
		{
		}

		public void Run(Multicast<TSource, TIntermediate, TResult> parent)
		{
			IConnectableObservable<TIntermediate> connectableObservable;
			IObservable<TResult> source;
			try
			{
				ISubject<TSource, TIntermediate> subject = parent._subjectSelector();
				connectableObservable = new ConnectableObservable<TSource, TIntermediate>(parent._source, subject);
				source = parent._selector(connectableObservable);
			}
			catch (Exception error)
			{
				ForwardOnError(error);
				return;
			}
			Run(source);
			_connection.Disposable = connectableObservable.Connect();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_connection.Dispose();
			}
			base.Dispose(disposing);
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly Func<ISubject<TSource, TIntermediate>> _subjectSelector;

	private readonly Func<IObservable<TIntermediate>, IObservable<TResult>> _selector;

	public Multicast(IObservable<TSource> source, Func<ISubject<TSource, TIntermediate>> subjectSelector, Func<IObservable<TIntermediate>, IObservable<TResult>> selector)
	{
		_source = source;
		_subjectSelector = subjectSelector;
		_selector = selector;
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
