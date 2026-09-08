namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedReadOnlyListMethod : SynthesizedImplementationMethod
{
	private readonly GenerateMethodBodyDelegate _generateMethodBody;

	internal override bool SynthesizesLoweredBoundBody => true;

	internal SynthesizedReadOnlyListMethod(NamedTypeSymbol containingType, MethodSymbol interfaceMethod, GenerateMethodBodyDelegate generateMethodBody)
		: base(interfaceMethod, containingType)
	{
		_generateMethodBody = generateMethodBody;
	}

	internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, this.GetNonNullSyntaxNode(), compilationState, diagnostics);
		syntheticBoundNodeFactory.CurrentFunction = this;
		try
		{
			BoundStatement body = _generateMethodBody(syntheticBoundNodeFactory, this, _interfaceMethod);
			syntheticBoundNodeFactory.CloseMethod(body);
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
		}
	}
}
