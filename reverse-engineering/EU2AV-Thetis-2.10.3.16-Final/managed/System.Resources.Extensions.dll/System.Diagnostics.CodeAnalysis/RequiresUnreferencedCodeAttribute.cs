namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
internal sealed class RequiresUnreferencedCodeAttribute : Attribute
{
	public bool ExcludeStatics { get; set; }

	public string Message { get; }

	public string Url { get; set; }

	public RequiresUnreferencedCodeAttribute(string message)
	{
		Message = message;
	}
}
