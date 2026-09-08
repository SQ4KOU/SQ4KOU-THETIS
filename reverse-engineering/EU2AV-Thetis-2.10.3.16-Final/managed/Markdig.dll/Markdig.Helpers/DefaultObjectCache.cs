namespace Markdig.Helpers;

public abstract class DefaultObjectCache<T> : ObjectCache<T> where T : class, new()
{
	protected override T NewInstance()
	{
		return new T();
	}
}
