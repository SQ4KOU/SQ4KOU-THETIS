using System.Threading;

namespace System.Reactive.Disposables;

public struct SingleAssignmentDisposableValue
{
	private IDisposable? _current;

	public bool IsDisposed => Volatile.Read(ref _current) == BooleanDisposable.True;

	public IDisposable? Disposable
	{
		get
		{
			return System.Reactive.Disposables.Disposable.GetValueOrDefault(ref _current);
		}
		set
		{
			if (System.Reactive.Disposables.Disposable.TrySetSingle(ref _current, value) == TrySetSingleResult.AlreadyAssigned)
			{
				throw new InvalidOperationException(Strings_Core.DISPOSABLE_ALREADY_ASSIGNED);
			}
		}
	}

	public void Dispose()
	{
		System.Reactive.Disposables.Disposable.Dispose(ref _current);
	}

	public override readonly bool Equals(object? obj)
	{
		return false;
	}

	public override readonly int GetHashCode()
	{
		return 0;
	}

	public static bool operator ==(SingleAssignmentDisposableValue left, SingleAssignmentDisposableValue right)
	{
		return false;
	}

	public static bool operator !=(SingleAssignmentDisposableValue left, SingleAssignmentDisposableValue right)
	{
		return true;
	}
}
