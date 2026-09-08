using System;

namespace Microsoft.CodeAnalysis;

internal static class SeparatedSyntaxListExtensions
{
	internal static int Count<TNode>(this SeparatedSyntaxList<TNode> list, Func<TNode, bool> predicate) where TNode : SyntaxNode
	{
		int count = list.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (predicate(list[i]))
			{
				num++;
			}
		}
		return num;
	}
}
