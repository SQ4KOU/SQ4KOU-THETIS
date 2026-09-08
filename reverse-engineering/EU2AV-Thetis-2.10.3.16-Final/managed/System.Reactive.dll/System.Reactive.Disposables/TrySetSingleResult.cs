namespace System.Reactive.Disposables;

internal enum TrySetSingleResult
{
	Success,
	AlreadyAssigned,
	Disposed
}
