using System.Collections.Generic;
using Markdig.Helpers;
using Markdig.Parsers;

namespace Markdig.Syntax;

public class CodeBlock : LeafBlock
{
	public class CodeBlockLine
	{
		public StringSlice TriviaBefore { get; set; }
	}

	private List<CodeBlockLine>? _codeBlockLines;

	public List<CodeBlockLine> CodeBlockLines => _codeBlockLines ?? (_codeBlockLines = new List<CodeBlockLine>());

	public CodeBlock(BlockParser parser)
		: base(parser)
	{
	}
}
