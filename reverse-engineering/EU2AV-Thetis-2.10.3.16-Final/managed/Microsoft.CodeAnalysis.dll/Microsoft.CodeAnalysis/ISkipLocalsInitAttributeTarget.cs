namespace Microsoft.CodeAnalysis;

internal interface ISkipLocalsInitAttributeTarget
{
	bool HasSkipLocalsInitAttribute { get; set; }
}
