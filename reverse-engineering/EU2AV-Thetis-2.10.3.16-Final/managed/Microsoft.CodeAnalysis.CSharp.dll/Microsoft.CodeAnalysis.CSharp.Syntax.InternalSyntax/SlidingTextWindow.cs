using System;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

[NonCopyable]
internal struct SlidingTextWindow
{
	public static class TestAccessor
	{
		public static int GetOffset(in SlidingTextWindow window)
		{
			return window._positionInText - window._characterWindowStartPositionInText;
		}

		public static int GetCharacterWindowStartPositionInText(in SlidingTextWindow window)
		{
			return window._characterWindowStartPositionInText;
		}

		public static ArraySegment<char> GetCharacterWindow(in SlidingTextWindow window)
		{
			return window._characterWindow;
		}
	}

	public const char InvalidCharacter = '\uffff';

	public const int DefaultWindowLength = 4096;

	private static readonly ObjectPool<char[]> s_windowPool = new ObjectPool<char[]>(() => new char[4096]);

	private readonly int _textEnd;

	private int _positionInText;

	private ArraySegment<char> _characterWindow;

	private int _characterWindowStartPositionInText;

	private readonly StringTable _strings;

	public SourceText Text { get; }

	public readonly int Position => _positionInText;

	public readonly ReadOnlySpan<char> CurrentWindowSpan
	{
		get
		{
			int num = _positionInText - _characterWindowStartPositionInText;
			if (num >= 0)
			{
				ArraySegment<char> characterWindow = _characterWindow;
				if (num < characterWindow.Count)
				{
					return System.MemoryExtensions.AsSpan(_characterWindow, num);
				}
			}
			return default(ReadOnlySpan<char>);
		}
	}

	private readonly int CharacterWindowEndPositionInText
	{
		get
		{
			int characterWindowStartPositionInText = _characterWindowStartPositionInText;
			ArraySegment<char> characterWindow = _characterWindow;
			return characterWindowStartPositionInText + characterWindow.Count;
		}
	}

	public SlidingTextWindow(SourceText text)
	{
		_positionInText = 0;
		_characterWindowStartPositionInText = 0;
		Text = text;
		_textEnd = text.Length;
		_strings = StringTable.GetInstance();
		_characterWindow = new ArraySegment<char>(s_windowPool.Allocate());
		ReadChunkAt(0);
	}

	public void Free()
	{
		s_windowPool.Free(_characterWindow.Array);
		_strings.Free();
	}

	private void ReadChunkAt(int position)
	{
		position = Math.Min(position, _textEnd);
		int count = Math.Min(_textEnd - position, 4096);
		Text.CopyTo(position, _characterWindow.Array, 0, count);
		_characterWindowStartPositionInText = position;
		_characterWindow = new ArraySegment<char>(_characterWindow.Array, 0, count);
	}

	private readonly bool PositionIsWithinWindow(int position)
	{
		if (position >= _characterWindowStartPositionInText)
		{
			return position < CharacterWindowEndPositionInText;
		}
		return false;
	}

	public readonly bool SpanIsWithinWindow(TextSpan span)
	{
		if (span.Start >= _characterWindowStartPositionInText)
		{
			return span.End <= CharacterWindowEndPositionInText;
		}
		return false;
	}

	public readonly bool TryGetTextIfWithinWindow(TextSpan span, out ReadOnlySpan<char> textSpan)
	{
		if (SpanIsWithinWindow(span))
		{
			textSpan = System.MemoryExtensions.AsSpan(_characterWindow, span.Start - _characterWindowStartPositionInText, span.Length);
			return true;
		}
		textSpan = default(ReadOnlySpan<char>);
		return false;
	}

	public void Reset(int position)
	{
		_positionInText = Math.Min(position, _textEnd);
		if (!PositionIsWithinWindow(_positionInText))
		{
			ReadChunkAt(_positionInText);
		}
	}

	public readonly bool IsReallyAtEnd()
	{
		return Position >= _textEnd;
	}

	public void AdvanceChar()
	{
		AdvanceChar(1);
	}

	public bool TryAdvance(char c)
	{
		if (PeekChar() != c)
		{
			return false;
		}
		AdvanceChar();
		return true;
	}

	public void AdvanceChar(int n)
	{
		_positionInText += n;
	}

	public void AdvancePastNewLine()
	{
		AdvanceChar(GetNewLineWidth());
	}

	public int GetNewLineWidth()
	{
		return GetNewLineWidth(PeekChar(), PeekChar(1));
	}

	public static int GetNewLineWidth(char currentChar, char nextChar)
	{
		if (currentChar != '\r' || nextChar != '\n')
		{
			return 1;
		}
		return 2;
	}

	public char NextChar()
	{
		char num = PeekChar();
		if (num != '\uffff')
		{
			AdvanceChar();
		}
		return num;
	}

	public char PeekChar()
	{
		if (IsReallyAtEnd())
		{
			return '\uffff';
		}
		int positionInText = _positionInText;
		if (!PositionIsWithinWindow(positionInText))
		{
			ReadChunkAt(positionInText);
		}
		return _characterWindow.Array[positionInText - _characterWindowStartPositionInText];
	}

	public char PeekChar(int delta)
	{
		int position = Position;
		AdvanceChar(delta);
		char result = PeekChar();
		Reset(position);
		return result;
	}

	public char PreviousChar()
	{
		return PeekChar(-1);
	}

	internal bool AdvanceIfMatches(string desired)
	{
		int length = desired.Length;
		for (int i = 0; i < length; i++)
		{
			if (PeekChar(i) != desired[i])
			{
				return false;
			}
		}
		AdvanceChar(length);
		return true;
	}

	public readonly string Intern(StringBuilder text)
	{
		return _strings.Add(text);
	}

	public readonly string Intern(char[] array, int start, int length)
	{
		return Intern(System.MemoryExtensions.AsSpan(array, start, length));
	}

	public readonly string Intern(ReadOnlySpan<char> chars)
	{
		return _strings.Add(chars);
	}

	public readonly string GetText(int startPosition, bool intern)
	{
		return GetText(startPosition, Position - startPosition, intern);
	}

	public readonly string GetText(int position, int length, bool intern)
	{
		TextSpan textSpan = new TextSpan(position, length);
		if (!SpanIsWithinWindow(textSpan))
		{
			return Text.ToString(textSpan);
		}
		int num = position - _characterWindowStartPositionInText;
		ArraySegment<char> characterWindow = _characterWindow;
		char[] array = characterWindow.Array;
		switch (length)
		{
		case 0:
			return string.Empty;
		case 1:
			switch (array[num])
			{
			case ' ':
				return " ";
			case '\n':
				return "\n";
			}
			break;
		case 2:
		{
			char c = array[num];
			char c2 = array[num + 1];
			switch (c)
			{
			case '\r':
				if (c2 != '\n')
				{
					break;
				}
				return "\r\n";
			case '/':
				if (c2 != '/')
				{
					break;
				}
				return "//";
			}
			break;
		}
		case 3:
			if (array[num] == '/' && array[num + 1] == '/' && array[num + 2] == ' ')
			{
				return "// ";
			}
			break;
		}
		if (!intern)
		{
			return new string(array, num, length);
		}
		return Intern(array, num, length);
	}
}
