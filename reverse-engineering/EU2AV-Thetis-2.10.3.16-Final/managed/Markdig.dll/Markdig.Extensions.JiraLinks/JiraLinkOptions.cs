using System;
using Markdig.Helpers;
using Markdig.Parsers;

namespace Markdig.Extensions.JiraLinks;

public class JiraLinkOptions : LinkOptions
{
	public string BaseUrl { get; set; }

	public string BasePath { get; set; }

	public JiraLinkOptions(string baseUrl)
	{
		base.OpenInNewWindow = true;
		BaseUrl = baseUrl;
		BasePath = "/browse";
	}

	public virtual string GetUrl()
	{
		Span<char> initialBuffer = stackalloc char[64];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		valueStringBuilder.Append(BaseUrl.AsSpan().TrimEnd('/'));
		valueStringBuilder.Append('/');
		valueStringBuilder.Append(BasePath.AsSpan().Trim('/'));
		return valueStringBuilder.ToString();
	}
}
