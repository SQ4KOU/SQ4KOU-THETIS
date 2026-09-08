using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedReadOnlyListEnumeratorConstructor : SynthesizedInstanceConstructor
{
	public override ImmutableArray<ParameterSymbol> Parameters { get; }

	internal override bool SynthesizesLoweredBoundBody => true;

	internal SynthesizedReadOnlyListEnumeratorConstructor(SynthesizedReadOnlyListEnumeratorTypeSymbol containingType, TypeSymbol parameterType)
		: base(containingType)
	{
		Parameters = ImmutableArray.Create(SynthesizedParameterSymbol.Create(this, TypeWithAnnotations.Create(parameterType), 0, RefKind.None, "item"));
	}

	internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, this.GetNonNullSyntaxNode(), compilationState, diagnostics);
		syntheticBoundNodeFactory.CurrentFunction = this;
		try
		{
			MethodSymbol method = ContainingType.BaseTypeNoUseSiteDiagnostics.InstanceConstructors.Single();
			FieldSymbol f = ContainingType.GetFieldsToEmit().First();
			ParameterSymbol p = Parameters.Single();
			BoundBlock body = syntheticBoundNodeFactory.Block(syntheticBoundNodeFactory.ExpressionStatement(syntheticBoundNodeFactory.Call(syntheticBoundNodeFactory.This(), method)), syntheticBoundNodeFactory.Assignment(syntheticBoundNodeFactory.Field(syntheticBoundNodeFactory.This(), f), syntheticBoundNodeFactory.Parameter(p)), syntheticBoundNodeFactory.Return());
			syntheticBoundNodeFactory.CloseMethod(body);
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
		}
	}
}
