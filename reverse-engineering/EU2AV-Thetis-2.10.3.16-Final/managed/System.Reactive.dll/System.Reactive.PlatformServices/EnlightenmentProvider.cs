namespace System.Reactive.PlatformServices;

public static class EnlightenmentProvider
{
	public static bool EnsureLoaded()
	{
		return PlatformEnlightenmentProvider.Current is CurrentPlatformEnlightenmentProvider;
	}
}
