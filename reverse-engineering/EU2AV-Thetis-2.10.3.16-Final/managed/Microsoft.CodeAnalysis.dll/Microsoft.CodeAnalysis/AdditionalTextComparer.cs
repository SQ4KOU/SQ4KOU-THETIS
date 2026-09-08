using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class AdditionalTextComparer : IEqualityComparer<AdditionalText>
{
	public static readonly AdditionalTextComparer Instance = new AdditionalTextComparer();

	public bool Equals(AdditionalText? x, AdditionalText? y)
	{
		if (x == y)
		{
			return true;
		}
		if (x == null || y == null)
		{
			return false;
		}
		if (!PathUtilities.Comparer.Equals(x.Path, y.Path))
		{
			return false;
		}
		SourceText textOrNullIfBinary = GetTextOrNullIfBinary(x);
		SourceText textOrNullIfBinary2 = GetTextOrNullIfBinary(y);
		if (textOrNullIfBinary == null && textOrNullIfBinary2 == null)
		{
			return true;
		}
		if (textOrNullIfBinary == null || textOrNullIfBinary2 == null || textOrNullIfBinary.Length != textOrNullIfBinary2.Length)
		{
			return false;
		}
		return ByteSequenceComparer.Equals(textOrNullIfBinary.GetChecksum(), textOrNullIfBinary2.GetChecksum());
	}

	public int GetHashCode(AdditionalText obj)
	{
		return Hash.Combine(PathUtilities.Comparer.GetHashCode(obj.Path), ByteSequenceComparer.GetHashCode(GetTextOrNullIfBinary(obj)?.GetChecksum() ?? ImmutableArray<byte>.Empty));
	}

	private static SourceText? GetTextOrNullIfBinary(AdditionalText text)
	{
		try
		{
			return text.GetText();
		}
		catch (InvalidDataException)
		{
			return null;
		}
	}
}
