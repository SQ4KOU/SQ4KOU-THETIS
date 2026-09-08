using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Collections;

internal interface IOrderedReadOnlySet<T> : IReadOnlySet<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>
{
}
