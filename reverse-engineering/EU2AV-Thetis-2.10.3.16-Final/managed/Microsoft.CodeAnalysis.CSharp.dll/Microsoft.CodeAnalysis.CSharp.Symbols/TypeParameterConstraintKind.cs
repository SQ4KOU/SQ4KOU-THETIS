using System;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

[Flags]
internal enum TypeParameterConstraintKind
{
	None = 0,
	ReferenceType = 1,
	ValueType = 2,
	Constructor = 4,
	Unmanaged = 8,
	NullableReferenceType = 0x11,
	NotNullableReferenceType = 0x21,
	ObliviousNullabilityIfReferenceType = 0x40,
	NotNull = 0x80,
	Default = 0x100,
	PartialMismatch = 0x200,
	ValueTypeFromConstraintTypes = 0x400,
	ReferenceTypeFromConstraintTypes = 0x800,
	AllowByRefLike = 0x1000,
	AllReferenceTypeKinds = 0x31,
	AllValueTypeKinds = ValueType | Unmanaged,
	AllNonNullableKinds = AllValueTypeKinds | ReferenceType | Constructor | AllowByRefLike
}
