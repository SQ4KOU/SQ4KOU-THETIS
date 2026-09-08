using System;
using System.Runtime.Serialization;

namespace Svg;

[Serializable]
public class SvgIDExistsException : SvgIDException
{
	public SvgIDExistsException()
	{
	}

	public SvgIDExistsException(string message)
		: base(message)
	{
	}

	public SvgIDExistsException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected SvgIDExistsException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
