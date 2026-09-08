using System;

namespace SkiaSharp;

public class GRContextOptions
{
	public bool AvoidStencilBuffers { get; set; }

	public int RuntimeProgramCacheSize { get; set; } = 256;

	public int GlyphCacheTextureMaximumBytes { get; set; } = 8388608;

	public bool AllowPathMaskCaching { get; set; } = true;

	public bool DoManualMipmapping { get; set; }

	public int BufferMapThreshold { get; set; } = -1;

	internal GRContextOptionsNative ToNative()
	{
		return new GRContextOptionsNative
		{
			fAllowPathMaskCaching = (AllowPathMaskCaching ? ((byte)1) : ((byte)0)),
			fAvoidStencilBuffers = (AvoidStencilBuffers ? ((byte)1) : ((byte)0)),
			fBufferMapThreshold = BufferMapThreshold,
			fDoManualMipmapping = (DoManualMipmapping ? ((byte)1) : ((byte)0)),
			fGlyphCacheTextureMaximumBytes = (IntPtr)GlyphCacheTextureMaximumBytes,
			fRuntimeProgramCacheSize = RuntimeProgramCacheSize
		};
	}
}
