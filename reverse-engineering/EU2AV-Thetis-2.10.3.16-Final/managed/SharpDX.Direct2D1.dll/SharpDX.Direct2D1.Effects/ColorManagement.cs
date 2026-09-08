namespace SharpDX.Direct2D1.Effects;

public class ColorManagement : Effect
{
	private ColorContext sourceContext;

	private ColorContext destinationContext;

	public ColorContext SourceContext
	{
		get
		{
			if (sourceContext == null)
			{
				sourceContext = GetComObjectValue<ColorContext>(0);
			}
			return sourceContext;
		}
		set
		{
			sourceContext = value;
			SetValue(0, sourceContext);
		}
	}

	public ColorManagementRenderingIntent SourceIntent
	{
		get
		{
			return GetEnumValue<ColorManagementRenderingIntent>(1);
		}
		set
		{
			SetEnumValue(1, value);
		}
	}

	public ColorContext DestinationContext
	{
		get
		{
			if (destinationContext == null)
			{
				destinationContext = GetComObjectValue<ColorContext>(2);
			}
			return destinationContext;
		}
		set
		{
			destinationContext = value;
			SetValue(2, destinationContext);
		}
	}

	public ColorManagementRenderingIntent DestinationIntent
	{
		get
		{
			return GetEnumValue<ColorManagementRenderingIntent>(3);
		}
		set
		{
			SetEnumValue(3, value);
		}
	}

	public ColorManagementAlphaMode AlphaMode
	{
		get
		{
			return GetEnumValue<ColorManagementAlphaMode>(4);
		}
		set
		{
			SetEnumValue(4, value);
		}
	}

	public ColorManagement(DeviceContext context)
		: base(context, Effect.ColorManagement)
	{
	}
}
