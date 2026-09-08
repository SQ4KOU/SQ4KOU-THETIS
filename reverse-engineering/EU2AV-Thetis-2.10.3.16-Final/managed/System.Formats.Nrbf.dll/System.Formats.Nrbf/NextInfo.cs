using System.Collections.Generic;
using System.Diagnostics;

namespace System.Formats.Nrbf;

[DebuggerDisplay("{Parent.RecordType}, {Allowed}, {PrimitiveType}")]
internal readonly struct NextInfo
{
	internal AllowedRecordTypes Allowed { get; }

	internal SerializationRecord Parent { get; }

	internal Stack<NextInfo> Stack { get; }

	internal PrimitiveType PrimitiveType { get; }

	internal NextInfo(AllowedRecordTypes allowed, SerializationRecord parent, Stack<NextInfo> stack, PrimitiveType primitiveType = (PrimitiveType)0)
	{
		Allowed = allowed;
		Parent = parent;
		Stack = stack;
		PrimitiveType = primitiveType;
	}

	internal NextInfo With(AllowedRecordTypes allowed, PrimitiveType primitiveType)
	{
		return new NextInfo(allowed, Parent, Stack, primitiveType);
	}
}
