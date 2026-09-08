using System.Diagnostics;

namespace System.Collections.Immutable;

internal sealed class ImmutableListBuilderDebuggerProxy<T>
{
	private readonly ImmutableList<T>.Builder _list;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public T[] Contents => _003CContents_003Ek__BackingField ?? (_003CContents_003Ek__BackingField = _list.ToArray(_list.Count));

	public ImmutableListBuilderDebuggerProxy(ImmutableList<T>.Builder builder)
	{
		Requires.NotNull(builder, "builder");
		_list = builder;
	}
}
