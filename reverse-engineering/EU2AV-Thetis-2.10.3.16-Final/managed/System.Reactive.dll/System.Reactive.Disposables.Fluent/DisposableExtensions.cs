namespace System.Reactive.Disposables.Fluent;

public static class DisposableExtensions
{
	public static T DisposeWith<T>(this T item, CompositeDisposable compositeDisposable) where T : IDisposable
	{
		if (compositeDisposable == null)
		{
			throw new ArgumentNullException("compositeDisposable");
		}
		compositeDisposable.Add(item);
		return item;
	}
}
