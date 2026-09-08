namespace System.Reactive.Linq.ObservableImpl;

internal interface IZip
{
	void Next(int index);

	void Fail(Exception error);

	void Done(int index);
}
