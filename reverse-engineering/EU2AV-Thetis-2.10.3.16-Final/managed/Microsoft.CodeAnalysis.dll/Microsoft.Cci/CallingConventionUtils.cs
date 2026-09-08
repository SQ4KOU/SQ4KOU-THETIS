using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;

namespace Microsoft.Cci;

internal static class CallingConventionUtils
{
	private const SignatureCallingConvention SignatureCallingConventionMask = (SignatureCallingConvention)15;

	private const SignatureAttributes SignatureAttributesMask = SignatureAttributes.Generic | SignatureAttributes.Instance | SignatureAttributes.ExplicitThis;

	internal static CallingConvention FromSignatureConvention(this SignatureCallingConvention convention)
	{
		if (!convention.IsValid())
		{
			throw new UnsupportedSignatureContent();
		}
		return (CallingConvention)(convention & (SignatureCallingConvention)0xF);
	}

	internal static bool IsValid(this SignatureCallingConvention convention)
	{
		if ((int)convention > 5)
		{
			return convention == SignatureCallingConvention.Unmanaged;
		}
		return true;
	}

	internal static SignatureCallingConvention ToSignatureConvention(this CallingConvention convention)
	{
		return (SignatureCallingConvention)((byte)convention & 0xF);
	}

	internal static bool IsCallingConvention(this CallingConvention original, CallingConvention compare)
	{
		return (original & (CallingConvention)0xF) == compare;
	}

	internal static bool HasUnknownCallingConventionAttributeBits(this CallingConvention convention)
	{
		return (convention & (CallingConvention)(-128)) != 0;
	}
}
