namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
internal sealed class RequiresDynamicCodeAttribute : Attribute
{
	public bool ExcludeStatics { get; set; }

	public string Message { get; }

	public string Url { get; set; }

	public RequiresDynamicCodeAttribute(string message)
	{
		Message = message;
	}
}
