using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis;

internal static class MethodImplOptionsExtensions
{
	extension(MethodImplOptions)
	{
		public static MethodImplOptions Async => (MethodImplOptions)8192;
	}
}
