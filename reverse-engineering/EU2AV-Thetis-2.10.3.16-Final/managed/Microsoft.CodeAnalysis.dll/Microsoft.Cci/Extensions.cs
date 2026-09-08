using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal static class Extensions
{
	public static bool ShouldInclude(this ITypeDefinitionMember member, EmitContext context)
	{
		if (context.IncludePrivateMembers)
		{
			return true;
		}
		IMethodDefinition methodDefinition = member as IMethodDefinition;
		if (methodDefinition != null && methodDefinition.IsVirtual)
		{
			return true;
		}
		bool flag = true;
		switch (member.Visibility)
		{
		case TypeMemberVisibility.Private:
			flag = context.IncludePrivateMembers;
			break;
		case TypeMemberVisibility.FamilyAndAssembly:
		case TypeMemberVisibility.Assembly:
			flag = context.IncludePrivateMembers || (context.Module.SourceAssemblyOpt?.InternalsAreVisible ?? false);
			break;
		}
		if (flag)
		{
			return true;
		}
		if (methodDefinition != null && methodDefinition.IsStatic)
		{
			foreach (MethodImplementation explicitImplementationOverride in methodDefinition.ContainingTypeDefinition.GetExplicitImplementationOverrides(context))
			{
				if (explicitImplementationOverride.ImplementingMethod == methodDefinition)
				{
					return true;
				}
			}
		}
		if (methodDefinition != null && (context.Module.PEEntryPoint == methodDefinition || context.Module.DebugEntryPoint == methodDefinition))
		{
			return true;
		}
		return false;
	}
}
