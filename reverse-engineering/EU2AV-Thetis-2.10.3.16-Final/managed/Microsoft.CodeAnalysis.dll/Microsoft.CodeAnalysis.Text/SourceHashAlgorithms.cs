using System;
using System.Security.Cryptography;

namespace Microsoft.CodeAnalysis.Text;

internal static class SourceHashAlgorithms
{
	public const SourceHashAlgorithm Default = SourceHashAlgorithm.Sha256;

	public const SourceHashAlgorithm OpenDocumentChecksumAlgorithm = SourceHashAlgorithm.Sha256;

	private static readonly Guid s_guidSha1 = new Guid(-15198484, -21922, 19728, 135, 247, 111, 73, 99, 131, 52, 96);

	private static readonly Guid s_guidSha256 = new Guid(-2010525681, 4536, 16915, 135, 139, 119, 14, 133, 151, 172, 22);

	public static bool IsSupportedAlgorithm(SourceHashAlgorithm algorithm)
	{
		return algorithm switch
		{
			SourceHashAlgorithm.Sha1 => true, 
			SourceHashAlgorithm.Sha256 => true, 
			_ => false, 
		};
	}

	public static Guid GetAlgorithmGuid(SourceHashAlgorithm algorithm)
	{
		return algorithm switch
		{
			SourceHashAlgorithm.Sha1 => s_guidSha1, 
			SourceHashAlgorithm.Sha256 => s_guidSha256, 
			_ => throw ExceptionUtilities.UnexpectedValue(algorithm), 
		};
	}

	public static SourceHashAlgorithm GetSourceHashAlgorithm(Guid guid)
	{
		if (!(guid == s_guidSha256))
		{
			if (!(guid == s_guidSha1))
			{
				return SourceHashAlgorithm.None;
			}
			return SourceHashAlgorithm.Sha1;
		}
		return SourceHashAlgorithm.Sha256;
	}

	private static HashAlgorithm CreateInstance(SourceHashAlgorithm algorithm)
	{
		return algorithm switch
		{
			SourceHashAlgorithm.Sha1 => SHA1.Create(), 
			SourceHashAlgorithm.Sha256 => SHA256.Create(), 
			_ => throw ExceptionUtilities.UnexpectedValue(algorithm), 
		};
	}

	public static HashAlgorithm CreateDefaultInstance()
	{
		return CreateInstance(SourceHashAlgorithm.Sha256);
	}
}
