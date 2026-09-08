namespace System.Reactive;

internal sealed class ImmutableList<T>
{
	public static readonly ImmutableList<T> Empty = new ImmutableList<T>();

	private readonly T[] _data;

	public T[] Data => _data;

	private ImmutableList()
	{
		_data = Array.Empty<T>();
	}

	public ImmutableList(T[] data)
	{
		_data = data;
	}

	public ImmutableList<T> Add(T value)
	{
		T[] array = new T[_data.Length + 1];
		Array.Copy(_data, array, _data.Length);
		array[_data.Length] = value;
		return new ImmutableList<T>(array);
	}

	public ImmutableList<T> Remove(T value)
	{
		int num = Array.IndexOf<T>(_data, value);
		if (num < 0)
		{
			return this;
		}
		int num2 = _data.Length;
		if (num2 == 1)
		{
			return Empty;
		}
		T[] array = new T[num2 - 1];
		Array.Copy(_data, 0, array, 0, num);
		Array.Copy(_data, num + 1, array, num, num2 - num - 1);
		return new ImmutableList<T>(array);
	}
}
