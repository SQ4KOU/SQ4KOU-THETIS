using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis;

internal static class MetadataBuilderExtensions
{
	internal static BlobHandle GetOrAddBlobAndFree(this MetadataBuilder metadataBuilder, PooledBlobBuilder builder)
	{
		BlobHandle orAddBlob = metadataBuilder.GetOrAddBlob(builder);
		builder.Free();
		return orAddBlob;
	}
}
