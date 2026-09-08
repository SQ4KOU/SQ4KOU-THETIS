using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;

namespace Roslyn.Utilities;

internal static class TextChangeRangeExtensions
{
	private readonly struct UnadjustedNewChange
	{
		public int SpanStart { get; }

		public int SpanLength { get; }

		public int NewLength { get; }

		public int SpanEnd => SpanStart + SpanLength;

		public UnadjustedNewChange(int spanStart, int spanLength, int newLength)
		{
			SpanStart = spanStart;
			SpanLength = spanLength;
			NewLength = newLength;
		}

		public UnadjustedNewChange(TextChangeRange range)
			: this(range.Span.Start, range.Span.Length, range.NewLength)
		{
		}
	}

	public static TextChangeRange? Accumulate(this TextChangeRange? accumulatedTextChangeSoFar, IReadOnlyList<TextChangeRange> changesInNextVersion)
	{
		if (changesInNextVersion.Count == 0)
		{
			return accumulatedTextChangeSoFar;
		}
		TextChangeRange value = ((changesInNextVersion.Count == 1) ? changesInNextVersion[0] : TextChangeRange.Collapse(changesInNextVersion));
		if (!accumulatedTextChangeSoFar.HasValue)
		{
			return value;
		}
		int start = accumulatedTextChangeSoFar.Value.Span.Start;
		int num = accumulatedTextChangeSoFar.Value.Span.End;
		int num2 = accumulatedTextChangeSoFar.Value.Span.Start + accumulatedTextChangeSoFar.Value.NewLength;
		if (value.Span.Start < start)
		{
			start = value.Span.Start;
		}
		if (num2 > value.Span.End)
		{
			num2 = num2 + value.NewLength - value.Span.Length;
		}
		else
		{
			num = num + value.Span.End - num2;
			num2 = value.Span.Start + value.NewLength;
		}
		return new TextChangeRange(TextSpan.FromBounds(start, num), num2 - start);
	}

	public static TextChangeRange ToTextChangeRange(this TextChange textChange)
	{
		return new TextChangeRange(textChange.Span, textChange.NewText?.Length ?? 0);
	}

	public static ImmutableArray<TextChangeRange> Merge(ImmutableArray<TextChangeRange> oldChanges, ImmutableArray<TextChangeRange> newChanges)
	{
		if (oldChanges.IsEmpty)
		{
			throw new ArgumentException("oldChanges");
		}
		if (newChanges.IsEmpty)
		{
			throw new ArgumentException("newChanges");
		}
		ArrayBuilder<TextChangeRange> instance = ArrayBuilder<TextChangeRange>.GetInstance();
		TextChangeRange oldChange = oldChanges[0];
		UnadjustedNewChange newChange = new UnadjustedNewChange(newChanges[0]);
		int oldIndex = 0;
		int newIndex = 0;
		int oldDelta = 0;
		while (true)
		{
			if (oldChange.Span.Length == 0 && oldChange.NewLength == 0)
			{
				if (!tryGetNextOldChange())
				{
					break;
				}
			}
			else if (newChange.SpanLength == 0 && newChange.NewLength == 0)
			{
				if (!tryGetNextNewChange())
				{
					break;
				}
			}
			else if (newChange.SpanEnd <= oldChange.Span.Start + oldDelta)
			{
				adjustAndAddNewChange(instance, oldDelta, newChange);
				if (!tryGetNextNewChange())
				{
					break;
				}
			}
			else if (newChange.SpanStart >= oldChange.NewEnd() + oldDelta)
			{
				addAndAdjustOldDelta(instance, ref oldDelta, oldChange);
				if (!tryGetNextOldChange())
				{
					break;
				}
			}
			else if (newChange.SpanStart < oldChange.Span.Start + oldDelta)
			{
				int num = oldChange.Span.Start + oldDelta - newChange.SpanStart;
				adjustAndAddNewChange(instance, oldDelta, new UnadjustedNewChange(newChange.SpanStart, num, 0));
				newChange = new UnadjustedNewChange(oldChange.Span.Start + oldDelta, newChange.SpanLength - num, newChange.NewLength);
			}
			else if (newChange.SpanStart > oldChange.Span.Start + oldDelta)
			{
				int num2 = newChange.SpanStart - (oldChange.Span.Start + oldDelta);
				int num3 = Math.Min(oldChange.Span.Length, num2);
				addAndAdjustOldDelta(instance, ref oldDelta, new TextChangeRange(new TextSpan(oldChange.Span.Start, num3), num2));
				oldChange = new TextChangeRange(new TextSpan(newChange.SpanStart - oldDelta, oldChange.Span.Length - num3), oldChange.NewLength - num2);
			}
			else if (newChange.SpanLength <= oldChange.NewLength)
			{
				oldChange = new TextChangeRange(oldChange.Span, oldChange.NewLength - newChange.SpanLength);
				oldDelta += newChange.SpanLength;
				newChange = new UnadjustedNewChange(newChange.SpanEnd, 0, newChange.NewLength);
				adjustAndAddNewChange(instance, oldDelta, newChange);
				if (!tryGetNextNewChange())
				{
					break;
				}
			}
			else
			{
				oldDelta = oldDelta - oldChange.Span.Length + oldChange.NewLength;
				int spanLength = newChange.SpanLength + oldChange.Span.Length - oldChange.NewLength;
				newChange = new UnadjustedNewChange(oldChange.Span.Start + oldDelta, spanLength, newChange.NewLength);
				if (!tryGetNextOldChange())
				{
					break;
				}
			}
		}
		bool num4 = oldIndex == oldChanges.Length;
		bool flag = newIndex == newChanges.Length;
		if (num4)
		{
			if (flag)
			{
				goto IL_044b;
			}
		}
		else if (!flag)
		{
			goto IL_044b;
		}
		while (oldIndex < oldChanges.Length)
		{
			addAndAdjustOldDelta(instance, ref oldDelta, oldChange);
			tryGetNextOldChange();
		}
		while (newIndex < newChanges.Length)
		{
			adjustAndAddNewChange(instance, oldDelta, newChange);
			tryGetNextNewChange();
		}
		return instance.ToImmutableAndFree();
		IL_044b:
		throw new InvalidOperationException();
		static void add(ArrayBuilder<TextChangeRange> builder, TextChangeRange change)
		{
			if (builder.Count > 0)
			{
				TextChangeRange textChangeRange = builder[builder.Count - 1];
				if (textChangeRange.Span.End == change.Span.Start)
				{
					builder[builder.Count - 1] = new TextChangeRange(new TextSpan(textChangeRange.Span.Start, textChangeRange.Span.Length + change.Span.Length), textChangeRange.NewLength + change.NewLength);
					return;
				}
				if (textChangeRange.Span.End > change.Span.Start)
				{
					throw new ArgumentOutOfRangeException("change");
				}
			}
			builder.Add(change);
		}
		static void addAndAdjustOldDelta(ArrayBuilder<TextChangeRange> builder, ref int reference, TextChangeRange change)
		{
			reference = reference - change.Span.Length + change.NewLength;
			add(builder, change);
		}
		static void adjustAndAddNewChange(ArrayBuilder<TextChangeRange> builder, int num5, UnadjustedNewChange unadjustedNewChange)
		{
			add(builder, new TextChangeRange(new TextSpan(unadjustedNewChange.SpanStart - num5, unadjustedNewChange.SpanLength), unadjustedNewChange.NewLength));
		}
		bool tryGetNextNewChange()
		{
			newIndex++;
			if (newIndex < newChanges.Length)
			{
				newChange = new UnadjustedNewChange(newChanges[newIndex]);
				return true;
			}
			newChange = default(UnadjustedNewChange);
			return false;
		}
		bool tryGetNextOldChange()
		{
			oldIndex++;
			if (oldIndex < oldChanges.Length)
			{
				oldChange = oldChanges[oldIndex];
				return true;
			}
			oldChange = default(TextChangeRange);
			return false;
		}
	}

	private static int NewEnd(this TextChangeRange range)
	{
		return range.Span.Start + range.NewLength;
	}
}
