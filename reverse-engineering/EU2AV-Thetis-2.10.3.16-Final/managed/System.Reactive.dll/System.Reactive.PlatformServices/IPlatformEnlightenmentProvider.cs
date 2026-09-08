using System.ComponentModel;

namespace System.Reactive.PlatformServices;

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IPlatformEnlightenmentProvider
{
	T? GetService<T>(params object[] args) where T : class;
}
