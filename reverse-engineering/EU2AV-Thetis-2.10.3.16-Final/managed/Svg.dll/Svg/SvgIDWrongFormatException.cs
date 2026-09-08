using System;
using System.Runtime.Serialization;

namespace Svg;

[Serializable]
public class SvgIDWrongFormatException : SvgIDException
{
	public SvgIDWrongFormatException()
	{
	}

	public SvgIDWrongFormatException(string message)
		: base(message)
	{
	}

	public SvgIDWrongFormatException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected SvgIDWrongFormatException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
