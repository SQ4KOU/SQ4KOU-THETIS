using System.Runtime.Serialization;

namespace System.Resources.Extensions.BinaryFormat;

internal static class SerializationExtensions
{
	public static SerializationException ConvertToSerializationException(this Exception ex)
	{
		if (!(ex is SerializationException result))
		{
			return new SerializationException(ex.Message, ex);
		}
		return result;
	}
}
