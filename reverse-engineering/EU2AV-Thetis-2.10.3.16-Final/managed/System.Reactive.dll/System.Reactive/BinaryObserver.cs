namespace System.Reactive;

internal sealed class BinaryObserver<TLeft, TRight> : IObserver<Either<Notification<TLeft>, Notification<TRight>>>
{
	public IObserver<TLeft> LeftObserver { get; }

	public IObserver<TRight> RightObserver { get; }

	public BinaryObserver(IObserver<TLeft> leftObserver, IObserver<TRight> rightObserver)
	{
		LeftObserver = leftObserver;
		RightObserver = rightObserver;
	}

	public BinaryObserver(Action<Notification<TLeft>> left, Action<Notification<TRight>> right)
		: this(left.ToObserver(), right.ToObserver())
	{
	}

	void IObserver<Either<Notification<TLeft>, Notification<TRight>>>.OnNext(Either<Notification<TLeft>, Notification<TRight>> value)
	{
		value.Switch(delegate(Notification<TLeft> left)
		{
			left.Accept(LeftObserver);
		}, delegate(Notification<TRight> right)
		{
			right.Accept(RightObserver);
		});
	}

	void IObserver<Either<Notification<TLeft>, Notification<TRight>>>.OnError(Exception exception)
	{
	}

	void IObserver<Either<Notification<TLeft>, Notification<TRight>>>.OnCompleted()
	{
	}
}
