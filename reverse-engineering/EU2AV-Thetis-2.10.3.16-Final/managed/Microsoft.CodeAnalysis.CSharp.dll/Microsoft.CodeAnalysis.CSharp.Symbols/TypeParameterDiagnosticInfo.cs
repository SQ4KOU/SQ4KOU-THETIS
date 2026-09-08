namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal readonly struct TypeParameterDiagnosticInfo(TypeParameterSymbol typeParameter, UseSiteInfo<AssemblySymbol> useSiteInfo)
{
	public readonly TypeParameterSymbol TypeParameter = typeParameter;

	public readonly UseSiteInfo<AssemblySymbol> UseSiteInfo = useSiteInfo;
}
