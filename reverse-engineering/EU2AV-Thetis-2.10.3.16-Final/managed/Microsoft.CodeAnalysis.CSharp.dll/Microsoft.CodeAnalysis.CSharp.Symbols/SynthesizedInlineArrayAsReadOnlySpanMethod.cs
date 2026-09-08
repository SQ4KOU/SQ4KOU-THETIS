using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedInlineArrayAsReadOnlySpanMethod : SynthesizedGlobalMethodSymbol
{
	internal SynthesizedInlineArrayAsReadOnlySpanMethod(SynthesizedPrivateImplementationDetailsType privateImplType, string synthesizedMethodName, NamedTypeSymbol spanType, NamedTypeSymbol intType)
		: base(privateImplType, synthesizedMethodName)
	{
		SetTypeParameters(ImmutableArray.Create((TypeParameterSymbol)new SynthesizedSimpleMethodTypeParameterSymbol(this, 0, "TBuffer"), (TypeParameterSymbol)new SynthesizedSimpleMethodTypeParameterSymbol(this, 1, "TElement")));
		SetReturnType(spanType.Construct(TypeParameters[1]));
		SetParameters(ImmutableArray.Create(SynthesizedParameterSymbol.Create(this, TypeWithAnnotations.Create(TypeParameters[0]), 0, RefKind.In, "buffer"), SynthesizedParameterSymbol.Create(this, TypeWithAnnotations.Create(intType), 1, RefKind.None, "length")));
	}

	internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, this.GetNonNullSyntaxNode(), compilationState, diagnostics);
		syntheticBoundNodeFactory.CurrentFunction = this;
		try
		{
			BoundReturnStatement body = syntheticBoundNodeFactory.Return(syntheticBoundNodeFactory.Call(null, syntheticBoundNodeFactory.WellKnownMethod(WellKnownMember.System_Runtime_InteropServices_MemoryMarshal__CreateReadOnlySpan).Construct(TypeParameters[1]), syntheticBoundNodeFactory.Call(null, syntheticBoundNodeFactory.WellKnownMethod(WellKnownMember.System_Runtime_CompilerServices_Unsafe__As_T).Construct(ImmutableArray<TypeSymbol>.CastUp(TypeParameters)), syntheticBoundNodeFactory.Call(null, syntheticBoundNodeFactory.WellKnownMethod(WellKnownMember.System_Runtime_CompilerServices_Unsafe__AsRef_T).Construct(TypeParameters[0]), syntheticBoundNodeFactory.Parameter(Parameters[0]))), syntheticBoundNodeFactory.Parameter(Parameters[1])));
			syntheticBoundNodeFactory.CloseMethod(body);
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
		}
	}
}
