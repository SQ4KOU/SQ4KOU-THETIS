namespace System.Formats.Nrbf;

[Flags]
internal enum AllowedRecordTypes : uint
{
	None = 0u,
	SerializedStreamHeader = 1u,
	ClassWithId = 2u,
	SystemClassWithMembersAndTypes = 0x10u,
	ClassWithMembersAndTypes = 0x20u,
	BinaryObjectString = 0x40u,
	BinaryArray = 0x80u,
	MemberPrimitiveTyped = 0x100u,
	MemberReference = 0x200u,
	ObjectNull = 0x400u,
	MessageEnd = 0x800u,
	BinaryLibrary = 0x1000u,
	ObjectNullMultiple256 = 0x2000u,
	ObjectNullMultiple = 0x4000u,
	ArraySinglePrimitive = 0x8000u,
	ArraySingleObject = 0x10000u,
	ArraySingleString = 0x20000u,
	Nulls = ObjectNull | ObjectNullMultiple256 | ObjectNullMultiple,
	Arrays = BinaryArray | ArraySinglePrimitive | ArraySingleObject | ArraySingleString,
	AnyObject = Arrays | ClassWithId | SystemClassWithMembersAndTypes | ClassWithMembersAndTypes | BinaryObjectString | MemberPrimitiveTyped | MemberReference | ObjectNull
}
