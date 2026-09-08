using System.Collections.Generic;
using System.Threading;

namespace System.Reactive.Disposables;

public abstract class StableCompositeDisposable : ICancelable, IDisposable
{
	private sealed class Binary : StableCompositeDisposable
	{
		private SingleAssignmentDisposableValue _disposable1;

		private SingleAssignmentDisposableValue _disposable2;

		public override bool IsDisposed => _disposable1.IsDisposed;

		public Binary(IDisposable disposable1, IDisposable disposable2)
		{
			_disposable1.Disposable = disposable1;
			_disposable2.Disposable = disposable2;
		}

		public override void Dispose()
		{
			_disposable1.Dispose();
			_disposable2.Dispose();
		}
	}

	private sealed class NAryEnumerable : StableCompositeDisposable
	{
		private volatile List<IDisposable>? _disposables;

		public override bool IsDisposed => _disposables == null;

		public NAryEnumerable(IEnumerable<IDisposable> disposables)
		{
			_disposables = new List<IDisposable>(disposables);
			if (_disposables.Contains(null))
			{
				throw new ArgumentException(Strings_Core.DISPOSABLES_CANT_CONTAIN_NULL, "disposables");
			}
		}

		public override void Dispose()
		{
			List<IDisposable> list = Interlocked.Exchange(ref _disposables, null);
			if (list == null)
			{
				return;
			}
			foreach (IDisposable item in list)
			{
				item.Dispose();
			}
		}
	}

	private sealed class NAryArray : StableCompositeDisposable
	{
		private IDisposable[]? _disposables;

		public override bool IsDisposed => Volatile.Read(ref _disposables) == null;

		public NAryArray(IDisposable[] disposables)
		{
			if (Array.IndexOf(disposables, null) != -1)
			{
				throw new ArgumentException(Strings_Core.DISPOSABLES_CANT_CONTAIN_NULL, "disposables");
			}
			int num = disposables.Length;
			IDisposable[] array = new IDisposable[num];
			Array.Copy(disposables, 0, array, 0, num);
			Volatile.Write(ref _disposables, array);
		}

		public override void Dispose()
		{
			IDisposable[] array = Interlocked.Exchange(ref _disposables, null);
			if (array != null)
			{
				IDisposable[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].Dispose();
				}
			}
		}
	}

	private sealed class NAryTrustedArray : StableCompositeDisposable
	{
		private IDisposable[]? _disposables;

		public override bool IsDisposed => Volatile.Read(ref _disposables) == null;

		public NAryTrustedArray(IDisposable[] disposables)
		{
			Volatile.Write(ref _disposables, disposables);
		}

		public override void Dispose()
		{
			IDisposable[] array = Interlocked.Exchange(ref _disposables, null);
			if (array != null)
			{
				IDisposable[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].Dispose();
				}
			}
		}
	}

	public abstract bool IsDisposed { get; }

	public static ICancelable Create(IDisposable disposable1, IDisposable disposable2)
	{
		if (disposable1 == null)
		{
			throw new ArgumentNullException("disposable1");
		}
		if (disposable2 == null)
		{
			throw new ArgumentNullException("disposable2");
		}
		return new Binary(disposable1, disposable2);
	}

	public static ICancelable Create(params IDisposable[] disposables)
	{
		if (disposables == null)
		{
			throw new ArgumentNullException("disposables");
		}
		return new NAryArray(disposables);
	}

	internal static ICancelable CreateTrusted(params IDisposable[] disposables)
	{
		return new NAryTrustedArray(disposables);
	}

	public static ICancelable Create(IEnumerable<IDisposable> disposables)
	{
		if (disposables == null)
		{
			throw new ArgumentNullException("disposables");
		}
		return new NAryEnumerable(disposables);
	}

	public abstract void Dispose();
}
