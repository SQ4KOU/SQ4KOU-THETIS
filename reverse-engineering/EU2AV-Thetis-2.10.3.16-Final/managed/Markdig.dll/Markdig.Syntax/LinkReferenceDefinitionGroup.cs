using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Markdig.Helpers;

namespace Markdig.Syntax;

public class LinkReferenceDefinitionGroup : ContainerBlock
{
	private static readonly StringComparer _unicodeIgnoreCaseComparer = StringComparer.InvariantCultureIgnoreCase;

	public Dictionary<string, LinkReferenceDefinition> Links { get; }

	public LinkReferenceDefinitionGroup()
		: base(null)
	{
		Links = new Dictionary<string, LinkReferenceDefinition>(_unicodeIgnoreCaseComparer);
	}

	public void Set(string label, LinkReferenceDefinition link)
	{
		if (link == null)
		{
			ThrowHelper.ArgumentNullException("link");
		}
		if (!Contains(link))
		{
			Add(link);
			Links.TryAdd(label, link);
		}
	}

	public bool TryGet(string label, [NotNullWhen(true)] out LinkReferenceDefinition? link)
	{
		return Links.TryGetValue(label, out link);
	}
}
