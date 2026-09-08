using System;

namespace SharpDX.IO;

[Flags]
public enum NativeFileAccess : uint
{
	Read = 0x80000000u,
	Write = 0x40000000u,
	ReadWrite = Read | Write,
	Execute = 0x20000000u,
	All = 0x10000000u
}
