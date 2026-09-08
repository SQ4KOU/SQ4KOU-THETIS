namespace System;

internal static class Error
{
	public static Exception ArgumentNull(string paramName)
	{
		return new ArgumentNullException(paramName);
	}

	public static Exception ArgumentOutOfRange(string paramName)
	{
		return new ArgumentOutOfRangeException(paramName);
	}

	public static Exception NoElements()
	{
		return new InvalidOperationException(Strings.NO_ELEMENTS);
	}

	public static Exception MoreThanOneElement()
	{
		return new InvalidOperationException(Strings.MORE_THAN_ONE_ELEMENT);
	}

	public static Exception NotSupported()
	{
		return new NotSupportedException(Strings.NOT_SUPPORTED);
	}
}
