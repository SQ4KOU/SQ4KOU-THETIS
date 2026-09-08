namespace System.Formats.Nrbf;

public enum SerializationRecordType
{
	SerializedStreamHeader = 0,
	ClassWithId = 1,
	SystemClassWithMembers = 2,
	ClassWithMembers = 3,
	SystemClassWithMembersAndTypes = 4,
	ClassWithMembersAndTypes = 5,
	BinaryObjectString = 6,
	BinaryArray = 7,
	MemberPrimitiveTyped = 8,
	MemberReference = 9,
	ObjectNull = 10,
	MessageEnd = 11,
	BinaryLibrary = 12,
	ObjectNullMultiple256 = 13,
	ObjectNullMultiple = 14,
	ArraySinglePrimitive = 15,
	ArraySingleObject = 16,
	ArraySingleString = 17,
	MethodCall = 21,
	MethodReturn = 22
}
