using System.Reflection;

namespace Microsoft.CodeAnalysis;

internal static class MethodImplAttributeExtensions
{
	extension(MethodImplAttributes)
	{
		public static MethodImplAttributes Async => (MethodImplAttributes)8192;
	}
}
