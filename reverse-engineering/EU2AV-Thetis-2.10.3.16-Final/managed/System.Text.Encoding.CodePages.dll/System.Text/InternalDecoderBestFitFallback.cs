using System.Diagnostics.CodeAnalysis;

namespace System.Text;

internal sealed class InternalDecoderBestFitFallback : DecoderFallback
{
	internal System.Text.BaseCodePageEncoding encoding;

	internal char[] arrayBestFit;

	internal char cReplacement = '?';

	public override int MaxCharCount => 1;

	internal InternalDecoderBestFitFallback(System.Text.BaseCodePageEncoding _encoding)
	{
		encoding = _encoding;
	}

	public override DecoderFallbackBuffer CreateFallbackBuffer()
	{
		return new System.Text.InternalDecoderBestFitFallbackBuffer(this);
	}

	public override bool Equals([NotNullWhen(true)] object value)
	{
		if (value is System.Text.InternalDecoderBestFitFallback internalDecoderBestFitFallback)
		{
			return encoding.CodePage == internalDecoderBestFitFallback.encoding.CodePage;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return encoding.CodePage;
	}
}
