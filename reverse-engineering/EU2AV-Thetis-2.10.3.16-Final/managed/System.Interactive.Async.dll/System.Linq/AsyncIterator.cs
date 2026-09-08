using System.Threading.Tasks;

namespace System.Linq;

internal abstract class AsyncIterator<TSource> : AsyncIteratorBase<TSource>
{
	protected TSource _current;

	public override TSource Current => _current;

	public override ValueTask DisposeAsync()
	{
		_current = default(TSource);
		return base.DisposeAsync();
	}
}
