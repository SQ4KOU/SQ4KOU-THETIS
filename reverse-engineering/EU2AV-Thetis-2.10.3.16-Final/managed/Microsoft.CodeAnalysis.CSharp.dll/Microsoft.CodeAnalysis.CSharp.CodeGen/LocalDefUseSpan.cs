namespace Microsoft.CodeAnalysis.CSharp.CodeGen;

internal readonly struct LocalDefUseSpan
{
	public readonly int Start;

	public readonly int End;

	public LocalDefUseSpan(int start)
		: this(start, start)
	{
	}

	private LocalDefUseSpan(int start, int end)
	{
		Start = start;
		End = end;
	}

	internal LocalDefUseSpan WithEnd(int end)
	{
		return new LocalDefUseSpan(Start, end);
	}

	public override string ToString()
	{
		string[] obj = new string[5] { "[", null, null, null, null };
		int start = Start;
		obj[1] = start.ToString();
		obj[2] = " ,";
		start = End;
		obj[3] = start.ToString();
		obj[4] = ")";
		return string.Concat(obj);
	}

	public bool ConflictsWith(LocalDefUseSpan other)
	{
		return Contains(other.Start) ^ Contains(other.End);
	}

	private bool Contains(int val)
	{
		if (Start < val)
		{
			return End > val;
		}
		return false;
	}

	public bool ConflictsWithDummy(LocalDefUseSpan dummy)
	{
		return Includes(dummy.Start) ^ Includes(dummy.End);
	}

	private bool Includes(int val)
	{
		if (Start <= val)
		{
			return End >= val;
		}
		return false;
	}
}
