using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Text;

public abstract class SourceText
{
	internal sealed class LineInfo : TextLineCollection
	{
		private readonly SourceText _text;

		private readonly SegmentedList<int> _lineStarts;

		private int _lastLineNumber;

		public override int Count => _lineStarts.Count;

		public override TextLine this[int index]
		{
			get
			{
				if (index < 0 || index >= _lineStarts.Count)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				int start = _lineStarts[index];
				if (index == _lineStarts.Count - 1)
				{
					return TextLine.FromSpanUnsafe(_text, TextSpan.FromBounds(start, _text.Length));
				}
				int end = _lineStarts[index + 1];
				return TextLine.FromSpanUnsafe(_text, TextSpan.FromBounds(start, end));
			}
		}

		public LineInfo(SourceText text, SegmentedList<int> lineStarts)
		{
			_text = text;
			_lineStarts = lineStarts;
		}

		public override int IndexOf(int position)
		{
			if (position < 0 || position > _text.Length)
			{
				throw new ArgumentOutOfRangeException("position");
			}
			int lastLineNumber = _lastLineNumber;
			if (position >= _lineStarts[lastLineNumber])
			{
				int num = Math.Min(_lineStarts.Count, lastLineNumber + 4);
				for (int i = lastLineNumber; i < num; i++)
				{
					if (position < _lineStarts[i])
					{
						return _lastLineNumber = i - 1;
					}
				}
			}
			int num2 = _lineStarts.BinarySearch(position);
			if (num2 < 0)
			{
				num2 = ~num2 - 1;
			}
			_lastLineNumber = num2;
			return num2;
		}

		public override TextLine GetLineFromPosition(int position)
		{
			return this[IndexOf(position)];
		}
	}

	private class StaticContainer : SourceTextContainer
	{
		private readonly SourceText _text;

		public override SourceText CurrentText => _text;

		public override event EventHandler<TextChangeEventArgs> TextChanged
		{
			add
			{
			}
			remove
			{
			}
		}

		public StaticContainer(SourceText text)
		{
			_text = text;
		}
	}

	private sealed class SourceTextWithAlgorithm : SourceText
	{
		private readonly SourceText _underlying;

		public override char this[int position] => _underlying[position];

		public override Encoding? Encoding => _underlying.Encoding;

		public override int Length => _underlying.Length;

		public SourceTextWithAlgorithm(SourceText underlying, SourceHashAlgorithm checksumAlgorithm)
			: base(default(ImmutableArray<byte>), checksumAlgorithm)
		{
			_underlying = underlying;
		}

		public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
		{
			_underlying.CopyTo(sourceIndex, destination, destinationIndex, count);
		}
	}

	private const int CharBufferSize = 32768;

	private const int CharBufferCount = 5;

	internal const int LargeObjectHeapLimitInChars = 40960;

	private static readonly ObjectPool<char[]> s_charArrayPool = new ObjectPool<char[]>(() => new char[32768], 5);

	private static readonly ObjectPool<XxHash128> s_contentHashPool = new ObjectPool<XxHash128>(() => new XxHash128());

	private readonly SourceHashAlgorithm _checksumAlgorithm;

	private SourceTextContainer? _lazyContainer;

	private TextLineCollection? _lazyLineInfo;

	private ImmutableArray<byte> _lazyChecksum;

	private readonly ImmutableArray<byte> _precomputedEmbeddedTextBlob;

	private ImmutableArray<byte> _lazyContentHash;

	private static readonly Encoding s_utf8EncodingWithNoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);

	public SourceHashAlgorithm ChecksumAlgorithm => _checksumAlgorithm;

	public abstract Encoding? Encoding { get; }

	public abstract int Length { get; }

	internal virtual int StorageSize => Length;

	internal virtual ImmutableArray<SourceText> Segments => ImmutableArray<SourceText>.Empty;

	internal virtual SourceText StorageKey => this;

	public bool CanBeEmbedded
	{
		get
		{
			if (_precomputedEmbeddedTextBlob.IsDefault)
			{
				return Encoding != null;
			}
			return !_precomputedEmbeddedTextBlob.IsEmpty;
		}
	}

	internal ImmutableArray<byte> PrecomputedEmbeddedTextBlob => _precomputedEmbeddedTextBlob;

	public abstract char this[int position] { get; }

	public virtual SourceTextContainer Container
	{
		get
		{
			if (_lazyContainer == null)
			{
				Interlocked.CompareExchange(ref _lazyContainer, new StaticContainer(this), null);
			}
			return _lazyContainer;
		}
	}

	public TextLineCollection Lines
	{
		get
		{
			TextLineCollection lazyLineInfo = _lazyLineInfo;
			return lazyLineInfo ?? Interlocked.CompareExchange(ref _lazyLineInfo, lazyLineInfo = GetLinesCore(), null) ?? lazyLineInfo;
		}
	}

	protected SourceText(ImmutableArray<byte> checksum = default(ImmutableArray<byte>), SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1, SourceTextContainer? container = null)
	{
		ValidateChecksumAlgorithm(checksumAlgorithm);
		if (!checksum.IsDefault && checksum.Length != CryptographicHashProvider.GetHashSize(checksumAlgorithm))
		{
			throw new ArgumentException(CodeAnalysisResources.InvalidHash, "checksum");
		}
		_checksumAlgorithm = checksumAlgorithm;
		_lazyChecksum = checksum;
		_lazyContainer = container;
	}

	internal SourceText(ImmutableArray<byte> checksum, SourceHashAlgorithm checksumAlgorithm, ImmutableArray<byte> embeddedTextBlob)
		: this(checksum, checksumAlgorithm)
	{
		if (!checksum.IsDefault && embeddedTextBlob.IsDefault)
		{
			_precomputedEmbeddedTextBlob = ImmutableArray<byte>.Empty;
		}
		else
		{
			_precomputedEmbeddedTextBlob = embeddedTextBlob;
		}
	}

	internal static void ValidateChecksumAlgorithm(SourceHashAlgorithm checksumAlgorithm)
	{
		if (!SourceHashAlgorithms.IsSupportedAlgorithm(checksumAlgorithm))
		{
			throw new ArgumentException(CodeAnalysisResources.UnsupportedHashAlgorithm, "checksumAlgorithm");
		}
	}

	public static SourceText From(string text, Encoding? encoding = null, SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		return new StringText(text, encoding, default(ImmutableArray<byte>), checksumAlgorithm);
	}

	public static SourceText From(TextReader reader, int length, Encoding? encoding = null, SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (length >= 40960)
		{
			return LargeText.Decode(reader, length, encoding, checksumAlgorithm);
		}
		return From(reader.ReadToEnd(), encoding, checksumAlgorithm);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static SourceText From(Stream stream, Encoding? encoding, SourceHashAlgorithm checksumAlgorithm, bool throwIfBinaryDetected)
	{
		return From(stream, encoding, checksumAlgorithm, throwIfBinaryDetected, false);
	}

	public static SourceText From(Stream stream, Encoding? encoding = null, SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1, bool throwIfBinaryDetected = false, bool canBeEmbedded = false)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanRead)
		{
			throw new ArgumentException(CodeAnalysisResources.StreamMustSupportReadAndSeek, "stream");
		}
		ValidateChecksumAlgorithm(checksumAlgorithm);
		encoding = encoding ?? s_utf8EncodingWithNoBOM;
		if (stream.CanSeek && GetMaxCharCountOrThrowIfHuge(encoding, stream) >= 40960)
		{
			return LargeText.Decode(stream, encoding, checksumAlgorithm, throwIfBinaryDetected, canBeEmbedded);
		}
		string text = Decode(stream, encoding, out encoding);
		if (throwIfBinaryDetected && IsBinary(text))
		{
			throw new InvalidDataException();
		}
		ImmutableArray<byte> checksum = CalculateChecksum(stream, checksumAlgorithm);
		ImmutableArray<byte> embeddedTextBlob = (canBeEmbedded ? EmbeddedText.CreateBlob(stream) : default(ImmutableArray<byte>));
		return new StringText(text, encoding, checksum, checksumAlgorithm, embeddedTextBlob);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static SourceText From(byte[] buffer, int length, Encoding? encoding, SourceHashAlgorithm checksumAlgorithm, bool throwIfBinaryDetected)
	{
		return From(buffer, length, encoding, checksumAlgorithm, throwIfBinaryDetected, false);
	}

	public static SourceText From(byte[] buffer, int length, Encoding? encoding = null, SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1, bool throwIfBinaryDetected = false, bool canBeEmbedded = false)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (length < 0 || length > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		ValidateChecksumAlgorithm(checksumAlgorithm);
		string text = Decode(buffer, length, encoding ?? s_utf8EncodingWithNoBOM, out encoding);
		if (throwIfBinaryDetected && IsBinary(text))
		{
			throw new InvalidDataException();
		}
		ImmutableArray<byte> checksum = CalculateChecksum(buffer, 0, length, checksumAlgorithm);
		ImmutableArray<byte> embeddedTextBlob = (canBeEmbedded ? EmbeddedText.CreateBlob(new ArraySegment<byte>(buffer, 0, length)) : default(ImmutableArray<byte>));
		return new StringText(text, encoding, checksum, checksumAlgorithm, embeddedTextBlob);
	}

	private static string Decode(Stream stream, Encoding encoding, out Encoding actualEncoding)
	{
		int bufferSize = 4096;
		if (stream.CanSeek)
		{
			stream.Seek(0L, SeekOrigin.Begin);
			int num = (int)stream.Length;
			if (num == 0)
			{
				actualEncoding = encoding;
				return string.Empty;
			}
			bufferSize = Math.Min(4096, num);
		}
		using StreamReader streamReader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize, leaveOpen: true);
		string result = streamReader.ReadToEnd();
		actualEncoding = streamReader.CurrentEncoding;
		return result;
	}

	private static string Decode(byte[] buffer, int length, Encoding encoding, out Encoding actualEncoding)
	{
		actualEncoding = TryReadByteOrderMark(buffer, length, out var preambleLength) ?? encoding;
		return actualEncoding.GetString(buffer, preambleLength, length - preambleLength);
	}

	internal static bool IsBinary(ReadOnlySpan<char> text)
	{
		int num = 1;
		while (num < text.Length)
		{
			if (text[num] == '\0')
			{
				if (text[num - 1] == '\0')
				{
					return true;
				}
				num++;
			}
			else
			{
				num += 2;
			}
		}
		return false;
	}

	internal static bool IsBinary(string text)
	{
		return IsBinary(System.MemoryExtensions.AsSpan(text));
	}

	public abstract void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count);

	internal void CheckSubSpan(TextSpan span)
	{
		if (span.End > Length)
		{
			throw new ArgumentOutOfRangeException("span");
		}
	}

	public virtual SourceText GetSubText(TextSpan span)
	{
		CheckSubSpan(span);
		int length = span.Length;
		if (length == 0)
		{
			return From(string.Empty, Encoding, ChecksumAlgorithm);
		}
		if (length == Length && span.Start == 0)
		{
			return this;
		}
		return new SubText(this, span);
	}

	public SourceText GetSubText(int start)
	{
		if (start < 0 || start > Length)
		{
			throw new ArgumentOutOfRangeException("start");
		}
		if (start == 0)
		{
			return this;
		}
		return GetSubText(new TextSpan(start, Length - start));
	}

	public void Write(TextWriter textWriter, CancellationToken cancellationToken = default(CancellationToken))
	{
		Write(textWriter, new TextSpan(0, Length), cancellationToken);
	}

	public virtual void Write(TextWriter writer, TextSpan span, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSubSpan(span);
		char[] array = s_charArrayPool.Allocate();
		try
		{
			int i = span.Start;
			int num;
			for (int end = span.End; i < end; i += num)
			{
				cancellationToken.ThrowIfCancellationRequested();
				num = Math.Min(array.Length, end - i);
				CopyTo(i, array, 0, num);
				writer.Write(array, 0, num);
			}
		}
		finally
		{
			s_charArrayPool.Free(array);
		}
	}

	public ImmutableArray<byte> GetChecksum()
	{
		if (_lazyChecksum.IsDefault)
		{
			using SourceTextStream stream = new SourceTextStream(this, useDefaultEncodingIfNull: true);
			ImmutableInterlocked.InterlockedInitialize(ref _lazyChecksum, CalculateChecksum(stream, _checksumAlgorithm));
		}
		return _lazyChecksum;
	}

	public ImmutableArray<byte> GetContentHash()
	{
		if (_lazyContentHash.IsDefault)
		{
			ImmutableInterlocked.InterlockedInitialize(ref _lazyContentHash, computeContentHash());
		}
		return _lazyContentHash;
		ImmutableArray<byte> computeContentHash()
		{
			XxHash128 xxHash = s_contentHashPool.Allocate();
			char[] array = s_charArrayPool.Allocate();
			try
			{
				int i = 0;
				for (int length = Length; i < length; i += 32768)
				{
					int num = Math.Min(32768, length - i);
					CopyTo(i, array, 0, num);
					Span<char> span = System.MemoryExtensions.AsSpan(array, 0, num);
					if (!BitConverter.IsLittleEndian)
					{
						ReverseEndianness(MemoryMarshal.Cast<char, short>(span));
					}
					xxHash.Append(MemoryMarshal.AsBytes(span));
				}
				return ImmutableCollectionsMarshal.AsImmutableArray(xxHash.GetHashAndReset());
			}
			finally
			{
				s_charArrayPool.Free(array);
				xxHash.Reset();
				s_contentHashPool.Free(xxHash);
			}
		}
	}

	internal static void ReverseEndianness(Span<short> shortSpan)
	{
		for (int i = 0; i < shortSpan.Length; i++)
		{
			shortSpan[i] = BinaryPrimitives.ReverseEndianness(shortSpan[i]);
		}
	}

	internal static ImmutableArray<byte> CalculateChecksum(byte[] buffer, int offset, int count, SourceHashAlgorithm algorithmId)
	{
		using HashAlgorithm hashAlgorithm = CryptographicHashProvider.TryGetAlgorithm(algorithmId);
		return ImmutableArray.Create(hashAlgorithm.ComputeHash(buffer, offset, count));
	}

	internal static ImmutableArray<byte> CalculateChecksum(Stream stream, SourceHashAlgorithm algorithmId)
	{
		using HashAlgorithm hashAlgorithm = CryptographicHashProvider.TryGetAlgorithm(algorithmId);
		if (stream.CanSeek)
		{
			stream.Seek(0L, SeekOrigin.Begin);
		}
		return ImmutableArray.Create(hashAlgorithm.ComputeHash(stream));
	}

	public override string ToString()
	{
		return ToString(new TextSpan(0, Length));
	}

	public virtual string ToString(TextSpan span)
	{
		CheckSubSpan(span);
		char[] array = s_charArrayPool.Allocate();
		int i = Math.Max(Math.Min(span.Start, Length), 0);
		int num = Math.Min(span.End, Length) - i;
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		instance.Builder.EnsureCapacity(num);
		int num2;
		for (; i < Length; i += num2)
		{
			if (num <= 0)
			{
				break;
			}
			num2 = Math.Min(array.Length, num);
			CopyTo(i, array, 0, num2);
			instance.Builder.Append(array, 0, num2);
			num -= num2;
		}
		string result = instance.ToStringAndFree();
		s_charArrayPool.Free(array);
		return result;
	}

	public virtual SourceText WithChanges(IEnumerable<TextChange> changes)
	{
		if (changes == null)
		{
			throw new ArgumentNullException("changes");
		}
		if (!changes.Any())
		{
			return this;
		}
		ArrayBuilder<SourceText> instance = ArrayBuilder<SourceText>.GetInstance();
		ArrayBuilder<TextChangeRange> instance2 = ArrayBuilder<TextChangeRange>.GetInstance();
		try
		{
			int num = 0;
			foreach (TextChange change in changes)
			{
				if (change.Span.End > Length)
				{
					throw new ArgumentException(CodeAnalysisResources.ChangesMustBeWithinBoundsOfSourceText, "changes");
				}
				if (change.Span.Start < num)
				{
					if (change.Span.End <= instance2.Last().Span.Start)
					{
						changes = changes.Where(delegate(TextChange c)
						{
							TextChange textChange = c;
							if (textChange.Span.IsEmpty)
							{
								textChange = c;
								string? newText = textChange.NewText;
								if (newText == null)
								{
									return false;
								}
								return newText.Length > 0;
							}
							return true;
						}).OrderBy(delegate(TextChange c)
						{
							TextChange textChange = c;
							return textChange.Span;
						}).ToList();
						return WithChanges(changes);
					}
					throw new ArgumentException(CodeAnalysisResources.ChangesMustNotOverlap, "changes");
				}
				int num2 = change.NewText?.Length ?? 0;
				if (change.Span.Length != 0 || num2 != 0)
				{
					if (change.Span.Start > num)
					{
						SourceText subText = GetSubText(new TextSpan(num, change.Span.Start - num));
						CompositeText.AddSegments(instance, subText);
					}
					if (num2 > 0)
					{
						SourceText text = From(change.NewText, Encoding, ChecksumAlgorithm);
						CompositeText.AddSegments(instance, text);
					}
					num = change.Span.End;
					instance2.Add(new TextChangeRange(change.Span, num2));
				}
			}
			if (num == 0 && instance.Count == 0)
			{
				return this;
			}
			if (num < Length)
			{
				SourceText subText2 = GetSubText(new TextSpan(num, Length - num));
				CompositeText.AddSegments(instance, subText2);
			}
			SourceText sourceText = CompositeText.ToSourceText(instance, this, adjustSegments: true);
			if (sourceText != this)
			{
				return new ChangedText(this, sourceText, instance2.ToImmutable());
			}
			return this;
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	public SourceText WithChanges(params TextChange[] changes)
	{
		return WithChanges((IEnumerable<TextChange>)changes);
	}

	public SourceText Replace(TextSpan span, string newText)
	{
		return WithChanges(new TextChange(span, newText));
	}

	public SourceText Replace(int start, int length, string newText)
	{
		return Replace(new TextSpan(start, length), newText);
	}

	public virtual IReadOnlyList<TextChangeRange> GetChangeRanges(SourceText oldText)
	{
		if (oldText == null)
		{
			throw new ArgumentNullException("oldText");
		}
		if (oldText == this)
		{
			return TextChangeRange.NoChanges;
		}
		return ImmutableArray.Create(new TextChangeRange(new TextSpan(0, oldText.Length), Length));
	}

	public virtual IReadOnlyList<TextChange> GetTextChanges(SourceText oldText)
	{
		int num = 0;
		List<TextChangeRange> list = GetChangeRanges(oldText).ToList();
		List<TextChange> list2 = new List<TextChange>(list.Count);
		foreach (TextChangeRange item in list)
		{
			int start = item.Span.Start + num;
			string newText;
			if (item.NewLength > 0)
			{
				TextSpan textSpan = new TextSpan(start, item.NewLength);
				newText = ToString(textSpan);
			}
			else
			{
				newText = string.Empty;
			}
			list2.Add(new TextChange(item.Span, newText));
			num += item.NewLength - item.Span.Length;
		}
		return list2.ToImmutableArrayOrEmpty();
	}

	internal bool TryGetLines([NotNullWhen(true)] out TextLineCollection? lines)
	{
		lines = _lazyLineInfo;
		return lines != null;
	}

	protected virtual TextLineCollection GetLinesCore()
	{
		return new LineInfo(this, ParseLineStarts());
	}

	private void EnumerateChars(Action<int, char[], int> action)
	{
		int i = 0;
		char[] array = s_charArrayPool.Allocate();
		int num;
		for (int length = Length; i < length; i += num)
		{
			num = Math.Min(length - i, array.Length);
			CopyTo(i, array, 0, num);
			action(i, array, num);
		}
		action(i, array, 0);
		s_charArrayPool.Free(array);
	}

	private SegmentedList<int> ParseLineStarts()
	{
		if (Length == 0)
		{
			return new SegmentedList<int> { 0 };
		}
		SegmentedList<int> lineStarts = new SegmentedList<int>(Length / 64) { 0 };
		bool lastWasCR = false;
		EnumerateChars(delegate(int position, char[] buffer, int length)
		{
			int num = 0;
			if (lastWasCR)
			{
				if (length > 0 && buffer[0] == '\n')
				{
					num++;
				}
				lineStarts.Add(position + num);
				lastWasCR = false;
			}
			while (num < length)
			{
				char c = buffer[num];
				num++;
				if ((uint)(c - 14) > 113u)
				{
					if (c == '\r')
					{
						if (num < length && buffer[num] == '\n')
						{
							num++;
						}
						else if (num >= length)
						{
							lastWasCR = true;
							continue;
						}
					}
					else if (!TextUtilities.IsAnyLineBreakCharacter(c))
					{
						continue;
					}
					lineStarts.Add(position + num);
				}
			}
		});
		return lineStarts;
	}

	public bool ContentEquals(SourceText other)
	{
		if (this == other)
		{
			return true;
		}
		ImmutableArray<byte> lazyContentHash = _lazyContentHash;
		ImmutableArray<byte> lazyContentHash2 = other._lazyContentHash;
		if (!lazyContentHash.IsDefault && !lazyContentHash2.IsDefault)
		{
			return lazyContentHash.SequenceEqual(lazyContentHash2);
		}
		return ContentEqualsImpl(other);
	}

	protected virtual bool ContentEqualsImpl(SourceText other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (Length != other.Length)
		{
			return false;
		}
		char[] array = s_charArrayPool.Allocate();
		char[] array2 = s_charArrayPool.Allocate();
		try
		{
			int i = 0;
			for (int length = Length; i < length; i += 32768)
			{
				int num = Math.Min(Length - i, 32768);
				CopyTo(i, array, 0, num);
				other.CopyTo(i, array2, 0, num);
				if (!System.MemoryExtensions.AsSpan(array, 0, num).SequenceEqual(System.MemoryExtensions.AsSpan(array2, 0, num)))
				{
					return false;
				}
			}
			return true;
		}
		finally
		{
			s_charArrayPool.Free(array2);
			s_charArrayPool.Free(array);
		}
	}

	internal static Encoding? TryReadByteOrderMark(byte[] source, int length, out int preambleLength)
	{
		if (length >= 2)
		{
			switch (source[0])
			{
			case 254:
				if (source[1] == byte.MaxValue)
				{
					preambleLength = 2;
					return System.Text.Encoding.BigEndianUnicode;
				}
				break;
			case byte.MaxValue:
				if (source[1] == 254)
				{
					preambleLength = 2;
					return System.Text.Encoding.Unicode;
				}
				break;
			case 239:
				if (source[1] == 187 && length >= 3 && source[2] == 191)
				{
					preambleLength = 3;
					return System.Text.Encoding.UTF8;
				}
				break;
			}
		}
		preambleLength = 0;
		return null;
	}

	internal static int GetMaxCharCountOrThrowIfHuge(Encoding encoding, Stream stream)
	{
		if (encoding.TryGetMaxCharCount(stream.Length, out var maxCharCount))
		{
			return maxCharCount;
		}
		throw new IOException(CodeAnalysisResources.StreamIsTooLong);
	}

	internal SourceText WithChecksumAlgorithm(SourceHashAlgorithm checksumAlgorithm)
	{
		if (checksumAlgorithm == SourceHashAlgorithm.None || checksumAlgorithm == ChecksumAlgorithm)
		{
			return this;
		}
		return new SourceTextWithAlgorithm(this, checksumAlgorithm);
	}
}
