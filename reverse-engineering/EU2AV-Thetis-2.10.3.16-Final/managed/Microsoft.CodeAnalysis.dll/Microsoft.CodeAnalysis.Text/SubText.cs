using System;
using System.Collections.Immutable;
using System.Text;

namespace Microsoft.CodeAnalysis.Text;

internal sealed class SubText : SourceText
{
	private sealed class SubTextLineInfo : TextLineCollection
	{
		private readonly SubText _subText;

		private readonly int _startLineNumberInUnderlyingText;

		private readonly int _lineCount;

		private readonly bool _endsWithinSplitCRLF;

		public override TextLine this[int lineNumber]
		{
			get
			{
				if (lineNumber < 0 || lineNumber >= _lineCount)
				{
					throw new ArgumentOutOfRangeException("lineNumber");
				}
				if (_endsWithinSplitCRLF && lineNumber == _lineCount - 1)
				{
					return TextLine.FromSpanUnsafe(_subText, new TextSpan(_subText.UnderlyingSpan.Length, 0));
				}
				TextLine textLine = _subText.UnderlyingText.Lines[lineNumber + _startLineNumberInUnderlyingText];
				int num = Math.Max(textLine.Start, _subText.UnderlyingSpan.Start);
				int num2 = Math.Min(textLine.EndIncludingLineBreak, _subText.UnderlyingSpan.End);
				int start = num - _subText.UnderlyingSpan.Start;
				int length = num2 - num;
				TextLine result = TextLine.FromSpanUnsafe(_subText, new TextSpan(start, length));
				bool num3 = lineNumber != _lineCount - 1;
				bool flag = result.EndIncludingLineBreak > result.End;
				if (num3 != flag)
				{
					throw new InvalidOperationException();
				}
				return result;
			}
		}

		public override int Count => _lineCount;

		public SubTextLineInfo(SubText subText)
		{
			_subText = subText;
			TextLine lineFromPosition = _subText.UnderlyingText.Lines.GetLineFromPosition(_subText.UnderlyingSpan.Start);
			TextLine lineFromPosition2 = _subText.UnderlyingText.Lines.GetLineFromPosition(_subText.UnderlyingSpan.End);
			_startLineNumberInUnderlyingText = lineFromPosition.LineNumber;
			_lineCount = lineFromPosition2.LineNumber - _startLineNumberInUnderlyingText + 1;
			int end = _subText.UnderlyingSpan.End;
			if (end == lineFromPosition2.End + 1 && end == lineFromPosition2.EndIncludingLineBreak - 1)
			{
				_endsWithinSplitCRLF = true;
				_lineCount++;
			}
		}

		public override int IndexOf(int position)
		{
			if (position < 0 || position > _subText.UnderlyingSpan.Length)
			{
				throw new ArgumentOutOfRangeException("position");
			}
			int position2 = position + _subText.UnderlyingSpan.Start;
			return _subText.UnderlyingText.Lines.IndexOf(position2) - _startLineNumberInUnderlyingText;
		}
	}

	public override Encoding? Encoding => UnderlyingText.Encoding;

	public SourceText UnderlyingText { get; }

	public TextSpan UnderlyingSpan { get; }

	public override int Length => UnderlyingSpan.Length;

	internal override int StorageSize => UnderlyingText.StorageSize;

	internal override SourceText StorageKey => UnderlyingText.StorageKey;

	public override char this[int position]
	{
		get
		{
			if (position < 0 || position > Length)
			{
				throw new ArgumentOutOfRangeException("position");
			}
			return UnderlyingText[UnderlyingSpan.Start + position];
		}
	}

	public SubText(SourceText text, TextSpan span)
		: base(default(ImmutableArray<byte>), text.ChecksumAlgorithm)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (span.Start < 0 || span.End > text.Length)
		{
			throw new ArgumentOutOfRangeException("span");
		}
		UnderlyingText = text;
		UnderlyingSpan = span;
	}

	protected override TextLineCollection GetLinesCore()
	{
		return new SubTextLineInfo(this);
	}

	public override string ToString(TextSpan span)
	{
		CheckSubSpan(span);
		return UnderlyingText.ToString(GetCompositeSpan(span.Start, span.Length));
	}

	public override SourceText GetSubText(TextSpan span)
	{
		CheckSubSpan(span);
		return new SubText(UnderlyingText, GetCompositeSpan(span.Start, span.Length));
	}

	public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		TextSpan compositeSpan = GetCompositeSpan(sourceIndex, count);
		UnderlyingText.CopyTo(compositeSpan.Start, destination, destinationIndex, compositeSpan.Length);
	}

	private TextSpan GetCompositeSpan(int start, int length)
	{
		int num = Math.Min(UnderlyingText.Length, UnderlyingSpan.Start + start);
		int num2 = Math.Min(UnderlyingText.Length, num + length);
		return new TextSpan(num, num2 - num);
	}
}
