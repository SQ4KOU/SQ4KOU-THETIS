using System.Diagnostics;

namespace System.Linq;

[AttributeUsage(AttributeTargets.Assembly)]
[Conditional("COMPILE_TIME_ONLY")]
internal sealed class DuplicateAsyncEnumerableAsAsyncEnumerableDeprecatedAttribute : Attribute
{
}
