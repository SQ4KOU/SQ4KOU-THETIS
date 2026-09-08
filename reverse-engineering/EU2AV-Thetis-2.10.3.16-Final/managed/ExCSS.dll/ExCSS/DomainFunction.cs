using System;

namespace ExCSS;

internal sealed class DomainFunction : DocumentFunction
{
	private readonly string _subdomain;

	public DomainFunction(string url)
		: base(FunctionNames.Domain, url)
	{
		_subdomain = "." + url;
	}

	public override bool Matches(Url url)
	{
		string hostName = url.HostName;
		if (!hostName.Isi(base.Data))
		{
			return hostName.EndsWith(_subdomain, StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}
}
