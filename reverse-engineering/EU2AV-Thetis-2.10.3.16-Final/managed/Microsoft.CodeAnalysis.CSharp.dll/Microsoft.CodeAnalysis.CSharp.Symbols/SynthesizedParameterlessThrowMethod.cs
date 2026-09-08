using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedParameterlessThrowMethod : SynthesizedGlobalMethodSymbol
{
	private readonly MethodSymbol _exceptionConstructor;

	internal SynthesizedParameterlessThrowMethod(SynthesizedPrivateImplementationDetailsType privateImplType, TypeSymbol returnType, string synthesizedMethodName, MethodSymbol exceptionConstructor)
		: base(privateImplType, returnType, synthesizedMethodName)
	{
		_exceptionConstructor = exceptionConstructor;
		SetParameters(ImmutableArray<ParameterSymbol>.Empty);
	}

	internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, this.GetNonNullSyntaxNode(), compilationState, diagnostics);
		syntheticBoundNodeFactory.CurrentFunction = this;
		try
		{
			BoundThrowStatement body = syntheticBoundNodeFactory.Throw(syntheticBoundNodeFactory.New(_exceptionConstructor, ImmutableArray<BoundExpression>.Empty));
			syntheticBoundNodeFactory.CloseMethod(body);
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
		}
	}
}
