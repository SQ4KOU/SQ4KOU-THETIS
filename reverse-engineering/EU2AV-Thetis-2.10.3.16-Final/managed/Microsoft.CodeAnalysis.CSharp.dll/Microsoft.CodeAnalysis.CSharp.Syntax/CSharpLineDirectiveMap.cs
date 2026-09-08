using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

internal class CSharpLineDirectiveMap : LineDirectiveMap<DirectiveTriviaSyntax>
{
	public CSharpLineDirectiveMap(SyntaxTree syntaxTree)
		: base(syntaxTree)
	{
	}

	protected override bool ShouldAddDirective(DirectiveTriviaSyntax directive)
	{
		bool flag = directive.IsActive;
		if (flag)
		{
			SyntaxKind syntaxKind = directive.Kind();
			bool flag2 = ((syntaxKind == SyntaxKind.LineDirectiveTrivia || syntaxKind == SyntaxKind.LineSpanDirectiveTrivia) ? true : false);
			flag = flag2;
		}
		return flag;
	}

	protected override LineMappingEntry GetEntry(DirectiveTriviaSyntax directiveNode, SourceText sourceText, LineMappingEntry previous)
	{
		int num = sourceText.Lines.IndexOf(directiveNode.SpanStart) + 1;
		if (directiveNode is LineSpanDirectiveTriviaSyntax spanDirective)
		{
			return GetLineSpanDirectiveEntry(spanDirective, num);
		}
		LineDirectiveTriviaSyntax lineDirectiveTriviaSyntax = (LineDirectiveTriviaSyntax)directiveNode;
		int num2 = num;
		int mappedLine = (((uint)previous.State == 3u) ? num2 : (previous.MappedLine + num - previous.UnmappedLine));
		string mappedPathOpt = (((uint)previous.State == 3u) ? null : previous.MappedPathOpt);
		LineDirectiveMap<DirectiveTriviaSyntax>.PositionState state = LineDirectiveMap<DirectiveTriviaSyntax>.PositionState.Unmapped;
		SyntaxToken line = lineDirectiveTriviaSyntax.Line;
		if (!line.IsMissing)
		{
			switch (line.Kind())
			{
			case SyntaxKind.HiddenKeyword:
				state = LineDirectiveMap<DirectiveTriviaSyntax>.PositionState.Hidden;
				break;
			case SyntaxKind.DefaultKeyword:
				mappedLine = num2;
				mappedPathOpt = null;
				state = LineDirectiveMap<DirectiveTriviaSyntax>.PositionState.Unmapped;
				break;
			case SyntaxKind.NumericLiteralToken:
				if (!line.ContainsDiagnostics)
				{
					object value = line.Value;
					if (value is int)
					{
						mappedLine = (int)value - 1;
					}
					if (lineDirectiveTriviaSyntax.File.Kind() == SyntaxKind.StringLiteralToken)
					{
						mappedPathOpt = (string)lineDirectiveTriviaSyntax.File.Value;
					}
					state = LineDirectiveMap<DirectiveTriviaSyntax>.PositionState.Remapped;
				}
				break;
			}
		}
		return new LineDirectiveMap<DirectiveTriviaSyntax>.LineMappingEntry(num2, mappedLine, mappedPathOpt, state);
	}

	private static LineMappingEntry GetLineSpanDirectiveEntry(LineSpanDirectiveTriviaSyntax spanDirective, int unmappedLine)
	{
		if (!spanDirective.HasErrors && tryGetPosition(spanDirective.Start, isEnd: false, out var position) && tryGetPosition(spanDirective.End, isEnd: true, out var position2) && tryGetOptionalCharacterOffset(spanDirective.CharacterOffset, out var value) && tryGetStringLiteralValue(spanDirective.File, out var value2))
		{
			return new LineDirectiveMap<DirectiveTriviaSyntax>.LineMappingEntry(unmappedLine, new LinePositionSpan(position, position2), value, value2);
		}
		return new LineDirectiveMap<DirectiveTriviaSyntax>.LineMappingEntry(unmappedLine, unmappedLine, (string?)null, LineDirectiveMap<DirectiveTriviaSyntax>.PositionState.Unmapped);
		static bool tryGetNumericLiteralValue(in SyntaxToken token, out int reference, bool oneBased)
		{
			if (!token.IsMissing && token.Kind() == SyntaxKind.NumericLiteralToken && token.Value is int num)
			{
				reference = num;
				if (oneBased)
				{
					reference--;
				}
				return true;
			}
			reference = 0;
			return false;
		}
		static bool tryGetOptionalCharacterOffset(in SyntaxToken token, out int? reference)
		{
			if (!token.IsMissing)
			{
				if (token.Kind() == SyntaxKind.None)
				{
					reference = null;
					return true;
				}
				int value3 = 0;
				if (tryGetNumericLiteralValue(in token, out value3, oneBased: false))
				{
					reference = value3;
					return true;
				}
			}
			reference = null;
			return false;
		}
		static bool tryGetPosition(LineDirectivePositionSyntax syntax, bool isEnd, out LinePosition reference)
		{
			if (tryGetNumericLiteralValue(syntax.Line, out var value3, oneBased: true) && tryGetNumericLiteralValue(syntax.Character, out var value4, oneBased: true))
			{
				reference = new LinePosition(value3, isEnd ? (value4 + 1) : value4);
				return true;
			}
			reference = default(LinePosition);
			return false;
		}
		static bool tryGetStringLiteralValue(in SyntaxToken token, out string? reference)
		{
			if (token.Kind() == SyntaxKind.StringLiteralToken)
			{
				reference = (string)token.Value;
				return true;
			}
			reference = null;
			return false;
		}
	}

	protected override LineMappingEntry InitializeFirstEntry()
	{
		return new LineDirectiveMap<DirectiveTriviaSyntax>.LineMappingEntry(0, 0, (string?)null, LineDirectiveMap<DirectiveTriviaSyntax>.PositionState.Unmapped);
	}

	public override LineVisibility GetLineVisibility(SourceText sourceText, int position)
	{
		LinePosition linePosition = sourceText.Lines.GetLinePosition(position);
		if (Entries.Length == 1)
		{
			return LineVisibility.Visible;
		}
		int num = FindEntryIndex(linePosition.Line);
		LineDirectiveMap<DirectiveTriviaSyntax>.LineMappingEntry lineMappingEntry = Entries[num];
		switch (lineMappingEntry.State)
		{
		case LineDirectiveMap<DirectiveTriviaSyntax>.PositionState.Unmapped:
			if (num == 0)
			{
				return LineVisibility.BeforeFirstLineDirective;
			}
			return LineVisibility.Visible;
		case LineDirectiveMap<DirectiveTriviaSyntax>.PositionState.Remapped:
		case LineDirectiveMap<DirectiveTriviaSyntax>.PositionState.RemappedSpan:
			return LineVisibility.Visible;
		case LineDirectiveMap<DirectiveTriviaSyntax>.PositionState.Hidden:
			return LineVisibility.Hidden;
		default:
			throw ExceptionUtilities.UnexpectedValue(lineMappingEntry.State);
		}
	}

	protected override LineVisibility GetUnknownStateVisibility(int index)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Syntax/CSharpLineDirectiveMap.cs", 226);
	}

	internal override FileLinePositionSpan TranslateSpanAndVisibility(SourceText sourceText, string treeFilePath, TextSpan span, out bool isHiddenPosition)
	{
		TextLineCollection lines = sourceText.Lines;
		LinePosition linePosition = lines.GetLinePosition(span.Start);
		LinePosition linePosition2 = lines.GetLinePosition(span.End);
		if (Entries.Length == 1)
		{
			isHiddenPosition = false;
			return new FileLinePositionSpan(treeFilePath, linePosition, linePosition2);
		}
		LineDirectiveMap<DirectiveTriviaSyntax>.LineMappingEntry entry = FindEntry(linePosition.Line);
		isHiddenPosition = (uint)entry.State == 6u;
		return TranslateSpan(in entry, treeFilePath, linePosition, linePosition2);
	}
}
