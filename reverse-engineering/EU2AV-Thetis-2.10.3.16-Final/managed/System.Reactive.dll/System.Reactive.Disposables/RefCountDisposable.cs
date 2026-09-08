using System.Threading;

namespace System.Reactive.Disposables;

public sealed class RefCountDisposable : ICancelable, IDisposable
{
	private sealed class InnerDisposable : IDisposable
	{
		private RefCountDisposable? _parent;

		public InnerDisposable(RefCountDisposable parent)
		{
			_parent = parent;
		}

		public void Dispose()
		{
			Interlocked.Exchange(ref _parent, null)?.Release();
		}
	}

	private readonly bool _throwWhenDisposed;

	private IDisposable? _disposable;

	private int _count;

	public bool IsDisposed => Volatile.Read(ref _count) == int.MinValue;

	public RefCountDisposable(IDisposable disposable)
		: this(disposable, throwWhenDisposed: false)
	{
	}

	public RefCountDisposable(IDisposable disposable, bool throwWhenDisposed)
	{
		_disposable = disposable ?? throw new ArgumentNullException("disposable");
		_count = 0;
		_throwWhenDisposed = throwWhenDisposed;
	}

	public IDisposable GetDisposable()
	{
		int num = Volatile.Read(ref _count);
		while (true)
		{
			if (num == int.MinValue)
			{
				if (_throwWhenDisposed)
				{
					throw new ObjectDisposedException("RefCountDisposable");
				}
				return Disposable.Empty;
			}
			if ((num & 0x7FFFFFFF) == int.MaxValue)
			{
				throw new OverflowException($"RefCountDisposable can't handle more than {int.MaxValue} disposables");
			}
			int num2 = Interlocked.CompareExchange(ref _count, num + 1, num);
			if (num2 == num)
			{
				break;
			}
			num = num2;
		}
		return new InnerDisposable(this);
	}

	public void Dispose()
	{
		int num = Volatile.Read(ref _count);
		while ((num & 0x80000000u) == 0L)
		{
			int num2 = num & 0x7FFFFFFF;
			int value = int.MinValue | num2;
			int num3 = Interlocked.CompareExchange(ref _count, value, num);
			if (num3 == num)
			{
				if (num2 == 0)
				{
					_disposable?.Dispose();
					_disposable = null;
				}
				break;
			}
			num = num3;
		}
	}

	private void Release()
	{
		int num = Volatile.Read(ref _count);
		int num4;
		while (true)
		{
			int num2 = (int)(num & 0x80000000u);
			int num3 = num & 0x7FFFFFFF;
			num4 = num2 | (num3 - 1);
			int num5 = Interlocked.CompareExchange(ref _count, num4, num);
			if (num5 == num)
			{
				break;
			}
			num = num5;
		}
		if (num4 == int.MinValue)
		{
			_disposable?.Dispose();
			_disposable = null;
		}
	}
}
