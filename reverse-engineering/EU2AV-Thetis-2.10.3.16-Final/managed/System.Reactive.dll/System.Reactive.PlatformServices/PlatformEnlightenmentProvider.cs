using System.ComponentModel;

namespace System.Reactive.PlatformServices;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class PlatformEnlightenmentProvider
{
	private static IPlatformEnlightenmentProvider _current = CreatePlatformProvider();

	public static IPlatformEnlightenmentProvider Current
	{
		get
		{
			return _current;
		}
		set
		{
			_current = value ?? throw new ArgumentNullException("value");
		}
	}

	private static IPlatformEnlightenmentProvider CreatePlatformProvider()
	{
		return new CurrentPlatformEnlightenmentProvider();
	}
}
