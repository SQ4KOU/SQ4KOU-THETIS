using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedThrowSwitchExpressionExceptionMethod : SynthesizedGlobalMethodSymbol
{
	internal SynthesizedThrowSwitchExpressionExceptionMethod(SynthesizedPrivateImplementationDetailsType privateImplType, TypeSymbol returnType, TypeSymbol paramType)
		: base(privateImplType, returnType, "ThrowSwitchExpressionException")
	{
		SetParameters(ImmutableArray.Create(SynthesizedParameterSymbol.Create(this, TypeWithAnnotations.Create(paramType), 0, RefKind.None, "unmatchedValue")));
	}

	internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, this.GetNonNullSyntaxNode(), compilationState, diagnostics);
		syntheticBoundNodeFactory.CurrentFunction = this;
		try
		{
			ParameterSymbol p = Parameters[0];
			BoundThrowStatement body = syntheticBoundNodeFactory.Throw(syntheticBoundNodeFactory.New(syntheticBoundNodeFactory.WellKnownMethod(WellKnownMember.System_Runtime_CompilerServices_SwitchExpressionException__ctorObject), ImmutableArray.Create((BoundExpression)syntheticBoundNodeFactory.Parameter(p))));
			syntheticBoundNodeFactory.CloseMethod(body);
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
		}
	}
}
