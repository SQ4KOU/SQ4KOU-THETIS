using System.Diagnostics.CodeAnalysis;

namespace System.Text;

internal sealed class InternalEncoderBestFitFallback : EncoderFallback
{
	internal System.Text.BaseCodePageEncoding encoding;

	internal char[] arrayBestFit;

	public override int MaxCharCount => 1;

	internal InternalEncoderBestFitFallback(System.Text.BaseCodePageEncoding _encoding)
	{
		encoding = _encoding;
	}

	public override EncoderFallbackBuffer CreateFallbackBuffer()
	{
		return new System.Text.InternalEncoderBestFitFallbackBuffer(this);
	}

	public override bool Equals([NotNullWhen(true)] object value)
	{
		if (value is System.Text.InternalEncoderBestFitFallback internalEncoderBestFitFallback)
		{
			return encoding.CodePage == internalEncoderBestFitFallback.encoding.CodePage;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return encoding.CodePage;
	}
}
