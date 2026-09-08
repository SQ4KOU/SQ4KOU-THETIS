using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedSpanSwitchHashMethod : SynthesizedGlobalMethodSymbol
{
	private readonly bool _isReadOnlySpan;

	internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, this.GetNonNullSyntaxNode(), compilationState, diagnostics);
		syntheticBoundNodeFactory.CurrentFunction = this;
		try
		{
			ParameterSymbol p = Parameters[0];
			NamedTypeSymbol newOwner = syntheticBoundNodeFactory.WellKnownType(_isReadOnlySpan ? WellKnownType.System_ReadOnlySpan_T : WellKnownType.System_Span_T).Construct(syntheticBoundNodeFactory.SpecialType(SpecialType.System_Char));
			LocalSymbol localSymbol = syntheticBoundNodeFactory.SynthesizedLocal(syntheticBoundNodeFactory.SpecialType(SpecialType.System_Int32));
			LocalSymbol localSymbol2 = syntheticBoundNodeFactory.SynthesizedLocal(syntheticBoundNodeFactory.SpecialType(SpecialType.System_UInt32));
			LabelSymbol label = syntheticBoundNodeFactory.GenerateLabel("again");
			LabelSymbol label2 = syntheticBoundNodeFactory.GenerateLabel("start");
			BoundBlock body = syntheticBoundNodeFactory.Block(ImmutableArray.Create(localSymbol2, localSymbol), syntheticBoundNodeFactory.Assignment(syntheticBoundNodeFactory.Local(localSymbol2), syntheticBoundNodeFactory.Literal(2166136261u)), syntheticBoundNodeFactory.Assignment(syntheticBoundNodeFactory.Local(localSymbol), syntheticBoundNodeFactory.Literal(0)), syntheticBoundNodeFactory.Goto(label2), syntheticBoundNodeFactory.Label(label), syntheticBoundNodeFactory.Assignment(syntheticBoundNodeFactory.Local(localSymbol2), syntheticBoundNodeFactory.Binary(BinaryOperatorKind.Multiplication, localSymbol2.Type, syntheticBoundNodeFactory.Binary(BinaryOperatorKind.Xor, localSymbol2.Type, syntheticBoundNodeFactory.Convert(localSymbol2.Type, syntheticBoundNodeFactory.Call(syntheticBoundNodeFactory.Parameter(p), syntheticBoundNodeFactory.WellKnownMethod(_isReadOnlySpan ? WellKnownMember.System_ReadOnlySpan_T__get_Item : WellKnownMember.System_Span_T__get_Item).AsMember(newOwner), syntheticBoundNodeFactory.Local(localSymbol)), Conversion.ImplicitNumeric), syntheticBoundNodeFactory.Local(localSymbol2)), syntheticBoundNodeFactory.Literal(16777619))), syntheticBoundNodeFactory.Assignment(syntheticBoundNodeFactory.Local(localSymbol), syntheticBoundNodeFactory.Binary(BinaryOperatorKind.Addition, localSymbol.Type, syntheticBoundNodeFactory.Local(localSymbol), syntheticBoundNodeFactory.Literal(1))), syntheticBoundNodeFactory.Label(label2), syntheticBoundNodeFactory.If(syntheticBoundNodeFactory.Binary(BinaryOperatorKind.LessThan, syntheticBoundNodeFactory.SpecialType(SpecialType.System_Boolean), syntheticBoundNodeFactory.Local(localSymbol), syntheticBoundNodeFactory.Call(syntheticBoundNodeFactory.Parameter(p), syntheticBoundNodeFactory.WellKnownMethod(_isReadOnlySpan ? WellKnownMember.System_ReadOnlySpan_T__get_Length : WellKnownMember.System_Span_T__get_Length).AsMember(newOwner))), syntheticBoundNodeFactory.Goto(label)), syntheticBoundNodeFactory.Return(syntheticBoundNodeFactory.Local(localSymbol2)));
			syntheticBoundNodeFactory.CloseMethod(body);
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
		}
	}

	internal SynthesizedSpanSwitchHashMethod(SynthesizedPrivateImplementationDetailsType privateImplType, TypeSymbol returnType, TypeSymbol paramType, bool isReadOnlySpan)
		: base(privateImplType, returnType, isReadOnlySpan ? "ComputeReadOnlySpanHash" : "ComputeSpanHash")
	{
		_isReadOnlySpan = isReadOnlySpan;
		SetParameters(ImmutableArray.Create(SynthesizedParameterSymbol.Create(this, TypeWithAnnotations.Create(paramType), 0, RefKind.None, "s")));
	}
}
