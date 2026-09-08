namespace System.Formats.Nrbf;

internal sealed class ObjectNullRecord : NullsRecord
{
	internal static ObjectNullRecord Instance { get; } = new ObjectNullRecord();

	public override SerializationRecordType RecordType => SerializationRecordType.ObjectNull;

	internal override int NullCount => 1;

	internal override object GetValue()
	{
		return null;
	}
}
