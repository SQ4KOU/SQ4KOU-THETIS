using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Cci;

internal readonly struct DebugSourceInfo
{
	public readonly Guid ChecksumAlgorithmId;

	public readonly ImmutableArray<byte> Checksum;

	public readonly ImmutableArray<byte> EmbeddedTextBlob;

	public DebugSourceInfo(ImmutableArray<byte> checksum, SourceHashAlgorithm checksumAlgorithm, ImmutableArray<byte> embeddedTextBlob = default(ImmutableArray<byte>))
		: this(checksum, SourceHashAlgorithms.GetAlgorithmGuid(checksumAlgorithm), embeddedTextBlob)
	{
	}

	public DebugSourceInfo(ImmutableArray<byte> checksum, Guid checksumAlgorithmId, ImmutableArray<byte> embeddedTextBlob = default(ImmutableArray<byte>))
	{
		ChecksumAlgorithmId = checksumAlgorithmId;
		Checksum = checksum;
		EmbeddedTextBlob = embeddedTextBlob;
	}
}
