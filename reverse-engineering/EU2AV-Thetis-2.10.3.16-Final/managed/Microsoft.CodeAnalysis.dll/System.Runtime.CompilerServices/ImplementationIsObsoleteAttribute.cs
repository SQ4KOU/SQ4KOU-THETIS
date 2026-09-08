namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
internal sealed class ImplementationIsObsoleteAttribute(string url) : Attribute
{
	public string Url { get; } = url;
}
