using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Text;

internal sealed class ChangedText : SourceText
{
	private class ChangeInfo
	{
		public ImmutableArray<TextChangeRange> ChangeRanges { get; }

		public WeakReference<SourceText> WeakOldText { get; }

		public ChangeInfo? Previous { get; private set; }

		public ChangeInfo(ImmutableArray<TextChangeRange> changeRanges, WeakReference<SourceText> weakOldText, ChangeInfo? previous)
		{
			ChangeRanges = changeRanges;
			WeakOldText = weakOldText;
			Previous = previous;
			Clean();
		}

		private void Clean()
		{
			ChangeInfo changeInfo = this;
			for (ChangeInfo changeInfo2 = this; changeInfo2 != null; changeInfo2 = changeInfo2.Previous)
			{
				if (changeInfo2.WeakOldText.TryGetTarget(out SourceText _))
				{
					changeInfo = changeInfo2;
				}
			}
			while (changeInfo != null)
			{
				ChangeInfo previous = changeInfo.Previous;
				changeInfo.Previous = null;
				changeInfo = previous;
			}
		}
	}

	internal static class TestAccessor
	{
		public static ImmutableArray<TextChangeRange> Merge(ImmutableArray<TextChangeRange> oldChanges, ImmutableArray<TextChangeRange> newChanges)
		{
			return TextChangeRangeExtensions.Merge(oldChanges, newChanges);
		}
	}

	private readonly SourceText _newText;

	private readonly ChangeInfo _info;

	public override Encoding? Encoding => _newText.Encoding;

	public IEnumerable<TextChangeRange> Changes => _info.ChangeRanges;

	public override int Length => _newText.Length;

	internal override int StorageSize => _newText.StorageSize;

	internal override ImmutableArray<SourceText> Segments => _newText.Segments;

	internal override SourceText StorageKey => _newText.StorageKey;

	public override char this[int position] => _newText[position];

	public ChangedText(SourceText oldText, SourceText newText, ImmutableArray<TextChangeRange> changeRanges)
		: base(default(ImmutableArray<byte>), oldText.ChecksumAlgorithm)
	{
		RequiresChangeRangesAreValid(oldText, newText, changeRanges);
		_newText = newText;
		_info = new ChangeInfo(changeRanges, new WeakReference<SourceText>(oldText), (oldText as ChangedText)?._info);
	}

	private static void RequiresChangeRangesAreValid(SourceText oldText, SourceText newText, ImmutableArray<TextChangeRange> changeRanges)
	{
		int num = 0;
		foreach (TextChangeRange item in changeRanges)
		{
			num += item.NewLength - item.Span.Length;
		}
		if (oldText.Length + num != newText.Length)
		{
			throw new InvalidOperationException("Delta length difference of change ranges didn't match before/after text length.");
		}
		int num2 = 0;
		foreach (TextChangeRange item2 in changeRanges)
		{
			if (item2.Span.Start < num2)
			{
				throw new InvalidOperationException("Change preceded current position in oldText");
			}
			if (item2.Span.Start > oldText.Length)
			{
				throw new InvalidOperationException("Change start was after the end of oldText");
			}
			if (item2.Span.End > oldText.Length)
			{
				throw new InvalidOperationException("Change end was after the end of oldText");
			}
			num2 = item2.Span.End;
		}
	}

	public override string ToString(TextSpan span)
	{
		return _newText.ToString(span);
	}

	public override SourceText GetSubText(TextSpan span)
	{
		return _newText.GetSubText(span);
	}

	public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		_newText.CopyTo(sourceIndex, destination, destinationIndex, count);
	}

	public override SourceText WithChanges(IEnumerable<TextChange> changes)
	{
		if (_newText.WithChanges(changes) is ChangedText changedText)
		{
			return new ChangedText(this, changedText._newText, changedText._info.ChangeRanges);
		}
		return this;
	}

	public override IReadOnlyList<TextChangeRange> GetChangeRanges(SourceText oldText)
	{
		if (oldText == null)
		{
			throw new ArgumentNullException("oldText");
		}
		if (this == oldText)
		{
			return TextChangeRange.NoChanges;
		}
		if (_info.WeakOldText.TryGetTarget(out SourceText target) && target == oldText)
		{
			return _info.ChangeRanges;
		}
		if (IsChangedFrom(oldText))
		{
			IReadOnlyList<ImmutableArray<TextChangeRange>> changesBetween = GetChangesBetween(oldText, this);
			if (changesBetween.Count > 1)
			{
				return Merge(changesBetween);
			}
		}
		if (target != null && target.GetChangeRanges(oldText).Count == 0)
		{
			return _info.ChangeRanges;
		}
		return ImmutableArray.Create(new TextChangeRange(new TextSpan(0, oldText.Length), _newText.Length));
	}

	private bool IsChangedFrom(SourceText oldText)
	{
		for (ChangeInfo changeInfo = _info; changeInfo != null; changeInfo = changeInfo.Previous)
		{
			if (changeInfo.WeakOldText.TryGetTarget(out SourceText target) && target == oldText)
			{
				return true;
			}
		}
		return false;
	}

	private static IReadOnlyList<ImmutableArray<TextChangeRange>> GetChangesBetween(SourceText oldText, ChangedText newText)
	{
		List<ImmutableArray<TextChangeRange>> list = new List<ImmutableArray<TextChangeRange>>();
		ChangeInfo changeInfo = newText._info;
		list.Add(changeInfo.ChangeRanges);
		while (changeInfo != null)
		{
			changeInfo.WeakOldText.TryGetTarget(out SourceText target);
			if (target == oldText)
			{
				return list;
			}
			changeInfo = changeInfo.Previous;
			if (changeInfo != null)
			{
				list.Insert(0, changeInfo.ChangeRanges);
			}
		}
		list.Clear();
		return list;
	}

	private static ImmutableArray<TextChangeRange> Merge(IReadOnlyList<ImmutableArray<TextChangeRange>> changeSets)
	{
		ImmutableArray<TextChangeRange> immutableArray = changeSets[0];
		for (int i = 1; i < changeSets.Count; i++)
		{
			immutableArray = TextChangeRangeExtensions.Merge(immutableArray, changeSets[i]);
		}
		return immutableArray;
	}

	protected override TextLineCollection GetLinesCore()
	{
		return _newText.Lines;
	}
}
