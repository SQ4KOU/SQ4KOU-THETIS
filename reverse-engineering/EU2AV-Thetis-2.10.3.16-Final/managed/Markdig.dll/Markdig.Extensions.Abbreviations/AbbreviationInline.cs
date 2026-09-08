using System.Diagnostics;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.Abbreviations;

[DebuggerDisplay("{Abbreviation}")]
public class AbbreviationInline : LeafInline
{
	public Abbreviation Abbreviation { get; set; }

	public AbbreviationInline(Abbreviation abbreviation)
	{
		Abbreviation = abbreviation;
	}
}
