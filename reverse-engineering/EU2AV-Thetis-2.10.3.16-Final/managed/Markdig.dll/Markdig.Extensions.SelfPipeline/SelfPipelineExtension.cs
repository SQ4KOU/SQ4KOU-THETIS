using System;
using Markdig.Helpers;
using Markdig.Renderers;

namespace Markdig.Extensions.SelfPipeline;

public sealed class SelfPipelineExtension : IMarkdownExtension
{
	public const string DefaultTag = "markdig";

	public string? DefaultExtensions { get; }

	public string SelfPipelineHintTagStart { get; }

	public SelfPipelineExtension(string? tag = null, string? defaultExtensions = null)
	{
		tag = tag?.Trim();
		tag = (string.IsNullOrEmpty(tag) ? "markdig" : tag);
		if (tag.AsSpan().IndexOfAny('<', '>') >= 0)
		{
			ThrowHelper.ArgumentException("Tag cannot contain `<`  or `>` characters", "tag");
		}
		if (defaultExtensions != null)
		{
			new MarkdownPipelineBuilder().Configure(defaultExtensions);
		}
		DefaultExtensions = defaultExtensions;
		SelfPipelineHintTagStart = "<!--" + tag + ":";
	}

	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (pipeline.Extensions.Count > 1)
		{
			ThrowHelper.InvalidOperationException("The SelfPipeline extension cannot be configured with other extensions");
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
	}

	public MarkdownPipeline CreatePipelineFromInput(string inputText)
	{
		if (inputText == null)
		{
			ThrowHelper.ArgumentNullException("inputText");
		}
		MarkdownPipelineBuilder markdownPipelineBuilder = new MarkdownPipelineBuilder();
		string text = DefaultExtensions;
		int num = inputText.IndexOf(SelfPipelineHintTagStart, StringComparison.OrdinalIgnoreCase);
		if (num >= 0)
		{
			int num2 = num + SelfPipelineHintTagStart.Length;
			int num3 = inputText.IndexOf("-->", num2, StringComparison.OrdinalIgnoreCase);
			if (num3 >= 0)
			{
				text = inputText.Substring(num2, num3 - num2).Trim();
			}
		}
		if (text != null && text.Length > 0)
		{
			markdownPipelineBuilder.Configure(text);
		}
		return markdownPipelineBuilder.Build();
	}
}
