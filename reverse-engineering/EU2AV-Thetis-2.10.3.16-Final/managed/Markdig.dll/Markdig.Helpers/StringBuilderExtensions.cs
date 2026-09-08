using System.Text;

namespace Markdig.Helpers;

public static class StringBuilderExtensions
{
	public static StringBuilder Append(this StringBuilder builder, StringSlice slice)
	{
		return builder.Append(slice.Text, slice.Start, slice.Length);
	}
}
