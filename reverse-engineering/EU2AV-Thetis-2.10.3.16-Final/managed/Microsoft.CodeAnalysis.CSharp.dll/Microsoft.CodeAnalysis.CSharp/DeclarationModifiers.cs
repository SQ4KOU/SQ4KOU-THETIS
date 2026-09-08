using System;

namespace Microsoft.CodeAnalysis.CSharp;

[Flags]
internal enum DeclarationModifiers : uint
{
	None = 0u,
	Abstract = 1u,
	Sealed = 2u,
	Static = 4u,
	New = 8u,
	Public = 0x10u,
	Protected = 0x20u,
	Internal = 0x40u,
	ProtectedInternal = 0x80u,
	Private = 0x100u,
	PrivateProtected = 0x200u,
	ReadOnly = 0x400u,
	Const = 0x800u,
	Volatile = 0x1000u,
	Extern = 0x2000u,
	Partial = 0x4000u,
	Unsafe = 0x8000u,
	Fixed = 0x10000u,
	Virtual = 0x20000u,
	Override = 0x40000u,
	Indexer = 0x80000u,
	Async = 0x100000u,
	Ref = 0x200000u,
	Required = 0x400000u,
	Scoped = 0x800000u,
	File = 0x1000000u,
	All = Abstract | Sealed | Static | New | Public | Protected | Internal | ProtectedInternal | Private | PrivateProtected | ReadOnly | Const | Volatile | Extern | Partial | Unsafe | Fixed | Virtual | Override | Indexer | Async | Ref | Required | Scoped | File,
	Unset = 0x2000000u,
	AccessibilityMask = 0x3F0u
}
