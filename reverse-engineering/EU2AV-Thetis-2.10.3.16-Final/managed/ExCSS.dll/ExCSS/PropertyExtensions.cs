namespace ExCSS;

internal static class PropertyExtensions
{
	public static IPropertyValue Guard<T>(this Property[] properties)
	{
		if (properties.Length != 1)
		{
			return null;
		}
		if (!(properties[0].DeclaredValue is T))
		{
			return null;
		}
		return properties[0].DeclaredValue;
	}
}
