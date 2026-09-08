using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Debugging;

internal readonly struct CustomDebugInfoRecord(CustomDebugInfoKind kind, byte version, ImmutableArray<byte> data)
{
	public readonly CustomDebugInfoKind Kind = kind;

	public readonly byte Version = version;

	public readonly ImmutableArray<byte> Data = data;
}
