namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal readonly struct CommonTypeNameFormatterOptions
{
	public int ArrayBoundRadix { get; }

	public bool ShowNamespaces { get; }

	public CommonTypeNameFormatterOptions(int arrayBoundRadix, bool showNamespaces)
	{
		ArrayBoundRadix = arrayBoundRadix;
		ShowNamespaces = showNamespaces;
	}
}
