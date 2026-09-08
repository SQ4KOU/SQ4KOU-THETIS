using System;
using System.Runtime.Serialization;

namespace Svg;

[Serializable]
public class SvgException : FormatException
{
	public SvgException()
	{
	}

	public SvgException(string message)
		: base(message)
	{
	}

	public SvgException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected SvgException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
