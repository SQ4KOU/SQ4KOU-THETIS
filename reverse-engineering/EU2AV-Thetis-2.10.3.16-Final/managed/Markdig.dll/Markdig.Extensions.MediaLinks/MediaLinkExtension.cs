using System;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.MediaLinks;

public class MediaLinkExtension : IMarkdownExtension
{
	public MediaOptions Options { get; }

	public MediaLinkExtension()
		: this(new MediaOptions())
	{
	}

	public MediaLinkExtension(MediaOptions? options)
	{
		Options = options ?? new MediaOptions();
	}

	public void Setup(MarkdownPipelineBuilder pipeline)
	{
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is HtmlRenderer htmlRenderer)
		{
			LinkInlineRenderer linkInlineRenderer = htmlRenderer.ObjectRenderers.FindExact<LinkInlineRenderer>();
			if (linkInlineRenderer != null)
			{
				linkInlineRenderer.TryWriters.Remove(TryLinkInlineRenderer);
				linkInlineRenderer.TryWriters.Add(TryLinkInlineRenderer);
			}
		}
	}

	private bool TryLinkInlineRenderer(HtmlRenderer renderer, LinkInline linkInline)
	{
		if (!linkInline.IsImage || linkInline.Url == null)
		{
			return false;
		}
		string text = linkInline.Url;
		bool isSchemaRelative = false;
		if (text.StartsWith("//", StringComparison.Ordinal))
		{
			text = "https:" + text;
			isSchemaRelative = true;
		}
		if (!Uri.TryCreate(text, UriKind.RelativeOrAbsolute, out var result))
		{
			return false;
		}
		if (result.IsAbsoluteUri && TryRenderIframeFromKnownProviders(result, isSchemaRelative, renderer, linkInline))
		{
			return true;
		}
		if (TryGuessAudioVideoFile(result, isSchemaRelative, renderer, linkInline))
		{
			return true;
		}
		return false;
	}

	private static HtmlAttributes GetHtmlAttributes(LinkInline linkInline)
	{
		HtmlAttributes htmlAttributes = new HtmlAttributes();
		linkInline.TryGetAttributes()?.CopyTo(htmlAttributes, mergeIdAndProperties: false, shared: false);
		return htmlAttributes;
	}

	private bool TryGuessAudioVideoFile(Uri uri, bool isSchemaRelative, HtmlRenderer renderer, LinkInline linkInline)
	{
		string text = (uri.IsAbsoluteUri ? uri.GetComponents(UriComponents.Path, UriFormat.Unescaped) : uri.ToString());
		int num = text.LastIndexOf('.');
		if (num >= 0 && Options.ExtensionToMimeType.TryGetValue(text.Substring(num), out string value))
		{
			HtmlAttributes htmlAttributes = GetHtmlAttributes(linkInline);
			bool num2 = value.StartsWith("audio", StringComparison.Ordinal);
			string text2 = (num2 ? "audio" : "video");
			renderer.Write("<" + text2);
			htmlAttributes.AddPropertyIfNotExist("width", Options.Width);
			if (!num2)
			{
				htmlAttributes.AddPropertyIfNotExist("height", Options.Height);
			}
			if (Options.AddControlsProperty)
			{
				htmlAttributes.AddPropertyIfNotExist("controls", null);
			}
			if (!string.IsNullOrEmpty(Options.Class))
			{
				htmlAttributes.AddClass(Options.Class);
			}
			renderer.WriteAttributes(htmlAttributes);
			renderer.Write("><source type=\"" + value + "\" src=\"" + linkInline.Url + "\"></source></" + text2 + ">");
			return true;
		}
		return false;
	}

	private bool TryRenderIframeFromKnownProviders(Uri uri, bool isSchemaRelative, HtmlRenderer renderer, LinkInline linkInline)
	{
		IHostProvider hostProvider = null;
		string iframeUrl = null;
		foreach (IHostProvider host in Options.Hosts)
		{
			if (host.TryHandle(uri, isSchemaRelative, out iframeUrl))
			{
				hostProvider = host;
				break;
			}
		}
		if (hostProvider == null)
		{
			return false;
		}
		HtmlAttributes htmlAttributes = GetHtmlAttributes(linkInline);
		renderer.Write("<iframe src=\"");
		renderer.WriteEscapeUrl(iframeUrl);
		renderer.Write('"');
		if (!string.IsNullOrEmpty(Options.Width))
		{
			htmlAttributes.AddPropertyIfNotExist("width", Options.Width);
		}
		if (!string.IsNullOrEmpty(Options.Height))
		{
			htmlAttributes.AddPropertyIfNotExist("height", Options.Height);
		}
		if (!string.IsNullOrEmpty(Options.Class))
		{
			htmlAttributes.AddClass(Options.Class);
		}
		string text = hostProvider.Class;
		if (text != null && text.Length > 0)
		{
			htmlAttributes.AddClass(text);
		}
		htmlAttributes.AddPropertyIfNotExist("frameborder", "0");
		if (hostProvider.AllowFullScreen)
		{
			htmlAttributes.AddPropertyIfNotExist("allowfullscreen", null);
		}
		renderer.WriteAttributes(htmlAttributes);
		renderer.Write("></iframe>");
		return true;
	}
}
