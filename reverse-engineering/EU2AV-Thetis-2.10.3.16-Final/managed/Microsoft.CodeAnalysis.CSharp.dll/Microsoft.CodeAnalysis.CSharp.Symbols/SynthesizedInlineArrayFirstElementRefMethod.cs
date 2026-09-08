using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedInlineArrayFirstElementRefMethod : SynthesizedGlobalMethodSymbol
{
	public override RefKind RefKind => RefKind.Ref;

	internal SynthesizedInlineArrayFirstElementRefMethod(SynthesizedPrivateImplementationDetailsType privateImplType, string synthesizedMethodName)
		: base(privateImplType, synthesizedMethodName)
	{
		SetTypeParameters(ImmutableArray.Create((TypeParameterSymbol)new SynthesizedSimpleMethodTypeParameterSymbol(this, 0, "TBuffer"), (TypeParameterSymbol)new SynthesizedSimpleMethodTypeParameterSymbol(this, 1, "TElement")));
		SetReturnType(TypeParameters[1]);
		SetParameters(ImmutableArray.Create(SynthesizedParameterSymbol.Create(this, TypeWithAnnotations.Create(TypeParameters[0]), 0, RefKind.Ref, "buffer")));
	}

	internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, this.GetNonNullSyntaxNode(), compilationState, diagnostics);
		syntheticBoundNodeFactory.CurrentFunction = this;
		try
		{
			BoundReturnStatement body = syntheticBoundNodeFactory.Return(syntheticBoundNodeFactory.Call(null, syntheticBoundNodeFactory.WellKnownMethod(WellKnownMember.System_Runtime_CompilerServices_Unsafe__As_T).Construct(ImmutableArray<TypeSymbol>.CastUp(TypeParameters)), syntheticBoundNodeFactory.Parameter(Parameters[0])));
			syntheticBoundNodeFactory.CloseMethod(body);
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
		}
	}
}
