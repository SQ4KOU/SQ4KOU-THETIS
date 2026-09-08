using System;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

public class PrintOptions
{
	private int _numberRadix = 10;

	private MemberDisplayFormat _memberDisplayFormat;

	private int _maximumOutputLength = 1024;

	public string Ellipsis { get; set; } = "...";

	public bool EscapeNonPrintableCharacters { get; set; } = true;

	public int NumberRadix
	{
		get
		{
			return _numberRadix;
		}
		set
		{
			if (!IsValidRadix(value))
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_numberRadix = value;
		}
	}

	public MemberDisplayFormat MemberDisplayFormat
	{
		get
		{
			return _memberDisplayFormat;
		}
		set
		{
			if (!value.IsValid())
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_memberDisplayFormat = value;
		}
	}

	public int MaximumOutputLength
	{
		get
		{
			return _maximumOutputLength;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_maximumOutputLength = value;
		}
	}

	protected virtual bool IsValidRadix(int radix)
	{
		if (radix == 10 || radix == 16)
		{
			return true;
		}
		return false;
	}
}
