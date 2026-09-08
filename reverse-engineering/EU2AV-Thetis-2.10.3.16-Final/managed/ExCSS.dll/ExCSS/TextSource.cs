using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExCSS;

public sealed class TextSource : IDisposable
{
	private enum EncodingConfidence : byte
	{
		Tentative,
		Certain,
		Irrelevant
	}

	private const int BufferSize = 4096;

	private readonly Stream _baseStream;

	private readonly MemoryStream _raw;

	private readonly byte[] _buffer;

	private readonly char[] _chars;

	private StringBuilder _content;

	private EncodingConfidence _confidence;

	private bool _finished;

	private Encoding _encoding;

	private Decoder _decoder;

	public string Text => _content.ToString();

	public char this[int index] => _content[index];

	public int Index { get; set; }

	public int Length => _content.Length;

	public Encoding CurrentEncoding
	{
		get
		{
			return _encoding;
		}
		set
		{
			if (_confidence != EncodingConfidence.Tentative)
			{
				return;
			}
			if (_encoding.IsUnicode())
			{
				_confidence = EncodingConfidence.Certain;
				return;
			}
			if (value.IsUnicode())
			{
				value = TextEncoding.Utf8;
			}
			if (value == _encoding)
			{
				_confidence = EncodingConfidence.Certain;
				return;
			}
			_encoding = value;
			_decoder = value.GetDecoder();
			byte[] array = _raw.ToArray();
			char[] array2 = new char[_encoding.GetMaxCharCount(array.Length)];
			int chars = _decoder.GetChars(array, 0, array.Length, array2, 0);
			string text = new string(array2, 0, chars);
			int num = Math.Min(Index, text.Length);
			if (text.Substring(0, num).Is(_content.ToString(0, num)))
			{
				_confidence = EncodingConfidence.Certain;
				_content.Remove(num, _content.Length - num);
				_content.Append(text.Substring(num));
				return;
			}
			Index = 0;
			_content.Clear().Append(text);
			throw new NotSupportedException();
		}
	}

	public void Dispose()
	{
		if (_content != null)
		{
			_raw.Dispose();
			_content.Clear().ToPool();
			_content = null;
		}
	}

	private TextSource(Encoding encoding)
	{
		_buffer = new byte[4096];
		_chars = new char[4097];
		_raw = new MemoryStream();
		Index = 0;
		_encoding = encoding ?? TextEncoding.Utf8;
		_decoder = _encoding.GetDecoder();
	}

	public TextSource(string source)
		: this(null, TextEncoding.Utf8)
	{
		_finished = true;
		_content.Append(source);
		_confidence = EncodingConfidence.Irrelevant;
	}

	public TextSource(Stream baseStream, Encoding encoding = null)
		: this(encoding)
	{
		_baseStream = baseStream;
		_content = Pool.NewStringBuilder();
		_confidence = EncodingConfidence.Tentative;
	}

	public char ReadCharacter()
	{
		if (Index < _content.Length)
		{
			return _content[Index++];
		}
		ExpandBuffer(4096L);
		int num = Index++;
		if (num >= _content.Length)
		{
			return '\uffff';
		}
		return _content[num];
	}

	public string ReadCharacters(int characters)
	{
		int index = Index;
		if (index + characters <= _content.Length)
		{
			Index += characters;
			return _content.ToString(index, characters);
		}
		ExpandBuffer(Math.Max(4096, characters));
		Index += characters;
		characters = Math.Min(characters, _content.Length - index);
		return _content.ToString(index, characters);
	}

	public async Task PrefetchAllAsync(CancellationToken cancellationToken)
	{
		if (_content.Length == 0)
		{
			await DetectByteOrderMarkAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		while (!_finished)
		{
			await ReadIntoBufferAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task DetectByteOrderMarkAsync(CancellationToken cancellationToken)
	{
		int num = await _baseStream.ReadAsync(_buffer, 0, 4096, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		int num2 = 0;
		if (num > 2 && _buffer[0] == 239 && _buffer[1] == 187 && _buffer[2] == 191)
		{
			_encoding = TextEncoding.Utf8;
			num2 = 3;
		}
		else if (num > 3 && _buffer[0] == byte.MaxValue && _buffer[1] == 254 && _buffer[2] == 0 && _buffer[3] == 0)
		{
			_encoding = TextEncoding.Utf32Le;
			num2 = 4;
		}
		else if (num > 3 && _buffer[0] == 0 && _buffer[1] == 0 && _buffer[2] == 254 && _buffer[3] == byte.MaxValue)
		{
			_encoding = TextEncoding.Utf32Be;
			num2 = 4;
		}
		else if (num > 1 && _buffer[0] == 254 && _buffer[1] == byte.MaxValue)
		{
			_encoding = TextEncoding.Utf16Be;
			num2 = 2;
		}
		else if (num > 1 && _buffer[0] == byte.MaxValue && _buffer[1] == 254)
		{
			_encoding = TextEncoding.Utf16Le;
			num2 = 2;
		}
		else if (num > 3 && _buffer[0] == 132 && _buffer[1] == 49 && _buffer[2] == 149 && _buffer[3] == 51)
		{
			_encoding = TextEncoding.Gb18030;
			num2 = 4;
		}
		if (num2 > 0)
		{
			num -= num2;
			Array.Copy(_buffer, num2, _buffer, 0, num);
			_decoder = _encoding.GetDecoder();
			_confidence = EncodingConfidence.Certain;
		}
		AppendContentFromBuffer(num);
	}

	private async Task ReadIntoBufferAsync(CancellationToken cancellationToken)
	{
		AppendContentFromBuffer(await _baseStream.ReadAsync(_buffer, 0, 4096, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	private void ExpandBuffer(long size)
	{
		if (!_finished && _content.Length == 0)
		{
			DetectByteOrderMarkAsync(CancellationToken.None).Wait();
		}
		while (size + Index > _content.Length && !_finished)
		{
			ReadIntoBuffer();
		}
	}

	private void ReadIntoBuffer()
	{
		int size = _baseStream.Read(_buffer, 0, 4096);
		AppendContentFromBuffer(size);
	}

	private void AppendContentFromBuffer(int size)
	{
		_finished = size == 0;
		int chars = _decoder.GetChars(_buffer, 0, size, _chars, 0);
		if (_confidence != EncodingConfidence.Certain)
		{
			_raw.Write(_buffer, 0, size);
		}
		_content.Append(_chars, 0, chars);
	}
}
