using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace System.Reactive.Disposables;

public static class Disposable
{
	private sealed class EmptyDisposable : IDisposable
	{
		public static readonly EmptyDisposable Instance = new EmptyDisposable();

		private EmptyDisposable()
		{
		}

		public void Dispose()
		{
		}
	}

	public static IDisposable Empty => EmptyDisposable.Instance;

	public static IDisposable Create(Action dispose)
	{
		if (dispose == null)
		{
			throw new ArgumentNullException("dispose");
		}
		return new AnonymousDisposable(dispose);
	}

	public static IDisposable Create<TState>(TState state, Action<TState> dispose)
	{
		if (dispose == null)
		{
			throw new ArgumentNullException("dispose");
		}
		return new AnonymousDisposable<TState>(state, dispose);
	}

	internal static IDisposable? GetValue([NotNullIfNotNull("fieldRef")] ref IDisposable? fieldRef)
	{
		IDisposable disposable = Volatile.Read(ref fieldRef);
		if (disposable != BooleanDisposable.True)
		{
			return disposable;
		}
		return null;
	}

	[return: NotNullIfNotNull("fieldRef")]
	internal static IDisposable? GetValueOrDefault([NotNullIfNotNull("fieldRef")] ref IDisposable? fieldRef)
	{
		IDisposable disposable = Volatile.Read(ref fieldRef);
		if (disposable != BooleanDisposable.True)
		{
			return disposable;
		}
		return EmptyDisposable.Instance;
	}

	internal static TrySetSingleResult TrySetSingle([NotNullIfNotNull("value")] ref IDisposable? fieldRef, IDisposable? value)
	{
		IDisposable disposable = Interlocked.CompareExchange(ref fieldRef, value, null);
		if (disposable == null)
		{
			return TrySetSingleResult.Success;
		}
		if (disposable != BooleanDisposable.True)
		{
			return TrySetSingleResult.AlreadyAssigned;
		}
		value?.Dispose();
		return TrySetSingleResult.Disposed;
	}

	internal static bool TrySetMultiple([NotNullIfNotNull("value")] ref IDisposable? fieldRef, IDisposable? value)
	{
		IDisposable disposable = Volatile.Read(ref fieldRef);
		while (true)
		{
			if (disposable == BooleanDisposable.True)
			{
				value?.Dispose();
				return false;
			}
			IDisposable disposable2 = Interlocked.CompareExchange(ref fieldRef, value, disposable);
			if (disposable == disposable2)
			{
				break;
			}
			disposable = disposable2;
		}
		return true;
	}

	internal static bool TrySetSerial([NotNullIfNotNull("value")] ref IDisposable? fieldRef, IDisposable? value)
	{
		IDisposable disposable = Volatile.Read(ref fieldRef);
		while (true)
		{
			if (disposable == BooleanDisposable.True)
			{
				value?.Dispose();
				return false;
			}
			IDisposable disposable2 = Interlocked.CompareExchange(ref fieldRef, value, disposable);
			if (disposable2 == disposable)
			{
				break;
			}
			disposable = disposable2;
		}
		disposable?.Dispose();
		return true;
	}

	internal static void Dispose([NotNullIfNotNull("fieldRef")] ref IDisposable? fieldRef)
	{
		IDisposable disposable = Interlocked.Exchange(ref fieldRef, BooleanDisposable.True);
		if (disposable != BooleanDisposable.True)
		{
			disposable?.Dispose();
		}
	}
}
