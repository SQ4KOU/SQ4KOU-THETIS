using System;
using System.Net;

namespace Discord.Net.Rest;

public static class DefaultRestClientProvider
{
	public static readonly RestClientProvider Instance = Create();

	public static RestClientProvider Create(bool useProxy = false, IWebProxy webProxy = null)
	{
		return delegate(string url)
		{
			try
			{
				return new DefaultRestClient(url, useProxy, webProxy);
			}
			catch (PlatformNotSupportedException inner)
			{
				throw new PlatformNotSupportedException("The default RestClientProvider is not supported on this platform.", inner);
			}
		};
	}
}
