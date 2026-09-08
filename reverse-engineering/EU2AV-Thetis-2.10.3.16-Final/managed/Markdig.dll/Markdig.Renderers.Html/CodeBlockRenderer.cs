using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Renderers.Html;

public class CodeBlockRenderer : HtmlObjectRenderer<CodeBlock>
{
	[CompilerGenerated]
	[MaybeNull]
	private FrozenSet<string> _003CSpecialBlockMapping_003Ek__BackingField;

	public bool OutputAttributesOnPre { get; set; }

	public HashSet<string> BlocksAsDiv { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	public Dictionary<string, string> BlockMapping { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private FrozenSet<string> SpecialBlockMapping
	{
		get
		{
			return _003CSpecialBlockMapping_003Ek__BackingField ?? CreateNew();
			[MethodImpl(MethodImplOptions.NoInlining)]
			FrozenSet<string> CreateNew()
			{
				HashSet<string> hashSet = new HashSet<string>();
				foreach (string item in BlocksAsDiv)
				{
					hashSet.Add(item);
				}
				foreach (string key in BlockMapping.Keys)
				{
					hashSet.Add(key);
				}
				HashSet<string> set = hashSet;
				return _003CSpecialBlockMapping_003Ek__BackingField = set.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
			}
		}
	}

	protected override void Write(HtmlRenderer renderer, CodeBlock obj)
	{
		renderer.EnsureLine();
		if (obj is FencedCodeBlock fencedCodeBlock)
		{
			string info = fencedCodeBlock.Info;
			if (info != null && SpecialBlockMapping.Contains(info))
			{
				string infoPrefix = (obj.Parser as FencedCodeBlockParser)?.InfoPrefix ?? "language-";
				string content = (BlockMapping.TryGetValue(info, out string value) ? value : "div");
				if (renderer.EnableHtmlForBlock)
				{
					renderer.WriteRaw('<');
					renderer.Write(content).WriteAttributes(obj.TryGetAttributes(), (string cls) => (!cls.StartsWith(infoPrefix, StringComparison.Ordinal)) ? cls : cls.Substring(infoPrefix.Length)).WriteRaw('>');
				}
				renderer.WriteLeafRawLines(obj, writeEndOfLines: true, escape: true, softEscape: true);
				if (renderer.EnableHtmlForBlock)
				{
					renderer.Write("</").Write(content).WriteLine(">");
				}
				goto IL_014c;
			}
		}
		if (renderer.EnableHtmlForBlock)
		{
			renderer.Write("<pre");
			if (OutputAttributesOnPre)
			{
				renderer.WriteAttributes(obj);
			}
			renderer.WriteRaw("><code");
			if (!OutputAttributesOnPre)
			{
				renderer.WriteAttributes(obj);
			}
			renderer.WriteRaw('>');
		}
		renderer.WriteLeafRawLines(obj, writeEndOfLines: true, renderer.EnableHtmlEscape);
		if (renderer.EnableHtmlForBlock)
		{
			renderer.WriteLine("</code></pre>");
		}
		goto IL_014c;
		IL_014c:
		renderer.EnsureLine();
	}
}
