using Markdig.Syntax;

namespace Markdig.Extensions.AutoIdentifiers;

public class HeadingLinkReferenceDefinition : LinkReferenceDefinition
{
	public HeadingBlock Heading { get; set; }

	public HeadingLinkReferenceDefinition(HeadingBlock headling)
	{
		Heading = headling;
	}
}
