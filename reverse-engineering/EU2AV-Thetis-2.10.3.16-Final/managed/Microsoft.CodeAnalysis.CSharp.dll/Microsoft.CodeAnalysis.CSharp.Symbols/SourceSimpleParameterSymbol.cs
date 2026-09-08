using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceSimpleParameterSymbol : SourceParameterSymbol
{
	private readonly TypeWithAnnotations _parameterType;

	public override TypeWithAnnotations TypeWithAnnotations => _parameterType;

	public override bool IsDiscard => false;

	internal override ConstantValue? ExplicitDefaultConstantValue => null;

	internal override bool IsMetadataOptional => false;

	protected override bool HasParamsModifier => false;

	public override bool IsParamsArray => false;

	public override bool IsParamsCollection => false;

	internal override bool HasDefaultArgumentSyntax => false;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	internal override bool HasEnumeratorCancellationAttribute => false;

	internal override SyntaxReference? SyntaxReference => null;

	internal override bool IsExtensionMethodThis => false;

	internal override bool IsIDispatchConstant => false;

	internal override bool IsIUnknownConstant => false;

	internal override bool IsCallerFilePath => false;

	internal override bool IsCallerLineNumber => false;

	internal override bool IsCallerMemberName => false;

	internal override int CallerArgumentExpressionParameterIndex => -1;

	internal override ImmutableArray<int> InterpolatedStringHandlerArgumentIndexes => ImmutableArray<int>.Empty;

	internal override bool HasInterpolatedStringHandlerArgumentError => false;

	internal override FlowAnalysisAnnotations FlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

	internal override ImmutableHashSet<string> NotNullIfParameterNotNull => ImmutableHashSet<string>.Empty;

	internal override MarshalPseudoCustomAttributeData? MarshallingInformation => null;

	internal override bool HasOptionalAttribute => false;

	internal override SyntaxList<AttributeListSyntax> AttributeDeclarationList => default(SyntaxList<AttributeListSyntax>);

	internal override ConstantValue? DefaultValueFromAttributes => null;

	internal override ScopedKind EffectiveScope => CalculateEffectiveScopeIgnoringAttributes();

	internal override bool HasUnscopedRefAttribute => false;

	public SourceSimpleParameterSymbol(Symbol owner, TypeWithAnnotations parameterType, int ordinal, RefKind refKind, string name, ImmutableArray<Location> locations)
		: this(owner, parameterType, ordinal, refKind, name, locations.FirstOrDefault())
	{
	}

	public SourceSimpleParameterSymbol(Symbol owner, TypeWithAnnotations parameterType, int ordinal, RefKind refKind, string name, Location? location)
		: base(owner, ordinal, refKind, ScopedKind.None, name, location)
	{
		_parameterType = parameterType;
	}

	internal override CustomAttributesBag<CSharpAttributeData> GetAttributesBag()
	{
		state.NotePartComplete(CompletionPart.Attributes);
		return CustomAttributesBag<CSharpAttributeData>.Empty;
	}
}
