using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis;

internal abstract class LineDirectiveMap<TDirective> where TDirective : SyntaxNode
{
	public enum PositionState : byte
	{
		Unknown,
		Unmapped,
		Remapped,
		RemappedSpan,
		RemappedAfterUnknown,
		RemappedAfterHidden,
		Hidden
	}

	internal readonly struct LineMappingEntry : IComparable<LineMappingEntry>
	{
		public readonly int UnmappedLine;

		public readonly int MappedLine;

		public readonly LinePositionSpan MappedSpan;

		public readonly int? UnmappedCharacterOffset;

		public readonly string? MappedPathOpt;

		public readonly PositionState State;

		public bool IsHidden => State == PositionState.Hidden;

		public LineMappingEntry(int unmappedLine)
		{
			UnmappedLine = unmappedLine;
			MappedLine = unmappedLine;
			MappedSpan = default(LinePositionSpan);
			UnmappedCharacterOffset = null;
			MappedPathOpt = null;
			State = PositionState.Unmapped;
		}

		public LineMappingEntry(int unmappedLine, int mappedLine, string? mappedPathOpt, PositionState state)
		{
			UnmappedLine = unmappedLine;
			MappedLine = mappedLine;
			MappedSpan = default(LinePositionSpan);
			UnmappedCharacterOffset = null;
			MappedPathOpt = mappedPathOpt;
			State = state;
		}

		public LineMappingEntry(int unmappedLine, LinePositionSpan mappedSpan, int? unmappedCharacterOffset, string? mappedPathOpt)
		{
			UnmappedLine = unmappedLine;
			MappedLine = -1;
			MappedSpan = mappedSpan;
			UnmappedCharacterOffset = unmappedCharacterOffset;
			MappedPathOpt = mappedPathOpt;
			State = PositionState.RemappedSpan;
		}

		public int CompareTo(LineMappingEntry other)
		{
			int unmappedLine = UnmappedLine;
			return unmappedLine.CompareTo(other.UnmappedLine);
		}
	}

	internal readonly ImmutableArray<LineMappingEntry> Entries;

	protected abstract bool ShouldAddDirective(TDirective directive);

	protected abstract LineMappingEntry GetEntry(TDirective directive, SourceText sourceText, LineMappingEntry previous);

	protected abstract LineMappingEntry InitializeFirstEntry();

	protected LineDirectiveMap(SyntaxTree syntaxTree)
	{
		IList<TDirective> directives = ((SyntaxNodeOrToken)syntaxTree.GetRoot()).GetDirectives<TDirective>(ShouldAddDirective);
		Entries = CreateEntryMap(syntaxTree, directives);
	}

	public FileLinePositionSpan TranslateSpan(SourceText sourceText, string treeFilePath, TextSpan span)
	{
		LinePosition linePosition = sourceText.Lines.GetLinePosition(span.Start);
		LinePosition linePosition2 = sourceText.Lines.GetLinePosition(span.End);
		return TranslateSpan(FindEntry(linePosition.Line), treeFilePath, linePosition, linePosition2);
	}

	protected FileLinePositionSpan TranslateSpan(in LineMappingEntry entry, string treeFilePath, LinePosition unmappedStartPos, LinePosition unmappedEndPos)
	{
		string? path = entry.MappedPathOpt ?? treeFilePath;
		LinePositionSpan span = ((entry.State == PositionState.RemappedSpan) ? TranslateEnhancedLineDirectiveSpan(in entry, unmappedStartPos, unmappedEndPos) : TranslateLineDirectiveSpan(in entry, unmappedStartPos, unmappedEndPos));
		return new FileLinePositionSpan(path, span, entry.MappedPathOpt != null);
	}

	private static LinePositionSpan TranslateLineDirectiveSpan(in LineMappingEntry entry, LinePosition unmappedStartPos, LinePosition unmappedEndPos)
	{
		return new LinePositionSpan(translatePosition(in entry, unmappedStartPos), translatePosition(in entry, unmappedEndPos));
		static LinePosition translatePosition(in LineMappingEntry reference, LinePosition unmapped)
		{
			int num = unmapped.Line - reference.UnmappedLine + reference.MappedLine;
			if (num != -1)
			{
				return new LinePosition(num, unmapped.Character);
			}
			return new LinePosition(unmapped.Character);
		}
	}

	private static LinePositionSpan TranslateEnhancedLineDirectiveSpan(in LineMappingEntry entry, LinePosition unmappedStartPos, LinePosition unmappedEndPos)
	{
		if (unmappedStartPos.Line == entry.UnmappedLine && unmappedStartPos.Character < entry.UnmappedCharacterOffset.GetValueOrDefault())
		{
			return entry.MappedSpan;
		}
		return new LinePositionSpan(translatePosition(in entry, unmappedStartPos), translatePosition(in entry, unmappedEndPos));
		static LinePosition translatePosition(in LineMappingEntry reference, LinePosition unmapped)
		{
			return new LinePosition(unmapped.Line - reference.UnmappedLine + reference.MappedSpan.Start.Line, (unmapped.Line == reference.UnmappedLine) ? (reference.MappedSpan.Start.Character + unmapped.Character - reference.UnmappedCharacterOffset.GetValueOrDefault()) : unmapped.Character);
		}
	}

	public abstract LineVisibility GetLineVisibility(SourceText sourceText, int position);

	internal abstract FileLinePositionSpan TranslateSpanAndVisibility(SourceText sourceText, string treeFilePath, TextSpan span, out bool isHiddenPosition);

	public bool HasAnyHiddenRegions()
	{
		return Entries.Any((LineMappingEntry e) => e.State == PositionState.Hidden);
	}

	protected LineMappingEntry FindEntry(int lineNumber)
	{
		int index = FindEntryIndex(lineNumber);
		return Entries[index];
	}

	protected int FindEntryIndex(int lineNumber)
	{
		int num = Entries.BinarySearch(new LineMappingEntry(lineNumber));
		if (num < 0)
		{
			return ~num - 1;
		}
		return num;
	}

	private ImmutableArray<LineMappingEntry> CreateEntryMap(SyntaxTree tree, IList<TDirective> directives)
	{
		ArrayBuilder<LineMappingEntry> instance = ArrayBuilder<LineMappingEntry>.GetInstance(directives.Count + 1);
		LineMappingEntry lineMappingEntry = InitializeFirstEntry();
		instance.Add(lineMappingEntry);
		if (directives.Count > 0)
		{
			SourceText text = tree.GetText();
			foreach (TDirective directive in directives)
			{
				lineMappingEntry = GetEntry(directive, text, lineMappingEntry);
				instance.Add(lineMappingEntry);
			}
		}
		return instance.ToImmutableAndFree();
	}

	protected abstract LineVisibility GetUnknownStateVisibility(int index);

	public IEnumerable<LineMapping> GetLineMappings(TextLineCollection lines)
	{
		LineMappingEntry entry = Entries[0];
		for (int i = 1; i < Entries.Length; i++)
		{
			LineMappingEntry next = Entries[i];
			int num = next.UnmappedLine - 2;
			if (num >= entry.UnmappedLine)
			{
				TextLine textLine = lines[num];
				int lineLength = textLine.EndIncludingLineBreak - textLine.Start;
				yield return CreateLineMapping(in entry, num, lineLength, i - 1);
			}
			entry = next;
		}
		TextLine textLine2 = lines[lines.Count - 1];
		if (entry.UnmappedLine <= textLine2.LineNumber)
		{
			int lineLength2 = textLine2.EndIncludingLineBreak - textLine2.Start;
			int lineNumber = textLine2.LineNumber;
			yield return CreateLineMapping(in entry, lineNumber, lineLength2, Entries.Length - 1);
		}
	}

	private LineMapping CreateLineMapping(in LineMappingEntry entry, int unmappedEndLine, int lineLength, int currentIndex)
	{
		LinePositionSpan span = new LinePositionSpan(new LinePosition(entry.UnmappedLine, 0), new LinePosition(unmappedEndLine, lineLength));
		if (entry.State == PositionState.Hidden || (entry.State == PositionState.Unknown && GetUnknownStateVisibility(currentIndex) == LineVisibility.Hidden))
		{
			return new LineMapping(span, null, default(FileLinePositionSpan));
		}
		string path = entry.MappedPathOpt ?? string.Empty;
		bool hasMappedPath = entry.MappedPathOpt != null;
		if (entry.State == PositionState.RemappedSpan)
		{
			return new LineMapping(span, entry.UnmappedCharacterOffset, new FileLinePositionSpan(path, entry.MappedSpan, hasMappedPath));
		}
		LinePositionSpan span2 = new LinePositionSpan(new LinePosition(entry.MappedLine, 0), new LinePosition(entry.MappedLine + unmappedEndLine - entry.UnmappedLine, lineLength));
		FileLinePositionSpan mappedSpan = new FileLinePositionSpan(path, span2, hasMappedPath);
		return new LineMapping(span, null, mappedSpan);
	}
}
