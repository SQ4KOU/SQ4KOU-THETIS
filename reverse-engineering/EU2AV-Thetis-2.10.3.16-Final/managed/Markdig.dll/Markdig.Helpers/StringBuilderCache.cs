using System;
using System.Text;

namespace Markdig.Helpers;

public static class StringBuilderCache
{
	[ThreadStatic]
	private static StringBuilder? local;

	public static StringBuilder Local()
	{
		StringBuilder stringBuilder = local ?? (local = new StringBuilder());
		if (stringBuilder.Length != 0)
		{
			stringBuilder.Length = 0;
		}
		return stringBuilder;
	}
}
