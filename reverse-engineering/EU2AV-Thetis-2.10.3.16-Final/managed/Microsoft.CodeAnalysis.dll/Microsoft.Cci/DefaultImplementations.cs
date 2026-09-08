namespace Microsoft.Cci;

internal static class DefaultImplementations
{
	internal static bool HasBody(IMethodDefinition methodDef)
	{
		if (!methodDef.IsAbstract && !methodDef.IsExternal)
		{
			return !methodDef.ContainingTypeDefinition.IsComObject;
		}
		return false;
	}
}
