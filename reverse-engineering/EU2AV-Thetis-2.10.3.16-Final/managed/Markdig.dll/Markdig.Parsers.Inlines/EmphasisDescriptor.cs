using System.Diagnostics;
using Markdig.Helpers;

namespace Markdig.Parsers.Inlines;

[DebuggerDisplay("Emphasis Char={Character}, Min={MinimumCount}, Max={MaximumCount}, EnableWithinWord={EnableWithinWord}")]
public sealed class EmphasisDescriptor
{
	public char Character { get; }

	public int MinimumCount { get; }

	public int MaximumCount { get; }

	public bool EnableWithinWord { get; }

	public EmphasisDescriptor(char character, int minimumCount, int maximumCount, bool enableWithinWord)
	{
		if (minimumCount < 1)
		{
			ThrowHelper.ArgumentOutOfRangeException("minimumCount", "minimumCount must be >= 1");
		}
		if (maximumCount < 1)
		{
			ThrowHelper.ArgumentOutOfRangeException("maximumCount", "maximumCount must be >= 1");
		}
		if (minimumCount > maximumCount)
		{
			ThrowHelper.ArgumentOutOfRangeException("minimumCount", "minimumCount must be <= maximumCount");
		}
		Character = character;
		MinimumCount = minimumCount;
		MaximumCount = maximumCount;
		EnableWithinWord = enableWithinWord;
	}
}
