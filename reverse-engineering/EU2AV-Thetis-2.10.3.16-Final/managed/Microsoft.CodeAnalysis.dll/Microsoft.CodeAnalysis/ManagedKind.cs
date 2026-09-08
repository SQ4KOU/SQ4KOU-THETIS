using System;

namespace Microsoft.CodeAnalysis;

[Flags]
internal enum ManagedKind : byte
{
	Unknown = 0,
	Unmanaged = 1,
	UnmanagedWithGenerics = 2,
	Managed = Unmanaged | UnmanagedWithGenerics
}
