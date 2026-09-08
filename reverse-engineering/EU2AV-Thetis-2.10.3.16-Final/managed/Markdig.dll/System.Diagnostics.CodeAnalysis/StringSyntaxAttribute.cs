namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
internal sealed class StringSyntaxAttribute : Attribute
{
	public const string Regex = "Regex";

	public const string Uri = "Uri";

	public const string Json = "Json";

	public const string Xml = "Xml";

	public const string CompositeFormat = "CompositeFormat";

	public string Syntax { get; }

	public object?[] Arguments { get; }

	public StringSyntaxAttribute(string syntax)
	{
		Syntax = syntax;
		Arguments = Array.Empty<object>();
	}

	public StringSyntaxAttribute(string syntax, params object?[] arguments)
	{
		Syntax = syntax;
		Arguments = arguments;
	}
}
