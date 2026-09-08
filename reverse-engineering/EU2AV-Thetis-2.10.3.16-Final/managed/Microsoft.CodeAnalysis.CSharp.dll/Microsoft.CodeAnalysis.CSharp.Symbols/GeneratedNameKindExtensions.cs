namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class GeneratedNameKindExtensions
{
	internal static bool IsTypeName(this GeneratedNameKind kind)
	{
		if (kind == GeneratedNameKind.DelegateCacheContainerType || (uint)(kind - 99) <= 1u || kind == GeneratedNameKind.DynamicCallSiteContainerType)
		{
			return true;
		}
		return false;
	}
}
