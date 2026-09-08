using System;
using System.Collections.Generic;
using System.Text;

namespace ExCSS;

internal abstract class LexerBase : IDisposable
{
	private readonly Stack<ushort> _columns;

	protected StringBuilder StringBuffer { get; private set; }

	public TextSource Source { get; }

	public ushort Line { get; private set; }

	public ushort Column { get; private set; }

	public int Position => Source.Index;

	protected char Current { get; private set; }

	public int InsertionPoint
	{
		get
		{
			return Source.Index;
		}
		protected set
		{
			int i;
			for (i = Source.Index - value; i > 0; i--)
			{
				BackNative();
			}
			for (; i < 0; i++)
			{
				AdvanceNative();
			}
		}
	}

	protected LexerBase(TextSource source)
	{
		StringBuffer = Pool.NewStringBuilder();
		_columns = new Stack<ushort>();
		Source = source;
		Current = '\0';
		Column = 0;
		Line = 1;
	}

	public string FlushBuffer()
	{
		string result = StringBuffer.ToString();
		StringBuffer.Clear();
		return result;
	}

	public void Dispose()
	{
		if (StringBuffer != null)
		{
			((IDisposable)Source)?.Dispose();
			StringBuffer.Clear().ToPool();
			StringBuffer = null;
		}
	}

	public TextPosition GetCurrentPosition()
	{
		return new TextPosition(Line, Column, Position);
	}

	protected char SkipSpaces()
	{
		char next = GetNext();
		while (next.IsSpaceCharacter())
		{
			next = GetNext();
		}
		return next;
	}

	protected char GetNext()
	{
		Advance();
		return Current;
	}

	protected char GetPrevious()
	{
		Back();
		return Current;
	}

	protected void Advance()
	{
		if (Current != '\uffff')
		{
			AdvanceNative();
		}
	}

	protected void Advance(int distance)
	{
		while (distance-- > 0 && Current != '\uffff')
		{
			AdvanceNative();
		}
	}

	protected void Back()
	{
		if (InsertionPoint > 0)
		{
			BackNative();
		}
	}

	protected void Back(int distance)
	{
		while (distance-- > 0 && InsertionPoint > 0)
		{
			BackNative();
		}
	}

	private void AdvanceNative()
	{
		if (Current == '\n')
		{
			_columns.Push(Column);
			Column = 1;
			Line++;
		}
		else
		{
			Column++;
		}
		Current = NormalizeForward(Source.ReadCharacter());
	}

	private void BackNative()
	{
		Source.Index--;
		if (Source.Index == 0)
		{
			Column = 0;
			Current = '\0';
			return;
		}
		char c = NormalizeBackward(Source[Source.Index - 1]);
		switch (c)
		{
		case '\n':
			Column = (ushort)((_columns.Count == 0) ? 1 : _columns.Pop());
			Line--;
			Current = c;
			break;
		default:
			Current = c;
			Column--;
			break;
		case '\0':
			break;
		}
	}

	private char NormalizeForward(char symbol)
	{
		if (symbol != '\r')
		{
			return symbol;
		}
		if (Source.ReadCharacter() != '\n')
		{
			Source.Index--;
		}
		return '\n';
	}

	private char NormalizeBackward(char symbol)
	{
		if (symbol != '\r')
		{
			return symbol;
		}
		if (Source.Index < Source.Length && Source[Source.Index] == '\n')
		{
			BackNative();
			return '\0';
		}
		return '\n';
	}
}
