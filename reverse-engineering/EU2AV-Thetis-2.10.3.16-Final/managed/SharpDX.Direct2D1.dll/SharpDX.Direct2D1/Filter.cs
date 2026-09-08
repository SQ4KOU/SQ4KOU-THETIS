namespace SharpDX.Direct2D1;

public enum Filter
{
	MinimumMagMipPoint = 0,
	MinimumMagPointMipLinear = 1,
	MinimumPointMagLinearMipPoint = 4,
	MinimumPointMagMipLinear = 5,
	MinimumLinearMagMipPoint = 16,
	MinimumLinearMagPointMipLinear = 17,
	MinimumMagLinearMipPoint = 20,
	MinimumMagMipLinear = 21,
	Anisotropic = 85
}
