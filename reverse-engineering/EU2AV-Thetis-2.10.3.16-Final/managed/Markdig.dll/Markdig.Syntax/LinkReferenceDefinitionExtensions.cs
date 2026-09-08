using System.Diagnostics.CodeAnalysis;
using Markdig.Helpers;

namespace Markdig.Syntax;

public static class LinkReferenceDefinitionExtensions
{
	private static readonly object DocumentKey = typeof(LinkReferenceDefinitionGroup);

	public static bool ContainsLinkReferenceDefinition(this MarkdownDocument document, string label)
	{
		if (label == null)
		{
			ThrowHelper.ArgumentNullException_label();
		}
		if (!(document.GetData(DocumentKey) is LinkReferenceDefinitionGroup linkReferenceDefinitionGroup))
		{
			return false;
		}
		return linkReferenceDefinitionGroup.Links.ContainsKey(label);
	}

	public static void SetLinkReferenceDefinition(this MarkdownDocument document, string label, LinkReferenceDefinition linkReferenceDefinition, bool addGroup)
	{
		if (label == null)
		{
			ThrowHelper.ArgumentNullException_label();
		}
		document.GetLinkReferenceDefinitions(addGroup).Set(label, linkReferenceDefinition);
	}

	public static bool TryGetLinkReferenceDefinition(this MarkdownDocument document, string label, [NotNullWhen(true)] out LinkReferenceDefinition? linkReferenceDefinition)
	{
		if (label == null)
		{
			ThrowHelper.ArgumentNullException_label();
		}
		linkReferenceDefinition = null;
		if (!(document.GetData(DocumentKey) is LinkReferenceDefinitionGroup linkReferenceDefinitionGroup))
		{
			return false;
		}
		return linkReferenceDefinitionGroup.TryGet(label, out linkReferenceDefinition);
	}

	public static LinkReferenceDefinitionGroup GetLinkReferenceDefinitions(this MarkdownDocument document, bool addGroup)
	{
		LinkReferenceDefinitionGroup linkReferenceDefinitionGroup = document.GetData(DocumentKey) as LinkReferenceDefinitionGroup;
		if (linkReferenceDefinitionGroup == null)
		{
			linkReferenceDefinitionGroup = new LinkReferenceDefinitionGroup();
			document.SetData(DocumentKey, linkReferenceDefinitionGroup);
			if (addGroup)
			{
				document.Add(linkReferenceDefinitionGroup);
			}
		}
		return linkReferenceDefinitionGroup;
	}
}
