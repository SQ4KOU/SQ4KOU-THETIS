using System;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

[Flags]
internal enum QuickAttributes : byte
{
	None = 0,
	TypeIdentifier = 1,
	TypeForwardedTo = 2,
	IndexerName = 4,
	AssemblyKeyName = 8,
	AssemblyKeyFile = 0x10,
	AssemblySignatureKey = 0x20,
	Last = AssemblySignatureKey
}
