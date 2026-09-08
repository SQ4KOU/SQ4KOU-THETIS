using Markdig.Helpers;

namespace Markdig.Syntax;

public static class MarkdownObjectDataExtensions
{
	public static T? GetData<T>(this IMarkdownObject markdownObject, object key)
	{
		if (markdownObject == null)
		{
			ThrowHelper.ArgumentNullException("markdownObject");
		}
		if (key == null)
		{
			ThrowHelper.ArgumentNullException("key");
		}
		object data = markdownObject.GetData(key);
		if (data is T)
		{
			return (T)data;
		}
		return default(T);
	}

	public static T? GetData<T>(this IMarkdownObject markdownObject, DataKey<T> key)
	{
		if (key == null)
		{
			ThrowHelper.ArgumentNullException("key");
		}
		return markdownObject.GetData<T>(key.Key);
	}

	public static T? GetData<T>(this IMarkdownObject markdownObject)
	{
		if (markdownObject == null)
		{
			ThrowHelper.ArgumentNullException("markdownObject");
		}
		return markdownObject.GetData<T>(typeof(T));
	}

	public static bool TryGetData<T>(this IMarkdownObject markdownObject, object key, out T? value)
	{
		if (markdownObject == null)
		{
			ThrowHelper.ArgumentNullException("markdownObject");
		}
		if (key == null)
		{
			ThrowHelper.ArgumentNullException("key");
		}
		if (markdownObject.GetData(key) is T val)
		{
			value = val;
			return true;
		}
		value = default(T);
		return false;
	}

	public static bool TryGetData<T>(this IMarkdownObject markdownObject, DataKey<T> key, out T? value)
	{
		if (key == null)
		{
			ThrowHelper.ArgumentNullException("key");
		}
		return markdownObject.TryGetData<T>(key.Key, out value);
	}

	public static void SetData<T>(this IMarkdownObject markdownObject, T value)
	{
		if (markdownObject == null)
		{
			ThrowHelper.ArgumentNullException("markdownObject");
		}
		markdownObject.SetData(typeof(T), value);
	}

	public static void SetData<T>(this IMarkdownObject markdownObject, DataKey<T> key, T value)
	{
		if (markdownObject == null)
		{
			ThrowHelper.ArgumentNullException("markdownObject");
		}
		if (key == null)
		{
			ThrowHelper.ArgumentNullException("key");
		}
		markdownObject.SetData(key.Key, value);
	}
}
