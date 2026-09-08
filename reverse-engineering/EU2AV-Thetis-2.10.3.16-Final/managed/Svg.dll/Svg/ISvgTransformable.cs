using Svg.Transforms;

namespace Svg;

public interface ISvgTransformable
{
	SvgTransformCollection Transforms { get; set; }

	void PushTransforms(ISvgRenderer renderer);

	void PopTransforms(ISvgRenderer renderer);
}
