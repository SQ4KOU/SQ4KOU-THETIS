using System;

namespace Microsoft.CodeAnalysis.CSharp;

internal readonly struct MemberResolutionResult<TMember> : IMemberResolutionResultWithPriority<TMember> where TMember : Symbol
{
	private readonly TMember _member;

	private readonly TMember _leastOverriddenMember;

	private readonly MemberAnalysisResult _result;

	internal readonly bool HasTypeArgumentInferredFromFunctionType;

	internal bool IsNull => (object)_member == null;

	internal bool IsNotNull => (object)_member != null;

	public TMember Member => _member;

	internal TMember LeastOverriddenMember => _leastOverriddenMember;

	public MemberResolutionKind Resolution => Result.Kind;

	public bool IsValid => Result.IsValid;

	public bool IsApplicable => Result.IsApplicable;

	internal bool HasUseSiteDiagnosticToReport => _result.HasUseSiteDiagnosticToReportFor(_member);

	internal MemberAnalysisResult Result => _result;

	TMember IMemberResolutionResultWithPriority<TMember>.MemberWithPriority => LeastOverriddenMember;

	internal MemberResolutionResult(TMember member, TMember leastOverriddenMember, MemberAnalysisResult result, bool hasTypeArgumentInferredFromFunctionType)
	{
		_member = member;
		_leastOverriddenMember = leastOverriddenMember;
		_result = result;
		HasTypeArgumentInferredFromFunctionType = hasTypeArgumentInferredFromFunctionType;
	}

	internal MemberResolutionResult<TMember> WithResult(MemberAnalysisResult result)
	{
		return new MemberResolutionResult<TMember>(Member, LeastOverriddenMember, result, HasTypeArgumentInferredFromFunctionType);
	}

	internal MemberResolutionResult<TMember> Worse()
	{
		return WithResult(MemberAnalysisResult.Worse());
	}

	internal MemberResolutionResult<TMember> Worst()
	{
		return WithResult(MemberAnalysisResult.Worst());
	}

	public override bool Equals(object? obj)
	{
		throw new NotSupportedException();
	}

	public override int GetHashCode()
	{
		throw new NotSupportedException();
	}
}
