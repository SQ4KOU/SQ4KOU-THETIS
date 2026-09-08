using System;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.SmartyPants;

public class HtmlSmartyPantRenderer : HtmlObjectRenderer<SmartyPant>
{
	private static readonly SmartyPantOptions DefaultOptions = new SmartyPantOptions();

	private readonly SmartyPantOptions options;

	public HtmlSmartyPantRenderer(SmartyPantOptions? options)
	{
		this.options = options ?? throw new ArgumentNullException("options");
	}

	protected override void Write(HtmlRenderer renderer, SmartyPant obj)
	{
		if (!options.Mapping.TryGetValue(obj.Type, out string value))
		{
			DefaultOptions.Mapping.TryGetValue(obj.Type, out value);
		}
		renderer.Write(value);
	}
}
