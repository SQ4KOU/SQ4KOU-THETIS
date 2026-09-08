using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace System.Reactive.PlatformServices;

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IExceptionServices
{
	[DoesNotReturn]
	void Rethrow(Exception exception);
}
