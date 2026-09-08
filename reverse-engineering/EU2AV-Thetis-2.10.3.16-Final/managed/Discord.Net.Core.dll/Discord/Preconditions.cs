using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord;

internal static class Preconditions
{
	public static void NotNull<T>(T obj, string name, string msg = null) where T : class
	{
		if (obj == null)
		{
			throw CreateNotNullException(name, msg);
		}
	}

	public static void NotNull<T>(Optional<T> obj, string name, string msg = null) where T : class
	{
		if (obj.IsSpecified && obj.Value == null)
		{
			throw CreateNotNullException(name, msg);
		}
	}

	private static ArgumentNullException CreateNotNullException(string name, string msg)
	{
		if (msg == null)
		{
			return new ArgumentNullException(name);
		}
		return new ArgumentNullException(name, msg);
	}

	public static void NotEmpty(string obj, string name, string msg = null)
	{
		if (obj.Length == 0)
		{
			throw CreateNotEmptyException(name, msg);
		}
	}

	public static void NotEmpty(Optional<string> obj, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value.Length == 0)
		{
			throw CreateNotEmptyException(name, msg);
		}
	}

	public static void NotNullOrEmpty(string obj, string name, string msg = null)
	{
		if (obj == null)
		{
			throw CreateNotNullException(name, msg);
		}
		if (obj.Length == 0)
		{
			throw CreateNotEmptyException(name, msg);
		}
	}

	public static void NotNullOrEmpty(Optional<string> obj, string name, string msg = null)
	{
		if (obj.IsSpecified)
		{
			if (obj.Value == null)
			{
				throw CreateNotNullException(name, msg);
			}
			if (obj.Value.Length == 0)
			{
				throw CreateNotEmptyException(name, msg);
			}
		}
	}

	public static void NotNullOrWhitespace(string obj, string name, string msg = null)
	{
		if (obj == null)
		{
			throw CreateNotNullException(name, msg);
		}
		if (obj.Trim().Length == 0)
		{
			throw CreateNotEmptyException(name, msg);
		}
	}

	public static void NotNullOrWhitespace(Optional<string> obj, string name, string msg = null)
	{
		if (obj.IsSpecified)
		{
			if (obj.Value == null)
			{
				throw CreateNotNullException(name, msg);
			}
			if (obj.Value.Trim().Length == 0)
			{
				throw CreateNotEmptyException(name, msg);
			}
		}
	}

	private static ArgumentException CreateNotEmptyException(string name, string msg)
	{
		return new ArgumentException(msg ?? "Argument cannot be blank.", name);
	}

	public static void WebhookMessageAtLeastOneOf(string text = null, MessageComponent components = null, ICollection<IEmbed> embeds = null, IEnumerable<FileAttachment> attachments = null, PollProperties poll = null)
	{
		if (!string.IsNullOrEmpty(text) || (components != null && components.Components.Count != 0) || (attachments != null && attachments.Count() != 0) || (embeds != null && embeds.Count != 0) || poll != null)
		{
			return;
		}
		throw new ArgumentException("At least one of 'Content', 'Embeds', 'Components', 'Attachments' or 'Poll' must be specified.");
	}

	public static void MessageAtLeastOneOf(string text = null, MessageComponent components = null, ICollection<IEmbed> embeds = null, ICollection<ISticker> stickers = null, IEnumerable<FileAttachment> attachments = null, PollProperties poll = null, MessageReference messageReference = null)
	{
		if (string.IsNullOrEmpty(text) && (components == null || components.Components.Count == 0) && (stickers == null || stickers.Count == 0) && (attachments == null || attachments.Count() == 0) && (embeds == null || embeds.Count == 0) && poll == null)
		{
			MessageReferenceType? messageReferenceType = messageReference?.ReferenceType.GetValueOrDefault(MessageReferenceType.Default);
			if (!messageReferenceType.HasValue || messageReferenceType != MessageReferenceType.Forward)
			{
				throw new ArgumentException("At least one of 'Content', 'Embeds', 'Components', 'Stickers', 'Attachments' or 'Poll' must be specified.");
			}
		}
	}

	public static void ValidatePoll(PollProperties poll)
	{
		if (poll != null)
		{
			int count = poll.Answers.Count;
			if ((count < 1 || count > 10) ? true : false)
			{
				throw new ArgumentOutOfRangeException("Answers", "Poll answers must be between 1 and 10.");
			}
			if (poll.Answers.Any((PollMediaProperties x) => x.Text.Length > 55))
			{
				throw new ArgumentOutOfRangeException("Answers", $"Poll answer text must be less than or equal to {55} characters.");
			}
			if (poll.Answers.All((PollMediaProperties x) => string.IsNullOrWhiteSpace(x.Text) && x.Emoji == null))
			{
				throw new ArgumentException("Poll answers must have at least one of text or emoji.", "Answers");
			}
			if (poll.Question == null)
			{
				throw new ArgumentNullException("Question", "Poll question must not be null.");
			}
			if (poll.Question.Text.Length > 300)
			{
				throw new ArgumentOutOfRangeException("Question", $"Poll question text must be less than or equal to {300} characters.");
			}
			if (string.IsNullOrWhiteSpace(poll.Question.Text) && poll.Question.Emoji == null)
			{
				throw new ArgumentException("Poll question must have at least one of text or emoji.", "Question");
			}
			uint duration = poll.Duration;
			if ((duration > 768 || duration == 0) ? true : false)
			{
				throw new ArgumentOutOfRangeException("Duration", "Poll duration must be between 1 and 768 hours.");
			}
		}
	}

	public static void NotEqual(sbyte obj, sbyte value, string name, string msg = null)
	{
		if (obj == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(byte obj, byte value, string name, string msg = null)
	{
		if (obj == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(short obj, short value, string name, string msg = null)
	{
		if (obj == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(ushort obj, ushort value, string name, string msg = null)
	{
		if (obj == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(int obj, int value, string name, string msg = null)
	{
		if (obj == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(uint obj, uint value, string name, string msg = null)
	{
		if (obj == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(long obj, long value, string name, string msg = null)
	{
		if (obj == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(ulong obj, ulong value, string name, string msg = null)
	{
		if (obj == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(Optional<sbyte> obj, sbyte value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(Optional<byte> obj, byte value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(Optional<short> obj, short value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(Optional<ushort> obj, ushort value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(Optional<int> obj, int value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(Optional<uint> obj, uint value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(Optional<long> obj, long value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(Optional<ulong> obj, ulong value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(sbyte? obj, sbyte value, string name, string msg = null)
	{
		if (obj == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(byte? obj, byte value, string name, string msg = null)
	{
		if (obj == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(short? obj, short value, string name, string msg = null)
	{
		if (obj == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(ushort? obj, ushort value, string name, string msg = null)
	{
		if (obj == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(int? obj, int value, string name, string msg = null)
	{
		if (obj == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(uint? obj, uint value, string name, string msg = null)
	{
		if (obj == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(long? obj, long value, string name, string msg = null)
	{
		if (obj == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(ulong? obj, ulong value, string name, string msg = null)
	{
		if (obj == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(Optional<sbyte?> obj, sbyte value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(Optional<byte?> obj, byte value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(Optional<short?> obj, short value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(Optional<ushort?> obj, ushort value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(Optional<int?> obj, int value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(Optional<uint?> obj, uint value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(Optional<long?> obj, long value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	public static void NotEqual(Optional<ulong?> obj, ulong value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value == value)
		{
			throw CreateNotEqualException(name, msg, value);
		}
	}

	private static ArgumentException CreateNotEqualException<T>(string name, string msg, T value)
	{
		return new ArgumentException(msg ?? $"Value may not be equal to {value}.", name);
	}

	public static void AtLeast(sbyte obj, sbyte value, string name, string msg = null)
	{
		if (obj < value)
		{
			throw CreateAtLeastException(name, msg, value);
		}
	}

	public static void AtLeast(byte obj, byte value, string name, string msg = null)
	{
		if (obj < value)
		{
			throw CreateAtLeastException(name, msg, value);
		}
	}

	public static void AtLeast(short obj, short value, string name, string msg = null)
	{
		if (obj < value)
		{
			throw CreateAtLeastException(name, msg, value);
		}
	}

	public static void AtLeast(ushort obj, ushort value, string name, string msg = null)
	{
		if (obj < value)
		{
			throw CreateAtLeastException(name, msg, value);
		}
	}

	public static void AtLeast(int obj, int value, string name, string msg = null)
	{
		if (obj < value)
		{
			throw CreateAtLeastException(name, msg, value);
		}
	}

	public static void AtLeast(uint obj, uint value, string name, string msg = null)
	{
		if (obj < value)
		{
			throw CreateAtLeastException(name, msg, value);
		}
	}

	public static void AtLeast(long obj, long value, string name, string msg = null)
	{
		if (obj < value)
		{
			throw CreateAtLeastException(name, msg, value);
		}
	}

	public static void AtLeast(ulong obj, ulong value, string name, string msg = null)
	{
		if (obj < value)
		{
			throw CreateAtLeastException(name, msg, value);
		}
	}

	public static void AtLeast(Optional<sbyte> obj, sbyte value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value < value)
		{
			throw CreateAtLeastException(name, msg, value);
		}
	}

	public static void AtLeast(Optional<byte> obj, byte value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value < value)
		{
			throw CreateAtLeastException(name, msg, value);
		}
	}

	public static void AtLeast(Optional<short> obj, short value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value < value)
		{
			throw CreateAtLeastException(name, msg, value);
		}
	}

	public static void AtLeast(Optional<ushort> obj, ushort value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value < value)
		{
			throw CreateAtLeastException(name, msg, value);
		}
	}

	public static void AtLeast(Optional<int> obj, int value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value < value)
		{
			throw CreateAtLeastException(name, msg, value);
		}
	}

	public static void AtLeast(Optional<uint> obj, uint value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value < value)
		{
			throw CreateAtLeastException(name, msg, value);
		}
	}

	public static void AtLeast(Optional<long> obj, long value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value < value)
		{
			throw CreateAtLeastException(name, msg, value);
		}
	}

	public static void AtLeast(Optional<ulong> obj, ulong value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value < value)
		{
			throw CreateAtLeastException(name, msg, value);
		}
	}

	private static ArgumentException CreateAtLeastException<T>(string name, string msg, T value)
	{
		return new ArgumentException(msg ?? $"Value must be at least {value}.", name);
	}

	public static void GreaterThan(sbyte obj, sbyte value, string name, string msg = null)
	{
		if (obj <= value)
		{
			throw CreateGreaterThanException(name, msg, value);
		}
	}

	public static void GreaterThan(byte obj, byte value, string name, string msg = null)
	{
		if (obj <= value)
		{
			throw CreateGreaterThanException(name, msg, value);
		}
	}

	public static void GreaterThan(short obj, short value, string name, string msg = null)
	{
		if (obj <= value)
		{
			throw CreateGreaterThanException(name, msg, value);
		}
	}

	public static void GreaterThan(ushort obj, ushort value, string name, string msg = null)
	{
		if (obj <= value)
		{
			throw CreateGreaterThanException(name, msg, value);
		}
	}

	public static void GreaterThan(int obj, int value, string name, string msg = null)
	{
		if (obj <= value)
		{
			throw CreateGreaterThanException(name, msg, value);
		}
	}

	public static void GreaterThan(uint obj, uint value, string name, string msg = null)
	{
		if (obj <= value)
		{
			throw CreateGreaterThanException(name, msg, value);
		}
	}

	public static void GreaterThan(long obj, long value, string name, string msg = null)
	{
		if (obj <= value)
		{
			throw CreateGreaterThanException(name, msg, value);
		}
	}

	public static void GreaterThan(ulong obj, ulong value, string name, string msg = null)
	{
		if (obj <= value)
		{
			throw CreateGreaterThanException(name, msg, value);
		}
	}

	public static void GreaterThan(Optional<sbyte> obj, sbyte value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value <= value)
		{
			throw CreateGreaterThanException(name, msg, value);
		}
	}

	public static void GreaterThan(Optional<byte> obj, byte value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value <= value)
		{
			throw CreateGreaterThanException(name, msg, value);
		}
	}

	public static void GreaterThan(Optional<short> obj, short value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value <= value)
		{
			throw CreateGreaterThanException(name, msg, value);
		}
	}

	public static void GreaterThan(Optional<ushort> obj, ushort value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value <= value)
		{
			throw CreateGreaterThanException(name, msg, value);
		}
	}

	public static void GreaterThan(Optional<int> obj, int value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value <= value)
		{
			throw CreateGreaterThanException(name, msg, value);
		}
	}

	public static void GreaterThan(Optional<uint> obj, uint value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value <= value)
		{
			throw CreateGreaterThanException(name, msg, value);
		}
	}

	public static void GreaterThan(Optional<long> obj, long value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value <= value)
		{
			throw CreateGreaterThanException(name, msg, value);
		}
	}

	public static void GreaterThan(Optional<ulong> obj, ulong value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value <= value)
		{
			throw CreateGreaterThanException(name, msg, value);
		}
	}

	private static ArgumentException CreateGreaterThanException<T>(string name, string msg, T value)
	{
		return new ArgumentException(msg ?? $"Value must be greater than {value}.", name);
	}

	public static void AtMost(sbyte obj, sbyte value, string name, string msg = null)
	{
		if (obj > value)
		{
			throw CreateAtMostException(name, msg, value);
		}
	}

	public static void AtMost(byte obj, byte value, string name, string msg = null)
	{
		if (obj > value)
		{
			throw CreateAtMostException(name, msg, value);
		}
	}

	public static void AtMost(short obj, short value, string name, string msg = null)
	{
		if (obj > value)
		{
			throw CreateAtMostException(name, msg, value);
		}
	}

	public static void AtMost(ushort obj, ushort value, string name, string msg = null)
	{
		if (obj > value)
		{
			throw CreateAtMostException(name, msg, value);
		}
	}

	public static void AtMost(int obj, int value, string name, string msg = null)
	{
		if (obj > value)
		{
			throw CreateAtMostException(name, msg, value);
		}
	}

	public static void AtMost(uint obj, uint value, string name, string msg = null)
	{
		if (obj > value)
		{
			throw CreateAtMostException(name, msg, value);
		}
	}

	public static void AtMost(long obj, long value, string name, string msg = null)
	{
		if (obj > value)
		{
			throw CreateAtMostException(name, msg, value);
		}
	}

	public static void AtMost(ulong obj, ulong value, string name, string msg = null)
	{
		if (obj > value)
		{
			throw CreateAtMostException(name, msg, value);
		}
	}

	public static void AtMost(Optional<sbyte> obj, sbyte value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value > value)
		{
			throw CreateAtMostException(name, msg, value);
		}
	}

	public static void AtMost(Optional<byte> obj, byte value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value > value)
		{
			throw CreateAtMostException(name, msg, value);
		}
	}

	public static void AtMost(Optional<short> obj, short value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value > value)
		{
			throw CreateAtMostException(name, msg, value);
		}
	}

	public static void AtMost(Optional<ushort> obj, ushort value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value > value)
		{
			throw CreateAtMostException(name, msg, value);
		}
	}

	public static void AtMost(Optional<int> obj, int value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value > value)
		{
			throw CreateAtMostException(name, msg, value);
		}
	}

	public static void AtMost(Optional<uint> obj, uint value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value > value)
		{
			throw CreateAtMostException(name, msg, value);
		}
	}

	public static void AtMost(Optional<long> obj, long value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value > value)
		{
			throw CreateAtMostException(name, msg, value);
		}
	}

	public static void AtMost(Optional<ulong> obj, ulong value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value > value)
		{
			throw CreateAtMostException(name, msg, value);
		}
	}

	private static ArgumentException CreateAtMostException<T>(string name, string msg, T value)
	{
		return new ArgumentException(msg ?? $"Value must be at most {value}.", name);
	}

	public static void LessThan(sbyte obj, sbyte value, string name, string msg = null)
	{
		if (obj >= value)
		{
			throw CreateLessThanException(name, msg, value);
		}
	}

	public static void LessThan(byte obj, byte value, string name, string msg = null)
	{
		if (obj >= value)
		{
			throw CreateLessThanException(name, msg, value);
		}
	}

	public static void LessThan(short obj, short value, string name, string msg = null)
	{
		if (obj >= value)
		{
			throw CreateLessThanException(name, msg, value);
		}
	}

	public static void LessThan(ushort obj, ushort value, string name, string msg = null)
	{
		if (obj >= value)
		{
			throw CreateLessThanException(name, msg, value);
		}
	}

	public static void LessThan(int obj, int value, string name, string msg = null)
	{
		if (obj >= value)
		{
			throw CreateLessThanException(name, msg, value);
		}
	}

	public static void LessThan(uint obj, uint value, string name, string msg = null)
	{
		if (obj >= value)
		{
			throw CreateLessThanException(name, msg, value);
		}
	}

	public static void LessThan(long obj, long value, string name, string msg = null)
	{
		if (obj >= value)
		{
			throw CreateLessThanException(name, msg, value);
		}
	}

	public static void LessThan(ulong obj, ulong value, string name, string msg = null)
	{
		if (obj >= value)
		{
			throw CreateLessThanException(name, msg, value);
		}
	}

	public static void LessThan(Optional<sbyte> obj, sbyte value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value >= value)
		{
			throw CreateLessThanException(name, msg, value);
		}
	}

	public static void LessThan(Optional<byte> obj, byte value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value >= value)
		{
			throw CreateLessThanException(name, msg, value);
		}
	}

	public static void LessThan(Optional<short> obj, short value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value >= value)
		{
			throw CreateLessThanException(name, msg, value);
		}
	}

	public static void LessThan(Optional<ushort> obj, ushort value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value >= value)
		{
			throw CreateLessThanException(name, msg, value);
		}
	}

	public static void LessThan(Optional<int> obj, int value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value >= value)
		{
			throw CreateLessThanException(name, msg, value);
		}
	}

	public static void LessThan(Optional<uint> obj, uint value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value >= value)
		{
			throw CreateLessThanException(name, msg, value);
		}
	}

	public static void LessThan(Optional<long> obj, long value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value >= value)
		{
			throw CreateLessThanException(name, msg, value);
		}
	}

	public static void LessThan(Optional<ulong> obj, ulong value, string name, string msg = null)
	{
		if (obj.IsSpecified && obj.Value >= value)
		{
			throw CreateLessThanException(name, msg, value);
		}
	}

	private static ArgumentException CreateLessThanException<T>(string name, string msg, T value)
	{
		return new ArgumentException(msg ?? $"Value must be less than {value}.", name);
	}

	public static void YoungerThanTwoWeeks(ulong[] collection, string name)
	{
		ulong num = SnowflakeUtils.ToSnowflake(DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(14.0)));
		for (int i = 0; i < collection.Length; i++)
		{
			if (collection[i] != 0L && collection[i] <= num)
			{
				throw new ArgumentOutOfRangeException(name, "Messages must be younger than two weeks old.");
			}
		}
	}

	public static void NotEveryoneRole(ulong[] roles, ulong guildId, string name)
	{
		for (int i = 0; i < roles.Length; i++)
		{
			if (roles[i] == guildId)
			{
				throw new ArgumentException("The everyone role cannot be assigned to a user.", name);
			}
		}
	}

	public static void Options(string name, string description)
	{
		NotNullOrEmpty(name, "name");
		NotNullOrEmpty(description, "description");
		AtLeast(name.Length, 1, "name");
		AtMost(name.Length, 32, "name");
		AtLeast(description.Length, 1, "description");
		AtMost(description.Length, 100, "description");
	}

	public static void ValidateMessageFlags(MessageFlags flags)
	{
		if (!flags.HasFlag(MessageFlags.None) && !flags.HasFlag(MessageFlags.SuppressEmbeds) && !flags.HasFlag(MessageFlags.SuppressNotification) && !flags.HasFlag(MessageFlags.ComponentsV2) && !flags.HasFlag(MessageFlags.Ephemeral))
		{
			throw new ArgumentException("The only valid MessageFlags are Ephemeral, SuppressEmbeds, SuppressNotification, ComponentsV2 and None.", "flags");
		}
	}
}
