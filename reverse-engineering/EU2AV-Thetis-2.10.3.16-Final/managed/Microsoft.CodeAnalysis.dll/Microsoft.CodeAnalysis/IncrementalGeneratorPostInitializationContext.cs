using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis;

public readonly struct IncrementalGeneratorPostInitializationContext
{
	internal readonly AdditionalSourcesCollection AdditionalSources;

	private readonly string _embeddedAttributeDefinition;

	public CancellationToken CancellationToken { get; }

	internal IncrementalGeneratorPostInitializationContext(AdditionalSourcesCollection additionalSources, string embeddedAttributeDefinition, CancellationToken cancellationToken)
	{
		AdditionalSources = additionalSources;
		_embeddedAttributeDefinition = embeddedAttributeDefinition;
		CancellationToken = cancellationToken;
	}

	public void AddSource(string hintName, string source)
	{
		AddSource(hintName, SourceText.From(source, Encoding.UTF8));
	}

	public void AddSource(string hintName, SourceText sourceText)
	{
		AdditionalSources.Add(hintName, sourceText);
	}

	public void AddEmbeddedAttributeDefinition()
	{
		AddSource("Microsoft.CodeAnalysis.EmbeddedAttribute", SourceText.From(_embeddedAttributeDefinition, Encoding.UTF8));
	}
}
