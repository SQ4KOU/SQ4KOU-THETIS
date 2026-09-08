using System;

namespace Svg.Helpers;

internal readonly ref struct StringPart(ReadOnlySpan<char> value)
{
	public ReadOnlySpan<char> Value { get; } = value;

	public static implicit operator ReadOnlySpan<char>(StringPart part)
	{
		return part.Value;
	}
}
