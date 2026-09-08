using System.Diagnostics;

namespace System.Linq;

[AttributeUsage(AttributeTargets.Method)]
[Conditional("COMPILE_TIME_ONLY")]
internal sealed class GenerateAsyncOverloadAttribute : Attribute
{
}
