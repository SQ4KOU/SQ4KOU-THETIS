namespace Microsoft.CodeAnalysis.CSharp;

internal interface IMemberResolutionResultWithPriority<TMember> where TMember : Symbol
{
	TMember? MemberWithPriority { get; }

	bool IsApplicable { get; }
}
