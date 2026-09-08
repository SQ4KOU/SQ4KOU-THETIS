namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
internal sealed class CollectionBuilderAttribute : Attribute
{
	public Type BuilderType { get; }

	public string MethodName { get; }

	public CollectionBuilderAttribute(Type builderType, string methodName)
	{
		BuilderType = builderType;
		MethodName = methodName;
	}
}
