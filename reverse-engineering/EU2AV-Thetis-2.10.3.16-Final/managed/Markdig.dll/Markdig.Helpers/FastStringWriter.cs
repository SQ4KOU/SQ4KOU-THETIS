using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Markdig.Helpers;

internal sealed class FastStringWriter : TextWriter
{
	private char[] _chars;

	private int _pos;

	private string _newLine;

	public override Encoding Encoding => System.Text.Encoding.Unicode;

	public override string NewLine
	{
		get
		{
			return _newLine;
		}
		[param: AllowNull]
		set
		{
			string obj = value ?? Environment.NewLine;
			string newLine = obj;
			_newLine = obj;
			base.NewLine = newLine;
		}
	}

	public FastStringWriter()
	{
		_chars = new char[1024];
		_newLine = "\n";
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void Write(char value)
	{
		char[] chars = _chars;
		int pos = _pos;
		if ((uint)pos < (uint)chars.Length)
		{
			chars[pos] = value;
			_pos = pos + 1;
		}
		else
		{
			GrowAndAppend(value);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void WriteLine(char value)
	{
		Write(value);
		WriteLine();
	}

	public override Task WriteAsync(char value)
	{
		Write(value);
		return Task.CompletedTask;
	}

	public override Task WriteLineAsync(char value)
	{
		WriteLine(value);
		return Task.CompletedTask;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void Write(string? value)
	{
		if (value != null)
		{
			if (_pos > _chars.Length - value.Length)
			{
				Grow(value.Length);
			}
			value.AsSpan().CopyTo(_chars.AsSpan(_pos));
			_pos += value.Length;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void WriteLine(string? value)
	{
		Write(value);
		WriteLine();
	}

	public override Task WriteAsync(string? value)
	{
		Write(value);
		return Task.CompletedTask;
	}

	public override Task WriteLineAsync(string? value)
	{
		WriteLine(value);
		return Task.CompletedTask;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void Write(char[]? buffer)
	{
		if (buffer != null)
		{
			if (_pos > _chars.Length - buffer.Length)
			{
				Grow(buffer.Length);
			}
			buffer.CopyTo(_chars.AsSpan(_pos));
			_pos += buffer.Length;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void WriteLine(char[]? buffer)
	{
		Write(buffer);
		WriteLine();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void Write(char[] buffer, int index, int count)
	{
		if (buffer != null)
		{
			if (_pos > _chars.Length - count)
			{
				Grow(buffer.Length);
			}
			buffer.AsSpan(index, count).CopyTo(_chars.AsSpan(_pos));
			_pos += count;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void WriteLine(char[] buffer, int index, int count)
	{
		Write(buffer, index, count);
		WriteLine();
	}

	public override Task WriteAsync(char[] buffer, int index, int count)
	{
		Write(buffer, index, count);
		return Task.CompletedTask;
	}

	public override Task WriteLineAsync(char[] buffer, int index, int count)
	{
		WriteLine(buffer, index, count);
		return Task.CompletedTask;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void WriteLine()
	{
		string newLine = _newLine;
		foreach (char value in newLine)
		{
			Write(value);
		}
	}

	public override Task WriteLineAsync()
	{
		WriteLine();
		return Task.CompletedTask;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GrowAndAppend(char value)
	{
		Grow(1);
		Write(value);
	}

	private void Grow(int additionalCapacityBeyondPos)
	{
		char[] array = new char[Math.Max((uint)(_pos + additionalCapacityBeyondPos), (uint)(_chars.Length * 2))];
		_chars.AsSpan(0, _pos).CopyTo(array);
		_chars = array;
	}

	public override void Flush()
	{
	}

	public override void Close()
	{
	}

	public override Task FlushAsync()
	{
		return Task.CompletedTask;
	}

	public void Reset()
	{
		_pos = 0;
	}

	public override string ToString()
	{
		return AsSpan().ToString();
	}

	public ReadOnlySpan<char> AsSpan()
	{
		return _chars.AsSpan(0, _pos);
	}
}
