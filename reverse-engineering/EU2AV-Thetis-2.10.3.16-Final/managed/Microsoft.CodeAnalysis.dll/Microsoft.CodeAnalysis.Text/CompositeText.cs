using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Text;

internal sealed class CompositeText : SourceText
{
	private sealed class CompositeTextLineInfo : TextLineCollection
	{
		private readonly CompositeText _compositeText;

		private readonly ImmutableArray<int> _segmentLineNumbers;

		private readonly int _lineCount;

		public override int Count => _lineCount;

		public override TextLine this[int lineNumber]
		{
			get
			{
				if (lineNumber < 0 || lineNumber >= _lineCount)
				{
					throw new ArgumentOutOfRangeException("lineNumber");
				}
				GetSegmentIndexRangeContainingLine(lineNumber, out var firstSegmentIndexInclusive, out var lastSegmentIndexInclusive);
				int num = _segmentLineNumbers[firstSegmentIndexInclusive];
				SourceText sourceText = _compositeText.Segments[firstSegmentIndexInclusive];
				int num2 = _compositeText._segmentOffsets[firstSegmentIndexInclusive];
				TextLine textLine = sourceText.Lines[lineNumber - num];
				int num3 = textLine.SpanIncludingLineBreak.Length;
				for (int i = firstSegmentIndexInclusive + 1; i < lastSegmentIndexInclusive; i++)
				{
					SourceText sourceText2 = _compositeText.Segments[i];
					num3 += sourceText2.Lines[0].SpanIncludingLineBreak.Length;
				}
				if (firstSegmentIndexInclusive != lastSegmentIndexInclusive)
				{
					SourceText sourceText3 = _compositeText.Segments[lastSegmentIndexInclusive];
					num3 += sourceText3.Lines[0].SpanIncludingLineBreak.Length;
				}
				return TextLine.FromSpanUnsafe(_compositeText, new TextSpan(num2 + textLine.Start, num3));
			}
		}

		public CompositeTextLineInfo(CompositeText compositeText)
		{
			int[] array = new int[compositeText.Segments.Length];
			int num = 0;
			for (int i = 0; i < compositeText.Segments.Length; i++)
			{
				array[i] = num;
				SourceText sourceText = compositeText.Segments[i];
				num += sourceText.Lines.Count - 1;
			}
			_compositeText = compositeText;
			_segmentLineNumbers = ImmutableCollectionsMarshal.AsImmutableArray(array);
			_lineCount = num + 1;
		}

		public override int IndexOf(int position)
		{
			if (position < 0 || position > _compositeText.Length)
			{
				throw new ArgumentOutOfRangeException("position");
			}
			_compositeText.GetIndexAndOffset(position, out var index, out var offset);
			int num = _compositeText.Segments[index].Lines.IndexOf(offset);
			return _segmentLineNumbers[index] + num;
		}

		private void GetSegmentIndexRangeContainingLine(int lineNumber, out int firstSegmentIndexInclusive, out int lastSegmentIndexInclusive)
		{
			int num = _segmentLineNumbers.BinarySearch(lineNumber);
			int num2 = (firstSegmentIndexInclusive = ((num >= 0) ? num : (~num - 1)));
			while (firstSegmentIndexInclusive > 0 && _segmentLineNumbers[firstSegmentIndexInclusive] == lineNumber)
			{
				SourceText sourceText = _compositeText.Segments[firstSegmentIndexInclusive - 1];
				if (TextUtilities.IsAnyLineBreakCharacter(sourceText[sourceText.Length - 1]))
				{
					break;
				}
				firstSegmentIndexInclusive--;
			}
			lastSegmentIndexInclusive = num2;
			while (lastSegmentIndexInclusive < _compositeText.Segments.Length - 1 && _segmentLineNumbers[lastSegmentIndexInclusive + 1] == lineNumber)
			{
				lastSegmentIndexInclusive++;
			}
		}
	}

	private readonly ImmutableArray<SourceText> _segments;

	private readonly int _length;

	private readonly int _storageSize;

	private readonly int[] _segmentOffsets;

	private readonly Encoding? _encoding;

	internal const int TARGET_SEGMENT_COUNT_AFTER_REDUCTION = 32;

	internal const int MAXIMUM_SEGMENT_COUNT_BEFORE_REDUCTION = 64;

	private const int INITIAL_SEGMENT_SIZE_FOR_COMBINING = 32;

	private const int MAXIMUM_SEGMENT_SIZE_FOR_COMBINING = 134217727;

	private static readonly ObjectPool<HashSet<SourceText>> s_uniqueSourcesPool = new ObjectPool<HashSet<SourceText>>(() => new HashSet<SourceText>(), 5);

	public override Encoding? Encoding => _encoding;

	public override int Length => _length;

	internal override int StorageSize => _storageSize;

	internal override ImmutableArray<SourceText> Segments => _segments;

	public override char this[int position]
	{
		get
		{
			GetIndexAndOffset(position, out var index, out var offset);
			return _segments[index][offset];
		}
	}

	private CompositeText(ImmutableArray<SourceText> segments, Encoding? encoding, SourceHashAlgorithm checksumAlgorithm)
		: base(default(ImmutableArray<byte>), checksumAlgorithm)
	{
		_segments = segments;
		_encoding = encoding;
		ComputeLengthAndStorageSize(segments, out _length, out _storageSize);
		_segmentOffsets = new int[segments.Length];
		int num = 0;
		for (int i = 0; i < _segmentOffsets.Length; i++)
		{
			_segmentOffsets[i] = num;
			num += _segments[i].Length;
		}
	}

	protected override TextLineCollection GetLinesCore()
	{
		return new CompositeTextLineInfo(this);
	}

	public override SourceText GetSubText(TextSpan span)
	{
		CheckSubSpan(span);
		int start = span.Start;
		int num = span.Length;
		GetIndexAndOffset(start, out var index, out var offset);
		ArrayBuilder<SourceText> instance = ArrayBuilder<SourceText>.GetInstance();
		try
		{
			while (index < _segments.Length && num > 0)
			{
				SourceText sourceText = _segments[index];
				int num2 = Math.Min(num, sourceText.Length - offset);
				AddSegments(instance, sourceText.GetSubText(new TextSpan(offset, num2)));
				num -= num2;
				index++;
				offset = 0;
			}
			return ToSourceText(instance, this, adjustSegments: false);
		}
		finally
		{
			instance.Free();
		}
	}

	private void GetIndexAndOffset(int position, out int index, out int offset)
	{
		int num = _segmentOffsets.BinarySearch(position);
		index = ((num >= 0) ? num : (~num - 1));
		offset = position - _segmentOffsets[index];
	}

	private bool CheckCopyToArguments(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (sourceIndex < 0)
		{
			throw new ArgumentOutOfRangeException("sourceIndex");
		}
		if (destinationIndex < 0)
		{
			throw new ArgumentOutOfRangeException("destinationIndex");
		}
		if (count < 0 || count > Length - sourceIndex || count > destination.Length - destinationIndex)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		return count > 0;
	}

	public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		if (CheckCopyToArguments(sourceIndex, destination, destinationIndex, count))
		{
			GetIndexAndOffset(sourceIndex, out var index, out var offset);
			while (index < _segments.Length && count > 0)
			{
				SourceText sourceText = _segments[index];
				int num = Math.Min(count, sourceText.Length - offset);
				sourceText.CopyTo(offset, destination, destinationIndex, num);
				count -= num;
				destinationIndex += num;
				index++;
				offset = 0;
			}
		}
	}

	internal static void AddSegments(ArrayBuilder<SourceText> segments, SourceText text)
	{
		if (!(text is CompositeText compositeText))
		{
			segments.Add(text);
		}
		else
		{
			segments.AddRange(compositeText._segments);
		}
	}

	internal static SourceText ToSourceText(ArrayBuilder<SourceText> segments, SourceText original, bool adjustSegments)
	{
		if (adjustSegments)
		{
			TrimInaccessibleText(segments);
			ReduceSegmentCountIfNecessary(segments);
		}
		RemoveSplitLineBreaksAndEmptySegments(segments);
		if (segments.Count == 0)
		{
			return SourceText.From(string.Empty, original.Encoding, original.ChecksumAlgorithm);
		}
		if (segments.Count == 1)
		{
			return segments[0];
		}
		return new CompositeText(segments.ToImmutable(), original.Encoding, original.ChecksumAlgorithm);
	}

	private static void RemoveSplitLineBreaksAndEmptySegments(ArrayBuilder<SourceText> segments)
	{
		if (segments.Count <= 1)
		{
			return;
		}
		segments.RemoveAll((SourceText s, int _, int _) => s.Length == 0, 0);
		bool flag = false;
		for (int num = 1; num < segments.Count; num++)
		{
			SourceText sourceText = segments[num - 1];
			SourceText sourceText2 = segments[num];
			if (sourceText.Length > 0)
			{
				if (sourceText[sourceText.Length - 1] == '\r' && sourceText2[0] == '\n')
				{
					flag = true;
					segments[num - 1] = sourceText.GetSubText(new TextSpan(0, sourceText.Length - 1));
					segments.Insert(num, SourceText.From("\r\n"));
					segments[num + 1] = sourceText2.GetSubText(new TextSpan(1, sourceText2.Length - 1));
					num++;
				}
			}
		}
		if (flag)
		{
			segments.RemoveAll((SourceText s, int _, int _) => s.Length == 0, 0);
		}
	}

	private static void ReduceSegmentCountIfNecessary(ArrayBuilder<SourceText> segments)
	{
		if (segments.Count > 64)
		{
			int minimalSegmentSizeToUseForCombining = GetMinimalSegmentSizeToUseForCombining(segments);
			CombineSegments(segments, minimalSegmentSizeToUseForCombining);
		}
	}

	private static int GetMinimalSegmentSizeToUseForCombining(ArrayBuilder<SourceText> segments)
	{
		for (int num = 32; num <= 134217727; num *= 2)
		{
			if (GetSegmentCountIfCombined(segments, num) <= 32)
			{
				return num;
			}
		}
		return 134217727;
	}

	private static int GetSegmentCountIfCombined(ArrayBuilder<SourceText> segments, int segmentSize)
	{
		int num = 0;
		for (int i = 0; i < segments.Count - 1; i++)
		{
			if (segments[i].Length <= segmentSize)
			{
				int num2 = 1;
				for (int j = i + 1; j < segments.Count && segments[j].Length <= segmentSize; j++)
				{
					num2++;
				}
				if (num2 > 1)
				{
					int num3 = num2 - 1;
					num += num3;
					i += num3;
				}
			}
		}
		return segments.Count - num;
	}

	private static void CombineSegments(ArrayBuilder<SourceText> segments, int segmentSize)
	{
		for (int i = 0; i < segments.Count - 1; i++)
		{
			if (segments[i].Length > segmentSize)
			{
				continue;
			}
			int num = segments[i].Length;
			int num2 = 1;
			for (int j = i + 1; j < segments.Count && segments[j].Length <= segmentSize; j++)
			{
				num2++;
				num += segments[j].Length;
			}
			if (num2 > 1)
			{
				Encoding? encoding = segments[i].Encoding;
				SourceHashAlgorithm checksumAlgorithm = segments[i].ChecksumAlgorithm;
				SourceTextWriter sourceTextWriter = SourceTextWriter.Create(encoding, checksumAlgorithm, num);
				for (int k = i; k < i + num2; k++)
				{
					segments[k].Write(sourceTextWriter);
				}
				SourceText item = sourceTextWriter.ToSourceText();
				segments.RemoveRange(i, num2);
				segments.Insert(i, item);
			}
		}
	}

	private static void ComputeLengthAndStorageSize(IReadOnlyList<SourceText> segments, out int length, out int size)
	{
		HashSet<SourceText> hashSet = s_uniqueSourcesPool.Allocate();
		length = 0;
		for (int i = 0; i < segments.Count; i++)
		{
			SourceText sourceText = segments[i];
			length += sourceText.Length;
			hashSet.Add(sourceText.StorageKey);
		}
		size = 0;
		foreach (SourceText item in hashSet)
		{
			size += item.StorageSize;
		}
		hashSet.Clear();
		s_uniqueSourcesPool.Free(hashSet);
	}

	private static void TrimInaccessibleText(ArrayBuilder<SourceText> segments)
	{
		ComputeLengthAndStorageSize(segments, out var length, out var size);
		if (length < size / 2)
		{
			Encoding? encoding = segments[0].Encoding;
			SourceHashAlgorithm checksumAlgorithm = segments[0].ChecksumAlgorithm;
			SourceTextWriter sourceTextWriter = SourceTextWriter.Create(encoding, checksumAlgorithm, length);
			foreach (SourceText segment in segments)
			{
				segment.Write(sourceTextWriter);
			}
			segments.Clear();
			segments.Add(sourceTextWriter.ToSourceText());
		}
	}
}
