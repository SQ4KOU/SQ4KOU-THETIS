using System.Diagnostics;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.Abbreviations;

[DebuggerDisplay("Abbr {Label} => {Text}")]
public class Abbreviation : LeafBlock
{
	public StringSlice Text;

	public SourceSpan LabelSpan;

	public string? Label { get; set; }

	public Abbreviation(BlockParser parser)
		: base(parser)
	{
	}
}
