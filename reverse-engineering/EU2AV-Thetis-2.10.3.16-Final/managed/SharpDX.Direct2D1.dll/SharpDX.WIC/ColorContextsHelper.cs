namespace SharpDX.WIC;

public static class ColorContextsHelper
{
	internal static Result TryGetColorContexts(ColorContextsProvider getColorContexts, ImagingFactory imagingFactory, out ColorContext[] colorContexts)
	{
		colorContexts = null;
		Result result = getColorContexts(0, null, out var actualCountRef);
		if (result.Success)
		{
			colorContexts = new ColorContext[actualCountRef];
			if (actualCountRef > 0)
			{
				for (int i = 0; i < actualCountRef; i++)
				{
					colorContexts[i] = new ColorContext(imagingFactory);
				}
				getColorContexts(actualCountRef, colorContexts, out var _);
			}
		}
		return result;
	}

	internal static ColorContext[] TryGetColorContexts(ColorContextsProvider getColorContexts, ImagingFactory imagingFactory)
	{
		Result result = TryGetColorContexts(getColorContexts, imagingFactory, out var colorContexts);
		if (ResultCode.UnsupportedOperation != result)
		{
			result.CheckError();
		}
		return colorContexts;
	}

	internal static ColorContext[] GetColorContexts(ColorContextsProvider getColorContexts, ImagingFactory imagingFactory)
	{
		TryGetColorContexts(getColorContexts, imagingFactory, out var colorContexts).CheckError();
		return colorContexts;
	}
}
