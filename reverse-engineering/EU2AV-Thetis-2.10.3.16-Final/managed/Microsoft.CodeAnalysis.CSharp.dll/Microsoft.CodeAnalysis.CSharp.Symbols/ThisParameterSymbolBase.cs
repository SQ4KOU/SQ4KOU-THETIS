using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class ThisParameterSymbolBase : ParameterSymbol
{
	internal const string SymbolName = "this";

	public sealed override string Name => "this";

	public sealed override bool IsDiscard => false;

	public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	internal sealed override ConstantValue? ExplicitDefaultConstantValue => null;

	internal sealed override ConstantValue? DefaultValueFromAttributes => null;

	internal sealed override bool IsMetadataOptional => false;

	public sealed override bool IsParamsArray => false;

	public sealed override bool IsParamsCollection => false;

	internal sealed override bool IsIDispatchConstant => false;

	internal sealed override bool IsIUnknownConstant => false;

	internal sealed override bool IsCallerFilePath => false;

	internal sealed override bool IsCallerLineNumber => false;

	internal sealed override bool IsCallerMemberName => false;

	internal sealed override int CallerArgumentExpressionParameterIndex => -1;

	internal sealed override FlowAnalysisAnnotations FlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

	internal sealed override ImmutableHashSet<string> NotNullIfParameterNotNull => ImmutableHashSet<string>.Empty;

	public sealed override int Ordinal => -1;

	public sealed override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	internal override bool HasEnumeratorCancellationAttribute => false;

	public sealed override bool IsThis => true;

	public sealed override bool IsImplicitlyDeclared => true;

	internal sealed override bool IsMetadataIn => false;

	internal sealed override bool IsMetadataOut => false;

	internal sealed override MarshalPseudoCustomAttributeData? MarshallingInformation => null;

	internal sealed override ImmutableArray<int> InterpolatedStringHandlerArgumentIndexes => ImmutableArray<int>.Empty;

	internal sealed override bool HasInterpolatedStringHandlerArgumentError => false;
}
