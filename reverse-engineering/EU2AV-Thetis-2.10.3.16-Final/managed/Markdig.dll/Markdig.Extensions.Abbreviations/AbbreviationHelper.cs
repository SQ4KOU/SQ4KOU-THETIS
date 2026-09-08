using System.Collections.Generic;
using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Extensions.Abbreviations;

public static class AbbreviationHelper
{
	private static readonly object DocumentKey = typeof(Abbreviation);

	public static bool HasAbbreviations(this MarkdownDocument document)
	{
		return document.GetAbbreviations() != null;
	}

	public static void AddAbbreviation(this MarkdownDocument document, string label, Abbreviation abbr)
	{
		if (document == null)
		{
			ThrowHelper.ArgumentNullException("document");
		}
		if (label == null)
		{
			ThrowHelper.ArgumentNullException_label();
		}
		if (abbr == null)
		{
			ThrowHelper.ArgumentNullException("abbr");
		}
		Dictionary<string, Abbreviation> dictionary = document.GetAbbreviations();
		if (dictionary == null)
		{
			dictionary = new Dictionary<string, Abbreviation>();
			document.SetData(DocumentKey, dictionary);
		}
		dictionary[label] = abbr;
	}

	public static Dictionary<string, Abbreviation>? GetAbbreviations(this MarkdownDocument document)
	{
		return document.GetData(DocumentKey) as Dictionary<string, Abbreviation>;
	}
}
