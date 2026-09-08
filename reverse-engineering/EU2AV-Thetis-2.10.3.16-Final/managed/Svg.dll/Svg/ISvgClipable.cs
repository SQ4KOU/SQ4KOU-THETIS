using System;

namespace Svg;

public interface ISvgClipable
{
	Uri ClipPath { get; set; }

	SvgClipRule ClipRule { get; set; }

	void SetClip(ISvgRenderer renderer);

	void ResetClip(ISvgRenderer renderer);
}
