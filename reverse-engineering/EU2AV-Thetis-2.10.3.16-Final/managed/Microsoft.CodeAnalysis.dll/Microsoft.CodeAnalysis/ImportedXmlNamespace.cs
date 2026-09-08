namespace Microsoft.CodeAnalysis;

public readonly struct ImportedXmlNamespace
{
	public string XmlNamespace { get; }

	public SyntaxReference? DeclaringSyntaxReference { get; }

	internal ImportedXmlNamespace(string xmlNamespace, SyntaxReference? declaringSyntaxReference)
	{
		XmlNamespace = xmlNamespace;
		DeclaringSyntaxReference = declaringSyntaxReference;
	}
}
