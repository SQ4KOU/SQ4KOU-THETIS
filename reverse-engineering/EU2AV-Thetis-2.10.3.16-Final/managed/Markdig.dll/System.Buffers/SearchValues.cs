namespace System.Buffers;

internal static class SearchValues
{
	public static SearchValues<char> Create(string values)
	{
		return Create(values.AsSpan());
	}

	public static SearchValues<char> Create(ReadOnlySpan<char> values)
	{
		return new PreNet8CompatSearchValues(values);
	}

	public static int IndexOfAny(this ReadOnlySpan<char> span, SearchValues<char> values)
	{
		return values.IndexOfAny(span);
	}

	public static int IndexOfAnyExcept(this ReadOnlySpan<char> span, SearchValues<char> values)
	{
		return values.IndexOfAnyExcept(span);
	}
}
internal abstract class SearchValues<T>
{
	public abstract bool Contains(T value);

	public abstract int IndexOfAny(ReadOnlySpan<char> span);

	public abstract int IndexOfAnyExcept(ReadOnlySpan<char> span);
}
