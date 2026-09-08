namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedRecordObjectMethod : SynthesizedRecordOrdinaryMethod
{
	protected abstract SpecialMember OverriddenSpecialMember { get; }

	protected SynthesizedRecordObjectMethod(SourceMemberContainerTypeSymbol containingType, string name, int memberOffset, bool isReadOnly)
		: base(containingType, name, memberOffset, (DeclarationModifiers)(0x40010 | (isReadOnly ? 1024 : 0)))
	{
	}

	protected sealed override void MethodChecks(BindingDiagnosticBag diagnostics)
	{
		base.MethodChecks(diagnostics);
		VerifyOverridesMethodFromObject(this, OverriddenSpecialMember, diagnostics);
	}

	internal static bool VerifyOverridesMethodFromObject(MethodSymbol overriding, SpecialMember overriddenSpecialMember, BindingDiagnosticBag diagnostics)
	{
		bool flag = false;
		if (!overriding.IsOverride)
		{
			flag = true;
		}
		else
		{
			MethodSymbol methodSymbol = overriding.OverriddenMethod?.OriginalDefinition;
			if ((object)methodSymbol != null && (!(methodSymbol.ContainingType is SourceMemberContainerTypeSymbol { IsRecord: not false }) || !(methodSymbol.ContainingModule == overriding.ContainingModule)))
			{
				MethodSymbol leastOverriddenMethod = overriding.GetLeastOverriddenMethod(null);
				flag = (object)leastOverriddenMethod != overriding.ContainingAssembly.GetSpecialTypeMember(overriddenSpecialMember) && leastOverriddenMethod.ReturnType.Equals(overriding.ReturnType, TypeCompareKind.AllIgnoreOptions);
			}
		}
		if (flag)
		{
			diagnostics.Add(ErrorCode.ERR_DoesNotOverrideMethodFromObject, overriding.GetFirstLocation(), overriding);
		}
		return flag;
	}

	internal sealed override int TryGetOverloadResolutionPriority()
	{
		return 0;
	}
}
