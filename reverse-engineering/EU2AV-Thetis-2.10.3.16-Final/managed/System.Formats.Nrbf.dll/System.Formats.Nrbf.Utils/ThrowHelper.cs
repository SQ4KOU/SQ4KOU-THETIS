using System.IO;
using System.Runtime.Serialization;

namespace System.Formats.Nrbf.Utils;

internal static class ThrowHelper
{
	internal static void ThrowDuplicateMemberName()
	{
		throw new SerializationException(System.SR.Serialization_DuplicateMemberName);
	}

	internal static void ThrowInvalidValue(int value)
	{
		throw new SerializationException(System.SR.Format(System.SR.Serialization_InvalidValue, value));
	}

	internal static void ThrowInvalidReference()
	{
		throw new SerializationException(System.SR.Serialization_InvalidReference);
	}

	internal static void ThrowInvalidTypeName()
	{
		throw new SerializationException(System.SR.Serialization_InvalidTypeName);
	}

	internal static void ThrowUnexpectedNullRecordCount()
	{
		throw new SerializationException(System.SR.Serialization_UnexpectedNullRecordCount);
	}

	internal static void ThrowArrayContainedNulls()
	{
		throw new SerializationException(System.SR.Serialization_ArrayContainedNulls);
	}

	internal static void ThrowInvalidAssemblyName()
	{
		throw new SerializationException(System.SR.Serialization_InvalidAssemblyName);
	}

	internal static void ThrowInvalidFormat()
	{
		throw new SerializationException(System.SR.Serialization_InvalidFormat);
	}

	internal static void ThrowEndOfStreamException()
	{
		throw new EndOfStreamException();
	}

	internal static void ThrowForUnexpectedRecordType(byte recordType)
	{
		if ((uint)(recordType - 2) <= 1u || (uint)(recordType - 18) <= 4u)
		{
			throw new NotSupportedException(System.SR.Format(System.SR.NotSupported_RecordType, recordType));
		}
		ThrowInvalidValue(recordType);
	}
}
