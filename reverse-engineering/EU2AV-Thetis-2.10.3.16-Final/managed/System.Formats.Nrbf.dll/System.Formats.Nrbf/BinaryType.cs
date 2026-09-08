namespace System.Formats.Nrbf;

internal enum BinaryType : byte
{
	Primitive,
	String,
	Object,
	SystemClass,
	Class,
	ObjectArray,
	StringArray,
	PrimitiveArray
}
