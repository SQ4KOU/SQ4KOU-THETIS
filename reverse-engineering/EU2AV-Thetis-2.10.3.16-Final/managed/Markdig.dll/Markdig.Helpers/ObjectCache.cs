using System.Collections.Concurrent;

namespace Markdig.Helpers;

public abstract class ObjectCache<T> where T : class
{
	private readonly ConcurrentQueue<T> _builders;

	protected ObjectCache()
	{
		_builders = new ConcurrentQueue<T>();
	}

	public void Clear()
	{
		_builders.Clear();
	}

	public T Get()
	{
		if (_builders.TryDequeue(out var result))
		{
			return result;
		}
		return NewInstance();
	}

	public void Release(T instance)
	{
		if (instance == null)
		{
			ThrowHelper.ArgumentNullException("instance");
		}
		Reset(instance);
		_builders.Enqueue(instance);
	}

	protected abstract T NewInstance();

	protected abstract void Reset(T instance);
}
