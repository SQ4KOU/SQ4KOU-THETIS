using System.Threading;

namespace Markdig.Helpers;

internal sealed class CustomArrayPool<T>
{
	private sealed class Bucket
	{
		private readonly T[][] _buffers;

		private int _index;

		private int _lock;

		public Bucket(int numberOfBuffers)
		{
			_buffers = new T[numberOfBuffers][];
		}

		public T[] Rent()
		{
			T[][] buffers = _buffers;
			T[] result = null;
			if (Interlocked.CompareExchange(ref _lock, 1, 0) == 0)
			{
				int index = _index;
				if ((uint)index < (uint)buffers.Length)
				{
					result = buffers[index];
					buffers[index] = null;
					_index = index + 1;
				}
				Interlocked.Decrement(ref _lock);
			}
			return result;
		}

		public void Return(T[] array)
		{
			T[][] buffers = _buffers;
			if (Interlocked.CompareExchange(ref _lock, 1, 0) == 0)
			{
				int num = _index - 1;
				if ((uint)num < (uint)buffers.Length)
				{
					buffers[num] = array;
					_index = num;
				}
				Interlocked.Decrement(ref _lock);
			}
		}
	}

	private readonly Bucket _bucket4;

	private readonly Bucket _bucket8;

	private readonly Bucket _bucket16;

	private readonly Bucket _bucket32;

	public CustomArrayPool(int size4, int size8, int size16, int size32)
	{
		_bucket4 = new Bucket(size4);
		_bucket8 = new Bucket(size8);
		_bucket16 = new Bucket(size16);
		_bucket32 = new Bucket(size32);
	}

	private Bucket? SelectBucket(int length)
	{
		return length switch
		{
			4 => _bucket4, 
			8 => _bucket8, 
			16 => _bucket16, 
			32 => _bucket32, 
			_ => null, 
		};
	}

	public T[] Rent(int length)
	{
		return SelectBucket(length)?.Rent() ?? new T[length];
	}

	public void Return(T[] array)
	{
		SelectBucket(array.Length)?.Return(array);
	}
}
