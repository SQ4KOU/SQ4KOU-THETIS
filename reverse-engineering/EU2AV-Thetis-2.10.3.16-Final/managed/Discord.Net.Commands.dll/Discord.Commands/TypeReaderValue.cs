using System;
using System.Diagnostics;

namespace Discord.Commands;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct TypeReaderValue
{
	public object Value { get; }

	public float Score { get; }

	private string DebuggerDisplay => $"[{Value}, {Math.Round(Score, 2)}]";

	public TypeReaderValue(object value, float score)
	{
		Value = value;
		Score = score;
	}

	public override string ToString()
	{
		return Value?.ToString();
	}
}
