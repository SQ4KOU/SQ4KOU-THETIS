using System;
using System.Text;

namespace Microsoft.CodeAnalysis;

internal static class EncodingExtensions
{
	internal static bool TryGetMaxCharCount(this Encoding encoding, long length, out int maxCharCount)
	{
		maxCharCount = 0;
		if (length <= int.MaxValue)
		{
			try
			{
				maxCharCount = encoding.GetMaxCharCount((int)length);
				return true;
			}
			catch (ArgumentOutOfRangeException)
			{
			}
		}
		return false;
	}
}
