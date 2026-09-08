using System.Collections.Generic;

namespace System.Reactive;

internal interface IConcatenatable<out TSource>
{
	IEnumerable<IObservable<TSource>> GetSources();
}
