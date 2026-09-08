using System;
using System.Runtime.Serialization;

namespace Svg;

[Serializable]
public class SvgIDException : FormatException
{
	public SvgIDException()
	{
	}

	public SvgIDException(string message)
		: base(message)
	{
	}

	public SvgIDException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected SvgIDException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
