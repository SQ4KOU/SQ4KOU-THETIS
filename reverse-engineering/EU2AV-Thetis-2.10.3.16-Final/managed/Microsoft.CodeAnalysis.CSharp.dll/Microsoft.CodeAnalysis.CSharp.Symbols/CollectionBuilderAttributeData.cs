namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class CollectionBuilderAttributeData
{
	public static readonly CollectionBuilderAttributeData Uninitialized = new CollectionBuilderAttributeData(null, null);

	public readonly TypeSymbol? BuilderType;

	public readonly string? MethodName;

	public CollectionBuilderAttributeData(TypeSymbol? builderType, string? methodName)
	{
		BuilderType = builderType;
		MethodName = methodName;
	}
}
