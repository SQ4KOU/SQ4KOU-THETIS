namespace System.Reactive.Linq.ObservableImpl;

internal interface ICombineLatest
{
	void Next(int index);

	void Fail(Exception error);

	void Done(int index);
}
